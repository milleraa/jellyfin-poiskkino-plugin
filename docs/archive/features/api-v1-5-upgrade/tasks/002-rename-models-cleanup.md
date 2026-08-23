# Task 002 — Переименование моделей V1_4, чистка комментариев, удаление мёртвого кода

- **Фича:** [api-v1-5-upgrade](../analysis.md)
- **Роль исполнителя:** developer-dotnet (C# / .NET 9 / xUnit)
- **Стек/область:** `PoiskKinoMetadataPlugin/Models/`, провайдеры (`Movie`/`Series`/`Image`), юнит-тесты моделей и фикстур
- **Ветка/Worktree:** `feature/api-v1-5-upgrade`, `/home/alex/src/my/jellyfin-metadata-plugin-api-v15`
- **Предусловие:** задача 001 принята
- **Ссылки:** analysis.md §1 (перечень), §8 (вопросы №1/№3), architecture.md §3 «Модели», решения Стейхолдера в status.md

## Что сделать

1. Переименовать класс `PoiskKinoMovieDtoV1_4` → `PoiskKinoMovieDto`:
   - файл `Models/PoiskKinoMovieDtoV1_4.cs` → `Models/PoiskKinoMovieDto.cs`;
   - все использования типа: `PoiskKinoApiClient`, `PoiskKinoMovieProvider`, `PoiskKinoSeriesProvider`, `PoiskKinoImageProvider`, тесты;
   - тест-класс `PoiskKinoMovieDtoV1_4Tests` → файл+класс `PoiskKinoMovieDtoTests`.
2. Убрать упоминания «v1.4» как целевой версии из XML-doc комментариев моделей (`PoiskKinoItem`, `PoiskKinoSearchResponse`, `PoiskKinoSeason`, `PoiskKinoEpisode`, переименованная модель). Допустимо сослаться на имя схемы спеки (`SearchMovieDtoV1_4` и т.п.), но не как на версию API-вызова.
3. Обновить комментарии фикстур `UnitTests/TestData/TestJsonData.cs:6,172` («matching GET /v1.4/...» → актуальные v1.5-пути). Содержимое JSON не менять.
4. Удалить мёртвый `Models/PoiskKinoSeasonResponse.cs` (класс нигде не используется).
5. JSON-контракты (`JsonPropertyName`) не менять.

## Acceptance Criteria

- [ ] Класса/файла `*V1_4*` в решении нет (`grep -rn "V1_4" --include='*.cs'` даёт только допустимые ссылки на имена схем спеки в XML-doc, если оставлены).
- [ ] `Models/PoiskKinoSeasonResponse.cs` удалён, сборка проходит.
- [ ] `dotnet test` зелёный без новых предупреждений.

## DoD / критерий приёмки Техлидом

Локальный commit без push, чистый grep по URL v1.4 и по типам `*V1_4` (кроме имён схем в комментариях), полный зелёный прогон тестов в отчёте агента.
