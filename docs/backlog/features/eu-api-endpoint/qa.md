# QA — `eu-api-endpoint`

Дата: 2026-11-28. QA-валидация после приёмки Техлидом (accept, DevOps impact check закрыт повторно 2026-11-28).
Ветка `feature/eu-api-endpoint`, HEAD на момент прогона — `9577df6`. Среда: локально, Linux, .NET SDK `10.0.115`,
`net10.0` / Jellyfin 12.0.0.

## Решение по инструментарию

Плагин — backend C#/.NET для Jellyfin, собственного веб-приложения нет; UI настроек — embedded HTML
(`Configuration/configPage.html`) внутри Dashboard Jellyfin. **Playwright неприменим** — см.
[playwright.md](../../../qa/playwright.md) и [test-strategy.md](../../../qa/test-strategy.md) (E2E «не планируется»),
как и в прошлых прогонах (`jellyfin-12-support`, `logo-images`, `api-v1-5-upgrade`).

Проверка выполнена собственными командами: `dotnet build`/`dotnet test` (xUnit + Moq), `rg`-инварианты по коду,
негативный контроль тестов (искусственная регрессия), пробы `GET /v1.5/token` на оба хоста, ревью диффа
`023460e..9577df6` и сверка документации с фактическим поведением `ApiBaseUrlResolver`.

## Покрытие по AC (analysis.md §3)

