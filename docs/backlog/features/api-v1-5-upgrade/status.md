# Status — `api-v1-5-upgrade`

Единая точка правды по стадии для Оркестратора. Обновляйте при каждом handoff (см. `.opencode/skills/role-handoff/SKILL.md`).

## Текущее состояние

| Поле | Значение |
|------|----------|
| **Стадия** | `analysis` |
| **Owner** | `Analyst` |
| **Worktree** | `/home/alex/src/my/jellyfin-metadata-plugin-api-v15` |
| **Ветка** | `feature/api-v1-5-upgrade` |
| **PR/MR** | _нет_ |
| **Commit со ссылкой на PR/MR** | _нет_ |
| **Блокеры** | _нет_ |

### Классификация (intake)

- **Тип:** новая фича, чисто техническая — миграция с deprecated API v1.4 на актуальное v1.5.
- **Постановка Стейхолдера:** проанализировать код и `api-docs/documentation.yaml`, найти использование устаревшего API и выполнить апгрейд.
- **Первичная фактура Оркестратора:**
  - `PoiskKinoApiClient.cs:81` → `GET /v1.4/movie/search` (deprecated) — актуальный аналог `/v1.5/movie/search`.
  - `PoiskKinoApiClient.cs:202` → `GET /v1.4/movie/{id}` (deprecated) — актуальный аналог `/v1.5/movie/{id}`.
  - `PoiskKinoApiClient.cs:320` → `GET /v1.5/season` — уже на актуальной версии.
  - Затронуты модели `Models/PoiskKino*V1_4*.cs` (`SearchMovieResponseDtoV1_4`, `MovieDtoV1_4`, `SeasonDocsResponseDtoV1_4` и связанные), юнит-тесты с фикстурами v1.4 JSON.
  - В спеке v1.5 — курсорная пагинация (`next`/`prev`/`hasNext`) и обновлённые схемы DTO; demo/free тариф ограничен 10 страницами пагинации.
  - Требование `api-docs/AGENTS.md`: опираться на https://api.poiskkino.dev/llms.txt (прочитан).

### Стадии (справочно)

`intake` → `analysis` → `analysis-review` → `backlog-paused` или `architecture` → `tech-decomposition` → `development` → `devops-check` → `tech-lead-review` → `qa` → `rework` → `finalization` → `pr-mr-ready` → `stakeholder-review` → `rework` или `accepted`

`analysis-review` — обязательная остановка после Аналитика: Стейхолдер решает продолжать сейчас или оставить фичу в backlog.

## Задачи реализации

| ID | Файл | Роль / стек | Статус |
|----|------|-------------|--------|
| — | — | — | — |

## DevOps impact check

| Вопрос | Результат |
|--------|-----------|
| Новые env vars / secrets | _нет / да_ |
| Новые интеграции / порты / очереди / storage / jobs | _нет / да_ |
| Docker / compose / CI/CD / deploy / Helm / K8s / observability | _нет / да_ |
| DevOps-задача | _не нужна / ссылка_ |

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

## Changelog (handoff)

### 2026-08-23 — создан шаблон

- Создана папка фичи из шаблона.
- Оркестратор: intake, классификация (техническая фича), ветка `feature/api-v1-5-upgrade`, worktree `/home/alex/src/my/jellyfin-metadata-plugin-api-v15`.
- Handoff → **Аналитик** (`.opencode/agents/analyst.md`): подготовить `analysis.md` по миграции v1.4 → v1.5.
