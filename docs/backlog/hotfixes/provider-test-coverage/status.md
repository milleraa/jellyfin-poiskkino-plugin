# Status — `provider-test-coverage`

| Поле | Значение |
|------|----------|
| **Стадия** | `tech-decomposition` |
| **Owner** | `Tech Lead` |
| **Срочность** | Hotfix по запросу Стейкхолдера; P-приоритет не задан |
| **Ветка** | `hotfix/provider-test-coverage` от `main` (`b08b586`) |
| **PR/MR** | Не открыт |
| **Commit со ссылкой на PR/MR** | — |

## Стадии (справочно)

`intake` → `root-cause` → `architecture` → `tech-decomposition` → `development` → `devops-check` → `tech-lead-review` → `qa` → `rework` → `finalization` → `pr-mr-ready` → `stakeholder-review` → `rework` или `accepted`

## Задачи реализации

| ID | Файл | Роль / стек | Статус |
|----|------|-------------|--------|
| — | — | — | — |

## DevOps impact check

| Вопрос | Результат |
|--------|-----------|
| Новые env vars / secrets | _нет / да_ |
| Новые интеграции / порты / очереди / storage / jobs | _нет / да_ |
| CI / упаковка релиза / установка / диагностика | _нет / да_ |
| DevOps-задача | _не нужна / ссылка_ |

## Недостаточность разрешений

Заполняет Техлид, если разработчик или DevOps не смог выполнить нужную команду. Это запись о блокере для Оркестратора; ограничение не обходят.

| Роль | Команда / паттерн | Цель | Текст отказа | Минимальный безопасный scope | Решение Оркестратора |
|------|-------------------|------|--------------|------------------------------|----------------------|
| — | — | — | — | — | _ожидается / разрешить отдельным изменением / отклонить_ |

## PR/MR feedback / rework

| Поле | Значение |
|------|----------|
| **Feedback source** | _чат / PR/MR comments / CI_ |
| **Feedback summary** | _нет_ |
| **Rework route** | _Analyst / Architect / Tech Lead / DevOps / QA_ |
| **Переключено на существующую ветку** | _нет / да_ |
| **Rework tasks** | _ссылки на tasks/_ |

## Process learning (неблокирующий triage)

| Поле | Значение |
|------|----------|
| **Learning triage** | _not-triggered / suppressed / candidate_ |
| **Class key** | _affected role + violated contract/gate + classification_ |
| **Learning evidence** | _ссылки на PR/MR feedback / CI / сообщение Стейкхолдера_ |
| **Suppression reason** | _нет / причина и evidence_ |
| **Process review** | _нет / ссылка на process-review.md_ |

Один unique reject получает `not-triggered` и не блокирует rework. `candidate` возможен только при двух или более unique comparable evidence-linked cases; изменения процесса выполняются только отдельным reviewed PR/MR. Правила: [process learning](../../../process/learning/README.md).

## Changelog

### 2026-11-28 — intake

- Результаты аудита и критерии приёмки записаны в [incident.md](incident.md). Реализация и QA не начаты.
- Следующий шаг: передать Техлиду для `tech-decomposition`. Отдельный архитектурный этап не нужен: меняются только тесты. Входы: этот статус, [incident.md](incident.md) и навык `test-audit`. Ожидаются план, задачи, ревью и DevOps impact check перед QA.

## 2026-11-28 11:42 — handoff
- from: Orchestrator
- to: Tech Lead
- stage: `tech-decomposition`
- artifacts: [incident.md](incident.md), [tech-plan.md](tech-plan.md)
- notes: Стейкхолдер поручил автономную реализацию с субагентами. Изменения только в тестах; отдельный архитектурный этап не требуется.
