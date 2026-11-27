# Tasks

Файлы `NNN-slug.md` создаёт Техлид. Шаблон: [task-template](../../../task-template.md). План и порядок выполнения: [tech-plan.md](../tech-plan.md).

Все задачи выполняются строго последовательно (001 → 002 → 003 → 004) в ветке `feature/eu-api-endpoint`, каждая завершается локальным commit без push.

| ID | Файл | Роль | Статус |
|----|------|------|--------|
| 001 | [001-api-base-url-resolver.md](001-api-base-url-resolver.md) — `ApiBaseUrlResolver` + unit-тесты | `developer-csharp` | done |
| 002 | [002-configuration-client-providers.md](002-configuration-client-providers.md) — конфигурация, клиент, 5 провайдеров, тесты фактического хоста | `developer-csharp` | todo |
| 003 | [003-config-page-field.md](003-config-page-field.md) — поле базового URL в `configPage.html` | `developer-csharp` | todo |
| 004 | [004-docs-base-url.md](004-docs-base-url.md) — `README.md` + `docs/architecture/overview.md` | `developer-csharp` | todo |
