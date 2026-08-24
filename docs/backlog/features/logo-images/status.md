# Status — `logo-images`

Единая точка правды по стадии для Оркестратора. Обновляйте при каждом handoff (см. `.opencode/skills/role-handoff/SKILL.md`).

## Текущее состояние

| Поле | Значение |
|------|----------|
| **Стадия** | `finalization` (rework-валидация QA пройдена) |
| **Owner** | `Orchestrator` (Финализатор повторно доставляет PR #3) |
| **Worktree** | `/home/alex/src/my/jellyfin-metadata-plugin-wt-logo-images` (сохранён Финализатором для возможного rework/archive; ранее ошибочно помечен как удалён — исправлено Оркестратором) |
| **Ветка** | `feature/logo-images` (push в `origin`; ветку и remote branch не удалять до merge/close) |
| **PR/MR** | [jellyfin-poiskkino-plugin#3](https://github.com/milleraa/jellyfin-poiskkino-plugin/pull/3) |
| **Commit со ссылкой на PR/MR** | `6b8ef58` (`docs(logo-images): open PR #3 …`) |
| **Блокеры** | нет — flaky-гонка устранена (commit `f3beb9a`, приёмка Техлида 2026-08-24: build 0 err, 5×163/163) |

> ⚠️ **ВАЖНО для Стейхолдера:** merge PR #3 пока **НЕ делать**. Ждём явного решения: `/accept-feature` или `/reject-feature`. При accept Финализатор выполнит archive finalization в ветке `feature/logo-images` (перенос папки в `docs/archive/features/logo-images/`, запись в `docs/history/completed-work.md`, стадия `accepted`) и финальный push — только после этого merge одним мержем. При reject — возврат в rework через `.opencode/skills/pr-mr-rework/SKILL.md`.

### Стадии (справочно)

`intake` → `analysis` → `analysis-review` → `backlog-paused` или `architecture` → `tech-decomposition` → `development` → `devops-check` → `tech-lead-review` → `qa` → `rework` → `finalization` → `pr-mr-ready` → `stakeholder-review` → `rework` или `accepted`

`analysis-review` — обязательная остановка после Аналитика: Стейхолдер решает продолжать сейчас или оставить фичу в backlog.

## Задачи реализации

| ID | Файл | Роль / стек | Статус |
|----|------|-------------|--------|
| 001 | [tasks/001-image-provider-logo-support.md](tasks/001-image-provider-logo-support.md) | developer-csharp (C# / Jellyfin adapter + unit-тесты) | done |
| 002 | [tasks/002-logo-tmdb-filter-tests.md](tasks/002-logo-tmdb-filter-tests.md) | developer-csharp (C# / xUnit) | done |
| 003 | [tasks/003-fix-imageurlhelper-test-isolation.md](tasks/003-fix-imageurlhelper-test-isolation.md) | developer-csharp (rework: xUnit изоляция, flaky CI) | done |

Порядок и зависимости: [tech-plan.md](tech-plan.md). Выполнение строго последовательное в единственном worktree.

## DevOps impact check

| Вопрос | Результат |
|--------|-----------|
| Новые env vars / secrets | _нет_ |
| Новые интеграции / порты / очереди / storage / jobs | _нет_ |
| Docker / compose / CI/CD / deploy / Helm / K8s / observability | _нет_ |
| DevOps-задача | _не нужна_ |

Повторная проверка Техлидом после реализации (2026-08-24, диффы `08db124`, `641b5df`): изменений инфраструктуры нет — только `PoiskKinoImageProvider.cs` + unit-тесты; новых env vars/secrets/интеграций/портов/миграций/Docker/CI-CD не появилось. Impact check закрыт.

Повторная проверка Техлидом после rework-фикса (2026-08-24, дифф `f3beb9a`): изменён только тестовый файл `PoiskKinoMetadataPlugin.UnitTests/Helpers/ImageUrlHelperTests.cs` (атрибут коллекции + save/restore `Plugin.Instance`). Новых env vars/secrets/портов/интеграций/миграций/Docker/compose/CI-CD/deploy/Helm/K8s/observability нет. Impact: **нет**, DevOps-задача не нужна.

## QA

| Дата | Результат | Ссылка на прогон |
|------|-------------|------------------|
| 2026-08-24 | **pass** — rework-валидация фикса 003: build 0 err, 5×163/163 подряд, ImageUrlHelperTests 13/13 (+ flaky-тест ×3 одиночно), регресс Logo PoiskKinoImageProviderTests 14/14; дефектов нет | [qa.md](qa.md), прогон 2 · [test-runs](../../../qa/test-runs.md) |
| 2026-08-24 | **pass** — AC-1..AC-12 все pass, дефектов нет | [qa.md](qa.md), прогон 1 · [test-runs](../../../qa/test-runs.md) (163/163 ×2, build 0 err) |

## PR/MR feedback / rework

| Поле | Значение |
|------|----------|
| **Feedback source** | чат Стейхолдера (решение: фикс flaky-теста до accept) + CI PR #3 (fail → pass → fail → pass) |
| **Feedback summary** | Предсуществующий тест `ImageUrlHelperTests.ShouldIgnoreTmdbImages_WhenPluginNotInitialized_ReturnsFalse` гонит с `PluginInstanceCollection` за статический `Plugin.Instance`; новые тесты фичи расширили окно гонки. Код фичи не скомпрометирован (локально 5×163/163). |
| **Rework route** | Tech Lead (нужен кодовый фикс тестовой изоляции; DevOps impact — нет) |
| **Worktree restored from branch** | да — worktree сохранён Финализатором, восстановление не требовалось (`/home/alex/src/my/jellyfin-metadata-plugin-wt-logo-images` @ `feature/logo-images`) |
| **Rework tasks** | [tasks/003-fix-imageurlhelper-test-isolation.md](tasks/003-fix-imageurlhelper-test-isolation.md) |

## Process learning (неблокирующий triage)

| Поле | Значение |
|------|----------|
| **Learning triage** | `not-triggered` |
| **Class key** | `tech-lead+pr-mr-ready-green-ci+implementation` |
| **Learning evidence** | PR #3 CI: fail/pass/fail/pass (~50%); падение предсуществующего `ImageUrlHelperTests.ShouldIgnoreTmdbImages_WhenPluginNotInitialized_ReturnsFalse` — гонка дефолтной коллекции с `PluginInstanceCollection` за статический `Plugin.Instance`; локально 5×163/163 |
| **Suppression reason** | нет |
| **Process review** | нет |

Один unique evidence-linked reject → `not-triggered`: обычный rework продолжается без создания process-review.

## Changelog (handoff)

### 2026-08-24 — rework-валидация QA pass, → finalization

- Повторная QA-валидация фикса 003 в worktree `feature/logo-images` @ `1bc63a8` (.NET SDK 10.0.111, Linux); предусловия соблюдены — приёмка Техлида и закрытый DevOps impact check по диффу `f3beb9a` из status.md.
- Валидация фикса: `git show f3beb9a --stat` — только `ImageUrlHelperTests.cs` (+23/-2), продакшн-код не изменён (`git log c069df2..HEAD` по коду — единственный коммит `f3beb9a`, тестовый файл). AC-1/AC-4 подтверждены кодом: класс в `[Collection("PluginInstanceCollection")]` (последовательный прогон с плагин-тестами), save/restore `Plugin.Instance` через reflection с громким `Assert.NotNull(instanceProperty)` в try/finally.
- Прогоны (независимо от Техлида): `dotnet build -c Release` — 0 ошибок; **5 полных прогонов** `dotnet test --no-build -c Release` подряд — **163 passed / 0 failed каждый**; целевой `ImageUrlHelperTests` — 13/13 (flaky `ShouldIgnoreTmdbImages_WhenPluginNotInitialized_ReturnsFalse` стабилен, дополнительно ×3 одиночных прогона — pass); регрессия Logo `PoiskKinoImageProviderTests` — 14/14.
- Регрессия AC фичи: продакшн-код идентичен прогону 1 → чеклист AC-1..AC-12 остаётся подтверждённым; ключевые сценарии Logo перепроверены целевым прогоном. Дефектов не найдено.
- Запись о прогоне: [qa.md](qa.md) (прогон 2) · [docs/qa/test-runs.md](../../../qa/test-runs.md).
- Handoff → Оркестратор: стадия `finalization`, owner `Finalizer` — повторная доставка PR #3 (push/merge не выполнялись).

### 2026-08-24 — rework-фикс принят Техлидом, → qa

- Задача [003](tasks/003-fix-imageurlhelper-test-isolation.md) **done** (commit `f3beb9a` `test(infra): isolate ImageUrlHelperTests from Plugin.Instance static state`, только `ImageUrlHelperTests.cs`): класс включён в `[Collection("PluginInstanceCollection")]` (внутри коллекции xUnit v2 последовательно — пересечения с параллельным прогоном больше нет, AC-1/AC-4); в тесте «not initialized» — save/restore `Plugin.Instance` через reflection с громкой проверкой `Assert.NotNull(propInfo)` вместо молчаливого `?.` (AC-1). Продакшн-код не изменён (AC-2), тест «not initialized» сохранён.
- Решение по незакоммиченному диффу: **принят за основу и доработан** — подход соответствовал ТЗ, дефект качества (тихий пропуск reflection при отсутствии свойства → возможный вакуумный pass) устранён developer-csharp.
- Приёмка Техлида: `dotnet build PoiskKinoMetadataPlugin.slnx` — 0 ошибок; 5 полных прогонов `dotnet test --no-build` подряд — все зелёные, 163/163 каждый (AC-3/AC-5). AC-4 подтверждён чтением кода/конфигурации: изоляция через общую коллекцию, `xunit.runner.json` не требуется.
- DevOps impact check по диффу `f3beb9a`: impact нет, DevOps-задача не нужна.
- Push не выполнялся; ветка `feature/logo-images` локально опережает origin на 4 коммита (2 docs rework + фикс + этот docs). Merge PR #3 по-прежнему НЕ выполнять.
- Handoff → Оркестратор: стадия `qa`, owner `QA` — повторная QA-валидация фикса (регресс: полный прогон + целевой тест `ImageUrlHelperTests`), затем Финализатор для повторной доставки PR #3.

### 2026-08-24 — pr-mr-ready → rework (решение Стейхолдера)

- Стейхолдер выбрал: **фикс flaky-теста до accept** (вариант 1); PR #3 остаётся открытым, merge по-прежнему не выполнять.
- Применён `.opencode/skills/pr-mr-rework`: feedback зафиксирован (чат + CI PR #3), стадия → `rework`, worktree сохранён (восстановление не требовалось), новая ветка не создавалась.
- Классификация: нужен кодовый фикс тестовой инфраструктуры → **Техлид**. Создана задача [tasks/003](tasks/003-fix-imageurlhelper-test-isolation.md) (pending).
- Learning triage: `not-triggered` (один unique evidence-linked reject), запись в `docs/process/learning/reject-log.md`.
- Handoff → Техлид (`.opencode/agents/tech-lead.md`): задача 003; после фикса — внутренний цикл приёмки, затем QA → Финализатор (повторная доставка PR #3).

### 2026-08-24 — ⚠️ BLOCKER: flaky CI воспроизводится (2/3 прогонов), PR #3 не готов к merge

- Второй CI-прогон PR #3 ([job](https://github.com/milleraa/jellyfin-poiskkino-plugin/actions/runs/32689565732/job/97320697578)) упал на **том же** тесте `ImageUrlHelperTests.ShouldIgnoreTmdbImages_WhenPluginNotInitialized_ReturnsFalse`. Итог по прогонам: fail → pass (re-run) → fail — вероятность фейла на CI ~2/3.
- Классификация обновлена: **не** разовый flake, а систематическая гонка изоляции тестов. Поставка с нестабильно-красным CI не может считаться ready для Стейхолдера.
- Продуктовый код фичи по-прежнему не скомпрометирован: все падения — в предсуществующем тесте `Helpers/ImageUrlHelperTests` вне коллекции `PluginInstanceCollection`; локально 5×163/163 зелёные. Механизм: параллельное выполнение дефолтной коллекции с `PluginInstanceCollection`, статический `Plugin.Instance` протекает; новые тесты фичи расширили окно гонки.
- **Решение Финализатора:** правки тестового кода вне мандата роли — работа возвращается Оркестратору для маршрутизации rework (`pr-mr-rework` → QA/TechLead): включить `ImageUrlHelperTests` в общую коллекцию + явный сброс `Plugin.Instance` перед проверкой «not initialized» (либо устранить зависимость от статики). После фикса — повторная доставка PR #3.
- Worktree сохранён до решения Стейхолдера (нужен и для rework, и для архивации при accept); ветка и remote branch не тронуты.
- Handoff → Оркестратор/Стейхолдер: blocker зафиксирован; merge PR #3 НЕ выполнять.

### 2026-08-24 — CI-инцидент на PR #3: flaky-тест изоляции (не код фичи)

- Первый CI-прогон PR #3 упал: `ImageUrlHelperTests.ShouldIgnoreTmdbImages_WhenPluginNotInitialized_ReturnsFalse` (`Assert.False() Failure`) — [лог](https://github.com/milleraa/jellyfin-poiskkino-plugin/actions/runs/32688926759/job/97318957305).
- Анализ: тест живёт в **дефолтной** xUnit-коллекции и требует `Plugin.Instance == null`; коллекция `PluginInstanceCollection` (инициализирует плагин, сброс только в `Dispose` fixture) выполняется xUnit v2 **параллельно** с ней. Новые тесты фичи расширили окно работы с инициализированным плагином → вероятность гонки выросла. Корень — предсуществующая слабая изоляция статического состояния, не изменения `PoiskKinoImageProvider`.
- Evidence: re-run CI — **pass** ([job](https://github.com/milleraa/jellyfin-poiskkino-plugin/actions/runs/32688926759/job/97320112071)); локально 5 полных прогонов подряд — 163/163 каждый. Классификация: вероятностный flaky, воспроизводимый под таймингами CI-runner'а.
- Неблокирующий follow-up (кандидат в backlog / rework по решению Стейхолдера): включить `ImageUrlHelperTests` в общую коллекцию с явным сбросом `Plugin.Instance` перед проверкой «not initialized» (либо убрать зависимость от статики через абстракцию).
- Продуктовый код фичи инцидентом не затронут. Стадия остаётся `pr-mr-ready`; решение о rework тестовой изоляции — за Стейхолдером вместе с accept/reject.

### 2026-08-24 — PR #3 открыт, → pr-mr-ready

- Финализатор: целостность ветки проверена — чистое дерево, линейная история `8d48967..f0bfb8c` (10 коммитов), diff против `main` соответствует scope фичи (код: `PoiskKinoImageProvider.cs` + тесты; docs: папка фичи + `docs/qa/test-runs.md`).
- Push ветки `feature/logo-images` в `origin`, создан PR [jellyfin-poiskkino-plugin#3](https://github.com/milleraa/jellyfin-poiskkino-plugin/pull/3): summary, test plan (163/163 unit ×2, build 0 err), ссылки на фичу/tasks/QA, риски, CI (GitHub Actions `ci.yaml`, build+test на PR).
- **Merge PR #3 не выполнять** до решения Стейхолдера (`/accept-feature` / `/reject-feature`); при accept — архивация в этой же ветке и финальный push, затем merge одним мержем.
- Локальный worktree удалён после повторного push; ветка и remote branch сохранены до merge/close.
- DevOps impact: нет (подтверждено ранее); ADR: не требуется.
- Handoff → Стейхолдер (через Оркестратора): стадия `pr-mr-ready`.

### 2026-08-24 — QA pass, → finalization

- QA-валидация в worktree `feature/logo-images` @ `c069df2` (.NET SDK 10.0.111, Linux): предусловия соблюдены — приёмка Техлида и закрытый DevOps impact check из status.md.
- Чеклист AC-1..AC-7 (функционал + фильтрация) и AC-8..AC-12 (тесты): **все pass**, дефектов не найдено; evidence с кодом/тестами — [qa.md](qa.md).
- Прогоны: `dotnet build -c Release` — 0 ошибок (warnings = baseline 61× CS1591, вне фичи их нет); `dotnet test` ×2 подряд — **163 passed / 0 failed** оба раза (совпало с отчётом Техлида); фильтрованный прогон `PoiskKinoImageProviderTests` — 14/14.
- Отдельно проверены: fallback-ветка поиска (OQ-1, порядок Primary → Backdrop → Logo детерминирован тестом) и отклонение Техлида (`"backdrop"` у «Оппенгеймера» в `MixedSearchResponseJson`) — потребители фикстуры не затронуты.
- Изоляция тестов подтверждена (одна коллекция `PluginInstanceCollection`, `SetUpPlugin` пересоздаёт конфиг). Playwright неприменим — серверный плагин без UI ([playwright.md](../../../qa/playwright.md)).
- Неблокирующие наблюдения (в backlog по желанию, не дефекты): whitespace-only `logo.url` без отдельного теста (покрыт guard'ом `IsNullOrWhiteSpace`); restore флага TMDB в тесте без try/finally (риск снят пересозданием плагина в каждом тесте).
- Handoff → Оркестратор: стадия `finalization`, owner `Finalizer`. Запись о прогоне: [test-runs](../../../qa/test-runs.md).

### 2026-08-24 — реализация принята Техлидом, → qa

- Задача 001 done (commit `08db124` `feat(images): return logo images from PoiskKino as ImageType.Logo`): `GetSupportedImages` → `[Primary, Backdrop, Logo]`; маппинг логотипа в обеих ветках с guard `IsNullOrWhiteSpace` (Poster/Backdrop не тронуты); XML-summary обновлён; тесты обновлены (`GetSupportedImages_*`, `GetImages_ByProviderId_*` переименованы под 3 типа; fallback-тест переписан на проверку по типам; новый тест «logo отсутствует»).
- Отступление от ТЗ задачи 001, принято Техлидом: в `MixedSearchResponseJson` записи «Оппенгеймер» добавлен также `"backdrop"` — иначе fallback вернул бы 2 изображения и проверку порядка Primary → Backdrop → Logo выполнить было бы невозможно; остальные потребители фикстуры проверены, набор зелёный.
- Задача 002 done (commit `641b5df` `test(images): cover TMDB filtering for logo type`): AC-6/AC-7 покрыты (TMDB-логотип фильтруется при дефолтном `IgnoreTmdbImages=true`, возвращается при `false` с восстановлением флага); продакшн-код не менялся.
- Приёмка: `dotnet build` 0 ошибок; `dotnet test` — **163 passed / 0 failed** (было 161 до фичи). Ревью против architecture.md: guard-несимметричность по AC-4 соблюдена, OQ-2 уважен (`ShouldFilterUrl` не введён), порядок Primary → Backdrop → Logo детерминирован.
- DevOps impact check: подтверждён повторно по диффам — impact нет, DevOps-задача не нужна.
- Handoff → Оркестратор: стадия `qa`, owner `QA`. QA запускает Оркестратор.

### 2026-08-24 — декомпозиция готова, → development

- Tech plan заполнен ([tech-plan.md](tech-plan.md)): две последовательные задачи для `developer-csharp` в единственном worktree.
- Задача 001: реализация Logo (`GetSupportedImages` → `[Primary, Backdrop, Logo]`, маппинг `movieData.Logo?.Url` и fallback `apiItem.Logo?.Url` по OQ-1, guard `IsNullOrWhiteSpace`, XML-summary) + синхронное обновление существующих тестов (fallback-тест переписывается на проверку по типам, `logo` добавляется в `MixedSearchResponseJson`).
- Задача 002: тесты TMDB-политики для Logo (AC-6/AC-7), продакшн-код не меняется; зависит от 001.
- DevOps impact check: **нет** по всем пунктам (подтверждено Архитектором) — DevOps-задача не создаётся, стадия `devops-check` будет закрыта фиксацией этого факта перед QA.
- Handoff → `developer-csharp` (задача 001).

### 2026-08-24 — архитектура готова, → tech-decomposition

- Дизайн зафиксирован в [architecture.md](architecture.md): расширение `PoiskKinoImageProvider` (`GetSupportedImages` → `[Primary, Backdrop, Logo]`), маппинг `logo.url` в обеих ветках (по ID и fallback из поиска по решению OQ-1), единая точка TMDB-фильтрации — существующий пост-фильтр.
- Решение Архитектора по OQ-2: защитный вызов `ShouldFilterUrl` при маппинге логотипа **не вводить** — пост-фильтр функционально эквивалентен, дублирование не нужно; триггер пересмотра зафиксирован.
- Новый ADR **не требуется**: тривиальное расширение существующего адаптера без новых границ/интеграций/компромиссов (зафиксировано явно).
- Внимание Техлида: существующий тест `GetImages_MovieFallsBackToSearch_ReturnsImagesFromSearch` сломается преднамеренно (`Assert.All(movie-poster.jpg)`) — переписать на проверку по типам + добавить `logo` в `MixedSearchResponseJson`; guard для логотипов — `IsNullOrWhiteSpace` (AC-3), постер/фон не трогать (AC-4).
- Документационные последствия: `docs/product/requirements.md:12` устареет после реализации (передано Аналитику/Стейхолдеру); DevOps impact — нет.
- Handoff → Оркестратор: стадия `tech-decomposition`, owner `Tech Lead`, вход: `analysis.md`, `architecture.md`.

### 2026-08-24 — analysis-review пройден, → architecture

- Стейхолдер: продолжаем к Архитектору.
- Решение по OQ-1: **в scope** — логотип добавляется и в fallback-ветке «только данные из поиска» (рекомендация Аналитика принята).
- OQ-2 (защитный вызов `ShouldFilterUrl`) — на решение Архитектора.
- Handoff → Архитектор (`.opencode/agents/architect.md`), вход: `analysis.md`, `brief.md`.

### 2026-08-24 — intake → analysis

- Стейхолдер взял фичу в работу; классификация: новая фича → Аналитик.
- Созданы ветка `feature/logo-images` и единственный worktree `/home/alex/src/my/jellyfin-metadata-plugin-wt-logo-images`.
- Handoff → Аналитик (`.opencode/agents/analyst.md`), вход: `brief.md`.

### 2026-08-24 — создан бриф

- Папка фичи создана из шаблона по запросу Стейхолдера.
- Точка расширения подтверждена: `IRemoteImageProvider.GetSupportedImages` + `ImageType.Logo` доступны в целевом Jellyfin 10.11.x.

### 2026-08-24 — analysis → analysis-review

- Анализ завершён (`analysis.md`): все гипотезы брифа проверены по коду и OpenAPI-спеке, блокирующих вопросов нет.
- Факт: `/v1.5/movie/search` не имеет `selectFields`; `logo` входит в дефолтную схему ответа — условный out-of-scope из брифа закрыт.
- Рекомендация Аналитика: `continue`. Открытый неблокирующий вопрос OQ-1 (логотип в fallback-ветке поиска) — решить на gate Стейхолдера.
- Handoff → Оркестратор: обязательная остановка на решении Стейхолдера перед архитектурой.
