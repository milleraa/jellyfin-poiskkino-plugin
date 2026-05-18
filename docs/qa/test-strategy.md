# Test strategy

*Источники: поиск `*Test*.csproj`, `tests/`, CI test jobs — **не найдено**; [`README.md`](../../README.md) (ручное тестирование).*

## Уровни

| Уровень | Статус в v1 | Примечание |
|---------|-------------|------------|
| **Unit** | не реализован | нет test project |
| **Integration** | не реализован | проверка через Jellyfin + реальный/демо API-ключ |
| **E2E (Playwright)** | не планируется | нет отдельного UI продукта — см. [`playwright.md`](playwright.md) |

## Обязательно перед PR/MR (текущая практика)

1. `dotnet build -c Release` без ошибок
2. Ручная проверка в Jellyfin: идентификация фильма/сериала, постеры, настройка API-ключа
3. При изменении API-клиента — учёт лимита 200 req/day и поведения кэша (см. README)

## Критерии для PR

- Сборка успешна
- Нет секретов в diff (API-ключ только в runtime-конфиге Jellyfin)
- Документация обновлена при изменении поведения ([`../engineering/testing.md`](../engineering/testing.md))

**После GitHub Actions:** обязательный green `dotnet build -c Release` в PR; ручная проверка в локальном Jellyfin — по усмотрению Стейхолдера. См. [`../environments/ci-cd.md`](../environments/ci-cd.md).
