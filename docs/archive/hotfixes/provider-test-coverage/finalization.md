# Finalization — покрытие поведения провайдеров

- **PR/MR:** [GitHub PR #10](https://github.com/milleraa/jellyfin-poiskkino-plugin/pull/10), `hotfix/provider-test-coverage` → `main`.
- **База:** после fetch локальный `main`, `origin/main` и merge-base ветки совпали на `4cbe4d4`, базе QA. Рабочее дерево перед первым push было чистым; HEAD `e71e128`.
- **Commit со ссылкой на PR/MR:** отдельный коммит `docs(hotfix): зафиксируй готовность PR #10 к ревью` в той же ветке.
- **Стадия:** `accepted`. Стейкхолдер принял PR 2026-11-28 в чате сообщением «ок, архивируй».

## DoD и test plan

- [001](tasks/001-series-search-year.md) и [002](tasks/002-metadata-person-roles.md) приняты Техлидом и выполнены последовательно в одной ветке. Изменения и переходы ролей зафиксированы коммитами до delivery.
- Техлид закрыл DevOps impact check до QA. Менялись только два файла тестов и документация QA/hotfix; обновления продуктовых, архитектурных и DevOps-документов не требуются. Тесты закрепляют существующее правило [ADR-0001](../../../architecture/decisions/ADR-0001-search-year-filter.md).
- [QA PASS](qa.md) (`042410c`), [журнал прогонов](../../../qa/test-runs.md): 283/283, Release-сборка без ошибок (61 существующее предупреждение `CS1591`). Обе контрольные поломки обнаружены, после восстановления — 283/283. Live smoke не запускался: HTTP подменён, продуктовый код не менялся. `git diff --check origin/main...HEAD` прошёл перед delivery.
- [CI / build-and-test](https://github.com/milleraa/jellyfin-poiskkino-plugin/actions/runs/46110776370/job/140873592434): success для `f64ce27` (HEAD перед архивацией). Статус текущего HEAD — во [вкладке Checks](https://github.com/milleraa/jellyfin-poiskkino-plugin/pull/10/checks).

## Acceptance / archive

- Hotfix перенесён из backlog в `docs/archive/hotfixes/provider-test-coverage/` в существующей ветке `hotfix/provider-test-coverage` до merge.
- Обновлены [статус](status.md), [индекс архива](../README.md), [журнал прогонов](../../../qa/test-runs.md) и [история завершённых работ](../../../history/completed-work.md#provider-test-coverage-2026-11-28).
- В коммите архивации меняется только документация; QA не запускалась повторно. PR оставлен открытым, локальная и удалённая ветки сохранены.
