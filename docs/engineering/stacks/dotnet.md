# C# / .NET standards

**Статус:** активен в v1.

*Источники: [`PoiskKinoMetadataPlugin.csproj`](../../../PoiskKinoMetadataPlugin.csproj), [`README.md`](../../../README.md).*

## Runtime и tooling

| Параметр | Значение |
|----------|----------|
| Target framework | `net9.0` |
| LangVersion | `latest` |
| Nullable | `enable` |
| GenerateDocumentationFile | `true` |
| Версия плагина | `1.0.0` |

**Команды:**

```bash
dotnet restore
dotnet build -c Release
```

## Зависимости (NuGet)

- `Jellyfin.Controller` 10.11.5
- `Jellyfin.Model` 10.11.5
- `Microsoft.Extensions.Http` 9.0.0

## Архитектура

- Один assembly, namespace `PoiskKinoMetadataPlugin`
- Плагин: `Plugin : BasePlugin<PluginConfiguration>, IHasWebPages`
- Провайдеры метаданных/изображений реализуют интерфейсы Jellyfin (`IRemoteMetadataProvider`, `IRemoteImageProvider`, `IExternalId`)
- HTTP: `PoiskKinoApiClient` + `IHttpClientFactory` (регистрация на стороне Jellyfin)
- Модели DTO: папка `Models/`
- Встроенный ресурс: `Configuration/configPage.html`

## Async и ошибки

- `async`/`await` с `CancellationToken` в API-клиенте и провайдерах
- Логирование через `ILogger<T>`; ошибки API 403/429/404 — см. README

## Тестирование

- Отдельного test project в репозитории **нет**
- Ручная проверка: сборка DLL и установка в каталог плагинов Jellyfin ([`README.md`](../../../README.md))

## Связанные правила Cursor

- `.cursor/rules/csharp-developer.mdc`
- `.cursor/agents/developer-csharp.md`
