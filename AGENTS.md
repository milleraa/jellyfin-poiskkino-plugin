# Инструкции для агента (проект) — OpenCode

## Режимы работы

| Режим | Как начать | Где правила |
|-------|------------|-------------|
| **Команда агентов (по умолчанию)** | Агент `orchestrator` (`default_agent`) | [`.opencode/agents/orchestrator.md`](.opencode/agents/orchestrator.md), [`docs/process/agile-workflow.md`](docs/process/agile-workflow.md) |
| **Свободная работа с кодом** | Встроенный агент `build` | Этот файл + [`docs/README.md`](docs/README.md); `status.md` и цепочка ролей не обязательны |

## Карта документации

Полная структура и владельцы разделов: **[`docs/README.md`](docs/README.md)**.

| Раздел | Путь | Зачем смотреть |
|--------|------|----------------|
| Процесс | `docs/process/` | Agile, роли, DoR/DoD |
| Продукт | `docs/product/` | Видение, требования |
| Работа | `docs/backlog/features/`, `docs/backlog/hotfixes/` | Фичи и hotfix; **живой статус — `status.md`** в папке |
| Архитектура | `docs/architecture/` | Обзор, ADR |
| Инженерия | `docs/engineering/` | Стандарты, [`stacks/`](docs/engineering/stacks/), тесты, DevOps |
| Окружения | `docs/environments/` | CI/CD, деплой (секреты не в git) |
| QA | `docs/qa/` | Playwright, прогоны |
| История | `docs/archive/`, `docs/history/` | Завершённые работы |

## Для агента `build` и работы вне процесса

- Опирайтесь на факты в `docs/` и код в репозитории; не выдумывайте версии, URL и политики.
- Не меняйте `status.md` и процессные артефакты без явной просьбы Стейхолдера.
- Секреты не коммитьте — см. политику в `docs/environments/` (если задана).
- Документация библиотек на GitHub: `use deepwiki` + `owner/repo` (таблицы в `docs/engineering/stacks/`).
