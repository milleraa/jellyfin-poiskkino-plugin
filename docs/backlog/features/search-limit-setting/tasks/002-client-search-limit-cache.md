# 002 — Клиент: параметр, резолвер валидации, ключ кэша

## Метаданные

- **ID:** 002
- **Фича:** [search-limit-setting](../tech-plan.md)
- **Назначено:** `developer-csharp`
- **Стек:** C# / .NET, xUnit, Moq
- **Ветка:** `feature/search-limit-setting`
- **Статус:** `done` — выполнено `developer-csharp` 2026-11-28, ожидает приёмки Техлида
- **Локальный коммит:** `feat(api): добавь параметр searchLimit, резолвер валидации и лимит в ключ кэша поиска` в ветке `feature/search-limit-setting` (без push)
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

- Из приёмки 001 (2026-11-28): в remarks `PluginConfiguration.SearchLimit` (`Configuration.cs`) добавить ссылку на `<see cref="SearchLimitResolver"/>` (место фразы про валидацию вне диапазона) — в 001 она отложена, чтобы не давать CS1574 до появления типа.</think><tool_call>edit<arg_key>newString</arg_key><arg_value>## Технические заметки

- Из приёмки 001 (2026-11-28): в remarks `PluginConfiguration.SearchLimit` (`Configuration.cs`) добавить ссылку на резолвер (`<see cref="SearchLimitResolver"/>`) — в 001 она отложена, чтобы не давать CS1574 до появления типа. Мелкая правка documentation-комментария в объёме 002.
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

Выполнено `developer-csharp` 2026-11-28. Изменены только `PoiskKinoApiClient.cs`, `SearchLimitResolver.cs` (новый), `Configuration.cs` (remarks) и тесты. Провайдеры (Movie/Series/Image), `configPage.html` и README не тронуты — зона задачи 003. Ссылок на `Plugin` в клиенте и резолвере нет (только namespace, User-Agent-строка и упоминания в doc-remarks).

**Что сделано.**

- AC-2: сигнатура `SearchAsync(string title, int? year, string apiKey, CancellationToken cancellationToken = default, int searchLimit = SearchLimitResolver.DefaultLimit)` — параметр строго после `CancellationToken`, существующие позиционные вызовы с `CancellationToken` (провайдеры `:79/:79/:95`, тесты `SearchAsync(…, CancellationToken.None)`) компилируются без правок; XML-doc дополнен (`<param name="searchLimit">` со ссылкой на резолвер). Дефолт — `SearchLimitResolver.DefaultLimit` (константа `3`), а не литерал: единственная точка хранения дефолта, по образцу `ApiBaseUrlResolver.DefaultBaseUrl`; для внешних вызовов значение неотличимо от `= 3`. URL — `…&limit={эффективное}` вместо хардкода.
- AC-4: новый `SearchLimitResolver.cs` по образцу `ApiBaseUrlResolver` — `internal static class`, чистая функция без состояния и без исключений; константы `MinLimit = 1`, `MaxLimit = 10`, `DefaultLimit = 3`; результат — `internal readonly record struct ResolvedSearchLimit(int EffectiveLimit, bool UsedFallback, string? FallbackReason)`; 1–10 — как есть, вне диапазона — дефолт + причина (fallback, не clamping). Причина на английском и называет сырое значение (попадает в Warning).
- AC-3: при дефолте запрос идентичен историческому — тест фиксирует точный query `?query=Test&limit=3`.
- AC-5 (вариант А): `GenerateCacheKey(string title, int? year, int searchLimit)` строит `search:{normalizedTitle}[:{year}]:{limit}` из того же разрешённого значения, что и URL; лимит в ключе всегда, без условного опускания; XML-doc remarks фиксируют детерминированность пары «запрос ⇔ ключ».
- AC-4 + A1: `Warning` «Invalid search limit {Raw} ({Reason}), falling back to default 3» пишется клиентом до построения URL/ключа (то есть и на кэш-хите — валидация по AC-4 единая и раньше ключа); дедупликация — приватное поле `_lastWarnedInvalidSearchLimit` (init `int.MinValue`) на экземпляре клиента: Warning только при смене невалидного значения, валидное значение сбрасывает памятку (метод `WarnInvalidSearchLimit` с remarks о потокобезопасности по tech-plan). API-ключ в сообщение не попадает (покрыто тестом).
- Приёмка 001: в remarks `PluginConfiguration.SearchLimit` добавлена ссылка `<see cref="SearchLimitResolver"/>` — CS1574 нет (тип существует), замечаний сборки нет.

