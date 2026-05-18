# DevOps

*Источники: README, подтверждение Стейхолдера (2026-05-18).*

## Docker и локальный запуск

- **Dockerfile / compose:** N/A
- **Локальная проверка:** `dotnet build -c Release` → `PoiskKinoMetadataPlugin.dll` → каталог плагинов локального Jellyfin → перезапуск ([`README.md`](../../README.md))

## CI/CD

| Параметр | Значение |
|----------|----------|
| **Платформа** | GitHub Actions (запланировано, файл workflow пока отсутствует) |
| **Сборка** | `dotnet build -c Release` на .NET 9 |
| **Артефакт** | `PoiskKinoMetadataPlugin.dll` |
| **Обязательность для PR** | после внедрения Actions — green build |
| **Релиз** | v1: ручная установка DLL; автоматизация release — отдельная будущая задача |

Подробности: [`../environments/ci-cd.md`](../environments/ci-cd.md).

## Deployment

- **Способ:** ручное копирование DLL в Jellyfin
- **Окружения:** только local dev (см. [`../environments/deployment-targets.md`](../environments/deployment-targets.md))
- **Smoke:** идентификация фильма/сериала в Jellyfin, проверка API-ключа в настройках плагина

## Secrets

API-ключ ПоискКино — только в конфигурации Jellyfin. См. [`../environments/secrets-policy.md`](../environments/secrets-policy.md).

## Связанные документы

- [Environments](../environments/README.md)
- [CI/CD](../environments/ci-cd.md)
- [Deployment targets](../environments/deployment-targets.md)
- [Delivery](delivery.md)
