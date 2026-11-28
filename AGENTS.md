# Инструкции для агента (проект)

## Режимы работы

| Режим | Как начать | Где правила |
|-------|------------|-------------|
| **SDLC** | Вызвать навык `sdlc` в основном агенте (в OpenCode v2 — агент `orchestrator` по умолчанию) | [Навык `sdlc`](.agents/skills/sdlc/SKILL.md) |
| **Свободная работа с кодом** | Обычный агент без `sdlc` (в OpenCode — `build`) | Этот файл + [`docs/README.md`](docs/README.md); `status.md` и цепочка ролей не обязательны |

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
| QA | `docs/qa/` | стратегия, ручные проверки и прогоны |
| История | `docs/archive/`, `docs/history/` | Завершённые работы |

## Для работы вне процесса

- Опирайтесь на факты в `docs/` и код в репозитории; не выдумывайте версии, URL и политики.
- Не меняйте `status.md` и процессные артефакты без явной просьбы Стейхолдера.
- Секреты не коммитьте — см. политику в `docs/environments/` (если задана).
- Если доступен DeepWiki, документация библиотек на GitHub: `use deepwiki` + `owner/repo` (таблицы в `docs/engineering/stacks/`).
