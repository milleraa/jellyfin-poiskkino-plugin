# 001 — Конфигурационное свойство и страница настроек

## Метаданные

- **ID:** 001
- **Фича:** [search-limit-setting](../tech-plan.md)
- **Назначено:** `developer-csharp`
- **Стек:** C# / .NET (Jellyfin plugin SDK), HTML страницы настроек Dashboard, xUnit
- **Ветка:** `feature/search-limit-setting`
- **Статус:** `todo`
- **Локальный коммит:** _заполняется исполнителем_ (без push)
- **Код:** [Configuration.cs](../../../../../PoiskKinoMetadataPlugin/Configuration.cs), [configPage.html](../../../../../PoiskKinoMetadataPlugin/Configuration/configPage.html), [ConfigurationTests.cs](../../../../../PoiskKinoMetadataPlugin.UnitTests/ConfigurationTests.cs)

## Описание

Добавить в `PluginConfiguration` целочисленное свойство `SearchLimit` с дефолтом `3` (сохранение текущего поведения, D-1) и числовое поле на странице конфигурации плагина с русской подсказкой. Задача — конфигурационный слой фичи: клиент и провайдеры значение пока не читают (задачи [002](002-client-search-limit-cache.md) и [003](003-providers-threading-readme.md)).

## Acceptance criteria

1. **AC-1.** `Configuration.cs` содержит `public int SearchLimit { get; set; } = 3` с XML-doc (назначение: количество результатов поиска ПоискКино; дефолт 3; диапазон 1–10; имя свойства — часть XML-схемы конфигурации, как `ApiBaseUrl` — см. remarks `ApiBaseUrl`).
2. **AC-1.** Дефолт при отсутствии элемента: unit-тест по образцу `Deserialize_LegacyXmlWithoutApiBaseUrl_KeepsDefaultAndOtherValues` (`ConfigurationTests.cs:22`) — XML без `<SearchLimit>` десериализуется в `3`, при этом `ApiKey`/`ApiBaseUrl`/`IgnoreTmdbImages` не сбрасываются. Дополнительно тест дефолта `new PluginConfiguration().SearchLimit == 3`.
3. **AC-6.** `configPage.html`: поле `<input id="searchLimit" name="searchLimit" type="number" min="1" max="10" is="emby-input">` с подписью и подсказкой на русском: что делает, допустимый диапазон 1–10, значение по умолчанию 3, применение к новым поискам без перезапуска Jellyfin; при значении вне диапазона плагин применит 3 и напишет предупреждение в лог. Подсказка — только UX, серверной валидации в UI нет (D-ARCH-4).
4. **AC-6.** Загрузка: `loadConfiguration` заполняет поле сохранённым значением; при отсутствии (`config.SearchLimit` undefined/0 после десериализации отсутствующего элемента — фактически придёт 3) показывается `3`. Сохранение: `saveConfiguration` включает `config.SearchLimit` в `updatePluginConfiguration` вместе с остальными полями.
5. Полный `dotnet test` зелёный; `dotnet build` без новых предупреждений. UI не ломает существующие поля (API-ключ, базовый URL, TMDB).

## Технические заметки

- Паттерн UI — существующие блоки `inputContainer`/`fieldDescription` в `configPage.html` (поле `apiBaseUrl` с `datalist`); script-блоку добавляются две строки в `loadConfiguration`/`saveConfiguration`.
- Нечисловой/повреждённый XML — вне задачи: существующее поведение Jellyfin (пересоздание дефолтной конфигурации) сохраняется, новый код не добавляет исключений (AC-4, R5). Серверная валидация — в клиенте, задача 002.
- README в этой задаче не меняется (документация фичи — задача 003, AC-10).
- Дизайн: [architecture.md](../architecture.md) (компонент «Configuration.cs» / «configPage.html»), [ADR-0004](../../../../../docs/architecture/decisions/ADR-0004-configurable-search-limit.md).

## DevOps impact check

По плану изменений нет: новое поле хранится в существующем XML-файле конфигурации Jellyfin. Новых секретов, env vars, интеграций, изменений CI, релиза, установки и диагностики нет. Техлид сверит с итоговым diff.

## Проверка и критерий готовности для Техлида

`dotnet test`, `git diff --check`. Приложить: diff `Configuration.cs` + `configPage.html`, новые тесты и их прогон (точечный `dotnet test --filter 'FullyQualifiedName~PluginConfigurationTests'` и полный), подтверждение, что провайдеры/клиент в этой задаче не менялись.

## Результат и проверки

_Заполняется исполнителем._

## Решение Техлида

_Заполняется при приёмке._
