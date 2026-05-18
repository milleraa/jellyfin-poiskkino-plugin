---
name: finalization-check
description: Проверяет delivery finalization и acceptance/archive finalization: PR/MR ready, rework loop, перенос в archive после принятия. Используй когда QA передал работу Финализатору или PR/MR принят.
disable-model-invocation: true
---

# Finalization check

## Delivery checklist

Сверься с `docs/process/definition-of-done.md`:

- [ ] Все задачи в `tasks/` закрыты или отменены с причиной.
- [ ] `qa.md` и `docs/qa/test-runs.md` отражают последний прогон.
- [ ] Все ролевые шаги с изменениями зафиксированы локальными commit без push.
- [ ] PR/MR открыт Финализатором, ссылка в `finalization.md` и `status.md`.
- [ ] Ссылка на PR/MR зафиксирована отдельным commit и повторным push.
- [ ] Локальный worktree удалён; branch/remote branch сохранены до merge/close PR/MR.
- [ ] Архитектурные изменения отражены (ADR / overview).
- [ ] `status.md` — стадия `pr-mr-ready`, PR/MR ссылка и changelog.
- [ ] Папка feature/hotfix остаётся в `docs/backlog/...` до принятия Стейхолдером или merge.

## Acceptance/archive checklist

Выполняй только после явного acceptance от Стейхолдера или merge PR/MR.

1. `git mv` (или эквивалент) папки фичи/hotfix из `docs/backlog/...` в `docs/archive/features/...` или `docs/archive/hotfixes/...`.
2. В архивном `status.md` выставь стадию `accepted`, owner `Stakeholder`.
3. Добавь строку в `docs/history/completed-work.md`: название, дата, ссылка на PR/MR, путь в архиве.
4. Если PR/MR не принят, не архивируй; верни Оркестратору для `.cursor/skills/pr-mr-rework/SKILL.md`.

## После

Сообщи Оркестратору и Стейхолдеру ссылку на PR/MR и путь в `docs/archive/`.
