# Status — `logo-images`

Единая точка правды по стадии для Оркестратора. Обновляйте при каждом handoff (см. `.opencode/skills/role-handoff/SKILL.md`).

## Текущее состояние

| Поле | Значение |
|------|----------|
| **Стадия** | `development` |
| **Owner** | `Tech Lead` |
| **Worktree** | `/home/alex/src/my/jellyfin-metadata-plugin-wt-logo-images` |
| **Ветка** | `feature/logo-images` |
| **PR/MR** | _нет_ |
| **Commit со ссылкой на PR/MR** | _нет_ |
| **Блокеры** | _нет_ |

### Стадии (справочно)

`intake` → `analysis` → `analysis-review` → `backlog-paused` или `architecture` → `tech-decomposition` → `development` → `devops-check` → `tech-lead-review` → `qa` → `rework` → `finalization` → `pr-mr-ready` → `stakeholder-review` → `rework` или `accepted`

`analysis-review` — обязательная остановка после Аналитика: Стейхолдер решает продолжать сейчас или оставить фичу в backlog.

## Задачи реализации

| ID | Файл | Роль / стек | Статус |
|----|------|-------------|--------|
| 001 | [tasks/001-image-provider-logo-support.md](tasks/001-image-provider-logo-support.md) | developer-csharp (C# / Jellyfin adapter + unit-тесты) | done |
| 002 | [tasks/002-logo-tmdb-filter-tests.md](tasks/002-logo-tmdb-filter-tests.md) | developer-csharp (C# / xUnit) | in progress |

Порядок и зависимости: [tech-plan.md](tech-plan.md). Выполнение строго последовательное в единственном worktree.

## DevOps impact check

| Вопрос | Результат |
|--------|-----------|
| Новые env vars / secrets | _нет_ |
| Новые интеграции / порты / очереди / storage / jobs | _нет_ |
| Docker / compose / CI/CD / deploy / Helm / K8s / observability | _нет_ |
| DevOps-задача | _не нужна_ |

## QA

| Дата | Результат | Ссылка на прогон |
|------|-------------|------------------|
| — | — | [test-runs](../../../qa/test-runs.md) |

## PR/MR feedback / rework

| Поле | Значение |
|------|----------|
| **Feedback source** | _чат / PR/MR comments / CI_ |
| **Feedback summary** | _нет_ |
| **Rework route** | _Analyst / Architect / Tech Lead / DevOps / QA_ |
| **Worktree restored from branch** | _нет / да_ |
| **Rework tasks** | _ссылки на tasks/_ |

## Process learning (неблокирующий triage)

| Поле | Значение |
|------|----------|
| **Learning triage** | _not-triggered_ |
| **Class key** | _—_ |
| **Learning evidence** | _—_ |
| **Suppression reason** | _нет_ |
| **Process review** | _нет_ |

Один unique reject получает `not-triggered` и не блокирует rework. `candidate` возможен только при двух или более unique comparable evidence-linked cases; изменения процесса выполняются только отдельным reviewed PR/MR. Правила: [process learning](../../../process/learning/README.md).

## Changelog (handoff)

### 2026-08-24 — декомпозиция готова, → development

- Tech plan заполнен ([tech-plan.md](tech-plan.md)): две последовательные задачи для `developer-csharp` в единственном worktree.
- Задача 001: реализация Logo (`GetSupportedImages` → `[Primary, Backdrop, Logo]`, маппинг `movieData.Logo?.Url` и fallback `apiItem.Logo?.Url` по OQ-1, guard `IsNullOrWhiteSpace`, XML-summary) + синхронное обновление существующих тестов (fallback-тест переписывается на проверку по типам, `logo` добавляется в `MixedSearchResponseJson`).
- Задача 002: тесты TMDB-политики для Logo (AC-6/AC-7), продакшн-код не меняется; зависит от 001.
- DevOps impact check: **нет** по всем пунктам (подтверждено Архитектором) — DevOps-задача не создаётся, стадия `devops-check` будет закрыта фиксацией этого факта перед QA.
- Handoff → `developer-csharp` (задача 001).

### 2026-08-24 — архитектура готова, → tech-decomposition

- Дизайн зафиксирован в [architecture.md](architecture.md): расширение `PoiskKinoImageProvider` (`GetSupportedImages` → `[Primary, Backdrop, Logo]`), маппинг `logo.url` в обеих ветках (по ID и fallback из поиска по решению OQ-1), единая точка TMDB-фильтрации — существующий пост-фильтр.
- Решение Архитектора по OQ-2: защитный вызов `ShouldFilterUrl` при маппинге логотипа **не вводить** — пост-фильтр функционально эквивалентен, дублирование не нужно; триггер пересмотра зафиксирован.
- Новый ADR **не требуется**: тривиальное расширение существующего адаптера без новых границ/интеграций/компромиссов (зафиксировано явно).
- Внимание Техлида: существующий тест `GetImages_MovieFallsBackToSearch_ReturnsImagesFromSearch` сломается преднамеренно (`Assert.All(movie-poster.jpg)`) — переписать на проверку по типам + добавить `logo` в `MixedSearchResponseJson`; guard для логотипов — `IsNullOrWhiteSpace` (AC-3), постер/фон не трогать (AC-4).
- Документационные последствия: `docs/product/requirements.md:12` устареет после реализации (передано Аналитику/Стейхолдеру); DevOps impact — нет.
- Handoff → Оркестратор: стадия `tech-decomposition`, owner `Tech Lead`, вход: `analysis.md`, `architecture.md`.

### 2026-08-24 — analysis-review пройден, → architecture

- Стейхолдер: продолжаем к Архитектору.
- Решение по OQ-1: **в scope** — логотип добавляется и в fallback-ветке «только данные из поиска» (рекомендация Аналитика принята).
- OQ-2 (защитный вызов `ShouldFilterUrl`) — на решение Архитектора.
- Handoff → Архитектор (`.opencode/agents/architect.md`), вход: `analysis.md`, `brief.md`.

### 2026-08-24 — intake → analysis

- Стейхолдер взял фичу в работу; классификация: новая фича → Аналитик.
- Созданы ветка `feature/logo-images` и единственный worktree `/home/alex/src/my/jellyfin-metadata-plugin-wt-logo-images`.
- Handoff → Аналитик (`.opencode/agents/analyst.md`), вход: `brief.md`.

### 2026-08-24 — создан бриф

- Папка фичи создана из шаблона по запросу Стейхолдера.
- Точка расширения подтверждена: `IRemoteImageProvider.GetSupportedImages` + `ImageType.Logo` доступны в целевом Jellyfin 10.11.x.

### 2026-08-24 — analysis → analysis-review

- Анализ завершён (`analysis.md`): все гипотезы брифа проверены по коду и OpenAPI-спеке, блокирующих вопросов нет.
- Факт: `/v1.5/movie/search` не имеет `selectFields`; `logo` входит в дефолтную схему ответа — условный out-of-scope из брифа закрыт.
- Рекомендация Аналитика: `continue`. Открытый неблокирующий вопрос OQ-1 (логотип в fallback-ветке поиска) — решить на gate Стейхолдера.
- Handoff → Оркестратор: обязательная остановка на решении Стейхолдера перед архитектурой.
