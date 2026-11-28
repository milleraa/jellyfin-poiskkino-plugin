# 003 — Провайдеры: проброс из конфигурации, README, финальный гейт

## Метаданные

- **ID:** 003
- **Фича:** [search-limit-setting](../tech-plan.md)
- **Назначено:** `developer-csharp`
- **Стек:** C# / .NET (Jellyfin plugin SDK), xUnit, Moq
- **Ветка:** `feature/search-limit-setting`
- **Статус:** `todo`
- **Локальный коммит:** _заполняется исполнителем_ (без push)
- **Код:** [PoiskKinoMovieProvider.cs](../../../../../PoiskKinoMetadataPlugin/PoiskKinoMovieProvider.cs), [PoiskKinoSeriesProvider.cs](../../../../../PoiskKinoMetadataPlugin/PoiskKinoSeriesProvider.cs), [PoiskKinoImageProvider.cs](../../../../../PoiskKinoMetadataPlugin/PoiskKinoImageProvider.cs), [README.md](../../../../../README.md), [PluginTestFixture.cs](../../../../../PoiskKinoMetadataPlugin.UnitTests/PluginTestFixture.cs), провайдер-тесты ([фильмы](../../../../../PoiskKinoMetadataPlugin.UnitTests/Providers/PoiskKinoMovieProviderTests.cs))
- **Зависимости:** строго после [001](001-config-search-limit.md) (читает `Configuration.SearchLimit`) и [002](002-client-search-limit-cache.md) (передаёт параметр в `SearchAsync`)

## Описание

Соединить цепочку «настройка → поиск»: три провайдера читают `SearchLimit` из конфигурации на каждый вызов (паттерн `ApiKey`, применение без перезапуска — AC-7) и передают параметром в `SearchAsync`. Завершить фичу: README, полный прогон тестов и Release-сборка (AC-10).

## Acceptance criteria

1. **AC-2 (потребители).** В каждом из трёх провайдеров добавлено свойство-выражение `private int SearchLimit => Plugin.Instance?.Configuration?.SearchLimit ?? 3;` по образцу `ApiKey` (`PoiskKinoMovieProvider.cs:32`, комментарий о статике/CS0236 там же учитывается: свойство экземплярное, как `ApiKey`) и единственный вызов `SearchAsync` (`PoiskKinoMovieProvider.cs:79`, `PoiskKinoSeriesProvider.cs:79`, `PoiskKinoImageProvider.cs:95`) дополнен именованным аргументом `searchLimit: SearchLimit`.
2. **Fallback-путь.** `GetMetadataInternal → GetSearchResultsInternal` (фильмы/сериалы) проходит через ту же точку вызова — отдельной правки не требует; в задаче подтвердить комментарием/прогоном, что fallback-путь использует тот же проброс.
3. **AC-7 (тест).** Юнит-тест на провайдере: `PluginTestFixture.SetUpPlugin(TestApiKey)` + `Plugin.Instance.Configuration.SearchLimit = 7` + подмена HTTP → запрос уходит с `limit=7`; смена значения конфигурации между двумя поисками (`7` → `4`) меняет URL второго запроса без пересоздания клиента/провайдера. Инфраструктура: существующий `Mock<HttpMessageHandler>(MockBehavior.Strict)` + `Mock<IHttpClientFactory>` в провайдер-тестах (`PoiskKinoMovieProviderTests.cs:20-34`); проверка query — через `ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.Query.Contains("limit=7"))` или захват запроса. Достаточно одного провайдера (фильмы) — механизм у всех общий; для Series/Image достаточно правки вызова + зелёного прогона их тестов.
4. **AC-10 (README).** Обновить [README.md](../../../../../README.md): перечень фич/«Настройка» (шаг про поле «Количество результатов поиска»: назначение, дефолт 3, диапазон 1–10, **применяется без перезапуска** — контраст с заметкой про базовый URL `README.md:52`); раздел «Кэширование» (`README.md:93-95`): ключ поиска включает лимит, смена настройки видна на следующем некэшированном поиске, записи с разными лимитами не смешиваются; API-пример (`README.md:86`, `limit=3`) — примечание, что лимит настраивается.
5. **AC-9.** Код-ревью точка: изменений в подсчёте запросов (`MaxRequestsPerDay = 200`, `PoiskKinoApiClient.cs:16`) нет — увеличение лимита не увеличивает число запросов (одна страница поиска).
6. **AC-10 (гейт).** Полный `dotnet test` зелёный; `dotnet build -c Release` без новых предупреждений; `git diff --check` без замечаний.
7. **AC-6/AC-8 (инструкции для QA).** В отчёте задачи зафиксировать шаги ручных проверок: UI — сохранить значение → перезагрузить страницу → на месте (AC-6); персистентность — сохранить → перезапустить Jellyfin → значение в UI и в XML-файле конфигурации (AC-8); применение без перезапуска — сменить настройку → следующий поиск в identify-диалоге с новым лимитом (AC-7 smoke). В юнит-контур не входят — выполняет QA.

## Технические заметки

- `?? 3` в свойстве провайдера — защита от `Plugin.Instance == null`/`Configuration == null` (тот же контракт, что у `ApiKey` → `string.Empty`); валидация диапазона — не здесь, а в клиенте (002).
- Именованный аргумент `searchLimit:` обязателен: параметр стоит после `cancellationToken` (решение задачи 002), позиционная передача пятого аргумента ухудшит читаемость.
- README пишется один раз на финальное поведение — поэтому документация собрана в этой задаче, а не в 001/002.
- Дизайн: [architecture.md](../architecture.md) («Решение», «Потоки данных», «Компоненты и границы»), [ADR-0004](../../../../../docs/architecture/decisions/ADR-0004-configurable-search-limit.md) п. 1.

## DevOps impact check

По плану изменений нет: правки провайдеров и документации, новых секретов, env vars, интеграций, изменений CI, релиза, установки и диагностики нет. Техлид сверит с итоговым diff — это финальная задача фичи, гейт закрывается на её diff.

## Проверка и критерий готовности для Техлида

`dotnet test` (полный), `dotnet build -c Release`, `git diff --check`, точечно `dotnet test --filter 'FullyQualifiedName~PoiskKinoMovieProviderTests'`. Приложить: diff трёх провайдеров, новый тест AC-7 с прогоном, diff README, подтверждение отсутствия изменений в подсчёте запросов (AC-9), шаги ручных проверок для QA (п. 7).

## Результат и проверки

_Заполняется исполнителем._

## Решение Техлида

_Заполняется при приёмке (финальный гейт фичи перед tech-lead-review и QA)._
