# Tech plan — `eu-api-endpoint`

Владелец: **Техлид**. Дата: 2026-11-28.
Входы: [analysis.md](analysis.md) (AC-1…AC-12, NFR, риски R1–R9, решения D-1…D-8, §12 — решения Стейхолдера), [architecture.md](architecture.md) (D-ARCH-1…D-ARCH-8, риски A1–A8, §10 — кандидаты на декомпозицию), [ADR-0003](../../../architecture/decisions/ADR-0003-configurable-api-base-url.md), [status.md](status.md).

Работа идёт в единственном worktree `/home/alex/src/my/jellyfin-metadata-plugin`, ветка `feature/eu-api-endpoint`. Task-branches/worktrees не используются; задачи выполняются строго последовательно 001 → 002 → 003 → 004. Каждая задача завершается **локальным commit без push**.

Действующие требования, которые нельзя пересматривать на реализации:

- поле `PluginConfiguration.ApiBaseUrl` — строка, инициализатор = дефолт `https://api.poiskkino.dev` (AC-1);
- политика — чистый хелпер `ApiBaseUrlResolver` (`internal static`, `ResolvedBaseUrl(Uri, bool UsedFallback, string? Reason)`, без исключений) (D-ARCH-1, AC-3/AC-4);
- клиент — `PoiskKinoApiClient(..., string? apiBaseUrl = null)`, резолв один раз в конструкторе, `Information`/`Warning` в лог (D-ARCH-2, D-ARCH-5, AC-9);
- 5 провайдеров передают **сырое** значение из `Plugin.Instance.Configuration` (как `ApiKey`);
- смена значения применяется после перезапуска Jellyfin — AC-7′ (решение Стейхолдера Q2, D-ARCH-6), в `README.md` это пишется явно;
- `GET /v1.5/token` в UI — вне scope (architecture.md §5);
- таймаут 120 с, кэш 24 ч / 1 ч (404), `SemaphoreSlim(1,1)`, лимит 200/сутки, обработка 401/403/429/404 не меняются (AC-2, AC-12, D-ARCH-8).

## Декомпозиция

| ID | Задача | Роль / стек | Содержание | AC |
|----|--------|-------------|------------|----|
| [001](tasks/001-api-base-url-resolver.md) | `ApiBaseUrlResolver` + unit-тесты | `developer-csharp` / C#, xUnit | Новый `PoiskKinoMetadataPlugin/ApiBaseUrlResolver.cs`: `DefaultBaseUrl = "https://api.poiskkino.dev"`, `internal readonly record struct ResolvedBaseUrl(Uri BaseAddress, bool UsedFallback, string? FallbackReason)`, `Resolve(string?)` по таблице правил 1–10 из D-ARCH-1 (без исключений, без зависимостей от Jellyfin/HTTP/логгера). Тесты `PoiskKinoMetadataPlugin.UnitTests/Helpers/ApiBaseUrlResolverTests.cs` — theory-таблица на все правила + «не бросает исключение ни на одном входе». | AC-1 (значение дефолта), AC-3, AC-4 (политика) |
| [002](tasks/002-configuration-client-providers.md) | Конфигурация + клиент + 5 провайдеров + тесты фактического хоста | `developer-csharp` / C#, xUnit, Moq | `PluginConfiguration.ApiBaseUrl` (инициализатор = `ApiBaseUrlResolver.DefaultBaseUrl`); `PoiskKinoApiClient`: новый необязательный параметр `string? apiBaseUrl = null`, удаление `const ApiBaseUrl`, `BaseAddress` из `Resolve`, `Information` один раз при создании + `Warning` при fallback (сырое значение ≤ 200 символов, ключ не логируется); 5 провайдеров (`Movie`, `Series`, `Season`, `Episode`, `Image`) получают `private string ApiBaseUrl => Plugin.Instance?.Configuration?.ApiBaseUrl ?? string.Empty;` и передают его в конструктор клиента. Тесты: дефолт для пустой/старой XML-конфигурации, кастомный хост в захваченном `HttpRequestMessage.RequestUri` (host+scheme) для всех трёх запросов, fallback без исключения, лог-ассерты `Information`/`Warning` без `ApiKey`, проводка через провайдера. | AC-1, AC-2, AC-4, AC-5, AC-8 (кодовая часть), AC-9, AC-10, AC-12 |
| [003](tasks/003-config-page-field.md) | Поле базового URL в `Configuration/configPage.html` | `developer-csharp` / HTML + JS (страница плагина) | Поле `apiBaseUrl` (`type="text"`, `is="emby-input"`) + подсказка на русском с обоими хостами и пояснением «сервер вне РФ → `eu-api.poiskkino.dev`» (опционально `<datalist>`); загрузка сохранённого значения с подстановкой дефолта при отсутствии; сохранение вместе с `ApiKey`/`IgnoreTmdbImages`. Нормализация в JS не дублируется (D-ARCH-4). | AC-6, AC-3 (не терять ввод), AC-11 (частично) |
| [004](tasks/004-docs-base-url.md) | Документация: `README.md` + `docs/architecture/overview.md` | `developer-csharp` / документация | `README.md`: поле «Базовый URL API», дефолт, оба хоста, кратко правила нормализации (схема добавляется, завершающий `/` не нужен, путь не поддерживается), **явно «смена применяется после перезапуска Jellyfin»** (AC-7′). `docs/architecture/overview.md`: настраиваемый базовый адрес вместо единственного `api.poiskkino.dev` (диаграмма §строка ~16 и таблица интеграций ~строка 37). Формулировки нейтральные, без ссылки на пункт `llms.txt` (R6); произвольные хосты не рекламируются (R9/A5). | AC-7′, AC-11, AC-3 (документирование правил) |
| [005](tasks/005-redact-userinfo-all-at.md) | **Rework после приёмки:** полная санитизация `userinfo` в `Warning` | `developer-csharp` / C# + xUnit | `RedactUserInfo` вырезает весь `userinfo` до **последнего** `@` в authority (сейчас — до первого, поэтому пароль с `@` оставляет хвост в логе); guard на пустой authority; тесты на пароль с `@` и на `@` в пути; `ApiBaseUrlResolver` и остальное не меняются. | AC-9 (риск A6), AC-4 (без исключений) |

