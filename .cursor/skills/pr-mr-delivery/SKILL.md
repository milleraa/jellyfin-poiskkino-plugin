---
name: pr-mr-delivery
description: Готовит описание и чеклист для PR/MR: summary, test plan, ссылки на фичу/ADR/DevOps notes, риски, статус CI. Используй перед открытием PR/MR или когда Финализатор/Техлид оформляет поставку Стейхолдеру.
disable-model-invocation: true
---

# PR/MR delivery

## Шаблон описания PR/MR

Скопируй в GitLab/Gitea:

```markdown
## Summary
- ...

## Test plan
- [ ] unit
- [ ] integration
- [ ] e2e / manual steps: ...

## Links
- Feature folder: docs/backlog/features/... или docs/backlog/hotfixes/...
- Tasks: ...
- ADR: ...

## Risks / rollout
- ...
- Deployment / rollback notes: ...

## CI
- Pipeline: ...
```

## Действия

1. Убедись, что ветка соответствует `docs/engineering/branching.md`.
2. Убедись, что работа велась в одном feature/hotfix worktree и все ролевые шаги зафиксированы локальными commit.
3. Проверь локально или через CI минимум из test plan.
4. Проверь clean tree перед push; если есть финальные изменения — сделай локальный commit.
5. Выполни push feature/hotfix ветки и создай/оформи PR/MR.
6. Если менялись Docker/CI/CD/deploy/infra — добавь ссылки на `docs/engineering/devops.md` и `docs/environments/ci-cd.md`.
7. Заполни `finalization.md` и `status.md` ссылкой на PR/MR.
8. Сделай отдельный commit со ссылкой на PR/MR и выполни повторный push.
9. После успешного повторного push удали локальный `git worktree`; ветку и remote branch не удаляй до merge/close PR/MR.
10. Обнови `status.md` стадия `pr-mr-ready`.
11. Не переноси папку feature/hotfix в `docs/archive/` до принятия Стейхолдером или merge PR/MR.

## Запреты

- Не включай секреты и токены в описание.
- Не выполняй push из ролей кроме Финализатора.
- Не удаляй branch/remote branch при удалении локального worktree.
