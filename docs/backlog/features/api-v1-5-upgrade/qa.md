# QA — `api-v1-5-upgrade`

Дата: 2026-08-23. QA-валидация после приёмки Техлидом (accept, DevOps-check закрыт).
Worktree: `/home/alex/src/my/jellyfin-metadata-plugin-api-v15`, ветка `feature/api-v1-5-upgrade`,
коммиты `73ea86f`, `8f724dd`, `1abee81` (HEAD на момент прогона: `8d11c10`).

## Решение по инструментарию

Проект — backend-плагин C#/.NET для Jellyfin, собственного UI/веб-интерфейса не имеет.
**Playwright неприменим** (подтверждено [test-strategy.md](../../../qa/test-strategy.md): E2E «не планируется»).
Проверка выполнена через: юнит-тесты (`dotnet test`, xUnit + Moq), статический анализ
(grep по исходникам, сравнение warnings сборки с baseline), анализ диффов коммитов.

## Покрытие

- AC из `analysis.md` §5 (все 6 пунктов).
- Замечания Техлида для QA из `status.md` (3 пункта).
- Регрессия: маппинг метаданных провайдеров (дифф + существующие тесты провайдеров).

## Результаты чеклиста

| # | Проверка | Статус | Комментарий |
|---|----------|--------|-------------|
| 1 | Полное отсутствие `/v1.4` в коде | ✅ pass | grep `v1.4|V1_4` по всем `.cs`: 0 в URL/коде. Остались только XML-doc комментарии со ссылками на имена схем спеки (`schema MovieDtoV1_4` и т.п., разрешено architecture.md §3) и пояснение контекста в doc-comment `SearchYearFilter.cs:15`. `api-docs/documentation.yaml` — внешний документ спеки, не код решения. `/v1.5/season` нетронут (`PoiskKinoApiClient.cs:320`). |
| 2 | Пути запросов в юнит-тестах | ✅ pass | `PoiskKinoApiClientTests.cs:120,138` — `Assert.EndsWith("/v1.5/movie/search", AbsolutePath)`; `:272` — `/v1.5/movie/535341`; `:122` — `year=2023` в query. Регрессия на v1.4 теперь падает. |
| 3a | Фильтр года: фильм — точное совпадение Year | ✅ pass | `SearchYearFilter.MatchesMovie` (`item.Year == year`); теория `MatchesMovie_ExactYearComparison` включает null-Year элемента. |
| 3b | Сериал — releaseYears (+Year fallback) | ✅ pass | `MatchesSeries`: совпадение `Year` ИЛИ попадание в диапазон `ReleaseYears` (null-границы открыты). Тесты: within/outside/no-releaseYears/open-ended range. |
| 3c | Пустой результат → неотфильтрованный список + debug-log | ✅ pass | `SearchYearFilter.Apply`: fallback возвращает тот же список (`Assert.Same`) + верификация debug-записи; пустой вход — без лога. |
| 3d | Год не указан → фильтрация не применяется | ✅ pass | `Apply_NoYear_ReturnsUnfiltered`; интеграция в провайдерах через `searchInfo.Year` (null-safe). |
| 4 | Регрессия маппинга | ✅ pass | Дифф `0cf9460..HEAD` по провайдерам: только переименование типа `PoiskKinoMovieDtoV1_4`→`PoiskKinoMovieDto` и 2 строки вызова `SearchYearFilter.Apply`; логика заполнения `RemoteSearchResult` (название/год/описание/рейтинги/постеры/персоны/жанры) не менялась. Все существующие тесты маппинга зелёные на прежних JSON-фикстурах (содержимое фикстур не менялось). |
| 5 | `dotnet test` | ✅ pass | **160 passed / 0 failed / 0 skipped**, 467 ms (.NETCoreApp v9.0) — совпадает с ожиданием Техлида. |
| 6 | `dotnet build` без новых warnings/errors | ✅ pass | 0 ошибок; предупреждения идентичны baseline (main `00900d1`, сборка до фичи): 61× CS1591 (missing XML-doc) в обоих случаях, новых типов предупреждений нет. |
| 7 | Smoke с реальным API-ключом | ⏭️ пропущено | Ключа нет — см. «Ограничения». |

## Exploratory notes

- Фильтрация корректно размещена **после** фильтра по типу контента (`.Where(r => r.IsSeries != true)`)
  и до построения `RemoteSearchResult` — предикаты соответствуют типу выдачи (фильмы vs сериалы).
- `ApiClient` остался без бизнес-правил (ADR-0001 §Решение п.3): `SearchYearFilter` лежит рядом с провайдерами,
  кэш хранит сырой ответ API — фильтрация применяется к каждому чтению, включая cache hit.
- `InternalsVisibleTo("PoiskKinoMetadataPlugin.UnitTests")` добавлен для тестирования internal-класса фильтра —
  допустимо, не расширяет публичный API.
- Мёртвый `Models/PoiskKinoSeasonResponse.cs` удалён; сезонный путь `/v1.5/season` и его тесты
  (`PoiskKinoSeasonProviderTests`, `PoiskKinoEpisodeProviderTests` — assertion `/v1.5/season`) не затронуты.
- Комментарии JSON-фикстур обновлены (`TestJsonData.cs:6,172`), содержимое JSON не менялось — десериализация
  v1.5 подтверждается прохождением всех модельных тестов.

## Автотесты

- Playwright: неприменим для backend/.NET-плагина — см. [playwright](../../../qa/playwright.md),
  решение зафиксировано выше в разделе «Инструментарий».
- Юнит-тесты: `PoiskKinoMetadataPlugin.UnitTests` — 160 passed (новые за фичу: пути запросов клиента ×3,
  `SearchYearFilterTests` ×10, интеграционные проверки фильтра в провайдерах).

## Ограничения QA (непроверенное)

1. **Smoke с реальным API-ключом не выполнялся** (ключ отсутствует): строгая серверная валидация
   недокументированного параметра `&year=` в `GET /v1.5/movie/search` остаётся открытым риском.
   План изоляции описан в ADR-0001 (Последствия): убрать `&year=` из URL — одна строка + один тест,
   локальная фильтрация уже гарантирует бизнес-поведение.
2. Интеграционная проверка в живом Jellyfin (Identify фильма/сериала с годом, постеры) не проводилась —
   требует окружения с ключом; рекомендуется Стейхолдеру после принятия PR.

## Вердикт

- [x] Pass
- [ ] Rework → Техлид (список дефектов ниже)

### Дефекты

Не обнаружено.