Разбиение — по предложенному Архитектором контуру (architecture.md §10), с двумя уточнениями по границам проверяемости:

1. `PluginConfiguration.ApiBaseUrl` перенесён из «политики» в задачу 002, а не в 001: поле — часть поверхности персистентности и проверяется вместе с пробросом значения до `HttpClient` (AC-1 + AC-8 одним диффом), тогда как 001 остаётся чистой функцией без влияния на поведение плагина (риск регрессии нулевой, тесты без моков).
2. Документация вынесена в отдельную задачу 004, а не приложена к 002: `README.md`/`overview.md` описывают поведение, которое должно быть уже принято в 002 (включая AC-7′), и не должны попадать в тот же review-дифф, что и код.

## Порядок выполнения

1. **001** (`developer-csharp`) — `ApiBaseUrlResolver` + тесты. Поведение плагина не меняется (новый файл + тесты). Приёмка: таблица правил D-ARCH-1 воспроизведена полностью, исключений нет, `dotnet build`/`dotnet test` зелёные.
2. **002** (`developer-csharp`) — конфигурация, клиент, 5 провайдеров, тесты. Приёмка: кастомный хост виден в `RequestUri` всех трёх запросов, дефолт для старой конфигурации, `Information`/`Warning` без ключа, хардкода хоста вне резолвера нет, кэш/таймаут/лимит не тронуты.
3. **003** (`developer-csharp`) — поле в `configPage.html`. Приёмка: значение загружается/сохраняется, при отсутствии подставляется дефолт, подсказка с двумя хостами на русском, JS не нормализует URL.
4. **004** (`developer-csharp`) — документация. Приёмка: оба хоста, дефолт, требование перезапуска, правила нормализации, нет упоминаний несуществующего пункта `llms.txt`.
5. **Техлид** — DevOps impact check по факту дизайна (см. ниже), ревью итогового диффа против `architecture.md`/ADR-0003, `status.md` → `qa`, owner `Orchestrator`; QA запускает Оркестратор.
6. **005** (`developer-csharp`) — rework по итогам приёмки (2026-11-28): полная санитизация `userinfo` в `Warning`. Приёмка: пароль с `@` не оставляет хвоста в логе, `@` вне authority не вырезается, резолвер и остальные AC не затронуты. После 005 — повторная приёмка Техлида и только затем `status.md` → `qa` (шаг 5).

## Зависимости

- **002 → 001** (клиент использует `ApiBaseUrlResolver.Resolve` и `DefaultBaseUrl`). **003 → 002** (UI сохраняет поле, которое читают провайдеры; имя `ApiBaseUrl` в camelCase `apiBaseUrl` задаётся моделью). **004 → 002, 003** (документируется фактическое поведение UI и клиента).
- Внешние зависимости: доступность `https://eu-api.poiskkino.dev` (объявлен в `servers` спеки, отвечает `401`); реального API-ключа нет (analysis F14, R2) — end-to-end smoke EU-зеркала остаётся за QA/администратором, unit-тесты проверяют хост запроса, а не сетевой ответ.
- Новые NuGet-пакеты, env-переменные, секреты, фоновые задачи не появляются (AC-12). `InternalsVisibleTo("PoiskKinoMetadataPlugin.UnitTests")` уже есть (`PoiskKinoMetadataPlugin/SearchYearFilter.cs:5`) — тесты 001 компилируются без правок csproj.