| AC | Требование | Статус | Evidence (проверено лично) |
|----|-----------|--------|----------------------------|
| AC-1 | Поле конфигурации с дефолтом | ✅ pass | `Configuration.cs:24` — `ApiBaseUrl` с инициализатором `ApiBaseUrlResolver.DefaultBaseUrl`; тесты `PluginConfigurationTests.ApiBaseUrl_Default_EqualsResolverDefault`, `ApiBaseUrlResolverTests.DefaultBaseUrl_IsPoiskkinoDev`; old XML без элемента → дефолт: `PluginConfigurationTests.Deserialize_LegacyXmlWithoutApiBaseUrl_KeepsDefaultAndOtherValues` (`ApiKey`/`IgnoreTmdbImages` сохраняются) |
| AC-2 | Дефолт без изменений поведения | ✅ pass | `rg`: `Timeout = TimeSpan.FromSeconds(120)` (`PoiskKinoApiClient.cs:46`), `CacheExpirationHours = 24` (`:15`), `MaxRequestsPerDay = 200` (`:16`), `SemaphoreSlim(1, 1)` (`:62`) — значения не менялись; дифф клиента меняет только `BaseAddress`, добавляет `Information`/`Warning` и два `private static` хелпера, блоки 401/403/429/404/exception не тронуты; тест `Constructor_WithoutBaseUrl_UsesDefaultHost` (двухаргументный конструктор → `api.poiskkino.dev`, `Information` без `Warning`) |
| AC-3 | Нормализация | ✅ pass | `ApiBaseUrlResolver.Resolve`: trim, пусто → дефолт без fallback, нет `://` → `https://`, завершающий `/` снимается, результат — абсолютный `Uri`. Тесты: `Resolve_SurroundingWhitespace_Trimmed`, `Resolve_MissingScheme_PrependsHttps`, `Resolve_TrailingSlash_Removed`, `Resolve_ExplicitPort_Preserved` (`:443` → без порта, `:8443` → с портом), `Resolve_HostCase_NormalizedToLowerCase`, `Resolve_IdnHost_NormalizedToPunycode`; в клиенте — `BaseUrlInputVariants_NormalizedBeforeRequest` (без схемы / с `/` / с пробелами → EU-хост) |
| AC-4 | Валидация без падения | ✅ pass | Fallback-ветки: `http`/`ftp`/`file`/`javascript`/`mailto` → дефолт+причина (`Resolve_NonHttpsScheme_FallsBackToDefault`); мусор/пустой host (`Resolve_UnparsableOrEmptyHost_FallsBackToDefault`); `userinfo`/`query`/`fragment` (`Resolve_UserInfoQueryOrFragment_FallsBackToDefault`); подпуть (`Resolve_NonRootPath_FallsBackToDefault`, включая `https://proxy.corp/poiskkino`); «никогда не бросает» — `Resolve_NeverThrows_ForHostileInputs` (29 входов, IPv6-литерал `https://[::1]`, длиной 5000, NUL-байт и т.п.); в клиенте — `Constructor_AnyInput_DoesNotThrow`, `InvalidBaseUrl_FallsBackToDefaultWithSingleWarning` (ровно один `Warning` на клиент, `RequestUri.Host == api.poiskkino.dev`) |
| AC-5 | Применение во всех запросах | ✅ pass | `CustomBaseUrl_AppliedToAllRequests`: три запроса на `eu-api.poiskkino.dev` с путями `/v1.5/movie/search`, `/v1.5/movie/535341`, `/v1.5/season`; проводка конфигурация→провайдер→клиент: `PoiskKinoMovieProviderTests.GetSearchResults_CustomApiBaseUrl_RequestGoesToConfiguredHost` (значение из `Plugin.Instance.Configuration.ApiBaseUrl`); `rg -n "poiskkino\.dev" PoiskKinoMetadataPlugin --glob '*.cs'` → **единственное вхождение** `ApiBaseUrlResolver.cs:49`; все 5 провайдеров передают `ApiBaseUrl` (строки `:21/:25/:26/:21/:26`) |
| AC-6 | UI (`configPage.html`) | ⚠️ pass (статически) | Поле `id/name="apiBaseUrl"`, `type="text"`, `<label for="apiBaseUrl">`, `datalist` с обоими хостами, подсказка на русском (дефолт, EU-при недоступности/вне РФ, схему можно не указывать, `/` не нужен, путь не поддерживается, «Изменение применяется после перезапуска Jellyfin»); `loadConfiguration` подставляет дефолт при отсутствии/пустом, `saveConfiguration` пишет сырое значение без trim/схемы/слэшей; `pluginUniqueId`, `ApiKey`, `IgnoreTmdbImages`, `processPluginConfigurationUpdateResult` не тронуты. **Живой Jellyfin не поднимался** — см. «Ограничения», п. 5 |
| AC-7′ | Смена применяется после перезапуска Jellyfin | ✅ pass | `README.md:52` («Изменение базового URL API применяется после перезапуска Jellyfin»), `README.md:64`, подсказка в `configPage.html:34`, `overview.md:38`; значение читается в конструкторе клиента (construction-time), клиент ленивый и один на провайдер — согласуется с текстом документации |
| AC-8 | Персистентность / совместимость XML | ⚠️ pass (в unit-уровне) | Round-trip через `System.Xml.Serialization`: `PluginConfigurationTests.SerializeAndDeserialize_WithCustomApiBaseUrl_PreservesValueVerbatim` (сырое `https://eu-api.poiskkino.dev/` не канонизируется), `Deserialize_LegacyXmlWithoutApiBaseUrl_KeepsDefaultAndOtherValues`; живой Jellyfin + файл `plugins/configurations/*.xml` — ручная проверка администратора (п. 5 ограничений) |
| AC-9 | Диагностируемость и отсутствие ключа в логах | ✅ pass | `Information` «PoiskKino API base URL: {BaseUrl}» и `Warning` с причиной fallback — один раз в конструкторе; `ApiKey_NeverAppearsInLogs` (ни одного сообщения с `test-api-key` в трёх запросах); усечение: `Constructor_InvalidLongBaseUrl_TruncatesRawValueInWarning` (500 симв. → 200 в логе, 201 — нет); санитизация `userinfo`: `Constructor_UserInfoInBaseUrl_RedactedInWarning` — 6 кейсов, включая пароль с `@` (`https://admin:p@ss@api.poiskkino.dev` → в `Warning` только `'https://api.poiskkino.dev'`, ассерты `!Contains("p@ss")` и `!Contains("ss@")`); `Constructor_AtSignOutsideAuthority_LoggedUnchanged` (`@` в пути/query/fragment не вырезается); `Constructor_DegenerateUserInfoInputs_DoNotThrow` (8 вырожденных входов) |
| AC-10 | Тесты и сборка | ✅ pass | `dotnet build -c Release --no-incremental`: 0 ошибок, 122× `CS1591` (по 61 в каждом из двух проектов) — baseline, новых типов предупреждений нет; `dotnet test -c Release`: **281/281** ×4 прогона; новые тесты за фичу: `ApiBaseUrlResolverTests` 71, `PoiskKinoApiClientBaseUrlTests` 35, `PluginConfigurationTests` 3, провайдерский проводочный 1 |
| AC-11 | Документация | ✅ pass | `README.md` — раздел «Базовый URL API» (дефолт, оба хоста, правила нормализации, отклонение `http`/подпути/`userinfo`/query/fragment, «пусто = дефолт, не ошибка», тексты сообщений лога), фраза о перезапуске, обновлённый пример запроса `/v1.5/movie/search`; `docs/architecture/overview.md` — схема потока, таблица компонентов (`ApiBaseUrlResolver`), таблица системных зависимостей (оба хоста + ADR-0003). Расхождений с кодом `ApiBaseUrlResolver`/клиента не найдено |
| AC-12 | Без новых зависимостей и поверхностей | ✅ pass | `*.csproj` — те же пакеты (Jellyfin.Controller/Model 12.0.0, Microsoft.Extensions.Http 10.0.11; в тестах xUnit 2.9.3 / Moq 4.20.72 / Test.Sdk 17.12.0); `rg "GetEnvironmentVariable\|Environment\.Variable"` → 0 совпадений; `rg "FromSeconds\(120\)\|CacheExpirationHours\|MaxRequestsPerDay\|SemaphoreSlim"` — прежние значения; дифф `023460e..9577df6` не содержит `.github/`, Docker/Helm/K8s/CI-файлов |

