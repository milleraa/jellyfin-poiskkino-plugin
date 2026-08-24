# Agile workflow

## Типы работ

| Тип | Аналитик | Далее |
|-----|----------|--------|
| Новая фича | Обязателен | Аналитик → **Стейхолдер решает продолжать или оставить в backlog** → Архитектор → Техлид → Разработчики/DevOps → … |
| Bugfix с ясной постановкой | Опционален | Оркестратор → Архитектор (или Техлид при тривиальности) → Разработчики/DevOps → … |
| Hotfix | Как bugfix | Папка `docs/backlog/hotfixes/<name>/`, ускоренный поток в `status.md` |

## Цепочка

1. **Оркестратор** — intake, классификация, bootstrap пустого проекта.
2. **Оркестратор** — создаёт рабочую ветку для feature/hotfix и фиксирует её в `status.md`.
3. **Аналитик** (фичи) — `analysis.md`, вопросы Стейхолдеру.
4. **Оркестратор → Стейхолдер** — обязательная остановка `analysis-review`: продолжать сейчас или оставить фичу в backlog (`backlog-paused`).
5. **Архитектор** — только после подтверждения продолжения; `architecture.md`, ADR при необходимости; может вернуть к Аналитику.
6. **Оркестратор → Техлид** — `tasks/*.md`, декомпозиция.
7. **Техлид ↔ Разработчик / DevOps** (внутренний цикл) — последовательные `task(developer-*)` / `task(devops)` по `tasks/NNN-*.md`, приёмка Техлидом, DevOps impact check.
8. **Техлид → Оркестратор** — отчёт после accept и закрытого DevOps-check.
9. **Оркестратор → QA** — Playwright по стратегии после handoff от Техлида.
10. **QA → Оркестратор** — вердикт pass или fail (при fail — Оркестратор → Техлид → rework).
11. **Оркестратор → Финализатор** — push, PR/MR, стадия `pr-mr-ready`.
12. **Стейхолдер** — review PR/MR: решение сначала сообщается агенту (`/accept-feature` или `/reject-feature`), **merge в системе управления кодом пока не выполняется**.
13. **Оркестратор → Финализатор** (после accept) — archive в [archive](../archive/README.md) и история: коммиты делаются в feature/hotfix ветке и пушатся в неё.
14. **Merge PR/MR** — после подтверждения архивации; merge выполняет Стейхолдер или агент. Все изменения попадают в main одним мержем.

Каждая роль после своего шага делает локальный commit без push, если меняла файлы. Push и PR/MR выполняет только Финализатор.

После `analysis-review` и подтверждения продолжения happy path идёт без плановых остановок до `pr-mr-ready`. Новые остановки допустимы только при блокере, эскалации к Стейхолдеру или rejected PR/MR.

## Делегирование через подзадачи

Оркестратор не исполняет все роли одним агентом. Каждый переход к роли означает отдельный вызов подзадачи/субагента с соответствующим ролевым промптом из `.opencode/agents/`.

Порядок на каждом шаге:

1. Оркестратор выбирает следующую роль по текущей стадии и `status.md`.
2. Оркестратор вызывает субагента (Task tool), передав feature/hotfix папку, `status.md`, релевантные артефакты и конкретный ожидаемый результат.
3. Роль выполняет свой шаг, обновляет артефакты, делает локальный commit без push, если меняла файлы, и возвращает отчёт.
4. Оркестратор принимает результат, обновляет handoff в `status.md` и только затем запускает следующую роль или эскалирует блокер Стейхолдеру.
5. Исключение hub: **Техлид** вызывает разработчиков и DevOps без возврата к Оркестратору между задачами; все остальные переходы — через Оркестратора.
6. После Аналитика по новой фиче Оркестратор всегда останавливается на `analysis-review` и ждёт решения Стейхолдера перед Архитектором.
7. Если разработчику или DevOps недостаёт разрешённой команды, он возвращает Техлиду точную команду, цель и текст отказа. Техлид фиксирует это в разделе «Недостаточность разрешений» текущего `status.md` и эскалирует Оркестратору; обход permission checks запрещён. Оркестратор периодически агрегирует evidence и меняет allow-list только отдельным reviewed изменением процесса.

## Happy path (новая фича)

Стейхолдер → Оркестратор → Аналитик → Оркестратор → gate `analysis-review` → Архитектор → Оркестратор → Техлид → (Разработчик → Техлид)\* → (DevOps → Техлид)? → Оркестратор → QA → Оркестратор → Финализатор → Стейхолдер (`pr-mr-ready`, merge отложен) → решение агенту (`/accept-feature`) → Финализатор (archive в ветке + push) → merge PR/MR одним мержем.

## Возврат PR/MR в работу

`pr-mr-ready` не означает `accepted`. Если Стейхолдер не принимает PR/MR, Оркестратор применяет `.opencode/skills/pr-mr-rework/SKILL.md`, сохраняет feedback в `status.md`, переключается на существующую feature/hotfix ветку и возвращает работу на нужную роль.

Цикл: `pr-mr-ready` → `rework` → нужная роль → `finalization` → `pr-mr-ready`. В архив переносится только принятая или merged работа.

### Неблокирующее обучение по reject

После полного `pr-mr-rework` Оркестратор может выполнить evidence-linked triage по [process learning](learning/README.md). Он фиксируется в case-level `status.md` и агрегированном [reject log](learning/reject-log.md), но не является новой стадией исходной feature/hotfix и не блокирует обычный rework.

- Один unique reject: `not-triggered`; только обычный rework.
- `candidate`: минимум два unique comparable evidence-linked cases из разных cases/independent attempts с идентичным `affected role + violated contract/gate + classification` и без suppression.
- `suppressed`: insufficient/duplicate/noisy/external evidence, другой class key, opt-out Стейкхолдера или unsafe hypothesis; причина и evidence обязательны.
- Candidate создаёт reviewable `process-review.md`, а не автоматическое изменение workflow.
- Только regression verdict `quality improvement demonstrated` без safety regression может рекомендовать отдельную process-change feature branch и reviewed PR/MR. Исходный rework от этого PR не зависит.

## Где смотреть статус

В папке фичи/hotfix: **`status.md`** — текущая стадия и owner.

## Навыки OpenCode

- Bootstrap: `.opencode/skills/project-bootstrap/SKILL.md`
- Handoff: `.opencode/skills/role-handoff/SKILL.md`
- PR/MR: `.opencode/skills/pr-mr-delivery/SKILL.md`
- Rework PR/MR: `.opencode/skills/pr-mr-rework/SKILL.md`
- Финализация: `.opencode/skills/finalization-check/SKILL.md`
