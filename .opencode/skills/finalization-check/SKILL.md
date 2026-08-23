---
name: finalization-check
description: Проверяет delivery finalization и acceptance/archive finalization: PR/MR ready, rework loop, перенос в archive после принятия. Используй когда QA передал работу Финализатору или PR/MR принят.
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

Выполняй только после явного acceptance от Стейхолдера и **до merge PR/MR** (штатный флоу: решение Стейхолдера сначала сообщается агенту).

1. Если локальный worktree удалён после delivery — восстанови его из существующей feature/hotfix ветки; новую ветку не создавай.
2. `git mv` (или эквивалент) папки фичи/hotfix из `docs/backlog/...` в `docs/archive/features/...` или `docs/archive/hotfixes/...`.
3. В архивном `status.md` выставь стадию `accepted`, owner `Stakeholder`.
4. Добавь строку в `docs/history/completed-work.md`: название, дата, ссылка на PR/MR, путь в архиве.
5. Сделай commit и push в ту же feature/hotfix ветку. Merge PR/MR — только после этого шага: тогда все изменения попадут в main одним мержем.
6. Если PR/MR уже merged к моменту acceptance — archive-коммиты выполняются в main, инцидент фиксируется в архивном `status.md`.
7. Если PR/MR не принят, не архивируй; верни Оркестратору для `.opencode/skills/pr-mr-rework/SKILL.md`.

## После

Сообщи Оркестратору и Стейхолдеру ссылку на PR/MR и путь в `docs/archive/`.
