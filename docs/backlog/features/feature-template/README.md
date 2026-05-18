# Шаблон фичи (копировать папку целиком)

Переименуйте `feature-template` → `<feature-name>`.

## Файлы

| Файл | Назначение |
|------|------------|
| [README.md](README.md) | Краткое описание и ссылки |
| [status.md](status.md) | **Стадия процесса и отчёты ролей** |
| [brief.md](brief.md) | Цель и scope |
| [analysis.md](analysis.md) | Аналитика, истории, AC |
| [architecture.md](architecture.md) | Дизайн, ссылки на ADR |
| [tech-plan.md](tech-plan.md) | План Техлида |
| [tasks/](tasks/README.md) | Задачи разработчиков и DevOps |
| [qa.md](qa.md) | Результаты QA |
| [finalization.md](finalization.md) | PR/MR delivery, acceptance/archive |

После принятия Стейхолдером или merge PR/MR Финализатор переносит папку в [archive](../../archive/features/README.md). Если PR/MR не принят, работа остаётся в backlog и возвращается через `.cursor/skills/pr-mr-rework/SKILL.md`.
