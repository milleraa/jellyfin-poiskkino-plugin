# QA — `logo-images`

Дата QA: 2026-08-24. Предусловие выполнено: приёмка реализации Техлидом зафиксирована в [status.md](status.md) (2026-08-24), DevOps impact check закрыт. Среда: worktree `/home/alex/src/my/jellyfin-metadata-plugin-wt-logo-images`, ветка `feature/logo-images` @ `c069df2`, .NET SDK 10.0.111 (target net9.0), Linux. Playwright неприменим — серверный плагин без UI ([playwright.md](../../../qa/playwright.md)); проверка кодом/тестами.

## Покрытие

- Функциональные AC-1..AC-5 и фильтрация AC-6..AC-7 из [analysis.md](analysis.md) — по коду + unit-тесты.
- Тестовые требования AC-8..AC-12 из analysis.md — по набору тестов и прогонам.
- Регрессия: полный набор `dotnet test`, два прогона подряд (стабильность).
- Fallback-ветка поиска (OQ-1) и отклонение Техлида (`backdrop` в `MixedSearchResponseJson`) проверены отдельно.

## Чеклист по AC

| AC | Сценарий | Статус | Evidence |
|----|----------|--------|----------|
| AC-1 | `GetSupportedImages` → `[Primary, Backdrop, Logo]` | ✅ Pass | Код: `PoiskKinoImageProvider.cs:48`; тест `GetSupportedImages_ReturnsPrimaryBackdropAndLogo` (3 типа, содержит Logo) |
| AC-2 | По ID: добавляется `RemoteImageInfo { Url = Logo.Url, Type = Logo }` при непустом URL | ✅ Pass | Код: строки 194–203 (после Poster/Backdrop); тест `GetImages_ByProviderId_ReturnsPosterBackdropAndLogo` — 3 изображения, Logo URL содержит `oppenheimer-logo.png` |
| AC-3 | Нет Logo при `logo == null` / пустом / whitespace URL — без ошибок | ✅ Pass | Guard `movieData.Logo != null && !string.IsNullOrWhiteSpace(movieData.Logo.Url)` (обе ветки); тест `GetImages_LogoMissing_ReturnsPosterAndBackdropWithoutLogo` (`"logo": null`) — ровно 2 изображения (Primary, Backdrop), Logo нет |
| AC-4 | Поведение Primary/Backdrop не меняется | ✅ Pass | Дифф `main...HEAD`: guard'ы Poster/Backdrop (`IsNullOrEmpty`) не тронуты; все существующие тесты зелёные |
| AC-5 | Типы `Supports` не меняются; логотип для всех поддерживаемых типов с PoiskKino ID | ✅ Pass | `Supports` без изменений (`PoiskKinoImageProvider.cs:40–43`), тесты `Supports_Movie/Series/Season/Episode` зелёные; путь маппинга Logo типо-независимый. Edge-case «Season без ProviderId» — поиск пропускается (существующая логика, зафиксировано в анализе) |
| AC-6 | `IgnoreTmdbImages=true`: логотип с `tmdb.org` отфильтрован, остальные возвращаются | ✅ Pass | Пост-фильтр поверх всего списка (`PoiskKinoImageProvider.cs:211–219`); тест `GetImages_TmdbLogo_FilteredWhenIgnoreTmdbEnabled`: Logo нет, Primary/Backdrop (не TMDB) присутствуют |
| AC-7 | `IgnoreTmdbImages=false`: логотип с любым URL возвращается как есть | ✅ Pass | Тест `GetImages_TmdbLogo_ReturnedWhenIgnoreTmdbDisabled`: 3 изображения, TMDB-логотип на месте; дефолтный флаг восстановлен после теста |
| AC-8 | Тест `GetSupportedImages_*` обновлён на 3 типа | ✅ Pass | Переименован в `GetSupportedImages_ReturnsPrimaryBackdropAndLogo`, assertion'ы по трём типам |
| AC-9 | Тест by-ID обновлён: 3 изображения включая Logo | ✅ Pass | `GetImages_ByProviderId_ReturnsPosterBackdropAndLogo`; фикстура `FullMovieDtoJson` содержит `logo.url` (`TestJsonData.cs:54–57`) |
| AC-10 | Новый тест «логотип отсутствует» | ✅ Pass | `GetImages_LogoMissing_ReturnsPosterAndBackdropWithoutLogo` |
| AC-11 | Новый тест «TMDB-логотип фильтруется» | ✅ Pass | `GetImages_TmdbLogo_FilteredWhenIgnoreTmdbEnabled` (фикстура `TmdbLogoMovieJson`: постер/фон вне TMDB, логотип на tmdb.org) |
| AC-12 | Полный набор зелёный | ✅ Pass | Два прогона `dotnet test`: 163/163 оба раза (см. Результаты) |