**Тесты** (аудит по `test-audit` перед написанием; +40 кейсов, 286 → 326): `SearchLimitResolverTests` (26: константы, границы 1/10 и середина диапазона, вне диапазона 0/−1/−100/11/100/±int.MaxValue → дефолт+причина, отсутствие clamping к 1/10, причина называет сырое значение, инвариант «причина ⇔ fallback», без исключений на всех входах); `PoiskKinoApiClientSearchLimitTests` (11, по образцу `PoiskKinoApiClientBaseUrlTests`: дефолт `limit=3` без Warning, явный 1/7/10 → `limit=N`, невалидные 0/−1/11 → `limit=3` + ровно один Warning без исключений, состав Warning без API-ключа, дедупликация «тот же значение → Times.Once», «другое невалидное → новое», «валидный между невалидными → сброс памятки»); расширения в `PoiskKinoApiClientTests` (3: `SearchAsync_CachesResult_WithExplicitSearchLimit`, `SearchAsync_NotFound_CachesNegativeResult_WithExplicitSearchLimit`, `SearchAsync_DifferentSearchLimits_SeparateCacheEntries` — разделение кэша и применение настройки без нового клиента на одном экземпляре). Существующие `SearchAsync_CachesResult` / `SearchAsync_NotFound_CachesNegativeResult` сохранены без потери смысла; `SearchAsync_WithYear_IncludesYearInUrl` / `SearchAsync_Success_RequestsV15SearchPath` прошли без правок. Тест-only код в продукте не потребовался.

**Точечная проверка чувствительности** (по одной правдоподобной ошибке, исходник восстанавливался точно):

| Эксперимент | Результат |
|-----------|-----------|
| Убрать `:{limit}` из ключа кэша | Падает `SearchAsync_DifferentSearchLimits_SeparateCacheEntries` (2 запроса → 1) — тест ловит главную регрессию фичи |
| Убрать дедупликацию в `WarnInvalidSearchLimit` | Падает `SearchAsync_RepeatedInvalidSearchLimit_WarnsOnce` (Times.Once → 2) |
| Убрать сброс памятки в `SearchAsync` (не выполнялся, следует из механики) | Третье предупреждение в `SearchAsync_ValidSearchLimitBetweenInvalidOnes_ResetsWarningMemo` было бы подавлено → `Times.Exactly(2)` упал бы |

**Прогоны.** Полный `dotnet test` — 326/326; точечный фильтр `PoiskKinoApiClientTests|SearchLimitResolverTests|PoiskKinoApiClientSearchLimitTests|PoiskKinoApiClientBaseUrlTests` — 63/63; `dotnet build` — 0 ошибок; `git diff --check` — чисто. Набор предупреждений сборки сравнён с базой f845ad3 (`--no-incremental`, diff пуст): все 61 — предсуществующие CS1591 в `Models/`, от изменённых файлов предупреждений нет.

**Ограничения проверок и замечания для Техлида.**

1. Крайний случай дедупликации по решению A1: самый первый поиск клиента с сырым значением ровно `int.MinValue` не даст Warning (памятка инициализирована `int.MinValue`, значение неотличимо от «предупреждений ещё не было»). Зафиксировано в tech-plan как принятый компромисс; на уровне резолвера `int.MinValue` покрыт и валиден, клиентские тесты используют 0/−1/11.
2. Валидация выполняется до кэш-lookup — Warning возникает и когда запрос обслужен из кэша. Это следствие AC-4 («валидация до построения URL и ключа кэша»); если Техлид считает, что на кэш-хите предупреждать не нужно, точку резолва надо переносить осознанно.
3. Интеграционный сценарий «сменил настройку в UI → следующий поиск применил» (AC-7 end-to-end) в этой задаче покрыт серверной частью на уровне клиента (один экземпляр, разные лимиты); живой цикл Jellyfin — ручная проверка QA в задаче 003.
4. Кэш старого формата (`search:title[:year]`) не мигрирует — истекает естественно (TTL 24 ч / 1 ч, in-memory), по ADR-0004.

## Решение Техлида

_Заполняется при приёмке (включая `test-audit` новых и расширенных тестов)._
