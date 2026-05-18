---
name: role-handoff
description: Стандартизирует передачу работы между ролями: обновление status.md, ссылки на артефакты, следующий owner. Используй при переходах Analyst→Architect→TechLead→Developer/DevOps→QA→Finalizer и при PR/MR rework.
disable-model-invocation: true
---

# Role handoff

## Шаблон записи в status.md

Добавь блок в конец changelog:

```text
## YYYY-MM-DD HH:MM — handoff
- from: <role>
- to: <role>
- stage: <stage>
- artifacts: <paths or links>
- notes: <кратко>
```

## Обязательно

- Обнови поля **текущая стадия** и **owner**.
- Укажи worktree, ветку, ссылки на PR/MR и задачи в `tasks/`.
- Если роль меняла файлы — сделай локальный commit без push перед handoff.
- Push выполняет только Финализатор на стадии PR/MR delivery.
- Если блокер — метка `blocked` и причина.

## Для Оркестратора

- Handoff к роли означает запуск отдельной подзадачи/субагента с ролевым промптом из `.cursor/agents/`.
- Передай в подзадачу только нужный контекст: feature/hotfix папку, `status.md`, релевантные артефакты, задачу и ожидаемый результат.
- После ответа подзадачи проверь артефакты, commit status и следующий рекомендуемый шаг; затем обнови `status.md`.
- Не запускай следующую роль, пока текущая подзадача не вернула результат или блокер.
- После Аналитика по новой фиче не запускай Архитектора автоматически: переведи работу в `analysis-review`, покажи summary Стейхолдеру и спроси `continue` или `defer`.
- Если Стейхолдер выбирает `defer`, выставь `backlog-paused`, owner `Stakeholder` или `Orchestrator`, и оставь фичу в `docs/backlog/features/<name>/`.
- После решения `continue` happy path идёт без плановых остановок до `pr-mr-ready`; останавливайся только при блокере, эскалации или rejected PR/MR.

## Стадии (справочно)

`intake` → `analysis` → `analysis-review` → `backlog-paused` или `architecture` → `tech-decomposition` → `development` → `devops-check` → `tech-lead-review` → `qa` → `rework` → `finalization` → `pr-mr-ready` → `stakeholder-review` → `rework` или `accepted`
