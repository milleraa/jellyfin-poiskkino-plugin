# Brief — `logo-images`

## Цель

Отдавать логотипы тайтлов из поля `logo` API ПоискКино через существующий `PoiskKinoImageProvider` как изображения типа `ImageType.Logo`. Логотип используется Jellyfin-клиентами (веб, TV) для оформления карточек и экранов деталей.

## Обоснование точки расширения

Проверено по исходникам Jellyfin (master):

- `IRemoteImageProvider.GetSupportedImages(BaseItem)` возвращает список поддерживаемых типов; плагин может объявить любой из `MediaBrowser.Model.Entities.ImageType` (официальный TmdbMovieImageProvider поддерживает `Primary`, `Backdrop`, `Logo`, `Thumb`).
- `RemoteImageInfo.Type = ImageType.Logo` — стандартный способ передать URL логотипа.
- Точка расширения доступна в целевой версии плагина **Jellyfin 10.11.x** (пакеты `Jellyfin.Controller`/`Jellyfin.Model` 10.11.5).

Модель уже готова: поле `Logo` декларируется в `PoiskKinoMovieDto.cs:254` (детальные данные `/v1.5/movie/{id}`) и `PoiskKinoItem.cs:134`, но нигде не используется.

## Scope (in)

- Добавить `ImageType.Logo` в `GetSupportedImages` провайдера `PoiskKinoImageProvider`.
- Маппинг `movieData.Logo.Url` → `RemoteImageInfo { Type = ImageType.Logo }` при получении данных по ID.
- Применять существующие правила фильтрации URL (`ImageUrlHelper.ShouldFilterUrl`) и TMDB-фильтр (`ShouldIgnoreTmdbImages`) к логотипам.
- Unit-тесты на новый тип изображения (по аналогии с `PoiskKinoImageProviderTests`).

## Out of scope

- Типы изображений кроме `Logo` (`Thumb`, `Banner`, `Disc` и т.д.).
- Логотипы в результатах поиска (`/v1.5/movie/search`) — если API не отдаёт `logo` в дефолтных полях поиска; проверяется на стадии analysis.
- Загрузка/кэширование изображений самим плагином (используется стандартный `GetImageResponse`).
- Логотипы сеток (`networks.logo`) и watchability — отдельные сущности, не тайтлы.

## Стейкхолдеры

См. [stakeholders](../../../product/stakeholders.md).