## Результаты прогонов

| # | Проверка | Результат |
|---|----------|-----------|
| 1 | `dotnet build PoiskKinoMetadataPlugin.slnx -c Release --no-incremental` | ✅ 0 ошибок, `CS1591` = 122 (baseline; 61 на проект), новых предупреждений нет |
| 2 | `dotnet test PoiskKinoMetadataPlugin.slnx -c Release` — прогон 1 | ✅ 281 passed / 0 failed / 0 skipped (465 ms) |
| 3 | То же — прогон 2 (flaky-контроль) | ✅ 281/281 (482 ms) |
| 4 | То же — прогон 3 | ✅ 281/281 (399 ms) |
| 5 | То же — прогон 4 (после восстановления файла после негативного контроля) | ✅ 281/281 (437 ms) |
| 6 | Фильтрованные прогоны по AC | ✅ `ApiBaseUrlResolverTests` 71/71; `PoiskKinoApiClientBaseUrlTests` 35/35; `PluginConfigurationTests` 3/3; `GetSearchResults_CustomApiBaseUrl_RequestGoesToConfiguredHost` 1/1 |
| 7 | **Негативный контроль AC-5** (искусственная регрессия) | ✅ Тесты ловят регрессию: временно заменил в конструкторе `ApiBaseUrlResolver.Resolve(apiBaseUrl)` → `Resolve(null)` → `PoiskKinoApiClientBaseUrlTests` **18 из 35 упали** с `Expected: "eu-api.poiskkino.dev" / Actual: "api.poiskkino.dev"`; файл восстановлен `git checkout --`, дерево чистое, прогон 5 снова 281/281 |
| 8 | Живые пробы хостов (без ключа) | ✅ `GET https://api.poiskkino.dev/v1.5/token` → `401`; `GET https://eu-api.poiskkino.dev/v1.5/token` → `401` — оба зеркала живы, аутентификация до роутинга (согласуется с analysis F10) |
| 9 | Инварианты AC-2/AC-12 (`rg`) | ✅ см. таблицу AC-2/AC-12: таймаут 120 с, кэш 24 ч, лимит 200, `SemaphoreSlim(1, 1)`, единственный литерал хоста `ApiBaseUrlResolver.cs:49`, ни новых пакетов, ни env-переменных |
| 10 | Сверка документации с кодом | ✅ `README.md`, `overview.md`, подсказка `configPage.html` не расходятся с правилами резолвера и поведением клиента |

Наблюдение по формату: `Information` логирует `HttpClient.BaseAddress` (тип `Uri`), поэтому для корневого адреса
в логе отображается завершающий `/` (`https://eu-api.poiskkino.dev/`) — это соответствует ручному сценарию 3 из
[task 003](tasks/003-config-page-field.md) и не противоречит формулировкам README/UI; сравнение в тестах идёт по `Host`/`Scheme`.

## Дефекты

Не обнаружено. AC-1…AC-12 (с AC-7′ вместо AC-7) выполнены; блокирующих расхождений кода, тестов и документации нет.

### Некритичные наблюдения (не блокируют, rework не требуется)

1. `PoiskKinoMetadataPlugin.UnitTests/ConfigurationTests.cs` содержит класс `PluginConfigurationTests` — имя файла и класса расходятся (остальные тестовые файлы совпадают с именем класса). Чисто стилистическое.
2. Подсказка в `configPage.html` короче README: не упоминает, что `userinfo`/query/fragment отвергаются (в README это есть). Противоречия нет, поведение серверное.
3. Подсказка/README обещают, что невалидный адрес даёт «предупреждение с причиной» — сообщения на английском (`Invalid API base URL … falling back to …`), как и остальные логи плагина; локализация логов не заявлена ни в одном AC.

## Автотесты

