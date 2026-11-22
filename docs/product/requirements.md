# Requirements

*Источники: [`README.md`](../../README.md), провайдеры `PoiskKino*Provider.cs`, [`Configuration.cs`](../../Configuration.cs), [`PoiskKinoApiClient.cs`](../../PoiskKinoApiClient.cs).*

Глобальные и кросс-фичевые требования. Детализация по фиче — в `docs/backlog/features/<feature>/analysis.md`.

## Функциональные (реализовано в v1)

- Метаданные фильмов (`IMovieMetadataProvider` — `PoiskKinoMovieProvider`)
- Метаданные сериалов (`ISeriesMetadataProvider` — `PoiskKinoSeriesProvider`)
- Метаданные сезонов и эпизодов (`PoiskKinoSeasonProvider`, `PoiskKinoEpisodeProvider`)
- Изображения: постеры и фоны (`IRemoteImageProvider` — `PoiskKinoImageProvider`)
- Внешние идентификаторы (`IExternalId` — `PoiskKinoExternalId`)
- Русские названия, описания, жанры; рейтинги КиноПоиск и IMDb
- Настройка API-ключа и опции `IgnoreTmdbImages` через веб-страницу плагина (`Configuration/configPage.html`)
- Поиск по названию и году через API `https://api.poiskkino.dev` (заголовок `X-API-KEY`)

## Нефункциональные (NFR)

- **Лимит API:** 200 запросов/сутки (демо-ключ); кэш результатов поиска 24 ч по ключу `название:год` ([`README.md`](../../README.md), [`PoiskKinoApiClient.cs`](../../PoiskKinoApiClient.cs))
- **Ошибки API:** обработка 403, 429, 404, таймаутов; кэш 404 на 1 ч
- **Потокобезопасность:** `ConcurrentDictionary`, `SemaphoreSlim`, `IHttpClientFactory`
- **Совместимость:** .NET 10 (`net10.0`), Jellyfin plugin API 12.x (сборка против `Jellyfin.Controller` / `Jellyfin.Model` 12.0.0). Политика поддержки — **последняя стабильная Jellyfin** (подтверждено Стейхолдером, 2026-08-23); сейчас 12.x; работа на версиях ниже 12.0 не проверялась
- **SLA / uptime:** не применимо (плагин внутри Jellyfin); лимит API **200 запросов/сутки** достаточен (подтверждено Стейхолдером, 2026-05-18)

## Вне scope (текущий горизонт)

- Провайдер персон (`IPersonMetadataProvider`) — см. «Планируемые возможности» в README
- Публикация в официальный каталог плагинов Jellyfin (GitHub Releases реализованы: `.github/workflows/release.yaml`)
