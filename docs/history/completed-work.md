# Completed work

Завершённые фичи и hotfix после переноса в [archive](../archive/README.md).

| Дата завершения | Название | Тип | PR/MR | Путь в archive |
|-----------------|----------|-----|--------|----------------|
| 2026-08-23 | `api-v1-5-upgrade` | feature | [#2](https://github.com/milleraa/jellyfin-poiskkino-plugin/pull/2) (open, не merged; принято Стейхолдером) | [archive/features/api-v1-5-upgrade](../archive/features/api-v1-5-upgrade/) |
| 2026-08-24 | `logo-images` | feature | [#3](https://github.com/milleraa/jellyfin-poiskkino-plugin/pull/3) (open, не merged; принято Стейхолдером до merge `/accept-feature`) | [archive/features/logo-images](../archive/features/logo-images/) |
| 2026-11-28 | `eu-api-endpoint` | feature | [#9](https://github.com/milleraa/jellyfin-poiskkino-plugin/pull/9) (open, не merged; принято Стейхолдером в чате при открытом PR) | [archive/features/eu-api-endpoint](../archive/features/eu-api-endpoint/) |
| 2026-11-28 | `provider-test-coverage` | hotfix | [#10](https://github.com/milleraa/jellyfin-poiskkino-plugin/pull/10) (принят Стейкхолдером до merge) | [archive/hotfixes/provider-test-coverage](../archive/hotfixes/provider-test-coverage/) |

## Резюме

### `api-v1-5-upgrade` (2026-08-23)

- Миграция PoiskKino API v1.4 → v1.5: `/v1.4/movie/search` и `/v1.4/movie/{id}` → `/v1.5/...` (`/v1.5/season` не тронут); переименование моделей `PoiskKinoMovieDtoV1_4` → `PoiskKinoMovieDto`, удалён мёртвый `Models/PoiskKinoSeasonResponse.cs`.
- [ADR-0001](../architecture/decisions/ADR-0001-search-year-filter.md): локальный фильтр года в провайдерах (`SearchYearFilter`) — фильмы по точному совпадению, сериалы по `releaseYears`, fallback на неотфильтрованный список.
- QA: **pass** — 160 passed / 0 failed; ограничение: smoke с реальным API-ключом не выполнялся (план изоляции в ADR-0001).
- Принято Стейхолдером явно в чате при открытом PR #2 (политика DoD допускает accept без merge).

### `logo-images` (2026-08-24)

- Логотипы ПоискКино как `ImageType.Logo`: `PoiskKinoImageProvider.GetSupportedImages` → `[Primary, Backdrop, Logo]`; маппинг `logo.url` по ID и fallback из поиска (OQ-1), guard `IsNullOrWhiteSpace`, постер/фон не тронуты; TMDB-логотипы фильтруются существующим пост-фильтром (`IgnoreTmdbImages`). ADR не требуется.
- Задачи: [001](../archive/features/logo-images/tasks/001-image-provider-logo-support.md) реализация Logo, [002](../archive/features/logo-images/tasks/002-logo-tmdb-filter-tests.md) тесты TMDB-политики, [003](../archive/features/logo-images/tasks/003-fix-imageurlhelper-test-isolation.md) rework-фикс изоляции предсуществующего `ImageUrlHelperTests` от статического `Plugin.Instance` (`f3beb9a`, только тестовый файл).
- QA: **pass** — AC-1..AC-12, 163/163 ×2, build 0 err; после rework-валидация 5×163/163 подряд, CI PR #3 зелёный ([run](https://github.com/milleraa/jellyfin-poiskkino-plugin/actions/runs/32750717011)).
- Принято Стейхолдером `/accept-feature` 2026-08-24 **до merge** PR #3 (штатный флоу DoD); архивация выполнена Финализатором в ветке `feature/logo-images` до merge — все изменения попадут в main одним мержем PR #3.

Финализатор обновляет эту таблицу при закрытии работы.

### `eu-api-endpoint` (2026-11-28)

- Настраиваемый базовый URL API ПоискКино: `ApiBaseUrlResolver` (trim, схема по умолчанию `https`, усечение завершающего `/`, дефолт при пустом значении, fallback без исключений → `ResolvedBaseUrl(Uri, UsedFallback, Reason)`), `PluginConfiguration.ApiBaseUrl` с инициализатором-дефолтом `https://api.poiskkino.dev` (legacy-XML без элемента работает без миграции), необязательный 3-й параметр конструктора `PoiskKinoApiClient(..., string? apiBaseUrl = null)` → `HttpClient.BaseAddress` с `Information`/`Warning`, 5 провайдеров передают значение по паттерну `ApiKey`, поле «Базовый URL API» в `Configuration/configPage.html` (с явным «применяется после перезапуска»); README + `docs/architecture/overview.md`.
- [ADR-0003](../architecture/decisions/ADR-0003-configurable-api-base-url.md): конфигурационный base URL вместо константы; root-only base URL (подпуть отвергается), `userinfo`/query/fragment → fallback, применение на этапе создания клиента (AC-7′ — после перезапуска Jellyfin).
- Задачи: [001](../archive/features/eu-api-endpoint/tasks/001-api-base-url-resolver.md) резолвер + тесты, [002](../archive/features/eu-api-endpoint/tasks/002-configuration-client-providers.md) конфигурация/клиент/провайдеры, [003](../archive/features/eu-api-endpoint/tasks/003-config-page-field.md) поле в UI, [004](../archive/features/eu-api-endpoint/tasks/004-docs-base-url.md) документация, [005](../archive/features/eu-api-endpoint/tasks/005-redact-userinfo-all-at.md) rework-фикс `RedactUserInfo` (вырезается весь `userinfo` до последнего `@`, пароль с `@` не утекает в `Warning`).
- QA: **pass** — AC-1…AC-12 (с AC-7′), 281/281 ×4 прогона, build 0 err (baseline `CS1591` = 122); ограничения: нет реального API-ключа → e2e-поиск на EU-зеркале не прогонялся (R2/A7), ручной UI-сценарий и запись `plugins/configurations/*.xml` — за администратором (tasks/003), live-переключение без рестарта не проверялось (AC-7′).
- Residual risk (принят Техлидом и QA): ключ, вставленный в поле базового URL; `userinfo` редактируется (`https://KEY@host` → `https://host`), но query/fragment и «голое значение» попадут в лог как часть адреса. Требуется ошибка администратора; `ApiKey` из конфигурации не логируется (AC-9). Follow-up вне scope: вырезать query/fragment в `Warning` и уточнить подсказку в UI.
- Принято Стейхолдером явно в чате (2026-11-28) при открытом, не merged PR #9 — политика DoD допускает accept без merge; archive finalization выполнена Финализатором в ветке `feature/eu-api-endpoint` **до** merge (код + архивация войдут в `main` одним мержем), CI PR #9 зелёный.

### `provider-test-coverage` (2026-11-28)

- Усилены тесты поиска сериалов по диапазонам годов и определения ролей персон в фильмах и сериалах через публичные методы провайдеров. Продуктовый код не менялся.
- [QA PASS](../archive/hotfixes/provider-test-coverage/qa.md): 283/283 тестов, Release-сборка без ошибок; обе контрольные поломки обнаружены. Живой Jellyfin и реальный API не проверялись.
- Стейкхолдер принял PR #10 в чате. Архивация выполнена в `hotfix/provider-test-coverage` до merge.
