# Шаблон задачи (developer task)

Файл: `docs/backlog/features/<feature>/tasks/NNN-short-name.md` (или в hotfix-папке).

## Метаданные

- **ID:** NNN
- **Feature / Hotfix:** ссылка на папку
- **Назначено:** роль исполнителя (C# / Go / TS / React / PostgreSQL / DevOps)
- **Статус:** draft | in progress | in review | done | cancelled
- **Ветка:** `feature/...` или `fix/...`

## Описание

Что сделать одним заходом PR.

## Acceptance criteria

1. …
2. …

## Технические заметки

- Зависимости, риски, ссылки на ADR.

## DevOps impact check

- Новые/изменённые переменные окружения или secrets: _нет / да, детали_
- Новые внешние интеграции, порты, очереди, storage, scheduled jobs: _нет / да, детали_
- Требуются изменения Docker/compose/CI/CD/deploy/Helm/Kubernetes/observability: _нет / да, детали_
- Если impact есть: ссылка на DevOps-задачу или причина, почему не нужна.

## Проверка

- Команды тестов / сценарии ручной проверки.

## Handoff

- После выполнения: обновить этот файл и `status.md`; уведомить Техлида.
