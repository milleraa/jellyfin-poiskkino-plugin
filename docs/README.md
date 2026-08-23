# Документация проекта

Карта для людей и агентов. **Точка входа для контекста фичи:** папка в [backlog/features](backlog/features/) или [backlog/hotfixes](backlog/hotfixes/) и файл **`status.md`**.

## Разделы

| Раздел | Назначение |
|--------|------------|
| [process](process/) | Agile, роли, DoR/DoD |
| [product](product/) | Видение, стейкхолдеры, требования |
| [backlog](backlog/) | Эпики, фичи, hotfix, шаблоны |
| [architecture](architecture/) | Обзор, принципы, ADR, диаграммы |
| [engineering](engineering/) | Код, ветки, доставка, тесты, стек |
| [environments](environments/) | GitLab/Gitea, CI/CD, деплой, секреты (политика) |
| [tasks](tasks/) | Индексы активных/заблокированных/закрытых задач |
| [qa](qa/) | Стратегия тестирования, Playwright, прогоны |
| [history](history/) | Лог решений, завершённые работы (ссылки) |
| [archive](archive/) | Завершённые фичи и hotfix (перенос из backlog) |

## Связь с OpenCode

- Глобальный конфиг: [`opencode.json`](../opencode.json) (instructions, default_agent; роли — не здесь)
- Ролевые агенты OpenCode: [`.opencode/agents/*.md`](../.opencode/agents/) (промпт, mode, permission)
- Навыки: [`.opencode/skills/`](../.opencode/skills/)
- Команды TUI: [`.opencode/commands/`](../.opencode/commands/) (`/accept-feature`, `/reject-feature`, `/init-empty-repo`, `/init-existing-repo`)
- Глобальные instructions (коротко, для всех агентов): [`AGENTS.md`](../AGENTS.md); agile-процесс — в [`.opencode/agents/orchestrator.md`](../.opencode/agents/orchestrator.md)
- MCP (DeepWiki и др.): конфигурация в [`opencode.json`](../opencode.json) (секция `mcp`)
- Внешняя документация по стекам: [engineering/stacks/](engineering/stacks/), [testing.md](engineering/testing.md), [devops.md](engineering/devops.md)

## Кто поддерживает разделы

| Раздел | Владелец по процессу |
|--------|----------------------|
| product | Аналитик + Стейхолдер |
| backlog/features, hotfixes | Оркестратор + Техлид |
| architecture | Архитектор |
| engineering | Техлид + DevOps |
| environments | Оркестратор + Стейхолдер (факты доступа) + DevOps |
| qa | QA |
| archive, history | Финализатор |
