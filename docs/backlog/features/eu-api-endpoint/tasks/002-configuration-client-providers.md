# Task 002 — Конфигурация + клиент + 5 провайдеров: значение доходит до `HttpClient.BaseAddress` (AC-1, AC-2, AC-4, AC-5, AC-8, AC-9, AC-10, AC-12)

## Метаданные

- **ID:** 002
- **Feature / Hotfix:** [docs/backlog/features/eu-api-endpoint/](../)
- **Назначено:** `developer-csharp`
- **Стек / infra-область:** C# / .NET 10, Jellyfin 12.0.0 (`Jellyfin.Controller`/`Jellyfin.Model`), xUnit + Moq (`Moq.Protected`); HTTP-транспорт плагина
- **Статус:** todo
- **Ветка:** `feature/eu-api-endpoint` (worktree `/home/alex/src/my/jellyfin-metadata-plugin`); выполнять строго после задачи [001](001-api-base-url-resolver.md), task-branch не создавать

## Описание

Связать конфигурацию с транспортом: значение `ApiBaseUrl` из `Plugin.Instance.Configuration` должно попадать в `HttpClient.BaseAddress` через резолвер задачи 001. Дизайн — [architecture.md](../architecture.md) D-ARCH-2…D-ARCH-5, D-ARCH-8.

1. **`PoiskKinoMetadataPlugin/Configuration.cs`** — новое свойство:
   ```csharp
   public string ApiBaseUrl { get; set; } = ApiBaseUrlResolver.DefaultBaseUrl;
   ```
   с XML-summary (на русском, как существующие свойства). Имя `ApiBaseUrl` окончательное: оно становится частью XML-схемы конфигурации (риск A8), переименование позже = миграция.

2. **`PoiskKinoMetadataPlugin/PoiskKinoApiClient.cs`**:
   - удалить `private const string ApiBaseUrl = "https://api.poiskkino.dev";` (дефолт живёт только в `ApiBaseUrlResolver.DefaultBaseUrl`);
   - конструктор: `public PoiskKinoApiClient(IHttpClientFactory httpClientFactory, ILogger<PoiskKinoApiClient> logger, string? apiBaseUrl = null)`;
   - в конструкторе: `var resolved = ApiBaseUrlResolver.Resolve(apiBaseUrl);` → `_httpClient.BaseAddress = resolved.BaseAddress;`;
   - логирование один раз на создание клиента: `Information` фактического `BaseAddress` (`"PoiskKino API base URL: {BaseUrl}"`); при `resolved.UsedFallback` — один `Warning` с усечённым до 200 символов сырым значением и `FallbackReason`. `ApiKey` в лог не попадает (резолвер его не получает);
   - ничего больше не менять: `Timeout = 120 s`, `User-Agent`, кэш, `SemaphoreSlim(1,1)`, лимит 200/сутки, обработка 401/403/429/404 и исключений остаются как есть (D-ARCH-8).

3. **5 провайдеров** — `PoiskKinoMovieProvider`, `PoiskKinoSeriesProvider`, `PoiskKinoSeasonProvider`, `PoiskKinoEpisodeProvider`, `PoiskKinoImageProvider`: рядом с существующим `ApiKey` добавить
   ```csharp
   private string ApiBaseUrl => Plugin.Instance?.Configuration?.ApiBaseUrl ?? string.Empty;
   ```
   и передать **сырое** значение вторым/третьим аргументом в `new PoiskKinoApiClient(..., ApiBaseUrl)` внутри `Lazy<PoiskKinoApiClient>`. Никакой нормализации/валидации в провайдерах (границы из architecture.md §2).

4. **Тесты** (новый файл `PoiskKinoMetadataPlugin.UnitTests/ApiClient/PoiskKinoApiClientBaseUrlTests.cs`, плюс один тест проводки в `PoiskKinoMetadataPlugin.UnitTests/Providers/PoiskKinoMovieProviderTests.cs`):
   - дефолт: `new PoiskKinoApiClient(factory, logger)` (2 аргумента) → захваченный `RequestUri.Host == "api.poiskkino.dev"`, `Scheme == "https"` (AC-1, AC-2);
   - кастом: `apiBaseUrl: "https://eu-api.poiskkino.dev"` → все три запроса (`SearchAsync`, `GetMovieByIdAsync`-путь `/v1.5/movie/{id}`, сезонный `/v1.5/season`) идут на `eu-api.poiskkino.dev` (AC-5);
   - нормализация на входе клиента: `"eu-api.poiskkino.dev"`, `"https://eu-api.poiskkino.dev/"`, `"  https://eu-api.poiskkino.dev  "` → все дают `https://eu-api.poiskkino.dev` в запросе (AC-3 в интеграции);
   - fallback: `"http://evil.example"` и `"not a url"` → запрос уходит на дефолт, исключения нет, `Warning` в лог есть, `Information` есть (AC-4, AC-9);
   - `ApiKey` не попадает в лог: логгер-мок проверяется на отсутствие `test-api-key` в отформатированных сообщениях (AC-9);
   - проводка через провайдера: `Plugin.Instance.Configuration.ApiBaseUrl = "https://eu-api.poiskkino.dev"` → `GetSearchResults` даёт захваченный `RequestUri.Host == "eu-api.poiskkino.dev"` (AC-5); после теста вернуть значение в дефолт/пересоздать плагин (`PluginTestFixture.SetUpPlugin`), чтобы не протекало в другие тесты коллекции.

   Для тестов клиента нужен `IHttpClientFactory`-мок, возвращающий `new HttpClient(handlerMock)` **без** `BaseAddress` и без `Timeout` (клиент задаёт их сам); факт запроса собирается через `Callback<HttpRequestMessage, CancellationToken>` по образцу `CreateCapturingHandlerMock` в существующем `ApiClient/PoiskKinoApiClientTests.cs`.

