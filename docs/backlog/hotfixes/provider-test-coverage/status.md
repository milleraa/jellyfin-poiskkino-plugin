# Status — `provider-test-coverage`

| Поле | Значение |
|------|----------|
| **Стадия** | `development` |
| **Owner** | `developer-csharp` |
| **Блокер** | Нет: Оркестратор вызывает исполнителей напрямую по регламенту `4cbe4d4` |
| **Срочность** | Hotfix по запросу Стейкхолдера; P-приоритет не задан |
| **Ветка** | `hotfix/provider-test-coverage` от `main` (`b08b586`) |
| **PR/MR** | Не открыт |
| **Commit со ссылкой на PR/MR** | — |

## Стадии (справочно)

`intake` → `root-cause` → `architecture` → `tech-decomposition` → `development` → `devops-check` → `tech-lead-review` → `qa` → `rework` → `finalization` → `pr-mr-ready` → `stakeholder-review` → `rework` или `accepted`

## Задачи реализации

| ID | Файл | Роль / стек | Статус |
|----|------|-------------|--------|
| 001 | [Поиск сериалов по году](tasks/001-series-search-year.md) | `developer-csharp` / C# · .NET | `in-progress` |
| 002 | [Роли персон](tasks/002-metadata-person-roles.md) | `developer-csharp` / C# · .NET | `draft` — после приёмки 001 |

## DevOps impact check

| Вопрос | Результат |
|--------|-----------|
| Новые env vars / secrets | По плану нет: локальные подмены, ключ реального сервиса не нужен |
| Новые интеграции / порты / очереди / storage / jobs | По плану нет: без сетевых вызовов и новой инфраструктуры |
| CI / упаковка релиза / установка / диагностика | По плану нет: существующий тестовый проект уже запускается в CI; настройки запуска, сборки релиза, установки и диагностики не меняются |
| DevOps-задача | По плану не нужна. Итоговая проверка по diff отложена до получения реализации; гейт не закрыт |

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

## 2026-11-28 11:44 — handoff
- from: Tech Lead
- to: Orchestrator
- stage: `tech-decomposition` (`blocked`)
- artifacts: [tech-plan.md](tech-plan.md), [001](tasks/001-series-search-year.md), [002](tasks/002-metadata-person-roles.md), [incident.md](incident.md)
- notes: Декомпозиция завершена. Поиск инструментов `subagent` и делегирования не обнаружил запуска вложенных ролей; `agent_send` только отправляет сообщение родителю. Это ограничение доступности роли, не отказ в конкретной команде разработчика. Тесты не изменены, новых прогонов и приёмки не было. QA не запускать.

## 2026-11-28 11:50 — handoff
- from: Orchestrator
- to: developer-csharp
- stage: `development`
- artifacts: [план](tech-plan.md), [001](tasks/001-series-search-year.md)
- notes: Блокер снят по обновлённому SDLC. Задача 002 — после ревью 001.
