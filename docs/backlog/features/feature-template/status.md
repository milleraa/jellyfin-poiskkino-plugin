# Status — `<feature-name>`

Единая точка правды по стадии для Оркестратора. Обновляйте при каждом handoff (см. `.cursor/skills/role-handoff/SKILL.md`).

## Текущее состояние

| Поле | Значение |
|------|----------|
| **Стадия** | `intake` |
| **Owner** | `Orchestrator` |
| **Worktree** | _нет_ |
| **Ветка** | _нет_ |
| **PR/MR** | _нет_ |
| **Commit со ссылкой на PR/MR** | _нет_ |
| **Блокеры** | _нет_ |

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

### YYYY-MM-DD — создан шаблон

- Создана папка фичи из шаблона.
