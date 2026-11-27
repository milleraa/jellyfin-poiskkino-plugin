# Status — `eu-api-endpoint`

Единая точка правды по стадии для Оркестратора. Обновляйте при каждом handoff (см. `.opencode/skills/role-handoff/SKILL.md`).

## Текущее состояние

| Поле | Значение |
|------|----------|
| **Стадия** | `development` |
| **Owner** | `Orchestrator` |
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
| 001 | [tasks/001-api-base-url-resolver.md](tasks/001-api-base-url-resolver.md) — `ApiBaseUrlResolver` + unit-тесты (AC-1, AC-3, AC-4) | `developer-csharp` / C# + xUnit | `done` (`365d395`) |
| 002 | [tasks/002-configuration-client-providers.md](tasks/002-configuration-client-providers.md) — `PluginConfiguration.ApiBaseUrl`, клиент, 5 провайдеров, тесты фактического хоста (AC-1, AC-2, AC-4, AC-5, AC-8, AC-9, AC-10, AC-12) | `developer-csharp` / C# + xUnit + Moq | `done` (`f076662`) |
| 003 | [tasks/003-config-page-field.md](tasks/003-config-page-field.md) — поле «Базовый URL API» в `Configuration/configPage.html` (AC-6) | `developer-csharp` / HTML + JS | `done` (`1693590`) |
| 004 | [tasks/004-docs-base-url.md](tasks/004-docs-base-url.md) — `README.md` + `docs/architecture/overview.md`, включая AC-7′ (AC-7′, AC-11) | `developer-csharp` / Markdown | `done` (`1106693`) |

Порядок: 001 → 002 → 003 → 004, строго последовательно в ветке `feature/eu-api-endpoint`, каждая задача — локальный commit без push. План: [tech-plan.md](tech-plan.md).

## DevOps impact check

**Статус: закрыт** — 2026-11-28, Техлид, по факту дизайна ([architecture.md](architecture.md), [ADR-0003](../../../architecture/decisions/ADR-0003-configurable-api-base-url.md)). DevOps-задача не создавалась, стадия `devops-check` не открывается.

| Вопрос | Результат |
|--------|-----------|
| Новые env vars / secrets | _нет_ — значение хранится в XML-конфигурации плагина (`plugins/configurations/PoiskKinoMetadataPlugin.xml`), новых секретов нет; API-ключ не логируется (AC-9) |
| Новые интеграции / порты / очереди / storage / jobs | _нет_ — новый исходящий хост `eu-api.poiskkino.dev` уже объявлен API (`servers` спеки) и используется только по явному выбору администратора; новых портов/очередей/БД/фоновых задач нет |
| Миграции данных | _нет_ — отсутствующий элемент XML оставляет инициализатор-дефолт (analysis OQ-4, F6/F7) |
| Новые зависимости (NuGet/образы) | _нет_ — xUnit/Moq уже подключены (AC-12) |
| Docker / compose / CI/CD / deploy / Helm / K8s / observability | _нет_ — нет новых шагов CI и изменений инфраструктуры; наблюдаемость — существующий `ILogger` (`Information`/`Warning`, D-ARCH-5), runbook не требуется |
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

### 2026-11-27 23:58 — решение Стейхолдера на gate `analysis-review`

- **Продолжать:** да (`continue`), стадия → `architecture`.
- **Q1 (UI):** свободное поле URL — принято; whitelist двух хостов отклонён.
- **Q2 (применение настройки):** перезапуск Jellyfin допустим — принято; действует **AC-7′** (в `README.md` явно указать, что смена хоста применяется после перезапуска), реализация AC-7 «без перезапуска» не требуется.
- **Q3 (`GET /v1.5/token` в UI):** «если дёшево» — решение по стоимости за Архитектором: если диагностика укладывается в текущую фичу без заметного роста диффа/рисков — включить, иначе зафиксировать как отдельную будущую фичу и оставить вне scope.
- Прочие рекомендации Аналитика (OQ-1…OQ-4, D-1…D-8, AC-1…AC-6, AC-8…AC-12) приняты без изменений.

### 2026-11-27 — handoff

