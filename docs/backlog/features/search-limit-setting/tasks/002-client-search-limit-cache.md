# 002 — Клиент: параметр, резолвер валидации, ключ кэша

## Метаданные

- **ID:** 002
- **Фича:** [search-limit-setting](../tech-plan.md)
- **Назначено:** `developer-csharp`
- **Стек:** C# / .NET, xUnit, Moq
- **Ветка:** `feature/search-limit-setting`
- **Статус:** `todo`
- **Локальный коммит:** _заполняется исполнителем_ (без push)
- **Код:** [PoiskKinoApiClient.cs](../../../../../PoiskKinoMetadataPlugin/PoiskKinoApiClient.cs), [ApiBaseUrlResolver.cs](../../../../../PoiskKinoMetadataPlugin/ApiBaseUrlResolver.cs) (образец), [PoiskKinoApiClientTests.cs](../../../../../PoiskKinoMetadataPlugin.UnitTests/ApiClient/PoiskKinoApiClientTests.cs), [ApiBaseUrlResolverTests.cs](../../../../../PoiskKinoMetadataPlugin.UnitTests/Helpers/ApiBaseUrlResolverTests.cs) (образец)
- **Зависимости:** после [001](001-config-search-limit.md) (порядок плана); по коду не зависит — клиент конфигурацию не читает

## Описание

Заменить в `PoiskKinoApiClient.SearchAsync` хардкод `limit=3` (`PoiskKinoApiClient.cs:105`) параметром `int searchLimit = 3` с единой валидацией в клиенте и включить эффективное значение в ключ кэша поиска. Клиент остаётся транспортным адаптером без ссылок на `Plugin` (ADR-0001, ADR-0004).

## Acceptance criteria

1. **AC-2 (клиентская часть).** `SearchAsync` принимает `int searchLimit = 3`; URL строится как `…&limit={эффективное}` вместо хардкода. Параметр добавляется **после** `CancellationToken cancellationToken = default` (итоговая сигнатура `SearchAsync(string title, int? year, string apiKey, CancellationToken cancellationToken = default, int searchLimit = 3)`) — единственный вариант, при котором существующие позиционные вызовы с `CancellationToken` (провайдеры `:79/:79/:95`, тесты `SearchAsync(…, CancellationToken.None)`) компилируются без правок; XML-doc обновляется. Новый код в 003 передаёт `searchLimit` именованным аргументом.
2. **AC-4.** Единая валидация до построения URL и ключа кэша — внутренний статический резолвер `SearchLimitResolver` (новый файл по образцу `ApiBaseUrlResolver`: чистая функция без состояния, без исключений; константы `MinLimit = 1`, `MaxLimit = 10`, `DefaultLimit = 3`; результат — readonly-структура вида `ResolvedSearchLimit(int EffectiveLimit, bool UsedFallback, string? FallbackReason)`): значение 1–10 применяется как есть; 0, отрицательные и >10 — дефолт `3` + причина fallback (fallback, не clamping — отвергнутая альтернатива ADR-0004).
3. **AC-3.** При дефолтном значении запрос идентичен текущему: `/v1.5/movie/search?query=…&limit=3[&year=…]`.
4. **AC-5 (вариант А).** `GenerateCacheKey` (`PoiskKinoApiClient.cs:481-485`) получает эффективный лимит и строит `search:{normalizedTitle}[:{year}]:{limit}`; лимит присутствует в ключе всегда (без условного опускания при дефолте). Пара «запрос ⇔ ключ» детерминирована — оба строятся из одного разрешённого значения.
5. **AC-4 + A1.** При fallback пишется `Warning` (сырое значение + причина; API-ключ в лог не попадает) с дедупликацией по решению Техлида: приватное поле клиента «последнее предупреждённое невалидное значение» (`int`, init `int.MinValue`); предупреждение — только при отличии значения от памятки, памятка сбрасывается при применении валидного значения. Резолвер остаётся без состояния.
6. **AC-4 (тесты).** Таблица валидации: `0, −1, 11` → URL с `limit=3` + Warning; границы `1, 10` → применяются как есть без Warning; лог-ассерт через Moq-логгер (`PluginTestFixture.CreateLoggerMock<T>()`); дедупликация: два поиска с тем же невалидным значением → `Times.Once`, другое невалидное значение → новое предупреждение; исключения не выбрасываются ни в одном кейсе.
7. **AC-2/AC-3/AC-5 (тесты).** Через контур `CreateCapturingHandlerMock` (`PoiskKinoApiClientTests.cs:32`): явный `searchLimit: 7` → `limit=7` в query; дефолт → `limit=3`; кэш: тот же title/год и тот же лимит → один сетевой запрос; тот же title/год с другим лимитом → второй сетевой запрос (записи не смешиваются).
8. **Существующие кэш-тесты.** `SearchAsync_CachesResult` (`:183`) и `SearchAsync_NotFound_CachesNegativeResult` (`:242`) сохраняются без потери смысла (одинаковый лимит → тот же ключ) и расширяются: вариант с явным одинаковым `searchLimit` в обоих вызовах + парный тест разделения кэша по лимиту (п. 7). `SearchAsync_WithYear_IncludesYearInUrl` / `SearchAsync_Success_RequestsV15SearchPath` продолжают проходить без правок.
9. Тесты резолвера — отдельным классом по образцу `ApiBaseUrlResolverTests` (границы 1/10, 0, −1, 11, причина fallback).
10. Полный `dotnet test` зелёный; провайдеры и конфигурация в этой задаче не меняются.

## Технические заметки

- Лог-сообщение fallback по образцу клиента: без `{apiKey}`, например `Invalid search limit {rawValue} ({reason}), falling back to default {defaultLimit}`; `TruncateForLog` не требуется (int).
- Существующие тесты кэша проходят за счёт дефолта `searchLimit = 3`: дважды один лимит → тот же ключ. Их смысл по AC-5 не меняется — расширяются, не заменяются.
- Нечисловой XML сюда не входит: в резолвер приходит уже `int`; поведение Jellyfin при повреждённом конфиге не меняется (AC-4, R5 — код-ревью точка).
- Кэш старого формата истекает естественно (TTL 24 ч, негативный 1 ч, in-memory) — миграции нет (A2/A3 архитектуры).
- Дизайн: [architecture.md](../architecture.md) («Валидация диапазона», «Раскладка ключа кэша», «Влияние на тесты»), [ADR-0004](../../../../../docs/architecture/decisions/ADR-0004-configurable-search-limit.md) п. 2–3; решение A1 зафиксировано в [tech-plan.md](../tech-plan.md).

## DevOps impact check

По плану изменений нет: логика клиента и ключ кэша in-memory. Новых секретов, env vars, интеграций, изменений CI, релиза, установки и диагностики нет. Техлид сверит с итоговым diff.

## Проверка и критерий готовности для Техлида

`dotnet test`, точечно `dotnet test --filter 'FullyQualifiedName~PoiskKinoApiClientTests|FullyQualifiedName~SearchLimitResolverTests'`, `git diff --check`. Приложить: сигнатуру `SearchAsync` и резолвер, diff ключа кэша, перечень новых/расширенных тестов с прогоном до/после; подтверждение отсутствия ссылок на `Plugin` в клиенте.

## Результат и проверки

_Заполняется исполнителем._

## Решение Техлида

_Заполняется при приёмке (включая `test-audit` новых и расширенных тестов)._
