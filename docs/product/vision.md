# Vision

*Источники: [`README.md`](../../README.md), [`PoiskKinoMetadataPlugin.csproj`](../../PoiskKinoMetadataPlugin.csproj), [`PoiskKinoMetadataPlugin.cs`](../../PoiskKinoMetadataPlugin.cs).*

## Продукт

- **Название:** PoiskKino Metadata Plugin для Jellyfin (ПоискКино Metadata)
- **Тип продукта:** плагин для медиасервера (Jellyfin plugin / library extension), не отдельное веб-приложение
- **Проблема пользователя:** в Jellyfin не хватает качественных русскоязычных метаданных для фильмов и сериалов из открытых источников с удобной интеграцией в библиотеку
- **Ценность:** автоматическое обогащение библиотеки Jellyfin названиями, описаниями, жанрами, рейтингами (КиноПоиск, IMDb), постерами и внешними ID через API [ПоискКино](https://poiskkino.dev), с кэшированием и учётом лимита API (200 запросов/сутки)
- **Целевая платформа:** Jellyfin 10.9+ (зависимости в csproj: `Jellyfin.Controller` / `Jellyfin.Model` 10.11.5)
- **Лицензия:** GPLv2 ([`README.md`](../../README.md))

## Вне v1 (из README)

Провайдер метаданных для персон (`IPersonMetadataProvider`) — в roadmap, в коде пока не реализован.
