# QA — `search-limit-setting`

QA-проверка после финальной приёмки Техлида (accept, коммит `c894715`); прогон с HEAD `25d623a` ветки `feature/search-limit-setting`. Код и тесты QA не меняла — только evidence.

## Покрытие

- Сценарии из `analysis.md`: AC-1…AC-10 — сведены по задачам (001 `d01ee2d` → AC-1, AC-6 сервер; 002 `a0febfe` → AC-2 клиент, AC-3…AC-5; 003 `c894715` → AC-2 провайдеры, AC-7, AC-9, AC-10; AC-6 UI / AC-8 — ручные по инструкции в [tasks/003](tasks/003-providers-threading-readme.md)).
- Регрессия: полный набор юнит-тестов проекта (327) + Release-сборка.
- Риски: R1 (smoke с demo-ключом) — не проверен, нет окружения (см. «Ограничения»).

## Результаты

| Сценарий | Статус | Комментарий |
|----------|--------|-------------|
| Автоматические прогоны (AC-1…AC-5, AC-7 unit, AC-9, AC-10) | **pass** | `dotnet test` — 327/327 (0 failed/skipped); `dotnet build -c Release --no-incremental` — 0 ошибок, предупреждения = baseline (61 × CS1591, все в `Models/`, идентично принятой базе 002/003); деталей в [test-runs](../../../qa/test-runs.md) |
| Целевой прогон фичи (лимит и ключ кэша) | **pass** | Фильтр `PoiskKinoMovieProviderTests|CacheKey|SearchLimitResolver|PluginConfiguration` — 67/67: провайдерский тест AC-7 `GetSearchResults_SearchLimitFromConfiguration_ChangesRequestLimitWithoutRestart` (смена 7→4 на том же экземпляре меняет URL запроса), резолвер валидации 1–10 (fallback дефолт + Warning, дедупликация/сброс памятки), конфигурация (дефолт 3, legacy XML без `<SearchLimit>` → 3, round-trip 7) |
| AC-6 (UI: поле «Количество результатов поиска», сохранение, дефолт на чистой конфигурации) | **не выполнена — нет окружения** | Нужен живой Jellyfin; в среде QA сервера нет (нет процесса/портов 8096, 8920, docker, HTTP на localhost:8096). Серверная часть AC-6 (дефолт 3, персистентность свойства) покрыта юнит-тестами (`PluginConfigurationTests`, 6/6) |
| AC-8 (персистентность: 7 → перезапуск → UI + `<SearchLimit>7</SearchLimit>` в XML, прочие настройки целы) | **не выполнена — нет окружения** | Нужен живой Jellyfin с перезапуском; XML round-trip и legacy-дефолт покрыты юнит-тестами (`SearchLimit_Default_IsThree`, `Deserialize_LegacyXmlWithoutSearchLimit_KeepsDefaultAndOtherValues`, `SerializeAndDeserialize_WithCustomSearchLimit_PreservesValue`) |
| AC-7 (применение без перезапуска в identify-диалоге: 8 → 2, сравнение направления) | **не выполнена — нет окружения** | Нужен живой Jellyfin с настроенным плагином; серверная часть (чтение конфигурации на каждый вызов, лимит в ключе кэша) покрыта провайдерским тестом AC-7 и клиентскими тестами `PoiskKinoApiClientSearchLimitTests` |
| R1 (smoke с demo-ключом: дефолт 3 и максимум 10, без 403/429, лог плагина чистый) | **не выполнена — нет окружения** | Демо-ключ в среде отсутствует (ключ хранится только в runtime-конфиге Jellyfin, сервера нет) |

## Автотесты

- Среда: локально, KDE neon (Linux x86_64), .NET SDK 10.0.115, `net10.0`; ветка `feature/search-limit-setting` @ `25d623a`.
- `dotnet test` — **327/327** passed, 0 failed, 0 skipped (ожидание 327/327 подтверждено).
- `dotnet build -c Release --no-incremental` — exit 0, **0 ошибок**, 61 предупреждение `CS1591` (все в `PoiskKinoMetadataPlugin/Models/`, новых нет).
- Целевой фильтр — **67/67**: `PoiskKinoMovieProviderTests` 35, `SearchLimitResolverTests` 26, `PluginConfigurationTests` 6.
- Клиентские тесты лимита (`PoiskKinoApiClientSearchLimitTests`: лимит в запросе, вне диапазона → дефолт + одиночный Warning с RawValue и без ApiKey, дедупликация/сброс памятки) — зелёные в полном прогоне.
- Ручные проверки: см. [smoke-чеклист Jellyfin](../../../qa/jellyfin-smoke.md) и инструкцию в [tasks/003](tasks/003-providers-threading-readme.md) («Инструкция ручного QA»).

## Ограничения evidence

- Ручные проверки на живом Jellyfin (AC-6 UI, AC-8, AC-7 live, R1) **не выполнены**: в среде QA нет запущенного Jellyfin (нет процесса, портов 8096/8920, docker-контейнера, HTTP-ответа на localhost:8096) и нет demo-ключа. Результаты не выдумывались; незакрытые проверки перечислены в таблице выше.
- Прецедент в репозитории: [eu-api-endpoint](../../../archive/features/eu-api-endpoint/qa.md) (2026-11-28) — pass с аналогичными ручными ограничениями.
- Решение о принятии фичи с отложенным smoke (или проведение smoke администратором на целевом сервере) — за Оркестратором/Стейхолдером.

## Вердикт

- [x] Pass — дефектов не найдено; автоматическая часть полностью зелёная. Вердикт относится к выполненной (автоматической) части; ручной smoke отложен из-за отсутствия окружения (не дефект) — см. «Ограничения evidence».
- [ ] Rework → Техлид (список дефектов ниже)

### Дефекты

1. _нет_
