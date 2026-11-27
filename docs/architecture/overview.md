# Architecture overview

*Источники: [`PoiskKinoMetadataPlugin.cs`](../../PoiskKinoMetadataPlugin.cs), `PoiskKino*Provider.cs`, [`PoiskKinoApiClient.cs`](../../PoiskKinoApiClient.cs), [`api-docs/documentation.yaml`](../../api-docs/documentation.yaml).*

## Контекст

Плагин расширяет медиасервер **Jellyfin**: Jellyfin вызывает провайдеры плагина при идентификации и обогащении медиатеки; плагин обращается к внешнему REST API **ПоискКино**.

```text
[Jellyfin Server]
    → Plugin (BasePlugin + Configuration)
        → Metadata providers (Movie, Series, Season, Episode)
        → Image provider
        → External ID provider
        → PoiskKinoApiClient (cache, rate awareness)
            → HTTPS → api.poiskkino.dev | eu-api.poiskkino.dev (X-API-KEY, базовый URL настраивается)
    ← метаданные / URL изображений в модель Jellyfin
```

## Компоненты

| Компонент | Назначение |
|-----------|------------|
| `Plugin` | Точка входа, конфигурация, embedded config page |
| `PoiskKinoApiClient` | HTTP, JSON, кэш 24 ч, семафор запросов |
| `ApiBaseUrlResolver` | Политика базового URL (ADR-0003): дефолт, нормализация, fallback без исключений |
| `PoiskKinoMovieProvider` / `Series` / `Season` / `Episode` | `IRemoteMetadataProvider` |
| `PoiskKinoImageProvider` | `IRemoteImageProvider` |
| `PoiskKinoExternalId` | `IExternalId` |
| `Models/*` | DTO ответов API |
| `Configuration/configPage.html` | UI настроек в Dashboard Jellyfin |

## Интеграции

| Система | Направление | Примечание |
|---------|-------------|------------|
| Jellyfin 12.x | host | plugin API `Jellyfin.Controller` / `Model` 12.0.0 (.NET 10 ABI) |
| api.poiskkino.dev / eu-api.poiskkino.dev | исходящий REST | базовый адрес настраивается (`PluginConfiguration.ApiBaseUrl`): по умолчанию `api.poiskkino.dev`, зеркало `eu-api.poiskkino.dev`; эндпоинты v1.5: `movie/search`, `movie/{id}`, `season`; лимит 200 req/day; смена — после перезапуска Jellyfin (ADR-0003) |
| TMDB (изображения) | опционально | `IgnoreTmdbImages` в конфиге |

## Данные

- **БД плагина:** нет; персистентность настроек — механизм Jellyfin для plugin configuration
- **Кэш:** in-memory `ConcurrentDictionary` в `PoiskKinoApiClient`

## Deployment unit

Один .NET 10 assembly (`PoiskKinoMetadataPlugin.dll`), GPLv2.

## Ссылки

- [Principles](principles.md)
- [ADR index](decisions/README.md)
- [OpenAPI / docs snapshot](../../api-docs/documentation.yaml)
