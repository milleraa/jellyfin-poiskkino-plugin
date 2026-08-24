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

## ⚠️ Историческая заметка: BLOCKER flaky CI (устранён в rework)

> ✅ **Разрешено (2026-08-24):** фикс изоляции доставлен (`f3beb9a`, задача 003), повторная доставка PR #3 — CI зелёный ([run](https://github.com/milleraa/jellyfin-poiskkino-plugin/actions/runs/32750717011)); далее фича принята Стейхолдером `/accept-feature`. Текст ниже сохранён как история инцидента.

**Merge PR #3 НЕ выполнять.** Стадия формально `pr-mr-ready`, но поставка заблокирована нестабильным CI:

- Flaky-тест `ImageUrlHelperTests.ShouldIgnoreTmdbImages_WhenPluginNotInitialized_ReturnsFalse` упал 2 раза из 3 CI-прогонов (гонка изоляции, продуктовый код фичи не затронут). Детали и evidence — в [status.md](status.md), Changelog.
- **Маршрут:** Оркестратор → rework (`pr-mr-rework`) → QA/TechLead: фикс тестовой изоляции → повторная доставка PR. Либо явное решение Стейхолдера принять PR с известным flake (не рекомендуется).
- При `/accept-feature` несмотря на blocker: сначала фикс изоляции в этой ветке, затем archive finalization (перенос в `docs/archive/features/logo-images/`, запись в `docs/history/completed-work.md`, стадия `accepted`), commit + push, затем merge одним мержем.

### Worktree

Локальный worktree `/home/alex/src/my/jellyfin-metadata-plugin-wt-logo-images` **сохранён** (отклонение от DoD-пункта об удалении — осознанное, причина: активный blocker; worktree потребуется и для rework-фикса, и для архивации при accept). Ветка `feature/logo-images` и remote branch сохранены до merge/close.

## Acceptance/archive

- **Accepted:** да — Стейхолдер `/accept-feature` 2026-08-24 **до merge** PR #3 (штатный флоу DoD «Accepted / Done»). Merge Финализатором не выполнялся. Архивация выполнена в ветке `feature/logo-images` до merge: папка перенесена в `docs/archive/features/logo-images/`, запись добавлена в [completed-work](../../../history/completed-work.md), стадия `accepted` в [status.md](status.md). Merge PR #3 — следующий шаг Стейхолдера: все изменения попадут в main одним мержем вместе с архивацией.