## Что проверяется на приёмке (Tech Lead review)

| Задача | Чеклист приёмки |
|--------|-----------------|
| 001 | `ApiBaseUrlResolver` — `internal static`, без `using` Jellyfin/HTTP/логирования; `DefaultBaseUrl` = `https://api.poiskkino.dev`; правила 1–10 покрыты теорией построчно; ни один вход (в т.ч. `null`, `""`, `"   "`, `"ftp://x"`, `"https://host/path"`, `"https://u:p@host"`, мусор) не даёт исключения; `UsedFallback`/`Reason` выставлены ровно по таблице; завершающие `/` удалены, порт сохранён; локальный commit без push. |
| 002 | `grep` по `PoiskKinoMetadataPlugin/`: константа хоста только в `ApiBaseUrlResolver`; 3 теста захватывают `RequestUri` и проверяют `Scheme`/`Host`/`Port` для кастомного значения; 2-аргументный конструктор клиента даёт прежний дефолт (существующие тесты не переписаны «под результат»); XML без `<ApiBaseUrl>` → дефолт, `ApiKey`/`IgnoreTmdbImages` сохранены; `Warning` при fallback один раз и без `ApiKey`; дифф не содержит изменений кэша, таймаута, семафора, лимита, обработки ошибок; 5 провайдеров передают сырое значение (никакой валидации в провайдерах); `dotnet build -c Release` и `dotnet test` зелёные, новых предупреждений кроме baseline CS1591 нет. |
| 003 | Поле присутствует, `loadConfiguration` подставляет дефолт при `undefined`/пустом `ApiBaseUrl`, `saveConfiguration` пишет значение в конфигурацию; подсказка содержит оба хоста и требование перезапуска; нормализация в JS отсутствует; остальные поля (`apiKey`, `ignoreTmdbImages`) работают как раньше; ручной сценарий «сохранить → перезагрузить страницу настроек» описан в задаче и передаётся QA. |
| 004 | `README.md` содержит поле, дефолт, оба хоста, правила нормализации и явную фразу о перезапуске (AC-7′); `docs/architecture/overview.md` больше не утверждает единственный хост; нет ссылки на пункт `llms.txt` про EU (R6), нет рекламы произвольных прокси (R9/A5); стиль и язык соответствуют текущим файлам. |

## DevOps impact check (по факту дизайна)

| Вопрос | Результат |
|--------|-----------|
| Новые/изменённые env vars или secrets | **нет** — настройка хранится в XML-конфигурации плагина (`plugins/configurations/PoiskKinoMetadataPlugin.xml`), не в env/секретах; новых секретов нет |
| Новые внешние интеграции, порты, очереди, storage, scheduled jobs | **нет** — новый исходящий хост `https://eu-api.poiskkino.dev` уже объявлен API и используется только по явному выбору администратора; новых портов/очередей/БД/фоновых задач нет |
| Docker / compose / CI/CD / deploy / Helm / Kubernetes / observability | **нет** — нет новых шагов CI, нет изменений Dockerfile/compose/Helm, нет новых метрик; наблюдаемость ограничена существующим `ILogger` (`Information`/`Warning`, D-ARCH-5), runbook не требуется |
| Миграции данных | **нет** — отсутствующий элемент XML оставляет инициализатор-дефолт (analysis OQ-4, F6/F7) |
| Новые зависимости (NuGet/образы) | **нет** — AC-12; тесты на xUnit/Moq уже подключены |
| Вывод | **DevOps-задача не нужна**, стадия `devops-check` не открывается |

## Вне scope и follow-up (не задачи этой фичи)

- Live-переключение базового URL без перезапуска (AC-7) и включение хоста в ключ кэша (architecture.md D-ARCH-6, A3) — отдельная фича с инкрементом ADR.
- Серверная диагностика «проверить подключение» через `GET {base}/v1.5/token` (architecture.md §5) — отдельная фича, требует новой HTTP-поверхности плагина.
- Поддержка base URL с подпутём (`https://proxy.corp/poiskkino`, D-ARCH-3) — отвергнуто: относительные пути с ведущим `/` теряют путь; при потребности — отдельная фича с правкой трёх URL.
- Автоматический failover между `api.` и `eu-api.` (OQ-2), per-provider URL, прокси/mTLS/`http://`, новые эндпоинты API — out of scope (analysis §5).
- Замечание architecture.md §7 про `docs/backlog/features/eu-api-endpoint/README.md` («Статус `intake`») уже снято коммитом `bbfda17`: в README стадия не дублируется, живой статус — только в [status.md](status.md). Действий не требуется.