## Acceptance criteria

1. **AC-1:** `PluginConfiguration.ApiBaseUrl` инициализирован `ApiBaseUrlResolver.DefaultBaseUrl`; `XmlSerializer`-десериализация старой конфигурации (`<PluginConfiguration><ApiKey>k</ApiKey><IgnoreTmdbImages>false</IgnoreTmdbImages></PluginConfiguration>`, без `<ApiBaseUrl>`) даёт `ApiBaseUrl == "https://api.poiskkino.dev"` при сохранённых `ApiKey`/`IgnoreTmdbImages` — отдельный unit-тест с `new XmlSerializer(typeof(PluginConfiguration))` (AC-1 + AC-8, кодовая часть).
2. **AC-2:** при пустом/отсутствующем значении все запросы идут на `https://api.poiskkino.dev`; таймаут, кэш, семафор, лимит, обработка ошибок не изменены — подтверждается диффом.
3. **AC-4:** невалидное значение не приводит к исключению при создании клиента или запросе: используется дефолт, пишется ровно один `Warning` на клиент, `UriFormatException`/`ArgumentException` не вылетает.
4. **AC-5:** `RequestUri` всех трёх запросов строится от сконфигурированного base URL (host + scheme проверяются в тестах); `grep` по `PoiskKinoMetadataPlugin/**/*.cs` показывает отсутствие строкового литерала `poiskkino.dev` вне `ApiBaseUrlResolver.cs` (и вне тестов).
5. **AC-5 (проводка):** все 5 провайдеров передают значение из `Plugin.Instance.Configuration` — проверяется ревью диффа (по одной строке свойства + по одному аргументу в `new PoiskKinoApiClient(...)`), бизнес-логика провайдеров не тронута.
6. **AC-9:** `Information` с фактическим базовым URL пишется один раз при создании клиента; `Warning` — только при fallback; `ApiKey` отсутствует в логах; сырое значение в `Warning` усечено до 200 символов.
7. **AC-10:** `dotnet build -c Release PoiskKinoMetadataPlugin.slnx` без новых предупреждений (baseline — только CS1591 по существующим местам) и `dotnet test PoiskKinoMetadataPlugin.slnx` зелёный; существующие тесты не переписаны «под результат» — 2-аргументный конструктор остаётся валидным и даёт прежний хост.
8. **AC-12:** нет новых NuGet-пакетов, env-переменных, секретов, фоновых задач; сетевой хост по умолчанию тот же.
9. Локальный commit без push.

## Технические заметки

- Зависимость: задача 001 принята Техлидом (`ApiBaseUrlResolver`, `ResolvedBaseUrl`, `DefaultBaseUrl`).
- `HttpClient.BaseAddress` можно переустановить только до первого запроса — в тестах создавать новый `HttpClient` на тест/провайдер, не переиспользовать отправленный.
- `IHttpClientFactory.CreateClient()` вызывается без имени (как сейчас), `BaseAddress` ставится на конкретный экземпляр (D-ARCH-7).
- Клиент **не читает** `Plugin.Instance` — значение приходит параметром (DIP, тестируемость); провайдеры передают сырое значение, нормализация только в резолвере (D-ARCH-2).
- Не логировать заголовки и `ApiKey`; в `Warning` не выводить полный текст конфигурации (D-ARCH-5).
- Не добавлять нормализацию в `Plugin.UpdateConfiguration` и не записывать нормализованное значение обратно в конфигурацию (D-ARCH-4).
- Запрещено: менять относительные пути запросов, вводить поддержку base URL с подпутём (D-ARCH-3), трогать `ImageUrlHelper`/`SearchYearFilter`.

## DevOps impact check

- Новые/изменённые переменные окружения или secrets: **нет** (значение — в XML-конфигурации плагина)
- Новые внешние интеграции, порты, очереди, storage, scheduled jobs: **нет** (`eu-api.poiskkino.dev` уже объявлен API, используется только по выбору администратора)
- Docker/compose/CI/CD/deploy/Helm/Kubernetes/observability: **нет** (остаётся существующий `ILogger`; runbook не требуется)
- Итог: DevOps-задача **не нужна**.

## Проверка

```bash
dotnet build -c Release PoiskKinoMetadataPlugin.slnx
dotnet test PoiskKinoMetadataPlugin.slnx
rg -n "poiskkino\.dev" PoiskKinoMetadataPlugin --glob '!obj/**' --glob '!bin/**'
```

Ожидаемо: сборка без новых предупреждений, `0 failed`, единственное вхождение литерала хоста — в `ApiBaseUrlResolver.cs`. Полный end-to-end smoke с живым ключом в этой задаче не выполняется (ключ отсутствует, R2/A7) — хост проверяется по `RequestUri` в unit-тестах.

## Handoff

- После выполнения: локальный commit без push (например, `feat(api): use configurable base URL in API client and providers`), статус задачи → `in review`, сообщить Техлиду: список изменённых файлов, число новых тестов, вывод `build`/`test` и `rg`-проверки, явно указать, что в диффе нет изменений таймаута/кэша/лимита.
