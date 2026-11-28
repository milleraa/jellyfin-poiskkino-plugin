# 001 — Конфигурационное свойство и страница настроек

## Метаданные

- **ID:** 001
- **Фича:** [search-limit-setting](../tech-plan.md)
- **Назначено:** `developer-csharp`
- **Стек:** C# / .NET (Jellyfin plugin SDK), HTML страницы настроек Dashboard, xUnit
- **Ветка:** `feature/search-limit-setting`
- **Статус:** `accepted` — приёмка Техлидом пройдена 2026-11-28
- **Локальный коммит:** `feat(config): добавь лимит результатов поиска в конфигурацию и страницу настроек` в ветке `feature/search-limit-setting` (без push)
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

Выполнено `developer-csharp` 2026-11-28. Изменены только `Configuration.cs`, `Configuration/configPage.html`, `ConfigurationTests.cs` (клиент, провайдеры, кэш и ключ кэша не тронуты — задачи 002/003).

**Что сделано.**

- AC-1: в `PluginConfiguration` добавлено `public int SearchLimit { get; set; } = 3` с XML-doc и remarks в стиле `ApiBaseUrl` (диапазон 1–10, дефолт 3 сохраняет историческое поведение, вне диапазона — замена на 3 с предупреждением в логе без исправления конфигурации, D-ARCH-4, ADR-0004; имя свойства — часть XML-схемы). Свойство добавлено последним членом класса, чтобы порядок XML-элементов в существующих файлах конфигурации не менялся. Ссылки на `SearchLimitResolver` в remarks нет: тип появится в задаче 002, и `<see cref>` дал бы предупреждение CS1574.
- AC-6: в `configPage.html` добавлен блок `inputContainer` с `<input id="searchLimit" name="searchLimit" type="number" min="1" max="10" is="emby-input">`, подписью и русской подсказкой (назначение, диапазон 1–10, дефолт 3, расход дневного лимита, замена вне диапазона на 3 с предупреждением в логе, применение без перезапуска). `loadConfiguration` показывает сохранённое значение, при отсутствии (undefined/0) — 3; `saveConfiguration` включает `config.SearchLimit` в `updatePluginConfiguration`, приводя значение поля к числу (`Number(...)`), чтобы в JSON уходил int, а не строка из input. Клиентская валидация/JS сверх этого не добавлялись.

**Тесты** (`PluginConfigurationTests`, по образцу существующих XML-тестов; аудит по `test-audit` перед написанием): `SearchLimit_Default_IsThree`, `Deserialize_LegacyXmlWithoutSearchLimit_KeepsDefaultAndOtherValues` (XML без `<SearchLimit>` → 3, прочие поля не сброшены), `SerializeAndDeserialize_WithCustomSearchLimit_PreservesValue` (значение 7 проходит цикл сохранения/загрузки). Тест-only код в продукте не потребовался.

Точечная проверка чувствительности: из `Configuration.cs` временно удалён инициализатор `= 3`; точный фильтр `FullyQualifiedName~PluginConfigurationTests`.

| Состояние | Результат |
|-----------|-----------|
| С инициализатором | 6/6 (3 существующих + 3 новых) |
| Без инициализатора | 4 прошло, 2 упали: `SearchLimit_Default_IsThree`, `Deserialize_LegacyXmlWithoutSearchLimit_KeepsDefaultAndOtherValues` — ровно те, что проверяют дефолт |
| После восстановления инициализатора | 6/6 |

**Прогоны.** Полный `dotnet test` — 286/286; `dotnet build` — 0 ошибок; `git diff --check` — без замечаний. В сборке видны предсуществующие предупреждения CS1591 в `Models/` (файлы не менялись, вне зоны задачи); предупреждений от изменённых файлов нет.

**Ограничения проверок.** XML-тесты проверяют сериализацию `XmlSerializer`, а не живой цикл конфигурации Jellyfin; UI-подсказка и применение значения без перезапуска — ручная проверка QA в живом Jellyfin (инструкция — в задаче 003). Серверная валидация вне диапазона — задача 002 (в UI её нет по D-ARCH-4). Отклонений от задачи нет.

## Решение Техлида

**Accept** (2026-11-28). Задача 001 закрыта; Оркестратору можно назначать задачу 002.

Проверено по diff `d01ee2d`:

- **AC-1** — `public int SearchLimit { get; set; } = 3` с XML-doc/remarks в стиле `ApiBaseUrl`; свойство добавлено последним членом класса — порядок XML-элементов существующих конфигов не меняется, отсутствующий элемент даёт дефолт 3 (подтверждено тестом legacy-XML). Ссылки в remarks ведут только на существующие члены — предупреждений CS1574 нет.
- **AC-6** — `configPage.html` консистентен с существующими полями: `inputContainer`/`inputLabelUnfocused` с `for="searchLimit"`, `id`=`name`, `is="emby-input"`, `min=1 max=10`; русская подсказка покрывает назначение, диапазон, дефолт, расход дневного лимита, fallback 3 + предупреждение в логе, применение без перезапуска. `loadConfiguration` — паттерн `apiBaseUrl` (undefined/0 → 3); `saveConfiguration` включает `config.SearchLimit` в `updatePluginConfiguration`.
- **Границы** — тронуты только `Configuration.cs`, `configPage.html`, `ConfigurationTests.cs`; клиент, провайдеры, кэш и ключ кэша не изменены (зона 002/003).
- **Прогоны (воспроизведено Техлидом)** — `dotnet build`: 0 ошибок; предупреждения только предсуществующие CS1591 в `Models/` (вне изменённых файлов — 0); `dotnet test` — 286/286; точечный фильтр `PluginConfigurationTests` — 6/6; `git diff --check` — чисто.
- **test-audit** — новые тесты проверяют поведение (дефолт, совместимость старого XML, round-trip), по образцу существующих, дублей нет; чувствительность к регрессии дефолта подтверждена мутационной проверкой разработчика (удаление инициализатора `= 3` роняет ровно два дефолт-теста).

Решения по замечаниям разработчика:

1. `Number(...)` в `saveConfiguration` — принято как тип-коерция (int в JSON), не валидация: соответствует D-ARCH-4 (UI не доверяем, серверная валидация — 002). Пустой/нечисловой ввод даёт `0` — резолвер 002 вернёт дефолт с `Warning`.
2. Отсутствие чекбоксов в файле задачи — несущественно: AC-покрытие полно в «Результате и проверках» с evidence.
3. cref на `SearchLimitResolver` в remarks — корректно отложен на 002 (сейчас тип не существует, `<see cref>` дал бы CS1574). В задачу 002 добавлен пункт: после создания резолвера дополнить remarks ссылку.
4. Серверная валидация диапазона — 002; ручной QA UI/персистентности — 003: соответствует DoD tech-plan (AC-6 в 001 — unit + код-ревью).

DevOps impact по итоговому diff: изменений нет (поле хранится в существующем XML-конфиге; секреты/env/CI/упаковка не затронуты) — DevOps-задача не требуется.
