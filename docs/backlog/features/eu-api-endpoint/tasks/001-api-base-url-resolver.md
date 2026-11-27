# Task 001 — `ApiBaseUrlResolver`: политика нормализации/валидации базового URL + unit-тесты (AC-1, AC-3, AC-4)

## Метаданные

- **ID:** 001
- **Feature / Hotfix:** [docs/backlog/features/eu-api-endpoint/](../)
- **Назначено:** `developer-csharp`
- **Стек / infra-область:** C# / .NET 10, xUnit unit-тесты; чистая функция без Jellyfin, HTTP и логгера
- **Статус:** todo
- **Ветка:** `feature/eu-api-endpoint` (worktree `/home/alex/src/my/jellyfin-metadata-plugin`); выполнять первой, task-branch не создавать

## Описание

Добавить единственную точку политики базового URL — новый файл `PoiskKinoMetadataPlugin/ApiBaseUrlResolver.cs` строго по [architecture.md](../architecture.md) D-ARCH-1 и [ADR-0003](../../../../architecture/decisions/ADR-0003-configurable-api-base-url.md):

```csharp
internal readonly record struct ResolvedBaseUrl(Uri BaseAddress, bool UsedFallback, string? FallbackReason);

internal static class ApiBaseUrlResolver
{
    public const string DefaultBaseUrl = "https://api.poiskkino.dev";
    public static ResolvedBaseUrl Resolve(string? rawValue);
}
```

Правила `Resolve` (детерминированные, **никаких исключений наружу**):

| # | Вход | Результат |
|---|------|-----------|
| 1 | `null`, `""`, пробелы | дефолт, `UsedFallback = false`, `FallbackReason = null` |
| 2 | пробелы по краям | обрезаются |
| 3 | нет схемы (`://`) | подставляется `https://` |
| 4 | схема не `https` (`http`, `ftp`, `file`, …) | дефолт + `UsedFallback = true` + `Reason` |
| 5 | пустой host / не парсится как абсолютный `Uri` | дефолт + `UsedFallback = true` + `Reason` |
| 6 | есть `userinfo`, `query` или `fragment` | дефолт + `UsedFallback = true` + `Reason` |
| 7 | непустой путь (кроме `/`) | дефолт + `UsedFallback = true` + `Reason` |
| 8 | завершающие `/` | удаляются → `https://host[:port]` |
| 9 | явный порт | сохраняется |
| 10 | регистр/IDN | нормализует сам `Uri` (host в нижнем регистре, punycode) |

Тесты — новый файл `PoiskKinoMetadataPlugin.UnitTests/Helpers/ApiBaseUrlResolverTests.cs`: xUnit-теория (табличный `[Theory]/[InlineData]`) на каждое правило и на граничные случаи (`"   "`, `"api.poiskkino.dev"`, `"https://eu-api.poiskkino.dev/"`, `"https://eu-api.poiskkino.dev:8443"`, `"http://api.poiskkino.dev"`, `"https://host/path"`, `"https://u:p@host"`, `"https://host?x=1"`, `"https://host#f"`, `"not a url"`, очень длинная строка) + отдельный тест «`Resolve` не бросает исключение ни на одном входе». XML-doc на публичные члены (`GenerateDocumentationFile=true`, baseline предупреждений — только CS1591 по существующим местам, новые CS1591 не добавлять).

## Acceptance criteria

1. **AC-1 (значение дефолта):** `ApiBaseUrlResolver.DefaultBaseUrl == "https://api.poiskkino.dev"`; константа объявлена один раз и является единственным местом хранения дефолта в коде.
2. **AC-3 (нормализация):** правила 1, 2, 3, 8, 10 покрыты теорией построчно; для валидных значений `BaseAddress` — абсолютный `Uri` со схемой `https` и без завершающего `/` в `AbsoluteUri` (кроме корневого).
3. **AC-4 (валидация без падения):** правила 4, 5, 6, 7 возвращают дефолт с `UsedFallback = true` и непустым `FallbackReason`; ни один вход не приводит к `UriFormatException`/`ArgumentException` наружу.
4. Правило 9: явный порт сохраняется в `BaseAddress.Port`.
5. `UsedFallback = false` для пустого/`null` значения (это не ошибка конфигурации) и для любого валидного значения.
6. `ApiBaseUrlResolver` — `internal static`, без `using` из `MediaBrowser.*`, `Microsoft.Extensions.Logging`, `System.Net.Http`; резолвер не знает про API-ключ.
7. Тесты используют существующий `InternalsVisibleTo("PoiskKinoMetadataPlugin.UnitTests")` (`PoiskKinoMetadataPlugin/SearchYearFilter.cs:5`) — правок `.csproj` нет.
8. Поведение плагина не меняется: задача добавляет новый файл и тесты, существующий код не редактируется.
9. `dotnet build PoiskKinoMetadataPlugin.slnx` и `dotnet test PoiskKinoMetadataPlugin.slnx` зелёные.
10. Локальный commit без push.

## Технические заметки

- Источник дизайна: [architecture.md](../architecture.md) D-ARCH-1, D-ARCH-3 (почему отвергается непустой путь), D-ARCH-4 (значение в конфигурации не переписывается — резолвер ничего не сохраняет).
- Не использовать `Uri.TryCreate` без проверки схемы: `http://host` парсится успешно и должен дать fallback (правило 4).
- Проверка «нет схемы» — по `IndexOf("://")`, а не по `Uri.IsWellFormedUriString` (последний отклоняет `host:port` и IDN-кейсы непредсказуемо).
- `FallbackReason` — короткая английская строка-причина (пойдёт в `Warning`-лог в задаче 002), без значения API-ключа и без полного текста конфигурации.
- Стиль чистых хелперов — [`SearchYearFilter.cs`](../../../../../PoiskKinoMetadataPlugin/SearchYearFilter.cs), образец тестов — [`SearchYearFilterTests.cs`](../../../../../PoiskKinoMetadataPlugin.UnitTests/SearchYearFilterTests.cs).
- Запрещено: читать `Plugin.Instance`, логировать, добавлять NuGet-пакеты.

## DevOps impact check

- Новые/изменённые переменные окружения или secrets: **нет**
- Новые внешние интеграции, порты, очереди, storage, scheduled jobs: **нет**
- Docker/compose/CI/CD/deploy/Helm/Kubernetes/observability: **нет**
- Итог: DevOps-задача **не нужна** (новый файл + unit-тесты).

## Проверка

```bash
dotnet build PoiskKinoMetadataPlugin.slnx
dotnet test PoiskKinoMetadataPlugin.slnx
```

Ожидаемо: 0 failed, новые тесты видны в выводе. Если локального кэша `Jellyfin.Controller`/`Jellyfin.Model` 12.0.0 нет — `restore` может тянуть пакеты из сети; это не повод менять версии зависимостей.

## Handoff

- После выполнения: локальный commit без push (например, `feat(api): add ApiBaseUrlResolver for configurable base URL`), статус задачи → `in review`, сообщить Техлиду: путь к файлам, число новых тестов, вывод `dotnet test`.
