---
description: Стейхолдер принял PR/MR: acceptance/archive через Финализатора.
agent: orchestrator
subtask: false
---

Ты **Оркестратор**. Стейхолдер подтвердил приёмку работы **до merge PR/MR** — это штатный триггер. Архивация выполняется в feature/hotfix ветке, чтобы после merge все изменения попали в main одним мержем.

## Что сделать

1. Уточни у Стейхолдера в этом сообщении (если ещё не указано):
   - путь к папке фичи/hotfix: `docs/backlog/features/<name>/` или `docs/backlog/hotfixes/<name>/`;
   - ссылку на PR/MR.
2. Проверь `status.md`: стадия должна быть `pr-mr-ready` или `stakeholder-review`; ссылка на PR/MR должна совпадать с подтверждённой. Если PR/MR уже merged — это отклонение от флоу; действуй по fallback из `.opencode/agents/finalizer.md` и отметь инцидент.
3. Передай **Финализатору** (`@.opencode/agents/finalizer.md`) выполнить **acceptance/archive finalization в feature/hotfix ветке** по чеклисту «Accepted / Done» в `docs/process/definition-of-done.md` и разделу **Acceptance/archive checklist** в `.opencode/skills/finalization-check/SKILL.md`:
   - переключиться на существующую feature/hotfix ветку;
   - перенос папки в `docs/archive/features/...` или `docs/archive/hotfixes/...`;
   - обновление `docs/history/completed-work.md`;
   - в архивном `status.md`: стадия `accepted`, owner `Stakeholder` (или по договорённости);
   - commit и push в ту же feature/hotfix ветку.
4. После push архивации сообщи Стейхолдеру, что PR/MR готов к merge: пусть merge выполнит сам или попросит агента (через `gh`/`glab`). Не удаляй branch/remote branch до merge.
5. Не смешивай с rework: если Стейхолдер сообщил, что PR/MR не принят, применяй `.opencode/skills/pr-mr-rework/SKILL.md`.
6. Если ты как Оркестратор менял только `docs/` до передачи Финализатору — сделай локальный commit без push; push — только у Финализатора (см. [`.opencode/agents/finalizer.md`](../agents/finalizer.md)).
