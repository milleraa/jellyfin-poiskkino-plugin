# Finalization — `search-limit-setting`

## PR/MR

- **Ссылка:** [jellyfin-poiskkino-plugin#11](https://github.com/milleraa/jellyfin-poiskkino-plugin/pull/11) — `feat(config): настройка количества результатов поиска PoiskKino (1–10, дефолт 3)` (base `main` ← head `feature/search-limit-setting`)
- **Ветка:** `feature/search-limit-setting` → `main` (push в `origin`; ветку и remote branch не удалять до merge/close PR)
- **Commit со ссылкой на PR/MR:** `docs(chore): фиксируй PR #11 в архивных документах search-limit-setting` (обновлены [status.md](status.md), этот файл, индекс архива и completed-work), повторный push в ту же ветку

## Порядок финализации (отклонение по решению Стейхолдера)

Стейхолдер дал приёмку **до открытия PR/MR** (2026-11-28, дословно: «можно сразу архивировать и готовить архивированный mr. Версию повысь на один билд.»), поэтому порядок инвертирован относительно штатного: сначала архивация в ветке (стадия фактически `accepted`), затем push и создание PR/MR в `main`; merge выполняет Стейхолдер. Прецедент: [provider-test-coverage](../../hotfixes/provider-test-coverage/status.md).

## Acceptance/archive

- **Accepted:** **да** — Стейхолдер, 2026-11-28, явно (цитата выше); merge PR Финализатором не выполняется.
- **Archive finalization выполнена** в той же ветке `feature/search-limit-setting` (новая ветка не создавалась), до merge:
  - `git mv docs/backlog/features/search-limit-setting/` → `docs/archive/features/search-limit-setting/` (13 файлов, в backlog остался только `feature-template`);
  - версия в [csproj](../../../../PoiskKinoMetadataPlugin/PoiskKinoMetadataPlugin.csproj) повышена `2026.11.28` → `2026.11.28.1` (датная схема, +1 билд: `2026.11.28` занята релизом `8aeebd7`);
  - в архивном [status.md](status.md) — стадия `accepted`, owner `Stakeholder`, дословная запись решения, финальная запись Changelog;
  - починены ссылки, ломавшиеся переносом: `tasks/README.md`, `tasks/task-template.md` (`../../../../backlog/task-template.md`), внешние `docs/qa/test-runs.md` (2 ссылки) и [ADR-0004](../../../architecture/decisions/ADR-0004-configurable-search-limit.md);
  - запись в [completed-work](../../../history/completed-work.md) и в индекс [docs/archive/features/README.md](../../README.md).
- После merge PR/MR все изменения (код + версия + архивация) попадут в `main` одним мержем.
- Если PR/MR не будет принят: возврат через `.agents/skills/sdlc/references/pr-mr-rework.md`.

## Delivery checklist

См. `.agents/skills/sdlc/references/finalization-check.md` и [DoD](../../../process/definition-of-done.md): QA **pass** (327/327 + смоук Jellyfin 12.4.0: AC-6 UI, AC-8 закрыты; AC-7 live и R1 — принятые ограничения), задачи 001–003 `accepted`, DevOps impact — нет, архитектура — [ADR-0004](../../../architecture/decisions/ADR-0004-configurable-search-limit.md).
