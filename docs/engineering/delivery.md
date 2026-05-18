# Delivery — PR/MR

Приёмка Стейхолдером — по **готовому PR/MR** с описанием и зелёным CI (если настроен).

## Шаблон описания

См. `.cursor/skills/pr-mr-delivery/SKILL.md`.

## Обязательные элементы

- Summary, test plan, риски
- Ссылки на активную папку фичи/hotfix в `docs/backlog/…`
- Ссылки на ADR при архитектурных изменениях
- Для Docker/CI/CD/deploy/infra изменений: deployment notes, rollback/troubleshooting notes и ссылка на обновлённые [DevOps](devops.md) / [CI/CD](../environments/ci-cd.md) документы
- PR/MR создаёт и пушит только **Финализатор**.
- После создания PR/MR ссылка фиксируется отдельным commit и повторным push.
- Локальный worktree удаляется только после повторного push со ссылкой на PR/MR.
- `pr-mr-ready` не означает `accepted`: папка остаётся в `docs/backlog/…` до принятия Стейхолдером или merge.
- Если PR/MR не принят, Оркестратор возвращает работу через `.cursor/skills/pr-mr-rework/SKILL.md`.

## Соседнее

- [Environments: CI/CD](../environments/ci-cd.md)
- [DevOps](devops.md)