- from: Analyst
- to: Orchestrator (gate `analysis-review`, решение — Stakeholder)
- stage: `analysis-review`
- artifacts: [analysis.md](analysis.md) (user stories, AC-1…AC-12, NFR, out of scope, зависимости, риски R1–R9, ответы OQ-1…OQ-4, вопросы Q1–Q3, решения D-1…D-8), [brief.md](brief.md) (уточнена строка про llms.txt)
- notes: рекомендация `continue`. OQ-1 — свободное поле URL с серверной нормализацией и подсказкой (допустим `<datalist>` с двумя хостами); OQ-2 — failover подтверждён как out of scope; OQ-3 — enhancement подтверждён; OQ-4 — миграция не нужна: `BasePluginConfiguration` пуст, конфигурация читается `XmlSerializer.DeserializeFromFile` в один XML, отсутствующий элемент оставляет инициализатор-дефолт (evidence: исходники Jellyfin `BasePluginOfT.cs`/`MyXmlSerializer.cs`), остаточный smoke — AC-8. Реальные риски: R1 — base URL привязан к конструктору клиента, поэтому нужен AC-7 «применение без перезагрузки» (альтернатива AC-7′ — по решению Q2); R2 — нет реального API-ключа, end-to-end smoke EU-зеркала недоступен (только пробы 401 + unit-тесты); R6 — формулировка llms.txt о EU-зеркале не воспроизводится, опора на `servers` спеки; R3 — семантика лимита 200/сутки между хостами неизвестна (информационно через `/v1.5/token`). Вопросы Стейхолдеру: Q1 (свободный хост vs whitelist двух хостов), Q2 (применение без перезагрузки), Q3 (показывать ли `/v1.5/token` — рекомендовано вне scope). Блокирующих вопросов нет.

### 2026-11-27 20:10 — handoff

- from: Architect
- to: Orchestrator
- stage: `tech-decomposition`
- artifacts: [architecture.md](architecture.md), [ADR-0003](../../../architecture/decisions/ADR-0003-configurable-api-base-url.md), [ADR index](../../../architecture/decisions/README.md)
- notes:
  - **Дизайн:** `PluginConfiguration.ApiBaseUrl` (инициализатор = дефолт `https://api.poiskkino.dev`) + чистый хелпер `ApiBaseUrlResolver` (дефолт-константа, нормализация/валидация → `ResolvedBaseUrl(Uri, UsedFallback, Reason)`, без исключений) + необязательный 3-й параметр конструктора `PoiskKinoApiClient(..., string? apiBaseUrl = null)`; 5 провайдеров передают сырое значение из `Plugin.Instance.Configuration` по паттерну `ApiKey`; клиент один раз резолвит, ставит `HttpClient.BaseAddress`, логирует `Information`/`Warning`.
  - **Корневой base URL:** пути запросов относительные с ведущим `/`, поэтому base URL с подпутём отвергается с предупреждением (иначе `System.Uri` молча теряет путь); прокси с подпутём — out of scope, возможная будущая фича.
  - **Q2/AC-7′:** значение — construction-time, смена применяется после перезапуска; в `README.md` это должно быть написано явно. Уточнение фактуры: `UpdateConfiguration` в Jellyfin обновляет конфигурацию в памяти сразу, то есть AC-7 технически достижим, но по решению Стейхолдера не реализуется (зафиксировано альтернативой в ADR-0003).
  - **Q3 — решение: вне scope.** CORS у `/v1.5/token` открыт (preflight `204`, `allow-origin: *`, `allow-headers: x-api-key`), но браузерная проба измеряет путь браузера администратора, а не Jellyfin-сервера; корректный вариант требует серверного эндпоинта плагина (новая HTTP-поверхность, запрещено AC-12) и не тестируется в CI без ключа. Зафиксировано как отдельная будущая фича (серверная диагностика «проверить подключение»).
  - **Риски для Техлида (A1–A8 в architecture.md):** частичное применение значения между 5 ленивыми клиентами до перезапуска; кэш 24 ч не содержит хост в ключе (инвариант на случай будущего live-переключения); fallback при подпути/`http`/`userinfo`; нет реального ключа → E2E EU не проверить (unit-тесты на `RequestUri` + пробы 401); имя `ApiBaseUrl` становится частью XML-схемы.
  - **Уточнения требований (не блокирующие, повторный проход Аналитика не нужен):** AC-3 не описывает путь/порт/userinfo/query (принято: порт разрешён, остальное → fallback); AC-3/AC-6 не говорят о канонизации сохранённого значения (принято: значения в конфигурации/UI не переписываются, нормализация — при создании клиента).
  - **Техлиду:** 6 кандидатов на декомпозицию — §10 `architecture.md`; документационная часть — §7 (README включая AC-7′, overview.md, release notes на финализации).
  - **Вне моего файлового scope:** в `README.md` фичи до сих пор «Статус `intake`» — обновить Оркестратору.

