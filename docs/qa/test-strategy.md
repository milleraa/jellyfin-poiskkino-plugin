# Test strategy

*Источники: [`PoiskKinoMetadataPlugin.UnitTests/`](../../PoiskKinoMetadataPlugin.UnitTests/) (xUnit + Moq), [`.github/workflows/ci.yaml`](../../.github/workflows/ci.yaml), [`README.md`](../../README.md) (ручное тестирование).*

## Уровни

| Уровень | Статус | Примечание |
|---------|--------|------------|
| **Unit** | реализован | `PoiskKinoMetadataPlugin.UnitTests` (xUnit 2.9.3 + Moq 4.20.72); гоняются в CI (`dotnet test`) |
| **Integration** | вручную | проверка через Jellyfin + реальный/демо API-ключ |
| **E2E (Playwright)** | не планируется | нет отдельного UI продукта — см. [`playwright.md`](playwright.md) |

## Обязательно перед PR/MR

1. Green CI: `dotnet build -c Release` + `dotnet test` (`.github/workflows/ci.yaml`)
2. Локально: `dotnet test` без падений
3. Ручная проверка в Jellyfin при изменении поведения провайдеров: идентификация фильма/сериала, постеры, настройка API-ключа
4. При изменении API-клиента — учёт лимита 200 req/day и поведения кэша (см. README)

## Политика покрытия

Покрытие растёт **постепенно вместе с изменяемым кодом**: новые и изменяемые участки покрываются unit-тестами; ретроспективное покрытие всего legacy-кода не планируется (подтверждено Стейхолдером, 2026-08-23).

## Критерии для PR

- Сборка успешна, тесты зелёные
- Нет секретов в diff (API-ключ только в runtime-конфиге Jellyfin)
- Документация обновлена при изменении поведения ([`../engineering/testing.md`](../engineering/testing.md))
