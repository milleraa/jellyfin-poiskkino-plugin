# Accept feature / hotfix (Стейхолдер принял PR/MR)

Ты **Оркестратор**. Стейхолдер подтвердил приёмку: PR/MR принят или merged (или явно согласовано как «accepted без merge» по политике проекта).

## Что сделать

1. Уточни у Стейхолдера в этом сообщении (если ещё не указано):
   - путь к папке фичи/hotfix: `docs/backlog/features/<name>/` или `docs/backlog/hotfixes/<name>/`;
   - ссылку на PR/MR и факт merge или явного acceptance.
2. Проверь `status.md`: стадия должна быть `pr-mr-ready` или `stakeholder-review`; ссылка на PR/MR должна совпадать с подтверждённой.
3. Передай **Финализатору** (`@.cursor/agents/finalizer.md`) выполнить **только acceptance/archive finalization** по чеклисту «Accepted / Done» в `docs/process/definition-of-done.md` и разделу **Acceptance/archive checklist** в `.cursor/skills/finalization-check/SKILL.md`:
   - перенос папки в `docs/archive/features/...` или `docs/archive/hotfixes/...`;
   - обновление `docs/history/completed-work.md`;
   - в архивном `status.md`: стадия `accepted`, owner `Stakeholder` (или по договорённости).
4. Не смешивай с delivery: архив только после acceptance/merge. Если PR/MR ещё не merged и политика требует merge — эскалируй к Стейхолдеру.
5. Если ты как Оркестратор менял только `docs/` до передачи Финализатору — сделай локальный commit без push; дальнейшие push по ветке — по [Git discipline](../../AGENTS.md#git-discipline) у Финализатора при необходимости.
