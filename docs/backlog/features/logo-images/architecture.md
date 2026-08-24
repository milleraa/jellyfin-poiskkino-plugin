# Architecture — `logo-images`

## Контекст

Ссылки на глобальный обзор: [architecture overview](../../../architecture/overview.md).

Входы: [brief.md](brief.md), [analysis.md](analysis.md), решение Стейхолдера от 2026-08-24 (OQ-1 **в scope**; OQ-2 — на решение Архитектора).

Фича расширяет существующий адаптер изображений `PoiskKinoImageProvider : IRemoteImageProvider` третьим типом — `ImageType.Logo`. Источник данных — поле `logo` схемы `Logo { url }`, которое API ПоискКино уже отдаёт и в `/v1.5/movie/{id}` (`Models/PoiskKinoMovieDto.cs:254`), и в дефолтной схеме ответа поиска `SearchMovieDtoV1_4` (`Models/PoiskKinoItem.cs:134`). Дополнительных HTTP-запросов не требуется.

## Компоненты и границы

Изменение полностью локализовано в **инфраструктурном адаптере** Jellyfin; доменных сущностей в плагине нет, границы и интеграции не меняются.

| Компонент | Изменение | Слой |
|-----------|-----------|------|
| `PoiskKinoImageProvider` | `GetSupportedImages` → `[Primary, Backdrop, Logo]`; маппинг `logo.url` в обеих ветках `GetImages`; обновить summary в XML-доке класса («постеры, фоны и логотипы») | Adapter (Jellyfin Controller) |
| `Models/PoiskKinoMovieDto`, `Models/PoiskKinoItem`, `Models/PoiskKinoImage` | Без изменений — поля `Logo` уже декларированы | DTO |
| `ImageUrlHelper` | Без изменений — пост-фильтр TMDB применяется к итоговому списку как есть | Helper |
| `Plugin` / Configuration / DI | Без изменений — новых зависимостей конструктора нет | Host |

Clean architecture / DI: провайдер уже получает `IHttpClientFactory` и `ILogger*` через конструктор; новые зависимости не вводятся, бизнес-логики вне адаптера не появляется. Нарушений принципа инверсии зависимостей нет.

## Потоки данных

### Ветка A — полные данные по ID (основная)

```text
Jellyfin GetImages(item)
  → ProviderId PoiskKino → ApiClient.GetMovieByIdAsync(id)
      → movieData.Logo?.Url
          null / пусто / whitespace → логотип не добавляется (AC-3), постер/фон как раньше
          иначе → images.Add({ Url, Type = ImageType.Logo, ProviderName })
  → пост-фильтр TMDB (существующий) поверх всего списка, включая Logo
```

Добавление логотипа — после маппинга Poster/Backdrop из `movieData` (`PoiskKinoImageProvider.cs:163–181`); детерминированный порядок в списке: Primary → Backdrop → Logo.

Guard для URL логотипа: `string.IsNullOrWhiteSpace` (требование AC-3 про whitespace). Соседние проверки Poster/Backdrop остаются на `IsNullOrEmpty`, чтобы не менять их поведение (AC-4). Осознанная локальная несимметричность — см. Риски/Follow-up.

### Ветка B — fallback «только данные из поиска» (решение Стейхолдера по OQ-1: в scope)

```text
GetMovieByIdAsync недоступен (нет ID / ошибка / Season без ID — поиск пропускается)
  → ApiClient.SearchAsync(title, year)
      → выбор кандидата (существующая эвристика IsSeries/year/name)
      → apiItem.Logo?.Url → images.Add({ Url, Type = ImageType.Logo })   // НОВОЕ
      → return images (постер/фон/логотип из поиска)
```

Маппинг симметричен постеру/фону fallback-ветки (`PoiskKinoImageProvider.cs:128–153`), тот же guard `IsNullOrWhiteSpace`.

### Фильтрация TMDB (US-2)

Единая точка фильтрации — существующий пост-фильтр в конце `GetImages`: при `IgnoreTmdbImages=true` из списка удаляются все URL с `tmdb.org`, независимо от типа изображения. Логотипы покрываются автоматически, отдельный код не нужен.

## Решения

### OQ-2 — защитный вызов `ShouldFilterUrl` при маппинге логотипа: **НЕ вводить**

Решение Архитектора (2026-08-24):

1. **Одна точка применения политики** (SRP): в `GetImages` политика «игнорировать TMDB» уже применяется ровно один раз — пост-фильтром ко всему списку. Дублирование проверки при добавлении элемента создаёт два места, которые нужно синхронно менять при изменении политики.
2. **Функциональная эквивалентность**: `ShouldFilterUrl(url)` ≡ `ShouldIgnoreTmdbImages() && IsTmdbUrl(url)` — ровно то же условие, что применяет пост-фильтр. Защитный вызов не меняет поведение ни в одном кейсе (подтверждено анализом, analysis.md:14).
3. **Консистентность внутри компонента важнее кросс-компонентной**: в metadata-провайдерах `ShouldFilterUrl` вызывается при маппинге единичных URL в модель Jellyfin — там это оправдано контрактом. В image-провайдере список формируется целиком внутри метода, и единый выходной фильтр проще для рассуждений и тестирования.
4. **Триггер пересмотра**: если появится требование фильтровать источники дифференцированно по типу изображения (например, «логотипы из TMDB — да, постеры — нет») — это будет значимым решением и поводом для ADR. Сегодня такого драйвера нет.

