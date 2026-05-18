# Definition of Done (DoD)

## Ready for stakeholder review

Работа считается готовой к review Стейхолдера, если:

- [ ] Все задачи в `tasks/` закрыты или отменены с причиной.
- [ ] Работа велась в одном feature/hotfix worktree, задачи выполнялись последовательно.
- [ ] Все ролевые шаги с изменениями зафиксированы локальными commit без push.
- [ ] **Техлид** принял реализацию до передачи в QA.
- [ ] **Техлид** выполнил DevOps impact check после разработки.
- [ ] Если появились новые env vars, secrets, integrations, ports, migrations или изменялись Docker/CI/CD/deploy/infra файлы, **DevOps** проверил impact и обновил `docs/engineering/devops.md` / `docs/environments/*`.
- [ ] **QA** дала вердикт (pass / список дефектов закрыт).
- [ ] Тесты и проверки зафиксированы в `qa.md` и [test-runs](../qa/test-runs.md).
- [ ] **Финализатор** выполнил push, открыл **PR/MR**, записал ссылку в `status.md` / `finalization.md`, сделал отдельный commit с этой ссылкой и выполнил повторный push.
- [ ] Локальный worktree удалён после повторного push; branch/remote branch сохранены до merge/close PR/MR.
- [ ] Документация и ADR обновлены при изменении поведения или архитектуры.
- [ ] Папка feature/hotfix остаётся в `docs/backlog/...` до принятия Стейхолдером или merge PR/MR.

## Accepted / Done

Работа считается завершённой для команды агентов, если:

- [ ] Стейхолдер принял PR/MR или PR/MR merged по политике проекта.
- [ ] Если PR/MR не принят, Оркестратор вернул работу в `rework` через `.cursor/skills/pr-mr-rework/SKILL.md`.
- [ ] Финализатор перенёс принятую папку в [archive](../archive/README.md) и добавил запись в [completed-work](../history/completed-work.md).

Приёмка **слиянием** PR/MR остаётся за Стейхолдером или его политикой.
