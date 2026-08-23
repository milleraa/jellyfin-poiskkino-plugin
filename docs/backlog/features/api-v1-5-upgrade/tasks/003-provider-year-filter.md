# Task 003 — Локальная фильтрация поиска по году в провайдерах (ADR-0001)

- **Фича:** [api-v1-5-upgrade](../analysis.md)
- **Роль исполнителя:** developer-dotnet (C# / .NET 9 / xUnit)
- **Стек/область:** `PoiskKinoMetadataPlugin/PoiskKinoMovieProvider.cs`, `PoiskKinoSeriesProvider.cs`, новые юнит-тесты провайдеров
- **Ветка/Worktree:** `feature/api-v1-5-upgrade`, `/home/alex/src/my/jellyfin-metadata-plugin-api-v15`
- **Предусловие:** задачи 001, 002 приняты
- **Ссылки:** [ADR-0001](../../../../architecture/decisions/ADR-0001-search-year-filter.md), architecture.md §2, §3 «Приложение»

## Что сделать

Реализовать детерминированную локальную фильтрацию результатов поиска по году на стороне плагина (в провайдерах, НЕ в `ApiClient`; кэш хранит сырой ответ API):

1. `PoiskKinoMovieProvider.GetSearchResultsInternal`: если `searchInfo.Year.HasValue`, после фильтра `IsSeries != true` оставить только элементы с `item.Year == searchInfo.Year`.
2. `PoiskKinoSeriesProvider.GetSearchResultsInternal`: если `searchInfo.Year.HasValue`, после фильтра `IsSeries == true` оставить элементы, у которых год попадает в любой диапазон `item.ReleaseYears` (`start..end`, null-границы трактовать открытыми) ИЛИ совпадает с `item.Year`.
3. Fallback: если до фильтрации список был непуст, а после фильтрации пуст → вернуть неотфильтрованный список и записать debug-log (защита полноты при расхождении годов источников).
4. Год не указан → фильтрация не применяется (обычный refresh).
5. Маппинг на `RemoteSearchResult` не менять.

## Тесты (новые)

- фильм: точное совпадение года оставляет элемент;
- сериал: год внутри `releaseYears` оставляет; год вне диапазона отсекает; совпадение с `Year` при пустом `releaseYears` оставляет;
- fallback: ни один элемент не совпал по году → возвращён неотфильтрованный список;
- год не указан → все элементы проходят без фильтрации.

## Acceptance Criteria

- [ ] Фильтрация реализована в провайдерах, `ApiClient` не содержит бизнес-правил.
- [ ] Все перечисленные сценарии покрыты юнит-тестами.
- [ ] `dotnet test` зелёный.

## DoD / критерий приёмки Техлидом

Локальный commit без push; отчёт агента с прогоном тестов и указанием файлов изменений.