### 2026-11-28 00:20 — handoff

- from: Tech Lead
- to: Orchestrator
- stage: `development`
- artifacts: [tech-plan.md](tech-plan.md), [tasks/README.md](tasks/README.md), [tasks/001-api-base-url-resolver.md](tasks/001-api-base-url-resolver.md), [tasks/002-configuration-client-providers.md](tasks/002-configuration-client-providers.md), [tasks/003-config-page-field.md](tasks/003-config-page-field.md), [tasks/004-docs-base-url.md](tasks/004-docs-base-url.md)
- notes:
  - **Декомпозиция готова, плейсхолдеров нет.** 4 задачи, роль `developer-csharp` (субагенты в этой среде не запускаются Техлидом — задачи выполняет Оркестратор), строго последовательно в ветке `feature/eu-api-endpoint`, без task-branches, каждая — локальный commit без push.
  - **Порядок и зависимости:** 001 (хелпер `ApiBaseUrlResolver` + theory-таблица правил D-ARCH-1) → 002 (конфигурация + клиент с `string? apiBaseUrl = null` + 5 провайдеров + тесты на `RequestUri` и логирование) → 003 (поле в `configPage.html`) → 004 (`README.md` + `docs/architecture/overview.md`, AC-7′). 002 зависит от 001; 003 от 002; 004 от 002/003.
  - **Отличие от architecture.md §10 (обосновано в tech-plan.md):** `PluginConfiguration.ApiBaseUrl` перенесён из «политики» в задачу 002 (поле проверяется вместе с пробросом значения до `HttpClient`, а 001 остаётся чистой функцией без влияния на поведение); документация вынесена в отдельную 004, чтобы не смешивать её с review-диффом кода.
  - **DevOps impact check закрыт по факту дизайна:** новых env/secrets, интеграций, портов, миграций, зависимостей и изменений CI/Docker/Helm/K8s нет; наблюдаемость — существующий `ILogger` (D-ARCH-5). DevOps-задача не нужна, стадия `devops-check` не открывается.
  - **Что проверяет Техлид на приёмке:** 001 — правила 1–10 покрыты построчно, ни один вход не бросает исключение, `UsedFallback`/`Reason` по таблице; 002 — кастомный хост в `RequestUri` трёх запросов, дефолт для старой XML-конфигурации, `grep` отсутствия хардкода хоста вне резолвера, `Information`/`Warning` без `ApiKey`, дифф без изменений кэша/таймаута/лимита, `build -c Release` + `test` зелёные; 003 — загрузка/сохранение поля, дефолт при отсутствии, подсказка с двумя хостами и перезапуском, JS не нормализует URL; 004 — оба хоста, дефолт, правила нормализации, явная фраза о перезапуске, нет ссылки на `llms.txt`/рекламы прокси.
  - **Ограничение проверки:** реального API-ключа нет (analysis F14, R2/A7) — end-to-end smoke EU-зеркала остаётся за QA/администратором; unit-тесты проверяют `RequestUri`, а не сетевой ответ.
  - **Вне scope / follow-up:** live-переключение base URL и хост в ключе кэша, серверная диагностика `/v1.5/token`, base URL с подпутём, failover — отдельные будущие фичи (tech-plan.md «Вне scope и follow-up»).
  - **Недостаточность разрешений:** не зафиксирована — декомпозиция выполнена без запуска сборки и субагентов.
  - **Замечание architecture.md §7 про README фичи снято** коммитом `bbfda17` (стадия в README не дублируется) — действий не требуется.
