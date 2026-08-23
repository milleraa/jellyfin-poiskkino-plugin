# Learning from rejected PR/MR

Этот каталог содержит **документальный и reviewable** контур улучшения OpenCode workflow. Он не заменяет и не задерживает обычный rework.

## Модель v1

1. `/reject-feature` сначала выполняет `.opencode/skills/pr-mr-rework/SKILL.md` полностью: evidence, `status.md`, stage `rework`, worktree recovery и маршрутизация роли.
2. Затем Оркестратор добавляет нормализованную ссылочную запись в [reject log](reject-log.md).
3. Один уникальный reject получает `not-triggered`; обычный rework продолжается.
4. `process-review.md` создаётся как `candidate` лишь при двух или более уникальных сопоставимых evidence-linked cases без suppression.
5. Candidate не является стадией исходной feature/hotfix и не блокирует её rework, QA, finalization либо PR/MR.
6. Proposal не применяет изменения автоматически. Одобренное изменение создаётся отдельной process-change feature, branch/worktree и reviewed PR/MR.

## Evidence и class key

Допустимая evidence — ссылка на PR/MR comment, failed check или сообщение Стейкхолдера, связанная с `status.md` конкретного case. Не копируйте secrets, PII, полный feedback или raw CI logs.

Сопоставимость v1 требует точного совпадения:

```text
class_key = affected_role + violated_contract_or_gate + classification
```

`classification` — только `implementation` или `process`. Cases должны быть разными feature/hotfix либо независимыми документированными delivery attempts. Одна evidence-ссылка учитывается только один раз.

## Triage

| Result | Когда использовать | Последствие |
|---|---|---|
| `not-triggered` | Меньше двух unique comparable evidence-linked cases | Обычный rework продолжается |
| `suppressed` | Недостаточный/противоречивый feedback, duplicate, временный внешний CI/provider incident, единичная субъективная правка, другой class key, explicit stakeholder opt-out или unsafe hypothesis | Зафиксировать причину и evidence; rework продолжается |
| `candidate` | Есть минимум два comparable cases с одинаковым class key и без suppression | Создать `process-review.md`; rework продолжается |

При сомнении выбирайте `suppressed` или `not-triggered`, а не candidate. Исправляйте журнал новой датированной записью либо `superseded`, не переписыванием истории.

## Proposal и regression gate

Шаблон proposal живёт в feature/hotfix templates как `process-review.md`. Он обязан включать evidence, falsifiable root-cause hypothesis, proposal, affected files/roles, сохранённые safety constraints и regression set.

Без regression evidence verdict — `inconclusive`, recommendation — `do not change process`. Только verdict `quality improvement demonstrated` без safety regression может рекомендовать **отдельный** human-reviewed process-change PR/MR.
