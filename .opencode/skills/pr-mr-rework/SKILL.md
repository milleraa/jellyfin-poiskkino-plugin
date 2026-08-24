---
name: pr-mr-rework
description: Возвращает непринятый PR/MR в работу: собирает feedback, переключается на существующую ветку и маршрутизирует доработку нужной роли. Используй когда Стейхолдер сообщил, что PR/MR не принят.
---

# PR/MR rework

## Когда использовать

Используй, если Стейхолдер сообщил, что PR/MR не принят, попросил доработку или оставил blocking comments в PR/MR.

## Входы

- Ссылка на PR/MR.
- Причины отказа из чата Стейхолдера.
- Комментарии PR/MR и CI/checks, если доступ к GitLab/Gitea/GitHub доступен.
- Папка feature/hotfix в `docs/backlog/...` и её `status.md`.

## Действия Оркестратора

1. Зафиксируй ссылку на PR/MR и источник feedback в `status.md`.
2. Собери причины отказа:
   - из сообщения Стейхолдера;
   - из комментариев PR/MR;
   - из failed checks / pipeline, если отказ связан с CI.
3. Переведи `status.md` в стадию `rework`, owner `Orchestrator`.
4. Переключись на существующую feature/hotfix ветку. Новую ветку не создавай.
5. Классифицируй причину и назначь следующую роль:
   - требования или ожидаемое поведение не совпали — **Аналитик**;
   - архитектурное решение не принято — **Архитектор**;
   - дефект реализации, тестов или регрессия — **Техлид**;
   - CI/CD, deploy, env vars, secrets, инфраструктура — **Техлид → DevOps**;
   - flaky/e2e проблема — **QA** или **Техлид**, если нужен кодовый фикс.
6. Добавь или обнови задачи в `tasks/` так, чтобы доработка шла последовательно в одной ветке.
7. Перед handoff сделай локальный commit без push, если менял файлы.

## Process learning triage (после обязательного rework)

Выполняй этот triage **только после** шагов 1–7 выше. Он не является новой стадией исходной feature/hotfix, не задерживает rework, QA, finalization или обновление исходного PR/MR.

1. Добавь в `status.md` нормализованные поля: `Learning triage`, `Class key`, `Learning evidence`, `Suppression reason`, `Process review`.
2. Запиши краткую ссылочную строку в `docs/process/learning/reject-log.md`. Не копируй secrets, PII, полный feedback или raw CI logs.
3. Class key: `affected_role + violated_contract_or_gate + classification`; `classification` — `implementation` либо `process`.
4. Один unique evidence-linked reject получает `not-triggered`: process-review не создаётся, а обычный rework продолжается.
5. Поставь `suppressed` с причиной и evidence, если feedback недостаточен/противоречив, evidence — дубль, incident временный и внешний, правка единичная и субъективная, class key различается, Стейкхолдер сделал opt-out или гипотеза ослабляет safety/gate. Suppression не отменяет rework.
6. `candidate` допустим только при минимум двух unique comparable evidence-linked cases из разных feature/hotfix либо независимых delivery attempts с идентичным class key и без suppression. Тогда создай case-level `process-review.md` по template.
7. Candidate — только proposal: никакие commands, skills, agents, rules или workflow docs не меняй автоматически. Без regression evidence verdict proposal — `inconclusive` и `do not change process`. При `quality improvement demonstrated` создаётся отдельная process-change feature branch и human-reviewed PR/MR.

Полные правила и журнал: [process learning](../../../docs/process/learning/README.md).

## После доработки

- Дальше процесс идёт обычным циклом: Техлид → DevOps impact check → QA → Финализатор.
- Финализатор обновляет существующий PR/MR, делает push, фиксирует rework notes / ссылку на PR/MR отдельным commit и выполняет повторный push.
- Не архивируй feature/hotfix до принятия Стейхолдером или merge PR/MR.

## Запреты

- Не создавай новую ветку для rework, если исходная feature/hotfix ветка существует.
- Не закрывай и не пересоздавай PR/MR без явного решения Стейхолдера.
- Не удаляй branch/remote branch до merge/close PR/MR.
