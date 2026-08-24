# Completed work

Завершённые фичи и hotfix после переноса в [archive](../archive/README.md).

| Дата завершения | Название | Тип | PR/MR | Путь в archive |
|-----------------|----------|-----|--------|----------------|
| 2026-08-23 | `api-v1-5-upgrade` | feature | [#2](https://github.com/milleraa/jellyfin-poiskkino-plugin/pull/2) (open, не merged; принято Стейхолдером) | [archive/features/api-v1-5-upgrade](../archive/features/api-v1-5-upgrade/) |
| 2026-08-24 | `logo-images` | feature | [#3](https://github.com/milleraa/jellyfin-poiskkino-plugin/pull/3) (open, не merged; принято Стейхолдером до merge `/accept-feature`) | [archive/features/logo-images](../archive/features/logo-images/) |

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
