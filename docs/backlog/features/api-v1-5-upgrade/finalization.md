# Finalization — `api-v1-5-upgrade`

## PR/MR

- **Ссылка:** https://github.com/milleraa/jellyfin-poiskkino-plugin/pull/2 (base `main`)
- **Ветка:** `feature/api-v1-5-upgrade` (pushed в origin; branch/remote branch не удаляются до merge/close)
- **Commit со ссылкой на PR/MR:** см. `git log` после записи этого файла (docs(finalization): PR #2 …); выполнен отдельным commit + повторный push.
- **Worktree cleanup:** локальный worktree `/home/alex/src/my/jellyfin-metadata-plugin-api-v15` сохранён до acceptance (удаление — только после повторного push ссылки на PR/MR; здесь push выполнен, worktree оставлен по решению этапа delivery — задача требовала не удалять).

## Delivery checklist

См. `.opencode/skills/finalization-check/SKILL.md` и [DoD](../../../process/definition-of-done.md).

- [x] Задачи `tasks/001–003` закрыты (**done**).
- [x] `qa.md` и `docs/qa/test-runs.md` отражают последний прогон (160 passed / 0 failed, 2026-08-23).
- [x] Все ролевые шаги зафиксированы локальными commit (`73ea86f`, `8f724dd`, `1abee81`, QA `60f5286`).
- [x] PR открыт Финализатором; ссылка в `finalization.md` и `status.md`.
- [x] Ссылка на PR зафиксирована отдельным commit и повторно pushed.
- [ ] Локальный worktree удалён — **отложено** по указанию задачи (после acceptance).
- [x] Архитектурные изменения отражены: ADR-0001 + обновлённый `docs/architecture/overview.md`.
- [x] `status.md` — стадия `pr-mr-ready`, ссылка на PR, changelog.
- [x] Папка фичи остаётся в `docs/backlog/features/` до принятия Стейхолдером или merge.

## Acceptance/archive

- **Accepted / merged:** _ожидается решение Стейхолдера (gate `pr-mr-ready`)_
- После merge / приёмки Стейхолдером: папка переносится в `docs/archive/features/api-v1-5-upgrade/`, запись в `docs/history/completed-work.md`, архивный `status.md` → `accepted`.
- Если PR/MR не принят: работа возвращается через `.opencode/skills/pr-mr-rework/SKILL.md`, без переноса в архив.
