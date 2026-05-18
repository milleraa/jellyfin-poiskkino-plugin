# Architecture overview

*Источники: [`PoiskKinoMetadataPlugin.cs`](../../PoiskKinoMetadataPlugin.cs), `PoiskKino*Provider.cs`, [`PoiskKinoApiClient.cs`](../../PoiskKinoApiClient.cs), [`Api-docs/documentation.yaml`](../../Api-docs/documentation.yaml).*

## Контекст

Плагин расширяет медиасервер **Jellyfin**: Jellyfin вызывает провайдеры плагина при идентификации и обогащении медиатеки; плагин обращается к внешнему REST API **ПоискКино**.

```text
[Jellyfin Server]
    → Plugin (BasePlugin + Configuration)
        → Metadata providers (Movie, Series, Season, Episode)
        → Image provider
        → External ID provider
        → PoiskKinoApiClient (cache, rate awareness)
            → HTTPS → api.poiskkino.dev (X-API-KEY)
    ← метаданные / URL изображений в модель Jellyfin
```

## Компоненты

| Компонент | Назначение |
|-----------|------------|
| `Plugin` | Точка входа, конфигурация, embedded config page |
| `PoiskKinoApiClient` | HTTP, JSON, кэш 24 ч, семафор запросов |
| `PoiskKinoMovieProvider` / `Series` / `Season` / `Episode` | `IRemoteMetadataProvider` |
| `PoiskKinoImageProvider` | `IRemoteImageProvider` |
| `PoiskKinoExternalId` | `IExternalId` |
| `Models/*` | DTO ответов API |
| `Configuration/configPage.html` | UI настроек в Dashboard Jellyfin |

## Интеграции

| Система | Направление | Примечание |
|---------|-------------|------------|
| Jellyfin 10.9+ | host | plugin API `Jellyfin.Controller` / `Model` 10.11.5 |
| api.poiskkino.dev | исходящий REST | поиск, метаданные; лимит 200 req/day |
| TMDB (изображения) | опционально | `IgnoreTmdbImages` в конфиге |

## Данные

- **БД плагина:** нет; персистентность настроек — механизм Jellyfin для plugin configuration
- **Кэш:** in-memory `ConcurrentDictionary` в `PoiskKinoApiClient`

## Deployment unit

Один .NET 9 assembly (`PoiskKinoMetadataPlugin.dll`), GPLv2.

## Ссылки

- [Principles](principles.md)
- [ADR index](decisions/README.md)
- [OpenAPI / docs snapshot](../../Api-docs/documentation.yaml)
