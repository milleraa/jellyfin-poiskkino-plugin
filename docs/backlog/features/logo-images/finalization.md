# Finalization — `logo-images`

## PR/MR

- **Ссылка:** [jellyfin-poiskkino-plugin#3](https://github.com/milleraa/jellyfin-poiskkino-plugin/pull/3) — `feat(images): expose PoiskKino title logos as ImageType.Logo` (base `main` ← head `feature/logo-images`)
- **Ветка:** `feature/logo-images` (push в `origin`; ветку и remote branch не удалять до merge/close PR)
- **Commit со ссылкой на PR/MR:** `docs(logo-images): open PR #3, -> pr-mr-ready` (см. git log ветки; обновлены [status.md](status.md) и этот файл)
- **Worktree cleanup:** локальный worktree `/home/alex/src/my/jellyfin-metadata-plugin-wt-logo-images` удалён Финализатором после повторного push со ссылкой на PR.

## Delivery checklist

См. `.opencode/skills/finalization-check/SKILL.md` и [DoD](../../../process/definition-of-done.md) («Ready for stakeholder review»):

- [x] Задачи `tasks/001`, `tasks/002` закрыты (done, см. [status.md](status.md)).
- [x] Один worktree, последовательное выполнение.
- [x] Все ролевые шаги зафиксированы локальными commit (`8d48967..f0bfb8c`).
- [x] Техлид принял реализацию (2026-08-24) и закрыл DevOps impact check (impact — нет).
- [x] QA вердикт: pass ([qa.md](qa.md), AC-1..AC-12; 163/163 ×2, build 0 err; запись в [test-runs](../../../qa/test-runs.md)).
- [x] Push выполнен, PR #3 открыт, ссылка записана в `status.md`/`finalization.md`, отдельный commit со ссылкой сделан, повторный push выполнен.
- [x] Локальный worktree удалён после повторного push; branch/remote branch сохранены.
- [x] Документация: ADR не требуется (зафиксировано Архитектором); DevOps docs не менялись (impact нет). Устаревание `docs/product/requirements.md:12` передано Аналитику/Стейхолдеру на этапе архитектуры.
- [x] Папка фичи остаётся в `docs/backlog/features/logo-images/` до принятия Стейхолдером.

## ⚠️ Статус: ожидание решения Стейхолдера

**Merge PR #3 пока НЕ выполнять.** Стадия `pr-mr-ready` — review-состояние:

- `/accept-feature` → Финализатор выполняет archive finalization **в ветке `feature/logo-images` до merge**: восстановление worktree из существующей ветки, перенос папки в `docs/archive/features/logo-images/`, запись в `docs/history/completed-work.md`, стадия `accepted` в архивном `status.md`, commit + push → затем merge одним мержем.
- `/reject-feature` → возврат в rework через `.opencode/skills/pr-mr-rework/SKILL.md`.

## Acceptance/archive

- **Accepted / merged:** _TBD — ожидается решение Стейхолдера_
- После accept (до merge): папка переносится в `docs/archive/features/logo-images/` коммитами в `feature/logo-images` и пушится; merge PR выполняется после архивации — все изменения попадают в main одним мержем.
- Если PR не принят: работа возвращается через `.opencode/skills/pr-mr-rework/SKILL.md`, без переноса в архив.
