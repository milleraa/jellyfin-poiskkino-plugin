# Status — `<hotfix-name>`

| Поле | Значение |
|------|----------|
| **Стадия** | `intake` |
| **Owner** | `Orchestrator` |
| **Срочность** | _P1 / P2 / …_ |
| **Worktree** | _TBD_ |
| **Ветка** | _TBD_ |
| **PR/MR** | _TBD_ |
| **Commit со ссылкой на PR/MR** | _TBD_ |

## Стадии (справочно)

`intake` → `root-cause` → `architecture` → `tech-decomposition` → `development` → `devops-check` → `tech-lead-review` → `qa` → `rework` → `finalization` → `pr-mr-ready` → `stakeholder-review` → `rework` или `accepted`

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
| **Learning triage** | _not-triggered / suppressed / candidate_ |
| **Class key** | _affected role + violated contract/gate + classification_ |
| **Learning evidence** | _ссылки на PR/MR feedback / CI / сообщение Стейкхолдера_ |
| **Suppression reason** | _нет / причина и evidence_ |
| **Process review** | _нет / ссылка на process-review.md_ |

Один unique reject получает `not-triggered` и не блокирует rework. `candidate` возможен только при двух или более unique comparable evidence-linked cases; изменения процесса выполняются только отдельным reviewed PR/MR. Правила: [process learning](../../../process/learning/README.md).

## Changelog

### YYYY-MM-DD — создан шаблон

- …
