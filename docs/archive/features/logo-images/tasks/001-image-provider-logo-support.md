# Task 001 — Logo в `PoiskKinoImageProvider` (реализация + обновление существующих тестов)

## Метаданные

- **ID:** 001
- **Feature / Hotfix:** [docs/backlog/features/logo-images/](../)
- **Назначено:** `developer-csharp`
- **Стек / infra-область:** C# / .NET 8, Jellyfin 10.11.x (`Jellyfin.Controller` / `Jellyfin.Model`); код + unit-тесты, infra не затрагивается
- **Статус:** done
- **Ветка:** `feature/logo-images` (worktree `/home/alex/src/my/jellyfin-metadata-plugin-wt-logo-images`, task-branch/worktree не создавать)

## Описание

Реализовать возврат логотипов тайтлов из поля `logo` API ПоискКино как `ImageType.Logo` в существующем `PoiskKinoImageProvider`. Изменение локально в адаптере: DTO, `ImageUrlHelper`, `Plugin`/Configuration/DI не меняются ([architecture.md](../architecture.md), таблица «Компоненты и границы»).

### Изменения в `PoiskKinoMetadataPlugin/PoiskKinoImageProvider.cs`

1. `GetSupportedImages` → `[ImageType.Primary, ImageType.Backdrop, ImageType.Logo]`.
2. Ветка A (полные данные по ID): после маппинга Poster и Backdrop из `movieData` добавить логотип:
   - guard: `movieData.Logo != null && !string.IsNullOrWhiteSpace(movieData.Logo.Url)` (AC-3 — whitespace отсекается);
   - `images.Add(new RemoteImageInfo { Url = movieData.Logo.Url, Type = ImageType.Logo, ProviderName = Name })`;
   - порядок в списке детерминированный: Primary → Backdrop → Logo.
3. Ветка B (fallback «только данные из поиска», решение Стейхолдера по OQ-1 — в scope):
   - рядом с постером/фоном из `apiItem`: guard `apiItem.Logo != null && !string.IsNullOrWhiteSpace(apiItem.Logo.Url)`, добавление `Type = ImageType.Logo` аналогично.
4. Guards соседних Poster/Backdrop **не менять** (остаются `IsNullOrEmpty`) — AC-4; осознанная несимметричность зафиксирована в архитектуре как follow-up-кандидат.
5. Защитный вызов `ImageUrlHelper.ShouldFilterUrl(logoUrl)` при маппинге **НЕ вводить** (решение Архитектора OQ-2) — TMDB-политика применяется существующим пост-фильтром в конце `GetImages`.
6. XML-summary класса обновить: «Провайдер изображений (постеры, фоны и логотипы) из PoiskKino API.»
7. `Supports(...)`, `Order`, `GetImageResponse`, кэширование/HTTP-клиент — без изменений.

### Изменения в тестах (`PoiskKinoMetadataPlugin.UnitTests/Providers/PoiskKinoImageProviderTests.cs`, `TestData/TestJsonData.cs`)

1. `GetSupportedImages_ReturnsPrimaryAndBackdrop` → переименовать (например, `GetSupportedImages_ReturnsPrimaryBackdropAndLogo`), ожидать 3 типа, включая `ImageType.Logo`.
2. `GetImages_ByProviderId_ReturnsPosterAndBackdrop` → переименовать соответственно; ожидать 3 изображения: Primary, Backdrop, Logo (`FullMovieDtoJson` уже содержит `logo.url = …oppenheimer-logo.png`); проверить URL логотипа.
3. `GetImages_MovieFallsBackToSearch_ReturnsImagesFromSearch` — **сломается преднамеренно** (сейчас `Assert.All(images, i => Assert.Contains("movie-poster.jpg", i.Url!))`). Переписать на проверку по типам: Primary содержит `movie-poster.jpg`, Logo содержит url логотипа из фикстуры, порядок типов корректный. В `MixedSearchResponseJson` добавить `"logo": { "url": "https://example.com/movie-logo.png" }` в запись «Оппенгеймер» (id 535341).
4. Новый тест: логотип отсутствует в ответе API (`"logo": null` или поле отсутствует — использовать inline JSON по образцу TMDB-теории) → в результате нет изображений типа `Logo`, ошибок нет, постер/фон присутствуют.
5. Тесты TMDB-фильтрации для логотипа — **вне этой задачи** (задача 002).

## Acceptance criteria

1. AC-1: `GetSupportedImages` возвращает `[Primary, Backdrop, Logo]`.
2. AC-2: по ProviderId добавляется `RemoteImageInfo { Url = movieData.Logo.Url, Type = ImageType.Logo }` при непустом URL.
3. AC-3: `Logo == null` или `Logo.Url` пустой/whitespace → изображения `Logo` нет, ошибок нет.
4. AC-4: поведение `Primary`/`Backdrop` не изменилось (guards `IsNullOrEmpty` сохранены).
5. AC-5: `Supports` не изменён (Movie/Series/Season).
6. AC-8..AC-10: тесты из п. «Изменения в тестах» 1–4 реализованы и зелёные.
7. AC-12: `dotnet build` и `dotnet test` зелёные на всём решении.
8. Нет вызовов `ShouldFilterUrl` в image-провайдере; XML-summary обновлён.

## Технические заметки

- Ссылки на код: `PoiskKinoMetadataPlugin/PoiskKinoImageProvider.cs` (ветка A: строки ~163–181; ветка B: ~128–153; пост-фильтр: ~188–197).
- Фикстуры: `PoiskKinoMetadataPlugin.UnitTests/TestData/TestJsonData.cs` (`FullMovieDtoJson` строки 54–57 — logo уже есть; `MixedSearchResponseJson` строки 391–422 — добавить logo записи Оппенгеймера).
- Конфигурация: дефолт `IgnoreTmdbImages = true` (`PoiskKinoMetadataPlugin/Configuration.cs:19`); фикстура создаёт конфиг через `Activator.CreateInstance`, поэтому дефолт действует в каждом тесте после `SetUpPlugin`.
- ADR не требуется; OQ-2 закрыт в [architecture.md](../architecture.md).

## DevOps impact check

- Новые/изменённые переменные окружения или secrets: **нет**
- Новые внешние интеграции, порты, очереди, storage, scheduled jobs: **нет** (новых HTTP-запросов нет — `logo` приходит в тех же ответах API)
- Docker/compose/CI/CD/deploy/Helm/Kubernetes/observability: **нет**
- Итог: DevOps-задача **не нужна** (подтверждено Архитектором, см. status.md).

## Проверка

```bash
dotnet build PoiskKinoMetadataPlugin.slnx
dotnet test PoiskKinoMetadataPlugin.slnx
```

Обе команды — из корня worktree, результат: 0 failed.

## Handoff

- После выполнения: локальный commit без push (conventional commits, например `feat(images): ...`), обновить статус этой задачи на `in review` и сообщить Техлиду; `status.md` обновляет Техлид.