### Дополнительно проверено (вне нумерации AC)

| Сценарий | Статус | Комментарий |
|----------|--------|-------------|
| Fallback-ветка поиска (OQ-1, в scope) | ✅ Pass | Тест `GetImages_MovieFallsBackToSearch_ReturnsImagesFromSearch`: порядок детерминированный Primary → Backdrop → Logo, URLs `movie-poster.jpg` / `movie-backdrop.jpg` / `movie-logo.png` |
| Отклонение Техлида: `"backdrop"` у «Оппенгеймера» в `MixedSearchResponseJson` | ✅ Принято | Подтверждено в фикстуре (`TestJsonData.cs:400`); необходим для проверки порядка 3 типов. Прочие потребители фикстуры (`PoiskKinoMovieProviderTests` ×3, `PoiskKinoSeriesProviderTests`) не завязаны на поля изображений — набор зелёный |
| Guard-несимметричность (Logo `IsNullOrWhiteSpace` vs Poster/Backdrop `IsNullOrEmpty`) | ✅ Соответствует решению | Осознанная несимметричность по architecture.md (AC-4 не позволяет трогать соседние guard'ы); follow-up «унифицировать guards» остаётся кандидатом в backlog |
| OQ-2: защитный вызов `ShouldFilterUrl` не введён | ✅ Соответствует решению Архитектора | В коде только единый пост-фильтр; функциональная эквивалентность подтверждена тестами AC-6/AC-7 |

## Результаты прогонов

| № | Команда | Результат |
|---|---------|-----------|
| 1 | `dotnet build -c Release` | **0 ошибок**; 61 предупреждение — baseline CS1591 (XML-доки DTO), к фиче не относится (в изменённых файлах не-C1591 предупреждений нет) |
| 2 | `dotnet test` (прогон 1) | **163 passed / 0 failed / 0 skipped** |
| 3 | `dotnet test` (прогон 2, стабильность) | **163 passed / 0 failed / 0 skipped** |
| 4 | `dotnet test --filter FullyQualifiedName~PoiskKinoImageProviderTests` | **14/14 passed** (полный список имён подтверждён: Supports ×4, GetSupportedImages, EmptyApiKey, ByProviderId, FallbackToSearch, TmdbUrlFiltering ×2, TmdbLogo ×2, LogoMissing, NoItemTitle) |

Совпадает с отчётом Техлида (163 passed). Прирост против до-фичевого состояния: +2 теста (161 → 163).

## Автотесты

- Playwright: неприменим — серверный плагин без UI, см. [playwright](../../../qa/playwright.md).
- Изоляция unit-тестов: одна коллекция `PluginInstanceCollection` (последовательное выполнение); каждый тест вызывает `SetUpPlugin` → новый экземпляр плагина с дефолтным `IgnoreTmdbImages=true`, протекания конфигурации нет; тест с `false` дополнительно восстанавливает дефолт. CI-совместимость: обычные `dotnet build -c Release` + `dotnet test`, внешних зависимостей нет.

## Наблюдения (неблокирующие)

1. Отсутствует отдельный тест на whitespace-only `logo.url` (`"logo": { "url": "   " }`) — кейс покрыт конструкцией guard'а `IsNullOrWhiteSpace` (BCL-примитив, поведение подтверждено ревью кода); требования к тестам (analysis.md, пп. 8–12) отдельного whitespace-теста не содержат. Можно усилить в рамках follow-up «унифицировать guards».
2. `GetImages_TmdbLogo_ReturnedWhenIgnoreTmdbDisabled` восстанавливает дефолт флага без try/finally — при падении посреди теста значение могло бы протечь; риск снят тем, что все последующие тесты пересоздают плагин через `SetUpPlugin`. Косметика для будущих правок тестов.

Дефектов не найдено.

## Вердикт

- [x] Pass
- [ ] Rework → Техлид (список дефектов ниже)

### Дефекты

1. _нет_

Запись о прогоне: [docs/qa/test-runs.md](../../../qa/test-runs.md).
