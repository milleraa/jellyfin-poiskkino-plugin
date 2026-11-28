# 001 — Поиск сериалов по году

## Метаданные

- **ID:** 001
- **Hotfix:** [provider-test-coverage](../README.md)
- **Назначено:** `developer-csharp`
- **Стек:** C# / .NET 10, xUnit, Moq
- **Ветка:** `hotfix/provider-test-coverage`
- **Статус:** ready-for-review — ожидается приёмка Техлидом
- **Локальный коммит:** `test(providers): усиль покрытие поиска сериалов по году` в ветке `hotfix/provider-test-coverage` (без push)
- **Код:** [тесты сериалов](../../../../../PoiskKinoMetadataPlugin.UnitTests/Providers/PoiskKinoSeriesProviderTests.cs), [провайдер](../../../../../PoiskKinoMetadataPlugin/PoiskKinoSeriesProvider.cs)

## Описание

Усилить существующие тесты `GetSearchResults` с одной записью. Подмена API отдаёт два сериала с разными идентификаторами и непересекающимися диапазонами `releaseYears`, независимо от запрошенного года.

## Acceptance criteria

1. Для года только в одном диапазоне возвращается ровно одна запись с ID подходящего сериала.
2. Для года вне обоих диапазонов возвращаются обе записи; проверяются оба ID.
3. В изолированной копии при отключении вызова `SearchYearFilter.Apply` тест совпадения падает на утверждении о выдаче, а не на сборке или подготовке. До поломки и после восстановления тесты проходят.

## Технические заметки

Правило задано в [ADR-0001](../../../../architecture/decisions/ADR-0001-search-year-filter.md). Не менять тесты самого фильтра: здесь проверяется, что публичный поиск провайдера применяет фильтрацию, даже когда API её не выполняет. Зависимостей нет.

## DevOps impact check

По плану изменений нет: тесты используют локальные подмены, не требуют новых секретов, окружения, интеграций, CI, релиза, установки или диагностики. Техлид сверит это с итоговым diff.

## Проверка и критерий готовности для Техлида

`dotnet test --filter 'FullyQualifiedName~PoiskKinoSeriesProviderTests|FullyQualifiedName~SearchYearFilterTests'`, `git diff --check`. Приложить точную мутацию, команду и результат до/во время/после, включая упавшее утверждение. В коммите нет изменений кода продукта.

## Результат и проверки

Оба сценария вызывают публичный `GetSearchResults` с одним и тем же подменённым HTTP-ответом: ID `101` (2001–2004) и `202` (2011–2019). Для 2015 ожидается только `202`, для 2020 — оба ID независимо от порядка. Старая фикстура с одной записью не отличала фильтрацию от возврата исходного списка. Тесты самого фильтра и код плагина не изменены.

В изолированной копии из `git archive HEAD` (`0327b02`) из `PoiskKinoMetadataPlugin/PoiskKinoSeriesProvider.cs` временно удалена строка:

```csharp
series = SearchYearFilter.Apply(series, searchInfo.Year, SearchYearFilter.MatchesSeries, _logger);
```

Команда: `dotnet test --filter 'FullyQualifiedName~PoiskKinoSeriesProviderTests|FullyQualifiedName~SearchYearFilterTests'`.

| Состояние копии | Результат |
|-----------------|-----------|
| Исходные тесты, без мутации и с мутацией | 28/28 в обоих прогонах |
| Усиленные тесты, до мутации | 28/28 |
| Усиленные тесты, с мутацией | 27 прошло, 1 упал (код выхода 1): `GetSearchResults_YearWithinReleaseYears_ReturnsOnlyMatchingSeries` — `Assert.Single() Failure: The collection contained 2 items` |
| Усиленные тесты, после побайтового восстановления исходника (`cmp`) и пересборки | 28/28; полный `dotnet test` — 281/281 |

В рабочей ветке `dotnet test` — 281/281, `git diff --check` — без замечаний. При пересборке плагина видны существующие предупреждения `CS1591` в моделях. Подмена HTTP проверяет локальную фильтрацию, а не поведение живого API или Jellyfin.

## Handoff

См. [статус hotfix](../status.md). Задача 002 не начата и ждёт приёмки 001 Техлидом.
