# Completed work

Завершённые фичи и hotfix после переноса в [archive](../archive/README.md).

| Дата завершения | Название | Тип | PR/MR | Путь в archive |
|-----------------|----------|-----|--------|----------------|
| 2026-08-23 | `api-v1-5-upgrade` | feature | [#2](https://github.com/milleraa/jellyfin-poiskkino-plugin/pull/2) (open, не merged; принято Стейхолдером) | [archive/features/api-v1-5-upgrade](../archive/features/api-v1-5-upgrade/) |

## Резюме

### `api-v1-5-upgrade` (2026-08-23)

- Миграция PoiskKino API v1.4 → v1.5: `/v1.4/movie/search` и `/v1.4/movie/{id}` → `/v1.5/...` (`/v1.5/season` не тронут); переименование моделей `PoiskKinoMovieDtoV1_4` → `PoiskKinoMovieDto`, удалён мёртвый `Models/PoiskKinoSeasonResponse.cs`.
- [ADR-0001](../architecture/decisions/ADR-0001-search-year-filter.md): локальный фильтр года в провайдерах (`SearchYearFilter`) — фильмы по точному совпадению, сериалы по `releaseYears`, fallback на неотфильтрованный список.
- QA: **pass** — 160 passed / 0 failed; ограничение: smoke с реальным API-ключом не выполнялся (план изоляции в ADR-0001).
- Принято Стейхолдером явно в чате при открытом PR #2 (политика DoD допускает accept без merge).

Финализатор обновляет эту таблицу при закрытии работы.
