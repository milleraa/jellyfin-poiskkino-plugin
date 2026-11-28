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
| [environments](environments/) | GitHub, CI/CD, установка, политика секретов |
| [tasks](tasks/) | Индексы активных/заблокированных/закрытых задач |
| [qa](qa/) | Стратегия тестирования, ручные проверки и прогоны |
| [history](history/) | Лог решений, завершённые работы (ссылки) |
| [archive](archive/) | Завершённые фичи и hotfix (перенос из backlog) |

## Запуск SDLC

- Единый навык [`sdlc`](../.agents/skills/sdlc/SKILL.md) содержит регламент, процедуры и [промпты ролей](../.agents/skills/sdlc/role/).
- В OpenCode **v2** агент `orchestrator` запускается по умолчанию. Агенты и MCP описаны в [`opencode.json`](../opencode.json); `system` подставляет промпты из навыка. Проект не задаёт дополнительных разрешений: действуют стандартные правила OpenCode и настройки среды.
- В Pi запустите `/skill:sdlc`, в Codex — `$sdlc`. Передайте задачу или путь к существующему `status.md`. Если делегирование недоступно, Оркестратор подготовит ручную передачу роли.
- [Команды OpenCode](../.opencode/commands/) — необязательные сокращения для приёмки, возврата на доработку и bootstrap. В других средах можно сообщить те же решения обычным текстом.
- Общие инструкции — в [`AGENTS.md`](../AGENTS.md); внешняя документация по стекам — в [engineering/stacks/](engineering/stacks/).

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
