# CI/CD

*Источники: поиск `.github/workflows/`, `.gitlab-ci.yml`, `.gitea/workflows/` — **не найдено** (bootstrap 2026-05-18).*

| Поле | Значение |
|------|----------|
| **CI файл** | отсутствует в репозитории |
| **Обязательные checks** | нет автоматических checks в PR; ручная сборка: `dotnet build -c Release` ([`README.md`](../../README.md)) |
| **Артефакты** | `PoiskKinoMetadataPlugin.dll` после локальной сборки; публикация в репозиторий артефактов не настроена |
| **Владелец диагностики** | Maintainer / DevOps (при появлении pipeline) |

## Диагностика

- **Логи:** локально — вывод `dotnet build`; в Jellyfin — логи сервера при работе плагина
- **Rerun / эскалация:** *после настройки CI — уточнить*

## Связанное

- [DevOps](../engineering/devops.md)
