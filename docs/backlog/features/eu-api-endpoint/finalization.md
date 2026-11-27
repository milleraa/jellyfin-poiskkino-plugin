# Finalization — `eu-api-endpoint`

## PR/MR

- **Ссылка:** _TBD — заполняется сразу после `gh pr create` (отдельный commit со ссылкой)_
- **Ветка:** `feature/eu-api-endpoint` ← `main` (push в `origin`; ветку и remote branch не удалять до merge/close PR)
- **Диапазон коммитов:** `4610179..3ef319c` (23 коммита: снапшот спеки + intake → анализ → решения Стейхолдера → архитектура + ADR-0003 → декомпозиция → задачи 001–005 → QA)
- **Commit со ссылкой на PR/MR:** _TBD_
- **Issue:** [#8](https://github.com/milleraa/jellyfin-poiskkino-plugin/issues/8) — `feat(config): настраиваемый базовый URL API — зеркало eu-api.poiskkino.dev` (label `enhancement`)

## Что вошло в PR

| Область | Состав |
|---------|--------|
| **Резолвер** | `PoiskKinoMetadataPlugin/ApiBaseUrlResolver.cs` (новый, 130 строк): дефолт-константа, нормализация (trim, схема по умолчанию `https`, усечение завершающего `/`), правила валидации, результат `ResolvedBaseUrl(Uri, UsedFallback, Reason)` — без исключений на любом входе |
| **Конфигурация** | `PoiskKinoMetadataPlugin/Configuration.cs`: `PluginConfiguration.ApiBaseUrl` с инициализатором-дефолтом `https://api.poiskkino.dev` (отсутствующий элемент legacy-XML оставляет дефолт, миграция не нужна) |
| **Клиент** | `PoiskKinoApiClient.cs`: необязательный 3-й параметр конструктора `string? apiBaseUrl = null`, однократный резолв → `HttpClient.BaseAddress`, `Information` с фактическим хостом / `Warning` с причиной fallback; `RedactUserInfo` (вырезает весь `userinfo` до последнего `@` в authority) + `TruncateForLog(200)`; `ApiKey` не логируется |
| **Провайдеры** | 5 провайдеров (`PoiskKinoMovieProvider`, `PoiskKinoSeriesProvider`, `PoiskKinoSeasonProvider`, `PoiskKinoEpisodeProvider`, `PoiskKinoImageProvider`) передают сырое значение `ApiBaseUrl` из `Plugin.Instance.Configuration` по существующему паттерну `ApiKey`; инварианты (таймаут 120 с, кэш 24 ч, лимит 200/сутки, `SemaphoreSlim(1, 1)`, обработка 401/403/429/404) не тронуты |
| **UI** | `Configuration/configPage.html`: поле «Базовый URL API» (`type="text"`, `is="emby-input"`, `<datalist>` с двумя хостами), дефолт при отсутствии значения, сохранение без канонизации, подсказка с явной фразой «Изменение применяется после перезапуска Jellyfin» (AC-6, AC-7′) |
| **Тесты** | 4 файла: `Helpers/ApiBaseUrlResolverTests.cs` (71), `ApiClient/PoiskKinoApiClientBaseUrlTests.cs` (35), `ConfigurationTests.cs` (3), `Providers/PoiskKinoMovieProviderTests.cs` (1) — всего **281/281** в проекте |
| **Документация** | `README.md` (оба хоста, дефолт, правила нормализации, «после перезапуска»), `docs/architecture/overview.md` |
| **ADR** | `docs/architecture/decisions/ADR-0003-configurable-api-base-url.md` + обновлён `docs/architecture/decisions/README.md` |
| **Снапшот спеки** | `api-docs/documentation.yaml` (23 пути, в `servers` добавлен `https://eu-api.poiskkino.dev`) + раздел «Снапшот спецификации» в `api-docs/AGENTS.md` — смежное решение Стейхолдера, отдельным PR не ведётся |
| **Процесс / QA** | папка `docs/backlog/features/eu-api-endpoint/**` (brief, analysis, architecture, tech-plan, tasks 001–005, qa, status, README, process-review) и запись в `docs/qa/test-runs.md` |

**Ветка не пушилась ранее** — это первый push `feature/eu-api-endpoint` в `origin`.

## Delivery checklist

См. `.opencode/skills/finalization-check/SKILL.md` и [DoD](../../../process/definition-of-done.md) («Ready for stakeholder review»):

- [x] Все задачи `tasks/001`–`tasks/005` закрыты (`done`), отменённых и открытых нет — см. [status.md](status.md), [tasks/README.md](tasks/README.md).
- [x] Работа велась в одной ветке `feature/eu-api-endpoint`, задачи выполнялись строго последовательно (001 → 002 → 003 → 004 → rework 005).
- [x] Все ролевые шаги зафиксированы локальными commit без push (`4610179..3ef319c`, 23 коммита); рабочее дерево чистое перед финализацией.
- [x] Техлид принял реализацию до QA (2026-11-28, rev `023460e..0b4dd23`) и повторно принял rework задачи 005 (rev `6053daf..6af332b`).
- [x] Техлид выполнил DevOps impact check после разработки: **impact нет**, стадия `devops-check` не открывалась (подтверждено трижды — по дизайну, на приёмке реализации и на приёмке rework).
- [x] DevOps-обновления не требуются: новых env vars/secrets/интеграций/портов/миграций/зависимостей и изменений Docker/CI/CD/deploy нет; `docs/engineering/devops.md` и `docs/environments/ci-cd.md` актуальны без правок.
- [x] QA дала вердикт **pass** (2026-11-28, [qa.md](qa.md); AC-1…AC-12 с AC-7′; дефектов нет).
- [x] Тесты и проверки зафиксированы в [qa.md](qa.md) и [test-runs](../../../qa/test-runs.md).
- [ ] Push выполнен, PR/MR открыт, ссылка записана в `status.md` и в этом файле, отдельный commit со ссылкой сделан, повторный push выполнен — _в процессе: заполняется commit'ом со ссылкой на PR_.
- [x] Рабочая и remote ветки сохранены (до merge/close PR; `--force` не применяется, ветки не удаляются).
- [x] Документация и ADR обновлены при изменении поведения: `README.md`, `docs/architecture/overview.md`, ADR-0003 + индекс ADR.
- [x] Папка фичи остаётся в `docs/backlog/features/eu-api-endpoint/` до принятия Стейхолдером и **не** переносится в `docs/archive/` на стадии `pr-mr-ready`.
- [x] Секретов в диффе нет: проверено по именам файлов (`.env`, `*.key`, `*.pem`, `*.pfx`, `credentials*`, `*token*` — нет) и по содержимому диффа (единственное совпадение с шаблоном «секрет» — тестовый литерал `private const string TestApiKey = "test-api-key"` в unit-тестах).

## Финальные проверки (выполнены Финализатором, 2026-11-28)

Среда: .NET SDK `10.0.115`, `net10.0`, Jellyfin `12.0.0`.

```bash
dotnet build PoiskKinoMetadataPlugin.slnx -c Release --no-incremental
# 0 ошибок; 122 предупреждения, все CS1591 (baseline: 61 + 61 на два проекта), новых типов предупреждений нет

dotnet test PoiskKinoMetadataPlugin.slnx -c Release
# Пройдено: 281, не пройдено: 0, пропущено: 0, всего 281
```

CI (GitHub Actions, `.github/workflows/ci.yaml`): build + test на push/PR в `main`; статус run для `feature/eu-api-endpoint` — см. `status.md` (Changelog) и сам PR (check «build»/«test» в одном job). Workflow настроен на path-ignore `*.md` и `docs/` — изменения кода в этом PR триггерят CI.

## Test plan для ревьюера

### 1. Автоматические проверки (локально, ~30 с)

```bash
dotnet build PoiskKinoMetadataPlugin.slnx -c Release --no-incremental   # ожидается 0 ошибок, только baseline CS1591 = 122
dotnet test PoiskKinoMetadataPlugin.slnx -c Release                     # ожидается 281/281
```

Точечно, по слоям (опционально):

```bash
dotnet test PoiskKinoMetadataPlugin.slnx -c Release --filter "FullyQualifiedName~ApiBaseUrlResolverTests"      # 71
dotnet test PoiskKinoMetadataPlugin.slnx -c Release --filter "FullyQualifiedName~PoiskKinoApiClientBaseUrlTests" # 35
dotnet test PoiskKinoMetadataPlugin.slnx -c Release --filter "FullyQualifiedName~PluginConfigurationTests"      # 3
```

Негативный контроль (проверяет, что тесты действительно различают поведение): временно заменить в конструкторе `PoiskKinoApiClient` вызов `ApiBaseUrlResolver.Resolve(apiBaseUrl)` на `Resolve(null)` → должны упасть 18 из 35 тестов клиента (`Expected: "eu-api.poiskkino.dev"`, `Actual: "api.poiskkino.dev"`). После проверки — `git checkout -- <файл>`.

Статическая проверка инвариантов:

```bash
rg -n "poiskkino\.dev" PoiskKinoMetadataPlugin --glob '*.cs' --glob '!obj/**' --glob '!bin/**'
# единственный литерал хоста — ApiBaseUrlResolver.cs (дефолт-константа)
rg -n "Timeout|CacheExpirationHours|MaxRequestsPerDay|SemaphoreSlim" PoiskKinoMetadataPlugin --glob '*.cs' --glob '!obj/**'
# прежние значения: 120 с / 24 ч / 200 / SemaphoreSlim(1, 1)
```

### 2. Ручной UI-сценарий (живой Jellyfin; HTML автотестами не покрыт)

Из [task 003](tasks/003-config-page-field.md):

1. Dashboard → Plugins → PoiskKino Metadata → открыть настройки. На свежей и на старой конфигурации в поле «Базовый URL API» виден `https://api.poiskkino.dev` (не пусто).
2. Ввести `https://eu-api.poiskkino.dev` → «Сохранить» → дождаться успешного уведомления → F5: значение на месте; в `plugins/configurations/PoiskKinoMetadataPlugin.xml` элемент `<ApiBaseUrl>` содержит введённое значение **без канонизации**.
3. Ввести `eu-api.poiskkino.dev/` → «Сохранить» → после перезагрузки страницы показано ровно `eu-api.poiskkino.dev/` (JS не нормализует); после рестарта Jellyfin в логе `Information` «PoiskKino API base URL: https://eu-api.poiskkino.dev/» и запросы идут на EU-хост.
4. Очистить поле → «Сохранить» → перезагрузить страницу: подставлен дефолт `https://api.poiskkino.dev`; после рестарта в логе только `Information` с дефолтом, `Warning` нет.
5. Ввести заведомо невалидное `http://api.poiskkino.dev` → «Сохранить» → после рестарта один `Warning` с причиной, поиск работает на дефолтном хосте, страница показывает введённое значение.
6. Полный end-to-end с реальным поиском на EU-хосте требует API-ключа (R2/A7) — **в этом PR не проверялся**; остаётся за администратором/Стейхолдером после релиза.

### 3. Проверка логов (security, AC-9)

- `ApiKey` из конфигурации никогда не попадает в лог (тест `ApiKey_NeverAppearsInLogs`).
- В `Warning` нет `userinfo`: `https://admin:p@ss@host` логируется как `https://host` (задача 005).
- Остаточный риск: ключ, **вставленный в поле базового URL**, редактируется только в форме `userinfo`; query/fragment и «голое значение» попадают в лог как часть адреса — см. раздел «Риски».

## Риски для ревьюера

| ID | Риск | Оценка / что делать |
|----|------|---------------------|
| **A1** | Значение применяется не мгновенно и не одновременно: 5 клиентов создаются лениво, поэтому без перезапуска возможен кратковременный «смешанный» набор хостов | Средний, **осознанно принят** с AC-7′ (решение Стейхолдера: перезапуск допустим). Смягчение: `README.md` и подсказка в UI явно говорят «после перезапуска»; `Information` логирует фактический хост |
| **A3 / R7** | Хост не входит в ключ in-memory кэша (24 ч): в одном процессе при смене хоста теоретически возможно переиспользование закэшированных результатов с другого зеркала | Низкий при restart-семантике (кэш живёт в процессе, рестарт его сбрасывает). Инвариант «хост обязан попасть в ключ кэша» зафиксирован в ADR-0003 для будущего live-переключения |
| **R2 / A7** | Нет реального API-ключа → end-to-end поиск на EU-зеркале не проверен ни локально, ни в CI | Средний. Смягчение: unit-тесты фиксируют фактический `RequestUri` всех трёх запросов; пробы без ключа `GET /v1.5/token` → `401` на обоих хостах (оба зеркала живы); полный smoke — ручной за администратором |
| **Residual risk (ключ в поле базового URL)** | Если администратор вставит API-ключ в поле «Базовый URL API»: форма `https://KEY@host` безопасна (userinfo вырезается, в `Warning` остаётся `https://host`), но ключ в query/fragment (`https://host/?token=KEY`) или «голым значением» (`https://a1b2c3d4-…` → single-label host) попадёт в лог как часть адреса | Низкий/принят Техлидом и QA. Требуется ошибка администратора; ключ из конфигурации при этом не затронут (AC-9 зелёный); лог доступен тому же администратору. Follow-up (вне scope): вырезать query/fragment при логировании и добавить в UI/README подсказку «поле — только для адреса» |
| Прочее | `datalist` на `is="emby-input"` в Jellyfin 12.x автотестами не покрыт; IPv6-литерал и base URL с подпутём → fallback + `Warning` (подпуть документирован, IPv6 — нет); живой UI-сценарий не прогонялся агентом | Не блокирует. `datalist` удаляется тремя строками без влияния на поле |

Rollout / rollback: деплой — обычное ручное копирование DLL в каталог плагинов ([deployment targets](../../../environments/deployment-targets.md)). Значение по умолчанию совпадает с прежним поведением, поэтому откат — возврат прежней DLL или очистка `<ApiBaseUrl>`; миграции данных нет.

## Acceptance/archive

- **Accepted:** _нет_ — стадия `pr-mr-ready`, решение за Стейхолдером (`/accept-feature`). Merge PR Финализатором **не** выполняется.
- После явного acceptance и **до merge**: папка переносится в `docs/archive/features/eu-api-endpoint/`, запись добавляется в [completed-work](../../../history/completed-work.md), в архивном `status.md` выставляется `accepted`; commit + push в эту же ветку, затем merge одним мержем.
- Если PR не принят: работа возвращается через `.opencode/skills/pr-mr-rework/SKILL.md`, без переноса в архив.
