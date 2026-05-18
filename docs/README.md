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

## Связь с Cursor

- Правила: [`.cursor/rules/`](../.cursor/rules/)
- Роли: [`.cursor/agents/`](../.cursor/agents/)
- Навыки: [`.cursor/skills/`](../.cursor/skills/)
- Slash-команды чата: [`.cursor/commands/`](../.cursor/commands/) (`/accept-feature`, `/reject-feature`, `/init-empty-repo`, `/init-existing-repo`)
- Корневые инструкции: [`AGENTS.md`](../AGENTS.md)

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
