# Task 002 — TMDB-фильтрация логотипов: покрытие тестами (AC-6, AC-7)

## Метаданные

- **ID:** 002
- **Feature / Hotfix:** [docs/backlog/features/logo-images/](../)
- **Назначено:** `developer-csharp`
- **Стек / infra-область:** C# / xUnit unit-тесты; продакшн-код не меняется (если тест вскроет баг — согласовать с Техлидом до правок)
- **Статус:** done
- **Ветка:** `feature/logo-images` (тот же worktree `/home/alex/src/my/jellyfin-metadata-plugin-wt-logo-images`; выполнять строго после задачи 001)

## Описание

Добавить unit-тесты, фиксирующие политику «игнорировать изображения TMDB» для нового типа `Logo`. Механизм — существующий пост-фильтр в конце `GetImages` (`ImageUrlHelper.ShouldIgnoreTmdbImages()` + `IsTmdbUrl`), отдельный код фильтрации логотипов не нужен и не должен появляться ([architecture.md](../architecture.md), раздел «Фильтрация TMDB»).

Тесты в `PoiskKinoMetadataPlugin.UnitTests/Providers/PoiskKinoImageProviderTests.cs`, по образцу существующей теории `GetImages_TmdbUrlFiltering_RespectsConfiguration`:

1. **AC-6 / AC-11**: JSON по ID с `logo.url` на домене `tmdb.org` + постер/фон на не-TMDB доменах, конфиг по умолчанию (`IgnoreTmdbImages = true`, действует из дефолта после `SetUpPlugin`; можно выставить явно через `Plugin.Instance.Configuration.IgnoreTmdbImages = true` — архитектура рекомендует явность) → в результате нет изображений типа `Logo`, Primary и Backdrop присутствуют.
2. **AC-7**: тот же JSON, но `Plugin.Instance.Configuration.IgnoreTmdbImages = false` (выставить после `SetUpPlugin`) → логотип возвращается как есть; все три типа присутствуют.

## Acceptance criteria

1. Тест 1: TMDB-логотип отфильтрован при включённой настройке; постер/фон остаются (AC-6, AC-11).
2. Тест 2: TMDB-логотип возвращается при выключенной настройке (AC-7).
3. Изменение флага в тесте 2 не протекает в другие тесты коллекции: восстановить значение (`Plugin.Instance.Configuration.IgnoreTmdbImages = true`) в конце теста или завершить тест повторным `SetUpPlugin`.
4. Продакшн-код не изменён (допустимо только если тест вскрыл дефект — тогда остановиться и эскалировать Техлиду).
5. AC-12: `dotnet build` и `dotnet test` зелёные на всём решении, включая тесты задачи 001.

## Технические заметки

- Зависимость: задача 001 должна быть принята Техлидом (тесты 002 опираются на реализацию Logo).
- Пример inline JSON — см. теорию `GetImages_TmdbUrlFiltering_RespectsConfiguration` (raw string literal + `Replace("__URL__", …)`).
- `IgnoreTmdbImages` читается через статический `Plugin.Instance?.Configuration?.IgnoreTmdbImages` (`PoiskKinoMetadataPlugin/ImageUrlHelper.cs:27–29`) — мокать не нужно.
- Тесты выполняются в общей коллекции `PluginInstanceCollection` — каждый тест уже начинается с `SetUpPlugin(...)`, что пересоздаёт конфиг с дефолтом `true`.

## DevOps impact check

- Новые/изменённые переменные окружения или secrets: **нет**
- Новые внешние интеграции, порты, очереди, storage, scheduled jobs: **нет**
- Docker/compose/CI/CD/deploy/Helm/Kubernetes/observability: **нет**
- Итог: DevOps-задача **не нужна** (тесты только).

## Проверка

```bash
dotnet build PoiskKinoMetadataPlugin.slnx
dotnet test PoiskKinoMetadataPlugin.slnx
```

Результат: 0 failed; новые тесты видны в выводе (`Passed! ... Failed! 0`).

## Handoff

- После выполнения: локальный commit без push (например, `test(images): cover TMDB filtering for logo type`), статус задачи → `in review`, сообщить Техлиду.
