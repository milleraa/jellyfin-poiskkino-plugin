---
name: project-bootstrap
description: Инициализирует пустой или скопированный шаблон проекта: полный проход по списку файлов в docs/, пакеты вопросов Стейхолдеру, заполнение product/environments/engineering/архитектуры/QA без «немых» TBD. Используй при новом репозитории, пустых placeholder в docs/product или docs/environments, отсутствии осмысленного backlog после копирования шаблона.
disable-model-invocation: true
---

# Project bootstrap

## Когда применять

Признаки «пустого» проекта:

- В `docs/product/vision.md` или `docs/environments/git-remotes.md` остались `TODO` / `_TBD_` из шаблона.
- Не зафиксированы активные стеки в `docs/engineering/stack-notes.md` (или не помечены неиспользуемые стеки).
- Нет осознанной первой фичи/hotfix в `docs/backlog/features/` / `hotfixes/` (опционально после прохода манифеста).

## Критерий готовности (DoBootstrap)

Инициализация **завершена**, когда для **каждого** файла из раздела [Полный манифест `docs/`](#полный-манифест-docs) выполнено одно из:

1. **Заполнено** согласованными с Стейхолдером фактами (без выдуманных URL и секретов).
2. **Явно зафиксировано отсутствие в v1** формулировкой вроде `Не используется в v1`, `N/A`, `Нет UI — E2E не планируется` — одной строкой в начале раздела или файла, без «немых» `_TBD_` без контекста.
3. **Шаблон процесса** из раздела «Не изменять» — только проверка, что ссылки открываются; содержание не трогаем.

Запрещено оставлять общие `TODO (bootstrap)` без замены на факт или явный `N/A` в файлах манифеста блоков 1–5.

## Полный манифест `docs/`

Оркестратор **последовательно** открывает каждый путь ниже и приводит файл к состоянию DoBootstrap. Порядок блоков совпадает с пакетами вопросов.

### Блок 1 — Продукт (`docs/product/`)

| Файл | Обязательный результат |
|------|-------------------------|
| [`docs/product/vision.md`](../../../docs/product/vision.md) | Название продукта, проблема, ценность, тип (`cli` / `webapi` / `spa` / `client-server` / другое). Убрать или заменить шаблонный `TODO (bootstrap)`. |
| [`docs/product/stakeholders.md`](../../../docs/product/stakeholders.md) | Таблица ролей заполнена (хотя бы Стейхолдер и владелец продукта); без персональных данных в публичном репо — роли и каналы. |
| [`docs/product/requirements.md`](../../../docs/product/requirements.md) | Разделы функциональные / NFR / вне scope: либо пункты, либо явное «в v1 нет глобальных NFR — детализация в фичах». |

### Блок 2 — Окружения и доступ (`docs/environments/`)

| Файл | Обязательный результат |
|------|-------------------------|
| [`docs/environments/git-remotes.md`](../../../docs/environments/git-remotes.md) | Hosting, URL (HTTPS/SSH), default/develop, protected branches, target для PR/MR фич vs hotfix. |
| [`docs/environments/project-access.md`](../../../docs/environments/project-access.md) | Где Git, группа/namespace, кто выдаёт доступ; политика 2FA/guest — факт или `N/A` + кто решает. |
| [`docs/environments/ci-cd.md`](../../../docs/environments/ci-cd.md) | Где pipeline, имя/путь CI config, обязательные checks, артефакты, логи, rerun, эскалация. |
| [`docs/environments/deployment-targets.md`](../../../docs/environments/deployment-targets.md) | Таблица dev/stage/prod: URL или `N/A`, кто деплоит, примечание. Если окружений нет — одна строка «окружений нет, только локально». |
| [`docs/environments/secrets-policy.md`](../../../docs/environments/secrets-policy.md) | Если шаблон достаточен — добавь в конец строку: `Подтверждено при bootstrap: без хранения секретов в репо.` Имя ответственного за выдачу секретов (роль/команда). |

Файл [`docs/environments/environment-template.md`](../../../docs/environments/environment-template.md) — **справочник**, не требует заполнения; при необходимости скопировать блок в `deployment-targets.md`.

[`docs/environments/README.md`](../../../docs/environments/README.md) — проверить, что список ссылок актуален после правок (без обязательного переписывания).

### Блок 3 — Инженерия: стек и DevOps (`docs/engineering/`)

| Файл | Обязательный результат |
|------|-------------------------|
| [`docs/engineering/stack-notes.md`](../../../docs/engineering/stack-notes.md) | Явный список **активных в v1** стеков репозитория и краткая пометка монорепо vs один стек. |
| [`docs/engineering/stacks/dotnet.md`](../../../docs/engineering/stacks/dotnet.md) | Если .NET **не** в v1 — сразу после заголовка: `**Статус:** не используется в v1.` Если **в** v1 — заполнить минимум: версия SDK / TFM, команда сборки и тестов (или ссылка на скрипт в репо после появления). |
| [`docs/engineering/stacks/go.md`](../../../docs/engineering/stacks/go.md) | Аналогично .NET. |
| [`docs/engineering/stacks/typescript.md`](../../../docs/engineering/stacks/typescript.md) | Аналогично. |
| [`docs/engineering/stacks/react.md`](../../../docs/engineering/stacks/react.md) | Аналогично. |
| [`docs/engineering/stacks/postgresql.md`](../../../docs/engineering/stacks/postgresql.md) | Аналогично (версия, миграции, политика локального доступа или `N/A`). |
| [`docs/engineering/stacks/README.md`](../../../docs/engineering/stacks/README.md) | Проверить согласованность ссылок; при добавлении нового стека — обновить (иначе не трогать). |
| [`docs/engineering/devops.md`](../../../docs/engineering/devops.md) | Снять «немые» TODO: Docker/compose (да/нет/N/A), путь CI config, обязательные jobs, deploy flow, smoke — факт или явное `N/A` с причиной. Согласовать с блоком 2. |

Файлы [`docs/engineering/branching.md`](../../../docs/engineering/branching.md), [`docs/engineering/delivery.md`](../../../docs/engineering/delivery.md), [`docs/engineering/coding-standards.md`](../../../docs/engineering/coding-standards.md), [`docs/engineering/testing.md`](../../../docs/engineering/testing.md), [`docs/engineering/README.md`](../../../docs/engineering/README.md) — **не переписывать** при bootstrap; только при несовпадении имён веток с `git-remotes.md` поправить перекрёстные формулировки.

### Блок 4 — Архитектура и бэклог верхнего уровня

| Файл | Обязательный результат |
|------|-------------------------|
| [`docs/architecture/overview.md`](../../../docs/architecture/overview.md) | Заменить заглушку `[Client] → [API] → [DB]` на актуальную для v1 схему (даже текстом в 3–5 строках) или явное «монолит / один сервис» + внешние интеграции `N/A`. |
| [`docs/backlog/epics.md`](../../../docs/backlog/epics.md) | Либо первая строка таблицы с реальным epic и ссылкой на папку фичи, либо явно: «Epics не используются — фичи без epic до появления дорожной карты». |

[`docs/architecture/principles.md`](../../../docs/architecture/principles.md) — **не обязательно менять** (общие принципы); при отклонении от шаблона — одна строка в конце.

Каталоги [`docs/architecture/decisions/`](../../../docs/architecture/decisions/), [`docs/architecture/diagrams/`](../../../docs/architecture/diagrams/) — без ADR не требуют новых файлов; первый ADR — по первой архитектурной фиче.

### Блок 5 — QA (`docs/qa/`)

| Файл | Обязательный результат |
|------|-------------------------|
| [`docs/qa/test-strategy.md`](../../../docs/qa/test-strategy.md) | Уровни unit/integration/e2e: что обязательно в CI, что ручное; критерии для PR. |
| [`docs/qa/playwright.md`](../../../docs/qa/playwright.md) | Если нет UI / нет E2E в v1 — в начале: `**Статус:** E2E не планируется в v1.` Если планируется — путь к тестам, команда запуска, CI job (или «после появления репо — TBD с датой/условием» **нельзя**; только факт или `N/A`). |
| [`docs/qa/test-runs.md`](../../../docs/qa/test-runs.md) | Либо одна строка журнала с датой bootstrap и пометкой `docs-only`, либо явная строка в примечании таблицы: «Прогоны с первой фичи». |

[`docs/qa/README.md`](../../../docs/qa/README.md) — проверка ссылок при необходимости.

### Не изменять при bootstrap (шаблон процесса и артефакты фич)

Только убедись, что файлы на месте:

- `docs/process/**`, `docs/backlog/README.md`, `docs/backlog/task-template.md`
- `docs/backlog/features/feature-template/**`, `docs/backlog/hotfixes/hotfix-template/**`
- `docs/tasks/**`, `docs/archive/**`, `docs/history/completed-work.md`, `docs/history/decisions-log.md`

Их **не** наполняют проектными фактами на этапе init (кроме осознанного создания первой фичи — см. ниже).

## Пакеты вопросов Стейхолдеру (связь с манифестом)

Задавай **пакетами**; после каждого пакета — сразу обновляй соответствующие файлы манифеста.

**Пакет A — продукт** → блок 1 (`vision`, `stakeholders`, `requirements`).

**Пакет B — стек** → `stack-notes.md` + каждый `stacks/*.md` (активен / не в v1).

**Пакет C — Git и приёмка** → `git-remotes.md`, `project-access.md`, при необходимости уточнение в `branching` не трогать, только `git-remotes`.

**Пакет D — CI/CD и окружения** → `ci-cd.md`, `deployment-targets.md`, `secrets-policy` (подтверждение), `devops.md`.

**Пакет E — архитектура и тестирование v1** → `architecture/overview.md`, `qa/test-strategy.md`, `qa/playwright.md`, `qa/test-runs.md`.

**Пакет F — бэклог верхнего уровня** → `backlog/epics.md` + решение: создать первую папку фичи/hotfix из шаблона или явно «первая фича позже».

## Порядок работы Оркестратора

1. Кратко подтверди цель: **только init** или init + первая фича/hotfix.
2. Пройди манифест **по блокам 1→5**; в каждом файле добейся состояния DoBootstrap.
3. Обнови [`docs/README.md`](../../../docs/README.md), только если появились новые якорные ссылки или имя продукта вводится в оглавление (не обязательно).
4. Если Стейхолдер выбрал первую работу — создай `docs/backlog/features/<name>/` или `hotfixes/<name>/` из шаблона, `status.md` в `intake`, и при необходимости строку в `epics.md` со ссылкой.
5. Зафиксируй в changelog handoff в первой фиче или в `docs/history/decisions-log.md` одну строку «bootstrap завершён YYYY-MM-DD» (опционально).
6. Локальный commit без push, если менялись файлы.

## Запреты

- Не коммить секреты, токены, connection strings с паролями.
- Не начинать прикладной код продукта до явного старта фичи после bootstrap (допустимы только правки `docs/` и инфраструктурных шаблонов по согласованию).
