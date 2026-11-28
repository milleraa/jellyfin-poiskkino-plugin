# QA — `search-limit-setting`

QA-проверка после финальной приёмки Техлида (accept, коммит `c894715`); прогон с HEAD `25d623a` ветки `feature/search-limit-setting`. Код и тесты QA не меняла — только evidence. Дополнение 2026-11-28 (вечер): принят ручной смоук Оркестратора (docker Jellyfin 12.4.0.0, кодовая база `c894715`) — AC-6 UI и AC-8 закрыты.

## Покрытие

- Сценарии из `analysis.md`: AC-1…AC-10 — сведены по задачам (001 `d01ee2d` → AC-1, AC-6 сервер; 002 `a0febfe` → AC-2 клиент, AC-3…AC-5; 003 `c894715` → AC-2 провайдеры, AC-7, AC-9, AC-10; AC-6 UI / AC-8 — ручные по инструкции в [tasks/003](tasks/003-providers-threading-readme.md), закрыты смоуком Оркестратора).
- Регрессия: полный набор юнит-тестов проекта (327) + Release-сборка.
- Риски: R1 (smoke с demo-ключом) и AC-7 live — не проверены, нет demo-ключа (см. «Ограничения evidence»).

## Результаты

| Сценарий | Статус | Комментарий |
|----------|--------|-------------|
| Автоматические прогоны (AC-1…AC-5, AC-7 unit, AC-9, AC-10) | **pass** | `dotnet test` — 327/327 (0 failed/skipped); `dotnet build -c Release --no-incremental` — 0 ошибок, предупреждения = baseline (61 × CS1591, все в `Models/`, идентично принятой базе 002/003); деталей в [test-runs](../../../qa/test-runs.md) |
| Целевой прогон фичи (лимит и ключ кэша) | **pass** | Фильтр `PoiskKinoMovieProviderTests|CacheKey|SearchLimitResolver|PluginConfiguration` — 67/67: провайдерский тест AC-7 `GetSearchResults_SearchLimitFromConfiguration_ChangesRequestLimitWithoutRestart` (смена 7→4 на том же экземпляре меняет URL запроса), резолвер валидации 1–10 (fallback дефолт + Warning, дедупликация/сброс памятки), конфигурация (дефолт 3, legacy XML без `<SearchLimit>` → 3, round-trip 7) |
| AC-6 (UI: поле «Количество результатов поиска», сохранение, дефолт на чистой конфигурации) | **pass** — смоук Оркестратора | docker Jellyfin 12.4.0.0: поле — number/spinbutton, valuemin=1, valuemax=10, на чистой конфигурации значение 3, русская подсказка (диапазон 1–10, дефолт 3, fallback 3 + предупреждение, применение без перезапуска) полная; ввод 5 → «Сохранить» → перезагрузка страницы → 5; консоль браузера 0 ошибок. Серверная часть — юнит-тесты (`PluginConfigurationTests`, 6/6) |
| AC-8 (персистентность: значение → перезапуск → UI + `<SearchLimit>` в XML, прочие настройки целы) | **pass** — смоук Оркестратора | 5 → «Сохранить» → в XML `<SearchLimit>5</SearchLimit>`; перезагрузка страницы → 5; перезапуск контейнера → UI 5, XML 5; ApiBaseUrl и IgnoreTmdbImages не изменились. Нюансы (проверку не снижают): пример значения в инструкции 7, фактически 5 — механизм персистентности тот же; ApiKey был пуст изначально, проверка «не сброшен» для него вырождена — компенсируется юнит-тестом round-trip `SerializeAndDeserialize_WithCustomSearchLimit_PreservesValue` (ApiKey="k", SearchLimit=7) и юнит-тестом legacy-XML `Deserialize_LegacyXmlWithoutSearchLimit_KeepsDefaultAndOtherValues` |
| AC-7 (применение без перезапуска в identify-диалоге: 8 → 2, сравнение направления) | **не выполнена — нет demo-ключа** | Нужен поиск с действующим ключом; смоук выполнен на чистой конфигурации без ключа. Серверная часть (чтение конфигурации на каждый вызов, лимит в ключе кэша) покрыта провайдерским тестом AC-7 (`GetSearchResults_SearchLimitFromConfiguration_ChangesRequestLimitWithoutRestart`) и клиентскими тестами `PoiskKinoApiClientSearchLimitTests` |
| R1 (smoke с demo-ключом: дефолт 3 и максимум 10, без 403/429, лог плагина чистый) | **не выполнена — нет demo-ключа** | Демо-ключ в среде отсутствовал; сетевые запросы к API ПоискКино в смоуке не выполнялись |

