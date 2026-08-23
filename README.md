# PoiskKino Metadata Plugin для Jellyfin

Плагин для Jellyfin 10.11+, который предоставляет метаданные фильмов и сериалов на русском языке через API [ПоискКино](https://poiskkino.dev).

## Возможности

- ✅ Получение метаданных для фильмов (IMovieMetadataProvider)
- ✅ Получение метаданных для сериалов (ISeriesMetadataProvider)
- ✅ Русские названия, описания и жанры
- ✅ Рейтинги КиноПоиск и IMDb
- ✅ Постеры и фоновые изображения (IImageProvider)
- ✅ Кэширование запросов для соблюдения лимита API (200 запросов/сутки)
- ✅ Обработка ошибок (403 Forbidden, 429 Too Many Requests, 404, таймауты)
- ✅ Настройка API-ключа через веб-интерфейс Jellyfin

## Планируемые возможности

- 🔲 Провайдер метаданных для персон (IPersonMetadataProvider)
  - Отдельные страницы персон (актеры, режиссеры и т.д.)
  - Биографии и дополнительная информация
  - Фильмография актеров
  - Поиск персон по имени

## Требования

- Jellyfin 10.11 или выше (последняя стабильная; сборка против API 10.11.5)
- .NET 9

## Установка

1. Соберите проект:
   ```bash
   dotnet build -c Release
   ```

2. Скопируйте скомпилированную библиотеку (`PoiskKinoMetadataPlugin.dll`) в директорию плагинов Jellyfin:
   - Linux: `/var/lib/jellyfin/plugins/`
   - Windows: `%ProgramData%\Jellyfin\Server\plugins\`
   - Docker: `/config/plugins/`

3. Перезапустите Jellyfin

## Настройка

1. Перейдите в **Dashboard** → **Plugins** → **PoiskKino Metadata**
2. Введите ваш API-ключ от ПоискКино
3. Сохраните настройки

### Получение API-ключа

API-ключ можно получить на [poiskkino.dev](https://poiskkino.dev). Демо-ключ ограничен 200 запросами в сутки.

## API ПоискКино

Документация API: https://api.poiskkino.dev/documentation-json

Пример запроса:
```
GET https://api.poiskkino.dev/v1/search?query={title}&year={year}
Headers:
  X-API-KEY: ваш_api_ключ
```

## Особенности реализации

### Кэширование

Плагин кэширует результаты поиска на 24 часа по ключу `название:год` для минимизации запросов к API и соблюдения лимита 200 запросов в сутки.

### Обработка ошибок

- **403 Forbidden / 429 Too Many Requests**: Логируется предупреждение с сообщением от API (превышен суточный лимит)
- **404 Not Found**: Результат кэшируется на 1 час (чтобы не повторять неуспешные запросы)
- **Таймауты**: Логируются как предупреждения

### Потокобезопасность

Все классы используют потокобезопасные структуры данных:
- `ConcurrentDictionary` для кэша
- `SemaphoreSlim` для синхронизации запросов
- `HttpClient` через `IHttpClientFactory` (регистрируется Jellyfin)

## Разработка

### Зависимости

- `Jellyfin.Controller` (10.11.5)
- `Jellyfin.Model` (10.11.5)
- `Microsoft.Extensions.Http` (9.0.0)

### Сборка

```bash
dotnet restore
dotnet build -c Release
```

### Тестирование

Плагин можно протестировать локально, разместив DLL в директории плагинов Jellyfin и перезапустив сервер.

## Лицензия

GPLv2

## Ссылки

- [Документация Jellyfin по разработке плагинов](https://docs.jellyfin.org/general/plugin/)
- [Шаблон плагина Jellyfin](https://github.com/jellyfin/jellyfin-plugin-template)
- [Пример плагина метаданных (AniList)](https://github.com/jellyfin/jellyfin-plugin-anilist)
- [API ПоискКино](https://api.poiskkino.dev/documentation-json)

