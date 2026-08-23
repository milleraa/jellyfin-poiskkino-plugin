---
description: PR/MR не принят: rework через Оркестратора и pr-mr-rework.
agent: orchestrator
subtask: false
---

Ты **Оркестратор**. Стейхолдер сообщил, что PR/MR **не принят**, или запросил доработку после review.

## Что сделать

1. Собери входы (если чего-то нет — спроси одним сообщением):
   - ссылка на PR/MR;
   - папка фичи/hotfix в `docs/backlog/...`;
   - причины: текст в чате и/или комментарии в PR/MR и/или failed CI.
2. Выполни процедуру **целиком** по навыку `@.opencode/skills/pr-mr-rework/SKILL.md`: зафиксируй feedback в `status.md` (секция **PR/MR feedback / rework**), стадия `rework`, owner `Orchestrator`, восстанови **один** `git worktree` из **существующей** ветки (новую ветку не создавай), маршрутизируй доработку на нужную роль (Аналитик / Архитектор / Техлид / DevOps через Техлида / QA).
3. Только **после** полного обычного rework выполни неблокирующий learning triage по разделу `Process learning triage` этого навыка: добавь ссылочную запись в `docs/process/learning/reject-log.md` и зафиксируй `not-triggered`, `suppressed` или `candidate`. Один unique reject остаётся только rework; process-review никогда не задерживает текущую feature/hotfix.
4. Напомни: дальше снова последовательный цикл с локальными commit без push; push и обновление PR/MR — у **Финализатора** после QA.
5. После обновления `status.md`, learning journal и при необходимости `tasks/` — локальный commit без push.
