# DevOps

*Источники: [`.github/workflows/ci.yaml`](../../.github/workflows/ci.yaml), [`.github/workflows/release.yaml`](../../.github/workflows/release.yaml), [`README.md`](../../README.md), подтверждение Стейхолдера (2026-05-18).*

## Docker и локальный запуск

- **Dockerfile / compose:** N/A
- **Локальная проверка:** `dotnet build -c Release` → `PoiskKinoMetadataPlugin.dll` → каталог плагинов локального Jellyfin → перезапуск ([`README.md`](../../README.md))

## CI/CD

| Параметр | Значение |
|----------|----------|
| **Платформа** | GitHub Actions (реализовано 2026-06-30) |
| **Сборка** | `dotnet build -c Release` + `dotnet test` на push/PR в `main` (`.github/workflows/ci.yaml`) |
| **Артефакт** | zip: DLL + PDB + XML + контрольные суммы (md5/sha256) |
| **Обязательность для PR** | green build + тесты на PR в `main` |
| **Релиз** | автоматический по тегу `v*`: `dotnet publish` → zip → GitHub Release (`release.yaml`) |

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
