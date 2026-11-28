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
- Укажи ветку, ссылки на PR/MR и задачи в `tasks/`.
- Если роль меняла файлы — сделай локальный commit без push перед handoff.
- Push выполняет только Финализатор на стадии PR/MR delivery.
- Если блокер — метка `blocked` и причина.

## Для Оркестратора

- **Hub-модель:** handoff к роли → отчёт → обновление `status.md` → следующая подзадача. Все вызовы выполняет Оркестратор; вложенных вызовов нет.
- Передай в подзадачу только нужный контекст: feature/hotfix папку, `status.md`, релевантные артефакты, задачу и ожидаемый результат.
- После ответа подзадачи проверь артефакты, commit status и следующий рекомендуемый шаг; затем обнови `status.md`.
- Когда Техлид вернул план, вызови `developer-*` или `devops` по подходящей задаче. Верни результат Техлиду для ревью. Не переходи к QA без его приёмки и закрытого DevOps impact check.
- Не запускай следующую роль, пока текущая подзадача не вернула результат или блокер.
- После Аналитика по новой фиче не запускай Архитектора автоматически: переведи работу в `analysis-review`, покажи summary Стейхолдеру и спроси `continue` или `defer`.
- Если Стейхолдер выбирает `defer`, выставь `backlog-paused`, owner `Stakeholder` или `Orchestrator`, и оставь фичу в `docs/backlog/features/<name>/`.
- После решения `continue` happy path идёт без плановых остановок до `pr-mr-ready`; останавливайся только при блокере, эскалации или rejected PR/MR.

## Стадии (справочно)

`intake` → `analysis` → `analysis-review` → `backlog-paused` или `architecture` → `tech-decomposition` → `development` → `devops-check` → `tech-lead-review` → `qa` → `rework` → `finalization` → `pr-mr-ready` → `stakeholder-review` → `rework` или `accepted`