### Новый ADR — НЕ требуется

Зафиксировано явно: изменение является тривиальным расширением возможностей существующего адаптера — новый тип изображения в стандартной точке расширения Jellyfin (`IRemoteImageProvider.GetSupportedImages` / `RemoteImageInfo.Type`), без новых границ, интеграций, зависимостей или компромиссов; решение полностью обратимо. Единственное содержательное проектное решение фичи (OQ-2) зафиксировано здесь и носит локальный характер. Индекс ADR: [decisions/README.md](../../../architecture/decisions/README.md) — остаётся без изменений.

## Влияние на тесты

Файл: `PoiskKinoMetadataPlugin.UnitTests/Providers/PoiskKinoImageProviderTests.cs`, данные: `TestData/TestJsonData.cs`.

| Тест | Действие |
|------|----------|
| `GetSupportedImages_ReturnsPrimaryAndBackdrop` | Обновить: 3 типа, включая `ImageType.Logo`; переименовать соответственно (AC-8) |
| `GetImages_ByProviderId_ReturnsPosterAndBackdrop` | Обновить: ожидается 3 изображения, включая `Logo`; `FullMovieDtoJson` уже содержит `logo.url = oppenheimer-logo.png` (TestJsonData.cs:54–57) (AC-9) |
| `GetImages_MovieFallsBackToSearch_ReturnsImagesFromSearch` | **Сломается преднамеренно**: assertion `Assert.All(images, i => Assert.Contains("movie-poster.jpg", …))` перестанет выполняться после добавления логотипа в fallback. Переписать на проверку по типам (Primary содержит `movie-poster.jpg`, Logo содержит url логотипа). Тестовым данным `MixedSearchResponseJson` добавить `"logo"` записи «Оппенгеймер» (TestJsonData.cs:391–422) |
| Новый: `logo: null` в JSON по ID | В результате нет `Logo`, ошибок нет, постер/фон присутствуют (AC-10) |
| Новый: TMDB-логотип + `IgnoreTmdbImages=true` | Логотип отфильтрован, постер/фон остаются (AC-11). Внимание: фикстура `SetUpPlugin` создаёт конфиг через `Activator.CreateInstance`, поэтому действует дефолт `IgnoreTmdbImages = true` (Configuration.cs:19) — существующий TMDB-тест работает именно на этом; для явности можно выставлять флаг через `Plugin.Instance.Configuration` |
| Новый (рекомендуется): `IgnoreTmdbImages=false` | Логотип с любым URL возвращается (AC-7); выставлять `plugin.Configuration.IgnoreTmdbImages = false` после `SetUpPlugin` |

Регресс: весь набор зелёный (`dotnet test`) (AC-12). Покрытие AC-6 обеспечивается новым TMDB-тестом логотипа; AC-5 — типы `Supports` не трогаются, существующие тесты `Supports_*` остаются.

## Риски

| Риск | Вероятность/эффект | Митигация |
|------|--------------------|-----------|
| Клиенты запросят Logo у тайтлов без логотипа | Низкий / косметика | Guard `IsNullOrWhiteSpace`; отсутствие Logo — нормальный пустой ответ (AC-3) |
| Поломка существующего fallback-теста на `Assert.All(movie-poster.jpg)` | Гарантированная / тесты | Явно учтено выше: тест переписывается до коммита реализации |
| Несимметричность guards: Logo — `IsNullOrWhiteSpace`, Poster/Backdrop — `IsNullOrEmpty` (whitespace-URL постера сегодня пройдёт) | Средняя / минорная | Не трогаем из-за AC-4; зафиксирован follow-up-кандидат «унифицировать guards на `IsNullOrWhiteSpace`» — вынести в backlog отдельно |
| Лимит API 200 req/day | Нет влияния | Логотип приходит в тех же ответах `movie/{id}` / `search`; новых запросов нет (NFR подтверждён) |
| Season без ProviderId не получит логотип из поиска | Принято | Существующая логика пропуска поиска для сезонов сохраняется (edge-case таблица анализа) |

## Документационные последствия (→ Техлид / Аналитик)

- **Техлид** (в рамках реализации): обновить XML-summary `PoiskKinoImageProvider` («провайдер изображений (постеры, фоны и логотипы)»); DevOps impact — отсутствует (проверено в status.md).
- **Аналитик/Стейхолдер** (после принятия): строка `docs/product/requirements.md:12` «Изображения: постеры и фоны…» устареет — обновить формулировку на «постеры, фоны и логотипы». `docs/architecture/overview.md` правок не требует (компонент описан без перечисления типов).

## Открытые вопросы

Нет. OQ-1 решён Стейхолдером (в scope), OQ-2 решён выше. Блокеров для декомпозиции нет.
