# QA — покрытие поведения провайдеров

**Вердикт: PASS.** Дефектов и блокеров не обнаружено. Приёмка Техлида и закрытие DevOps impact check подтверждены в `eb78b38`; handoff в QA — `af52697`.

## Прогон — 2026-11-28

- Среда: локально, KDE neon 24.04 (Linux x86_64), .NET SDK 10.0.115, runtime 10.0.15, `net10.0`.
- Ветка `hotfix/provider-test-coverage`, проверен `af52697` относительно локального `main` (`4cbe4d4`).
- Полный `dotnet test`: 283 прошло, 0 упало, 0 пропущено (код 0). `dotnet build -c Release --no-incremental`: 0 ошибок, 61 существующее предупреждение `CS1591` в неизменённых моделях. Удалённый CI в рамках QA не проверялся.

## Критерии приёмки

| Критерий из [incident.md](incident.md) | Независимая проверка QA | Итог |
|---|---|---|
| Поиск сериалов по году и возврат исходной выдачи при отсутствии совпадений | Подмена HTTP всегда отдаёт два сериала: ID `101` (2001–2004) и `202` (2011–2019). При вызове `GetSearchResults` с 2015 тест проверяет единственный результат `202`; с 2020 — оба ID независимо от порядка. Соответствует ADR-0001. | Pass |
| Роли в метаданных фильмов и сериалов без потери сценариев | Сверены пары «профессия → роль» с `main`: сохранены 17/17 для фильмов и 8/8 для сериалов. В каждом провайдере добавлен случай `enProfession: "Director"` без поля `profession`. Тесты вызывают `GetMetadata` и проверяют `result.People`; подмена отдаёт JSON, рефлексивные вызовы удалены. Проверка нескольких персон фильма `GetMetadata_MapsPersons` не изменена. | Pass |
| Обе контрольные поломки обнаруживаются; восстановленная версия проходит | Сверены результаты экспериментов из задач [001](tasks/001-series-search-year.md#решение-техлида) и [002](tasks/002-metadata-person-roles.md#решение-техлида); оба эксперимента независимо повторены, результаты ниже. | Pass |
| Не изменены поведение плагина, настройки, CI и упаковка | `git diff --name-status main...af52697`: только два файла тестов провайдеров (+85/−51 строк суммарно) и документация hotfix. Нет изменений продукта, зависимостей, настроек, CI или упаковки. `git diff --check main...af52697` без замечаний. | Pass |

## Независимые мутационные прогоны

В отдельной временной копии `git archive af52697` вносили по одной мутации в `PoiskKinoMetadataPlugin/PoiskKinoSeriesProvider.cs`. После каждой исходник побайтово сверяли с оригиналом через `cmp` и повторно собирали и тестировали. Рабочая ветка не изменялась.

| Задача / намеренная поломка | До | С поломкой | После восстановления |
|---|---|---|---|
| 001 — удалить вызов `SearchYearFilter.Apply` в `GetSearchResults` | 29/29 | 28 прошло, 1 упал (код 1): `GetSearchResults_YearWithinReleaseYears_ReturnsOnlyMatchingSeries` — `Assert.Single() Failure: The collection contained 2 items` | 29/29 |
| 002 — заменить `Type = GetPersonKind(person.Profession ?? person.EnProfession)` на `Type = PersonKind.Actor` в маппинге сериала | 52/52 | 46 прошло, 6 упали (код 1): `GetMetadata_MapsPersonRole`, `Assert.Equal(expected, person.Type)`. Получен `Actor` вместо `Director` (3 случая, в том числе только `enProfession`), `Composer`, `Producer` и `Writer`. | 52/52 |

Команды точечных прогонов:

```bash
dotnet test --filter 'FullyQualifiedName~PoiskKinoSeriesProviderTests|FullyQualifiedName~SearchYearFilterTests'
dotnet test --filter 'FullyQualifiedName~PoiskKinoMovieProviderTests|FullyQualifiedName~PoiskKinoSeriesProviderTests'
```

В копии после восстановления обеих мутаций полный `dotnet test`: 283 прошло, 0 упало, 0 пропущено (код 0).

## Ограничения

Smoke в живом Jellyfin и запросы к реальному API не выполнялись. По [стратегии QA](../../../qa/test-strategy.md) и [smoke-чеклисту](../../../qa/jellyfin-smoke.md) ручная проверка не обязательна: изменения усиливают только тесты, поведение провайдеров и настройки не менялись. Подмена HTTP проверяет результат работы провайдеров и десериализацию, но не интеграцию с сервером или API. Это не блокирует приёмку.