- Playwright: **неприменим** для backend/.NET-плагина (см. [playwright.md](../../../qa/playwright.md)); UI настроек — только ручная проверка в живом Jellyfin.
- Unit: `PoiskKinoMetadataPlugin.UnitTests` — 281 passed, из них за фичу: `Helpers/ApiBaseUrlResolverTests.cs` (71), `ApiClient/PoiskKinoApiClientBaseUrlTests.cs` (35), `ConfigurationTests.cs` (3), `Providers/PoiskKinoMovieProviderTests.cs` (+1 проводочный).
- Регрессия: полный прогон 4 раза подряд (после фичи `logo-images` проект уже ловил flaky) — расхождений нет.

## Ограничения прогона

1. **UI (`configPage.html`) автотестами не покрыт** — ручная проверка в живом Jellyfin остаётся за администратором (стандартно для этого репозитория, Playwright неприменим). Сценарии из [tasks/003](tasks/003-config-page-field.md):
   1. свежая/старая конфигурация → в поле виден `https://api.poiskkino.dev`;
   2. `https://eu-api.poiskkino.dev` → «Сохранить» → F5 → значение на месте, `<ApiBaseUrl>` в `plugins/configurations/PoiskKinoMetadataPlugin.xml` без канонизации;
   3. `eu-api.poiskkino.dev/` → после F5 ровно введённое, после перезапуска `Information` «PoiskKino API base URL: https://eu-api.poiskkino.dev/» и запросы на EU-хост;
   4. пустое поле → дефолт в UI, после перезапуска только `Information`, `Warning` нет;
   5. `http://api.poiskkino.dev` → один `Warning` с причиной, поиск на дефолте, страница показывает введённое;
   6. лог-гигиена: `https://admin:p@ss@host` → в `Warning` только `https://host` (на стенде — только тестовый ключ/заглушки).
2. **Нет реального API-ключа (analysis R2 / architecture A7)** — end-to-end поиск через `eu-api.poiskkino.dev` в этом прогоне не выполнялся; проверены только пробы `401` на оба хоста и фактический хост в `RequestUri` (unit). Остаётся ручным smoke администратора, **не fail**.
3. **Live-переключение не проверялось** (по решению Стейхолдера действует AC-7′): до перезапуска 5 ленивых клиентов могут держать разные `BaseAddress` (A1). Документация ограничивает поведение фразой «после перезапуска» — расхождения с кодом нет.
4. **Кэш не включает хост в ключ** (analysis R7 / A3) — в текущей restart-семантике безопасно; для будущего live-переключения обязателен инвариант из ADR-0003.
5. **Живой Jellyfin 12.x не поднимался** (нет стенда в локальном окружении) — AC-6/AC-8 подтверждены статическим ревью страницы + unit-тестами XML round-trip; запись/чтение реального файла конфигурации — за администратором (п. 1).
6. **Residual risk (принят Техлидом, 2026-11-28, QA не блокирует): ключ, вставленный в поле «Базовый URL API».**
   - ключ как `userinfo` (`https://KEY@host`) — **безопасно**: `Warning` показывает только `https://host` (фикс `bf2f6bc`, тесты задачи 005);
   - ключ в query/fragment (`https://api.poiskkino.dev/?token=KEY`) — попадает в `Warning` как часть сырой строки (query не редактируется);
   - ключ как голое значение (`https://a1b2c3d4-e5f6-…`) — парсится как single-label host, попадает в `Information` как часть адреса.

   Требуется, чтобы администратор сам вставил секрет не в то поле; `ApiKey` из конфигурации в лог не попадает (AC-9, `ApiKey_NeverAppearsInLogs`). Лог доступен тому же администратору; эвристики «похоже на ключ» ломали бы легитимные intranet/single-label хосты. Необязательные follow-up вне scope: вырезать `?`/`#` при логировании `Warning`, добавить в README/UI «поле — только для адреса».
7. **IPv6-литерал** (`https://[::1]`) и **base URL с подпутём** отклоняются резолвером (дефолт + `Warning`); подпуть документирован в README, IPv6 — нет (риск незначительный, поведение безопасное).
8. **`datalist` на `is="emby-input"`** в Jellyfin 12.x автотестами не покрыт; удаляется тремя строками без влияния на поле и подсказку.

## Вердикт

- [x] **Pass**
- [ ] Rework → Техлид (список дефектов ниже)

Handoff: QA → Orchestrator, стадия `finalization`, owner `Orchestrator`. Дефектов нет; ограничения перечислены выше,
residual risk по ключу в поле базового URL принят Техлидом и подтверждён этим прогоном.
