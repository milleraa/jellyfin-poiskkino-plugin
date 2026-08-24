# Tech plan — `logo-images`

Владелец: **Техлид**.

Декомпозиция дизайна [architecture.md](architecture.md) для реализации в единственном worktree `/home/alex/src/my/jellyfin-metadata-plugin-wt-logo-images` (ветка `feature/logo-images`). Task-branches/worktrees не используются; задачи выполняются строго последовательно.

## Декомпозиция

| ID | Задача | Роль | Содержание | AC |
|----|--------|------|------------|----|
| [001](tasks/001-image-provider-logo-support.md) | Logo в `PoiskKinoImageProvider`: реализация + обновление существующих тестов | `developer-csharp` | `GetSupportedImages` → `[Primary, Backdrop, Logo]`; маппинг `movieData.Logo?.Url` (ветка A) и `apiItem.Logo?.Url` (fallback-ветка B по OQ-1); guard `IsNullOrWhiteSpace` только для логотипа; XML-summary; обновление/переименование тестов + `logo` в `MixedSearchResponseJson`; новый тест «logo отсутствует» | AC-1..AC-5, AC-8..AC-10, AC-12 |
| [002](tasks/002-logo-tmdb-filter-tests.md) | TMDB-фильтрация логотипов: покрытие тестами | `developer-csharp` | Тесты: TMDB-логотип фильтруется при `IgnoreTmdbImages=true` (дефолт), возвращается при `false`; без изменений продакшн-кода | AC-6, AC-7, AC-12 |

DevOps-задач нет: impact check отрицательный по всем пунктам (env vars/secrets, интеграции, порты, миграции, Docker/compose, CI/CD, deploy, Helm/K8s, observability) — подтверждено Архитектором и зафиксировано в каждой задаче и в [status.md](status.md).

## Порядок выполнения

1. **001** (`developer-csharp`): реализация Logo + синхронное обновление существующих тестов (тест fallback ломается преднамеренно и переписывается в том же шаге — набор остаётся зелёным после задачи). Приёмка: `dotnet build` / `dotnet test` зелёные, ревью диффа Техлидом.
2. **002** (`developer-csharp`): тесты TMDB-политики для Logo поверх реализации 001. Приёмка: полный набор зелёный, ревью Техлидом.
3. Техлид: DevOps impact check (зафиксирован: нет), ревью итогового диффа против architecture.md, accept → `status.md` → стадия `qa`.

## Зависимости

- 002 зависит от 001 (тесты опираются на реализацию `ImageType.Logo`). Обратных зависимостей нет.
- От внешних команд/сервисов зависимости отсутствуют: поле `logo` уже отдаётся API ПоискКино в `/v1.5/movie/{id}` и `/v1.5/movie/search` (см. analysis.md), новых HTTP-запросов нет.
- Внеполосные правки вне scope: `docs/product/requirements.md:12` (формулировка про изображения) — за Аналитиком/Стейхолдером после принятия; follow-up-кандидат «унифицировать guards на `IsNullOrWhiteSpace`» — в backlog отдельно (риски в architecture.md).
