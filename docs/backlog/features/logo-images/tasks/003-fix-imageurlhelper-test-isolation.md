# Task 003 — Rework: изоляция `ImageUrlHelperTests` от статического `Plugin.Instance` (flaky CI)

## Метаданные

- **ID:** 003 (rework, источник: PR/MR feedback)
- **Feature / Hotfix:** [docs/backlog/features/logo-images/](../)
- **Назначено:** `developer-csharp`
- **Стек / infra-область:** C# / xUnit тестовая инфраструктура; продакшн-код не меняется
- **Статус:** pending
- **Ветка:** `feature/logo-images` (тот же worktree `/home/alex/src/my/jellyfin-metadata-plugin-wt-logo-images`)

## Описание

Устранить гонку изоляции, дающую flaky CI на [PR #3](https://github.com/milleraa/jellyfin-poiskkino-plugin/pull/3) (fail → pass → fail → pass): предсуществующий тест `ImageUrlHelperTests.ShouldIgnoreTmdbImages_WhenPluginNotInitialized_ReturnsFalse` (`PoiskKinoMetadataPlugin.UnitTests/Helpers/ImageUrlHelperTests.cs`) требует `Plugin.Instance == null`, но живёт в дефолтной xUnit-коллекции и выполняется **параллельно** с `PluginInstanceCollection`, которая инициализирует плагин (сброс только в `Dispose`). Новые тесты фичи расширили окно гонки.

Решение (рекомендация Финализатора/QA, подтверждена Стейхолдером):

1. Включить класс `ImageUrlHelperTests` в общую коллекцию `[Collection(nameof(PluginInstanceCollection))]`.
2. В тесте «not initialized» явно обеспечить предусловие: сохранить текущее `Plugin.Instance`, выставить `null`, выполнить проверки, восстановить исходное значение в `finally`. Альтернатива (если чисто и минимально) — локальная непараллельная коллекция только для этого класса; выбор за исполнителем по принципу наименьшего диффа.

Запрещено: менять продакшн-код (`PoiskKinoImageProvider.cs`, `ImageUrlHelper.cs`), трогать тесты фичи 001/002 без необходимости, отключать/удалять сам тест «not initialized» (он проверяет реальное поведение).

## Acceptance criteria

1. Тест `ShouldIgnoreTmdbImages_WhenPluginNotInitialized_ReturnsFalse` детерминирован: не зависит от порядка/параллельности других коллекций.
2. Продакшн-код не изменён; поведение плагина не изменилось.
3. Локально ≥5 полных прогонов `dotnet test` подряд — все зелёные (163+ тестов).
4. Если в решении есть механизм параллельного прогона коллекций — убедиться, что `ImageUrlHelperTests` больше не пересекается с `PluginInstanceCollection` (проверка чтением конфигурации xUnit + кода).
5. `dotnet build` — 0 ошибок.

## Технические заметки

- Механизм гонки и evidence: Changelog `status.md` от 2026-08-24 («CI-инцидент» и «BLOCKER»), CI job'ы PR #3 (runs 32688926759, 32689565732).
- xUnit v2: коллекции выполняются параллельно друг другу; внутри одной коллекции — последовательно.
- Статическое состояние: `Plugin.Instance` (инициализация в `PluginInstanceCollection` fixture, сброс в `Dispose`).
- Не протянуть изменение конфига: тест «not initialized» обязан вернуть `Plugin.Instance` к исходному значению.

## DevOps impact check

- Новые/изменённые env vars / secrets: **нет**
- Новые интеграции/порты/очереди/storage/jobs: **нет**
- Docker/compose/CI/CD/deploy/Helm/K8s/observability: **нет**
- Итог: DevOps-задача **не нужна**

## Проверка

```bash
dotnet build PoiskKinoMetadataPlugin.slnx
for i in 1 2 3 4 5; do dotnet test PoiskKinoMetadataPlugin.slnx --no-build || break; done
```

Результат: 5 прогонов подряд без единого fail.

## Handoff

- После выполнения: локальный commit без push (`test(infra): isolate ImageUrlHelperTests from Plugin.Instance static state`), статус задачи → `in review`, сообщить Техлиду.
