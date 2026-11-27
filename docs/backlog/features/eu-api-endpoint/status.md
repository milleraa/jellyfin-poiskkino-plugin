# Status — `eu-api-endpoint`

Единая точка правды по стадии для Оркестратора. Обновляйте при каждом handoff (см. `.opencode/skills/role-handoff/SKILL.md`).

## Текущее состояние

| Поле | Значение |
|------|----------|
| **Стадия** | `finalization` |
| **Owner** | `Orchestrator` |
| **Ветка** | `feature/eu-api-endpoint` (локально, не запушена) |
| **PR/MR** | issue [#8](https://github.com/milleraa/jellyfin-poiskkino-plugin/issues/8) — открыт; PR/MR нет |
| **Commit со ссылкой на PR/MR** | _нет_ |
| **QA** | **pass** (2026-11-28, `9577df6`) — см. [qa.md](qa.md), [test-runs](../../../qa/test-runs.md) |
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
| 001 | [tasks/001-api-base-url-resolver.md](tasks/001-api-base-url-resolver.md) — `ApiBaseUrlResolver` + unit-тесты (AC-1, AC-3, AC-4) | `developer-csharp` / C# + xUnit | `done` — принято Техлидом |
| 002 | [tasks/002-configuration-client-providers.md](tasks/002-configuration-client-providers.md) — `PluginConfiguration.ApiBaseUrl`, клиент, 5 провайдеров, тесты фактического хоста (AC-1, AC-2, AC-4, AC-5, AC-8, AC-9, AC-10, AC-12) | `developer-csharp` / C# + xUnit + Moq | `done` — принято Техлидом |
| 003 | [tasks/003-config-page-field.md](tasks/003-config-page-field.md) — поле «Базовый URL API» в `Configuration/configPage.html` (AC-6) | `developer-csharp` / HTML + JS | `done` — принято Техлидом |
| 004 | [tasks/004-docs-base-url.md](tasks/004-docs-base-url.md) — `README.md` + `docs/architecture/overview.md`, включая AC-7′ (AC-7′, AC-11) | `developer-csharp` / Markdown | `done` — принято Техлидом |
| 005 | [tasks/005-redact-userinfo-all-at.md](tasks/005-redact-userinfo-all-at.md) — rework: `RedactUserInfo` вырезает `userinfo` до **последнего** `@` в authority (пароль с `@` не утекает в `Warning`) | `developer-csharp` / C# + xUnit | `done` (`bf2f6bc`) — принято Техлидом |

Порядок пройден: 001 → 002 → 003 → 004 → rework 005 → повторная приёмка Техлида (accept) → **QA**. Строго последовательно в ветке `feature/eu-api-endpoint`, каждый шаг — локальный commit без push. План: [tech-plan.md](tech-plan.md).

## DevOps impact check

**Статус: закрыт** — 2026-11-28, Техлид, по факту дизайна ([architecture.md](architecture.md), [ADR-0003](../../../architecture/decisions/ADR-0003-configurable-api-base-url.md)). DevOps-задача не создавалась, стадия `devops-check` не открывается. **Подтверждён на приёмке реализации** (rev `023460e..0b4dd23`): новых env-secrets, интеграций, портов, миграций, зависимостей, изменений CI/Docker/Helm/K8s в диффе нет; rework [task 005](tasks/005-redact-userinfo-all-at.md) меняет только текст `Warning` — impact тоже нет. **Подтверждён повторно на приёмке rework** (rev `6053daf..6af332b`): изменены только `PoiskKinoApiClient.RedactUserInfo` и `PoiskKinoApiClientBaseUrlTests.cs` (+ docs) — impact по-прежнему отсутствует, DevOps-задача не нужна.

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
| 2026-11-28 | **pass** — 281/281 ×4 прогона, build 0 ошибок (baseline `CS1591` = 122); AC-1…AC-12 (с AC-7′) pass, дефектов нет | [test-runs](../../../qa/test-runs.md), [qa.md](qa.md) |

## PR/MR feedback / rework

| Поле | Значение |
|------|----------|
| **Feedback source** | приёмка Техлида (`tech-lead-review`, rev `023460e..0b4dd23`) |
| **Feedback summary** | один дефект в security-фиксе `1533989`: `RedactUserInfo` вырезает `userinfo` до **первого** `@`, поэтому пароль с `@` (`https://admin:p@ss@host`) оставляет хвост (`ss@host`) в `Warning` — цель «пароль не в логе» достигается не всегда. Формальные AC-1… AC-12 выполнены; feature-код иначе не переделывается |
| **Rework route** | Tech Lead → `developer-csharp` (только `PoiskKinoApiClient` + тесты) |
| **Переключено на существующую ветку** | да — `feature/eu-api-endpoint`, task-branch не создаётся |
| **Rework tasks** | [tasks/005](tasks/005-redact-userinfo-all-at.md) — `done` (`bf2f6bc`), **фикс принят** (2026-11-28, ревью `6053daf..6af332b`) |

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

### 2026-11-28 — handoff QA → finalization

- from: QA (Playwright)
- to: Orchestrator
- stage: `finalization`
- artifacts: [qa.md](qa.md) (таблица AC-1…AC-12 → evidence, прогоны, ограничения, вердикт), запись в [test-runs](../../../qa/test-runs.md) от 2026-11-28, ветка `feature/eu-api-endpoint` @ `9577df6` (перед QA-коммитом)
- notes:
  - **Вердикт: pass, дефектов нет.** Все проверки выполнены собственными командами, не по отчётам предыдущих ролей.
  - **Сборка:** `dotnet build PoiskKinoMetadataPlugin.slnx -c Release --no-incremental` → 0 ошибок, все предупреждения — `CS1591` (122 в двух проектах, baseline), новых типов предупреждений нет; среда .NET SDK `10.0.115`, `net10.0`.
  - **Тесты:** `dotnet test PoiskKinoMetadataPlugin.slnx -c Release` — **281/281** (0 failed, 0 skipped) четыре прогона подряд (flaky-контроль после истории `logo-images` — расхождений нет). Фильтрованные: `ApiBaseUrlResolverTests` 71/71, `PoiskKinoApiClientBaseUrlTests` 35/35, `PluginConfigurationTests` 3/3, проводочный тест провайдера 1/1.
  - **Негативный контроль AC-5:** временно заменил в конструкторе `ApiBaseUrlResolver.Resolve(apiBaseUrl)` → `Resolve(null)` — упали **18 из 35** тестов клиента (`Expected: "eu-api.poiskkino.dev"`, `Actual: "api.poiskkino.dev"`); файл восстановлен (`git checkout --`, дерево чистое), повторный прогон 281/281. То есть тесты действительно отличают «ходим на сконфигурированный хост» от «ходим на дефолт».
  - **AC-покрытие:** AC-1 (дефолт в инициализаторе + legacy XML без `<ApiBaseUrl>`), AC-2 (инварианты `rg`: 120 с / 24 ч / 200 / `SemaphoreSlim(1, 1)`; блоки 401/403/429/404 не тронуты), AC-3 (trim, без схемы, завершающий `/`, порт, регистр/IDN), AC-4 (fallback без исключений: `http`, мусор, подпуть, `userinfo`/query/fragment, IPv6-литерал, 29 hostile-входов), AC-5 (три `RequestUri` + проводка из конфигурации, единственный литерал хоста `ApiBaseUrlResolver.cs:49`), AC-6/AC-8 (статически + XML round-trip), AC-7′ (фраза о перезапуске в README/overview/UI), AC-9 (`Information`/`Warning`, `ApiKey_NeverAppearsInLogs`, усечение 200, пароль с `@` редактируется), AC-10 (build/test), AC-11 (README + overview сверены с кодом), AC-12 (нет новых пакетов/env/CI).
  - **Пробы хостов (без ключа):** `GET /v1.5/token` → `401` на `https://api.poiskkino.dev` и `https://eu-api.poiskkino.dev` — оба зеркала живы.
  - **Ограничения прогона (не fail, зафиксированы в [qa.md](qa.md)):** UI и запись `plugins/configurations/*.xml` в живом Jellyfin — ручной сценарий администратора (6 шагов из [task 003](tasks/003-config-page-field.md)); e2e-поиск на EU-хосте без реального ключа (R2/A7); live-переключение не проверялось (действует AC-7′, A1); кэш без хоста в ключе (R7/A3) — безопасно при restart-семантике; IPv6-литерал и подпуть → fallback+`Warning`; `datalist` на `is="emby-input"` автотестами не покрыт.
  - **Residual risk (принят Техлидом, подтверждён прогоном):** ключ, вставленный в поле базового URL — `userinfo` редактируется (`https://KEY@host` → `https://host`), query/fragment и «голое значение» попадают в лог как часть адреса; `ApiKey` из конфигурации не логируется (AC-9). Требует ошибки администратора; QA не блокирует.
  - **Некритичные наблюдения:** имя файла `ConfigurationTests.cs` vs класс `PluginConfigurationTests`; подсказка в UI короче README (не упоминает `userinfo`/query/fragment); логи на английском. Rework не требуется.
  - **DevOps impact:** без изменений — новых env/secrets/интеграций/зависимостей/CI в диффе нет; стадия `devops-check` не открывается. Следующий шаг — Финализация (release notes, README/overview уже обновлены, PR/MR готовит Оркестратор).

### 2026-11-28 00:50 — приёмка rework: accept → QA

- from: Tech Lead
- to: Orchestrator
- stage: `qa`
- artifacts: `bf2f6bc` (фикс), `81bff03`/`6af332b` (docs), [tasks/005](tasks/005-redact-userinfo-all-at.md), [qa.md](qa.md), прогон Техлида `6053daf..6af332b`
- notes:
  - **Проверено мной, не по отчёту:** `dotnet build -c Release PoiskKinoMetadataPlugin.slnx --no-incremental` → 0 ошибок, все предупреждения только `CS1591` (122 в двух проектах, baseline); `dotnet test PoiskKinoMetadataPlugin.slnx -c Release` → **281/281 passed**, 0 failed, 0 skipped; ревью диффа `6053daf..6af332b` (5 файлов: клиент, тесты + 3 docs; `ApiBaseUrlResolver`, `Configuration.cs`, провайдеры, `configPage.html`, README/overview не тронуты).
  - **Дефект закрыт, тесты доказывают это.** Временно вернул старую реализацию клиента (блоб `1b3eaab`) и прогнал `--filter Constructor_UserInfoInBaseUrl_RedactedInWarning`: **3 из 6 строк упали** — именно новые кейсы с `@` в пароле, в сообщении об ошибке видна фактическая утечка `Invalid API base URL 'ss@api.poiskkino.dev'`. После восстановления файла (`git checkout HEAD -- …`, дерево чистое) — 281/281 зелёные. То есть новые ассерты (`!Contains("ss@")` + точное `'https://api.poiskkino.dev'`) действительно различают старое и новое поведение, а старые три кейса остались без правки ожиданий.
  - **Граничные кейсы проверены статически и тестами:** guard `authorityEnd <= authorityStart` закрывает `https://`, `https:///x`; `LastIndexOf('@', authorityEnd - 1, authorityEnd - authorityStart)` берёт именно последний `@` в authority; `@` в пути/query/fragment не вырезается (тест `Constructor_AtSignOutsideAuthority_LoggedUnchanged`); порядок `TruncateForLog(RedactUserInfo(raw), 200)` сохранён; `RedactUserInfo` остался `private static`, исключений нет (`Constructor_DegenerateUserInfoInputs_DoNotThrow`, 8 входов).
  - **Регресс-проверки:** `rg -n "poiskkino\.dev" PoiskKinoMetadataPlugin --glob '*.cs' --glob '!obj/**' --glob '!bin/**'` → только `ApiBaseUrlResolver.cs:49`; тесты на `RequestUri` трёх запросов (AC-5) без изменений и зелёные; `Timeout = 120 s`, `CacheExpirationHours = 24`, `MaxRequestsPerDay = 200`, `SemaphoreSlim(1, 1)`, обработка 401/403/429/404 и исключений — значения и код не менялись (AC-2/D-ARCH-8); формат `Information`/`Warning` не менялся.
  - **Residual risk (решение: принять, новой задачи не заводим, зафиксировано в [qa.md](qa.md)).** Замечание исполнителя уточнено эмпирической пробой резолвера: (a) ключ как `userinfo` (`https://KEY@host`) — **риска больше нет**, фикс вырезает всё до последнего `@` и в `Warning` остаётся `https://host`; (b) ключ в query/fragment (`https://api.poiskkino.dev/?token=KEY`) → fallback + `Warning` с сырой строкой, query не редактируется — **остаётся**; (c) ключ, вставленный как голое значение (`https://a1b2c3d4-e5f6-…`), парсится как single-label host → fallback нет → ключ попадает в `Information` как часть адреса — **остаётся**. Почему принимаем: требуется, чтобы администратор сам вставил секрет в поле «Базовый URL API» (ключ в конфигурации при этом не затронут — AC-9 выполняется, тест `ApiKey_NeverAppearsInLogs` зелёный); лог доступен тому же администратору; логирование фактического адреса — обязательная диагностика AC-9, а эвристики «похоже на ключ» подавляли бы легитимные single-label/intranet-хосты (`https://jellyfin`). Опциональные неблокирующие follow-up (вне scope, отдельной задачей не оформляем): вырезать `?`/`#`-часть при логировании `Warning` и добавить в README/UI подсказку «поле — только для адреса, не вставляйте сюда API-ключ». Риск пониженный, QA не блокирует.
  - **Риски для QA:** A1 — до перезапуска возможно кратковременное «смешанное» состояние хостов у 5 ленивых клиентов (README ограничивает: «после перезапуска»); A3/R7 — хост не входит в ключ in-memory кэша (24 ч) — допустимо при restart-семантике, инвариант для будущего live-переключения в ADR-0003; R2/A7 — реального API-ключа нет, e2e-поиск на EU-хосте проверяет только администратор; IPv6-литерал и base URL с подпутём → fallback+`Warning` (подпуть документирован, IPv6 — нет); `datalist` на `is="emby-input"` автотестами не покрыт (удаляется без влияния на поле).
  - **Сценарии ручной проверки (из [task 003](tasks/003-config-page-field.md)):** (1) свежая/старая конфигурация → в поле виден `https://api.poiskkino.dev`; (2) `https://eu-api.poiskkino.dev` → Сохранить → F5 → значение на месте, `<ApiBaseUrl>` в `plugins/configurations/PoiskKinoMetadataPlugin.xml` без канонизации; (3) `eu-api.poiskkino.dev/` → после F5 ровно введённое, после рестарта `Information` «PoiskKino API base URL: https://eu-api.poiskkino.dev/» и запросы на EU-хост; (4) пустое поле → дефолт в UI, после рестарта только `Information`, `Warning` нет; (5) `http://api.poiskkino.dev` → один `Warning` с причиной, поиск на дефолте, страница показывает введённое; (6) полный e2e с реальным поиском — только при наличии ключа (R2), плюс проверка лога: в `Warning` нет пароля/`userinfo` (в т.ч. `https://admin:p@ss@host` → `https://host`).
  - **DevOps impact check:** подтверждён закрытым повторно (rev `6053daf..6af332b`, impact нет), `devops-check` не открывается. `Недостаточность разрешений` — не зафиксирована. Следующий шаг — QA (запуск за Оркестратором).

### 2026-11-28 — handoff rework → tech-lead-review (раунд 2)

- from: Orchestrator
- to: Tech Lead
- stage: `tech-lead-review`
- artifacts: коммиты `bf2f6bc` (fix) и `81bff03` (статус задачи 005), [tasks/005-redact-userinfo-all-at.md](tasks/005-redact-userinfo-all-at.md)
- notes: дефект из приёмки исправлен — `RedactUserInfo` вырезает весь `userinfo` до **последнего** `@` в authority с guard на пустой authority. По заявлению исполнителя: `dotnet build` — 0 ошибок (только baseline `CS1591` = 122), `dotnet test` — 281/281 passed (+14 кейсов: пароль с `@`, `@` в пути/query/fragment, вырожденные входы `https:///x`, `@@@`, `://@`). Формат сообщения, усечение до 200 и `Information`-строка не менялись; резолвер, конфигурация, провайдеры, UI и документация не тронуты. Нужен повторный accept для перевода в `qa`.

### 2026-11-28 00:35 — приёмка Техлида: rework (1 узкая правка)

- from: Tech Lead
- to: Orchestrator (dispatch → `developer-csharp`)
- stage: `rework`
- artifacts: [tasks/005-redact-userinfo-all-at.md](tasks/005-redact-userinfo-all-at.md), [tasks/001](tasks/001-api-base-url-resolver.md) (уточнена формулировка AC-3 в тексте задачи), реализация `023460e..0b4dd23`
- notes:
  - **Проверено мной (не по отчёту исполнителя):** `dotnet build -c Release PoiskKinoMetadataPlugin.slnx --no-incremental` → 0 ошибок, все предупреждения — `CS1591` (122 в двух проектах, baseline); `dotnet test PoiskKinoMetadataPlugin.slnx -c Release` → **267/267 passed**, 0 failed, 0 skipped; `rg -n "poiskkino\.dev" PoiskKinoMetadataPlugin --glob '*.cs' --glob '!obj/**' --glob '!bin/**'` → единственное вхождение `ApiBaseUrlResolver.cs:49`; `rg` по `BaseAddress =`, `FromSeconds(120)`, `CacheExpirationHours`, `MaxRequestsPerDay`, `SemaphoreSlim` → все на прежних значениях (AC-2/D-ARCH-8); ревью диффа `023460e..0b4dd23` по 21 файлу.
  - **AC-1…AC-12 приняты:** дефолт в инициализаторе + round-trip XML старой конфигурации (`ConfigurationTests`); кастомный хост в `RequestUri` всех трёх запросов, fallback без исключений, усечение 200, `ApiKey` не в логах; правило 7 (путь) и `userinfo`/`query`/`fragment` → fallback; UI: `type="text"`, `datalist`, дефолт при отсутствии, сохранение без канонизации (D-ARCH-4); README содержит оба хоста, правила нормализации и явное «после перезапуска» (AC-7′), `overview.md` обновлён; новых зависимостей/env/CI нет (AC-12).
  - **Отклонение 1 (static `ApiBaseUrl` в 5 провайдерах) — принято.** `CS0236` в инициализаторе `Lazy<>` подтверждён; свойство читается внутри лямбды, то есть в момент первого запроса, а не при старте — семантика и источник (`Plugin.Instance?.Configuration`) не изменились, комментарий в коде есть. Переписывать 5 primary-конструкторов ради формы `instance` не требуется (scope/риски больше выгоды).
  - **Отклонение 2 (`userinfo`-redaction, `1533989`) — принято частично, одна правка → [task 005](tasks/005-redact-userinfo-all-at.md).** `ApiBaseUrlResolver` не изменён, поэтому нормализация/`UsedFallback`/`FallbackReason` и правило «userinfo → fallback» остались как в ADR-0003 — менялся только текст `Warning`, что согласуется с D-ARCH-5/A6. Дефект: `RedactUserInfo` вырезает до первого `@`, поэтому `https://admin:p@ss@api.poiskkino.dev` логируется как `https://ss@api.poiskkino.dev` — хвост пароля в логе. Требуется вырезать весь `userinfo` (до последнего `@` в authority) + тесты на пароль с `@` и на `@` в пути (без изменения поведения). Правка локальная: `PoiskKinoApiClient.RedactUserInfo` + `PoiskKinoApiClientBaseUrlTests`.
  - **Отклонение 3 (README: `GET /v1/search` → `GET /v1.5/movie/search` — принято.** Сверено со спекой: `api-docs/documentation.yaml:960` (`/v1.5/movie/search`, query-параметры `query`/`limit`/`page`, `limit` по умолчанию 10, максимум 250); клиент использует `limit=3` — пример корректен и в рамках раздела, разрешённого task 004.
  - **Решение по AC-3: правка `analysis.md` не требуется.** Формулировка AC-3 («удаляются завершающие `/`; результат приводится к абсолютному URI») корректна; неточность была только в тексте task 001 («без завершающего `/` в `AbsoluteUri`» — для `Uri.AbsoluteUri` корня это недостижимо, там всегда `/`). Задача 001 исправлена мной: проверка идёт по `OriginalString == "https://host[:port]"` + `AbsolutePath == "/"`, что и реализовано.
  - **Риски для QA:** A1 — до перезапуска возможно кратковременное «смешанное» состояние хостов у 5 ленивых клиентов (README это явно ограничивает: «после перезапуска»); A3/R7 — хост не входит в ключ in-memory кэша (24 ч) — в текущей restart-семантике допустимо, для будущего live-переключения обязателен инвариант из ADR-0003; R2/A7 — реального API-ключа нет, e2e-поиск на EU-хосте проверяется только администратором (unit-тесты фиксируют хост запроса, а не сетевой ответ); IPv6-литерал и base URL с подпутём отклоняются резолвером (в README документирован подпуть; IPv6 не документирован, поведение — fallback+`Warning`); `datalist` на `is="emby-input"` в Jellyfin 12.x автотестами не покрыт — при аномалии его можно удалить без влияния на поле.
  - **Сценарии для QA (из [task 003](tasks/003-config-page-field.md)):** (1) свежая/старая конфигурация → в поле виден `https://api.poiskkino.dev`; (2) `https://eu-api.poiskkino.dev` → Сохранить → F5 → значение на месте, `<ApiBaseUrl>` в `plugins/configurations/PoiskKinoMetadataPlugin.xml` без канонизации; (3) `eu-api.poiskkino.dev/` → после перезагрузки страницы ровно введённое, после рестарта `Information` «PoiskKino API base URL: https://eu-api.poiskkino.dev/» и запросы на EU-хост; (4) пустое поле → дефолт в UI, после рестарта только `Information`, `Warning` нет; (5) `http://api.poiskkino.dev` → один `Warning` с причиной, поиск работает на дефолте, страница показывает введённое; (6) полный e2e с поиском — только при наличии ключа (R2).
  - **DevOps impact check:** подтверждён закрытым (impact нет и для task 005); стадия `devops-check` не открывается. `Недостаточность разрешений` — не зафиксирована.

### 2026-11-28 — handoff development → tech-lead-review

- from: Orchestrator
- to: Tech Lead
- stage: `tech-lead-review`
- artifacts: коммиты `023460e..0bf0083` (задачи 001–004 + security-фикс), [tech-plan.md](tech-plan.md), [tasks/](tasks/README.md)
- notes: все четыре задачи отмечены `done` исполнителем, `dotnet build -c Release` — 0 ошибок (предупреждения только baseline `CS1591` = 122), `dotnet test` — 267/267 passed. На приёмку отдельно выносятся три отклонения от исходных задач: (1) в 5 провайдерах свойство `ApiBaseUrl` сделано `static` вместо `instance` — инициализатор `Lazy<>` не может обращаться к экземпляру (`CS0236`); (2) Оркестратор расширил scope 003 санитизацией `userinfo` перед `Warning` (коммит `1533989`, security); (3) в README исправлен устаревший пример запроса `/v1/search` → `/v1.5/movie/search`.

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
