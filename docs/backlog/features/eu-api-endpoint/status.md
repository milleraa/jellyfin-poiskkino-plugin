# Status — `eu-api-endpoint`

Единая точка правды по стадии для Оркестратора. Обновляйте при каждом handoff (см. `.opencode/skills/role-handoff/SKILL.md`).

## Текущее состояние

| Поле | Значение |
|------|----------|
| **Стадия** | `analysis-review` |
| **Owner** | `Orchestrator` / `Stakeholder decision` |
| **Ветка** | `feature/eu-api-endpoint` (локально, не запушена) |
| **PR/MR** | issue [#8](https://github.com/milleraa/jellyfin-poiskkino-plugin/issues/8) — открыт; PR/MR нет |
| **Commit со ссылкой на PR/MR** | _нет_ |
| **Блокеры** | _нет_ |

### Классификация (intake)

- **Тип:** enhancement-фича (новая конфигурационная возможность). Не hotfix: плагин работоспособен, дефекта нет — пользователи вне РФ не могут выбрать зеркало.
- **Постановка Стейхолдера:** «Завести фичу/hotfix под EU-endpoint (настройка base URL или автовыбор)».
- **Первичная фактура Оркестратора** — см. [brief.md](brief.md#фактура-проверено). Кратко:
  - `PoiskKinoApiClient.cs:15` — `private const string ApiBaseUrl = "https://api.poiskkino.dev"`, применяется в конструкторе (`:32`).
  - В актуальном снимке спеки (`api-docs/documentation.yaml`, обновлён 2026-11-27) в `servers` два адреса: `https://api.poiskkino.dev`, `https://eu-api.poiskkino.dev`; `llms.txt` указывает использовать EU-адрес при запросах с серверов за границей, токен и методы те же.
  - `GET https://eu-api.poiskkino.dev/v1.5/token` без ключа → `401` (зеркало живое, аутентификация до роутинга).
  - Все пути плагина относительные (`/v1.5/movie/search`, `/v1.5/movie/{id}`, `/v1.5/season`) — смена base URL их не затрагивает; все `SendAsync` только в `PoiskKinoApiClient.cs`.
  - Конфигурация расширяемая: `PluginConfiguration : BasePluginConfiguration` (`ApiKey`, `IgnoreTmdbImages`), UI — `Configuration/configPage.html`.
- **Смежное решение Стейхолдера:** снапшот спеки обновлён в этой же ветке (коммит `docs(api): обновить снапшот спецификации и завести фичу eu-api-endpoint`), отдельным PR не ведётся.

### Стадии (справочно)

`intake` → `analysis` → `analysis-review` → `backlog-paused` или `architecture` → `tech-decomposition` → `development` → `devops-check` → `tech-lead-review` → `qa` → `rework` → `finalization` → `pr-mr-ready` → `stakeholder-review` → `rework` или `accepted`

`analysis-review` — обязательная остановка после Аналитика: Стейхолдер решает продолжать сейчас или оставить фичу в backlog.

## Задачи реализации

| ID | Файл | Роль / стек | Статус |
|----|------|-------------|--------|
| — | — | — | — |

## DevOps impact check

| Вопрос | Результат |
|--------|-----------|
| Новые env vars / secrets | _нет_ (поле конфигурации плагина, не env) |
| Новые интеграции / порты / очереди / storage / jobs | _нет_ (новый внешний хост `eu-api.poiskkino.dev`, уже объявлен API) |
| Docker / compose / CI/CD / deploy / Helm / K8s / observability | _нет_ |
| DevOps-задача | _не нужна_ |

## Недостаточность разрешений

Заполняет Техлид, если разработчик или DevOps не смог выполнить нужную команду. Это evidence для отдельного reviewed изменения allow-list; ограничение не обходят.

| Роль | Команда / паттерн | Цель | Текст отказа | Минимальный безопасный scope | Решение Оркестратора |
|------|-------------------|------|--------------|------------------------------|----------------------|
| — | — | — | — | — | _ожидается / разрешить отдельным изменением / отклонить_ |

## QA

| Дата | Результат | Ссылка на прогон |
|------|-------------|------------------|
| — | — | [test-runs](../../../qa/test-runs.md) |

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

## Changelog (handoff)

### 2026-11-27 23:30 — intake

- Создана папка фичи из шаблона; в этой же ветке обновлён снапшот `api-docs/documentation.yaml` живой спекой (было 39 путей / 362 КБ → стало 23 пути / 250 КБ: удалены все `/v1.4/*`, deprecated `/v1.5/review` и `/v1/movie/possible-values-by-field`, добавлены `/v1.5/box-office`, `/v1.5/view-option`, в `servers` появился `https://eu-api.poiskkino.dev`).
- В `api-docs/AGENTS.md` добавлен раздел «Снапшот спецификации» (источник, дата, что изменилось).
- Заведён GitHub issue [#8](https://github.com/milleraa/jellyfin-poiskkino-plugin/issues/8) с label `enhancement`.
- Создана ветка `feature/eu-api-endpoint`.

### 2026-11-27 23:30 — handoff

- from: Orchestrator
- to: Analyst
- stage: `analysis`
- artifacts: [brief.md](brief.md), [analysis.md](analysis.md)
- notes: тип работы — enhancement-фича, Аналитик обязателен по процессу. В `brief.md` четыре открытых вопроса (OQ-1…OQ-4): UI свободное поле vs. выбор, нужен ли failover, фича vs. hotfix, миграция конфигурации.

### 2026-11-27 — handoff

- from: Analyst
- to: Orchestrator (gate `analysis-review`, решение — Stakeholder)
- stage: `analysis-review`
- artifacts: [analysis.md](analysis.md) (user stories, AC-1…AC-12, NFR, out of scope, зависимости, риски R1–R9, ответы OQ-1…OQ-4, вопросы Q1–Q3, решения D-1…D-8), [brief.md](brief.md) (уточнена строка про llms.txt)
- notes: рекомендация `continue`. OQ-1 — свободное поле URL с серверной нормализацией и подсказкой (допустим `<datalist>` с двумя хостами); OQ-2 — failover подтверждён как out of scope; OQ-3 — enhancement подтверждён; OQ-4 — миграция не нужна: `BasePluginConfiguration` пуст, конфигурация читается `XmlSerializer.DeserializeFromFile` в один XML, отсутствующий элемент оставляет инициализатор-дефолт (evidence: исходники Jellyfin `BasePluginOfT.cs`/`MyXmlSerializer.cs`), остаточный smoke — AC-8. Реальные риски: R1 — base URL привязан к конструктору клиента, поэтому нужен AC-7 «применение без перезагрузки» (альтернатива AC-7′ — по решению Q2); R2 — нет реального API-ключа, end-to-end smoke EU-зеркала недоступен (только пробы 401 + unit-тесты); R6 — формулировка llms.txt о EU-зеркале не воспроизводится, опора на `servers` спеки; R3 — семантика лимита 200/сутки между хостами неизвестна (информационно через `/v1.5/token`). Вопросы Стейхолдеру: Q1 (свободный хост vs whitelist двух хостов), Q2 (применение без перезагрузки), Q3 (показывать ли `/v1.5/token` — рекомендовано вне scope). Блокирующих вопросов нет.
