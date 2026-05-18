# DevOps

*Источники: поиск Dockerfile, compose, CI configs, deploy scripts — **не найдено**; [`README.md`](../../README.md) (ручная установка).*

## Зона ответственности

- Dockerfile, docker-compose и локальная инфраструктура разработки.
- CI/CD pipelines, runners, обязательные checks, артефакты.
- Deploy scripts, Helm charts, Kubernetes manifests, Terraform/infra code при наличии.
- Диагностика pipeline/deploy проблем и фиксация runbook notes.

## Docker и локальный запуск

- **Dockerfile / compose:** N/A в репозитории
- **Локальная проверка:** сборка `dotnet build -c Release`, копирование `PoiskKinoMetadataPlugin.dll` в каталог плагинов Jellyfin, перезапуск сервера ([`README.md`](../../README.md))

## CI/CD

- **CI config:** отсутствует (см. [`../environments/ci-cd.md`](../environments/ci-cd.md))
- **Обязательные jobs для PR:** N/A
- **Артефакты / cache:** N/A

## Deployment

- **Способ:** ручная установка DLL администратором Jellyfin; централизованный deploy из репо не описан
- **dev/stage/prod:** см. [`../environments/deployment-targets.md`](../environments/deployment-targets.md)
- **Smoke checks:** ручная идентификация метаданных фильма/сериала в Jellyfin после настройки API-ключа

## Secrets

Секреты не хранятся в репозитории. API-ключ — в конфигурации плагина Jellyfin. Подробности: [`../environments/secrets-policy.md`](../environments/secrets-policy.md).

## Связанные документы

- [Environments](../environments/README.md)
- [CI/CD](../environments/ci-cd.md)
- [Deployment targets](../environments/deployment-targets.md)
- [Delivery](delivery.md)
- `.cursor/agents/devops.md`
- `.cursor/rules/devops.mdc`
