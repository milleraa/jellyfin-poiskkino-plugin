# Task 001 — Миграция URL ApiClient на v1.5 + assertion путей в тестах

- **Фича:** [api-v1-5-upgrade](../analysis.md)
- **Роль исполнителя:** developer-dotnet (C# / .NET 9 / xUnit)
- **Стек/область:** `PoiskKinoMetadataPlugin/PoiskKinoApiClient.cs`, `UnitTests/ApiClient/PoiskKinoApiClientTests.cs`
- **Ветка/Worktree:** `feature/api-v1-5-upgrade`, `/home/alex/src/my/jellyfin-metadata-plugin-api-v15`
- **Предусловие:** нет (первая задача)
- **Ссылки:** analysis.md §1–2, architecture.md §1, §3 «Инфраструктура»

## Что сделать

1. `PoiskKinoApiClient.cs:81`: `/v1.4/movie/search` → `/v1.5/movie/search`. Параметры (`query`, `limit=3`, опциональный недокументированный `year`) и заголовки не менять — сохранение `&year=` зафиксировано [ADR-0001](../../../../architecture/decisions/ADR-0001-search-year-filter.md).
2. `PoiskKinoApiClient.cs:202` (и комментарии ~171/190/239): `/v1.4/movie/{id}` → `/v1.5/movie/{id}`.
3. `/v1.5/season` (~строка 320) НЕ трогать.
4. XML-doc комментарии методов клиента больше не должны называть v1.4 целевой версией.
5. Тесты `PoiskKinoApiClientTests`: добавить явный capture+assert фактического пути запроса:
   - `SearchAsync` → абсолютный URI заканчивается на `/v1.5/movie/search` (+query);
   - `GetMovieByIdAsync` → `/v1.5/movie/{id}` (например, `/v1.5/movie/535341`);
   - существующий `SearchAsync_WithYear_IncludesYearInUrl` сохранить, дополнить проверкой пути.
6. Кэш, семафор, обработку ошибок (400/401/403/404/429) не менять.

## Acceptance Criteria

- [ ] `grep -rn "v1\.4" PoiskKinoMetadataPlugin/PoiskKinoApiClient.cs` пуст; `/v1.5/season` без изменений.
- [ ] В тестах есть assertion пути для search и movie/{id}; регрессия на v1.4 падала бы.
- [ ] Сборка и полный `dotnet test` зелёные.

## DoD / критерий приёмки Техлидом

Локальный commit без push; в отчёте агента — вывод тестов и результат grep'а по `v1.4`.