## Ручной smoke (выполнен Оркестратором, принят QA — 2026-11-28)

- **Среда:** docker-контейнер `jellyfin/jellyfin:latest` — Jellyfin Server 12.4.0.0; плагин собран из ветки `feature/search-limit-setting` в Release/net10.0 по коду коммита `c894715` (последний кодовый коммит фичи; после него до HEAD `bdc91ea` менялись только docs — `git diff --stat c894715..HEAD`); версия плагина 2026.11.28 — совпадает с `<Version>` в csproj и логом загрузки; конфиг `/tmp/opencode/jf-smoke-searchlimit/config` (зачищен после смоука по требованию Стейхолдера); доступ через chrome-devtools, пользователь smoke-admin.
- **Загрузка плагина:** docker logs — `Loaded assembly PoiskKinoMetadataPlugin, Version=2026.11.28.0 … from /config/plugins/PoiskKino/2026.11.28/PoiskKinoMetadataPlugin.dll` и `PoiskKinoMetadataPlugin.Plugin: PoiskKino Metadata Plugin loaded`; ошибок загрузки нет.
- **Факты AC-6 / AC-8** — в таблице «Результаты» выше; страница конфигурации `#/configurationpage?name=ПоискКино Metadata` открылась штатно.
- **Консоль браузера** на странице настроек: 0 ошибок.
- **Evidence-носители:** скриншот `/tmp/opencode/poiskkino-smoke-settings.png` — эфемерный путь, в git не персистирован (по требованию Стейхолдера контейнер и конфиг зачищены). QA скриншот визуально не пересматривала (модель без image-входа) — интерпретация опирается на структурированные DOM-факты chrome-devtools (spinbutton/valuemin/valuemax/значение/подсказка), переданные Оркестратором. Смоук воспроизводим по инструкции в [tasks/003](tasks/003-providers-threading-readme.md) («Инструкция ручного QA»).

## Автотесты

- Среда: локально, KDE neon (Linux x86_64), .NET SDK 10.0.115, `net10.0`; ветка `feature/search-limit-setting` @ `25d623a`.
- `dotnet test` — **327/327** passed, 0 failed, 0 skipped (ожидание 327/327 подтверждено).
- `dotnet build -c Release --no-incremental` — exit 0, **0 ошибок**, 61 предупреждение `CS1591` (все в `PoiskKinoMetadataPlugin/Models/`, новых нет).
- Целевой фильтр — **67/67**: `PoiskKinoMovieProviderTests` 35, `SearchLimitResolverTests` 26, `PluginConfigurationTests` 6.
- Клиентские тесты лимита (`PoiskKinoApiClientSearchLimitTests`: лимит в запросе, вне диапазона → дефолт + одиночный Warning с RawValue и без ApiKey, дедупликация/сброс памятки) — зелёные в полном прогоне.
- Ручные проверки: см. [smoke-чеклист Jellyfin](../../../qa/jellyfin-smoke.md) и инструкцию в [tasks/003](tasks/003-providers-threading-readme.md) («Инструкция ручного QA»).

## Ограничения evidence

- Закрыто смоуком Оркестратора (принят QA 2026-11-28): **AC-6 UI** (поле, атрибуты, подсказка, дефолт 3 на чистой конфигурации, сохранение/перезагрузка) и **AC-8** (персистентность в XML, перезапуск, прочие настройки целы). Среда: docker Jellyfin 12.4.0.0, код `c894715` (= код HEAD — после него только docs).
- Остаются открытыми ручные ограничения: **AC-7 live** (identify-диалог, смена числа подсказок без перезапуска) и **R1** (403/429 с demo-ключом, дефолт 3 и максимум 10) — demo-ключ недоступен; серверная часть AC-7 покрыта юнит-тестом.
- Решение по открытым ограничениям — за Оркестратором/Стейхолдером: провести с demo-ключом администратором на целевом сервере (инструкция в [tasks/003](tasks/003-providers-threading-readme.md)) либо принять как отложенные.
- Прецедент в репозитории: [eu-api-endpoint](../../../archive/features/eu-api-endpoint/qa.md) (2026-11-28) — pass с аналогичными ручными ограничениями.

## Вердикт

- [x] **Pass** — дефектов не найдено: автоматическая часть полностью зелёная; смоук Оркестратора (docker Jellyfin 12.4.0.0, код `c894715`) закрыл AC-6 UI и AC-8, плагин загрузился без ошибок, консоль браузера чистая. Открытые ручные ограничения: AC-7 live и R1 (нет demo-ключа) — не дефекты, учтены в «Ограничениях evidence».
- [ ] Rework → Техлид (список дефектов ниже)

### Дефекты

1. _нет_
