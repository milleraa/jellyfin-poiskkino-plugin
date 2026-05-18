# CI/CD

*Источники: bootstrap 2026-05-18; планы Стейхолдера (2026-05-18). Workflow **ещё не добавлен** в репозиторий.*

| Поле | Значение |
|------|----------|
| **Платформа** | GitHub Actions (планируется) |
| **CI файл** | `.github/workflows/` — *создать отдельной задачей* |
| **Цель pipeline** | `dotnet build -c Release`, артефакт `PoiskKinoMetadataPlugin.dll` |
| **Обязательные checks** | после появления workflow — успешная сборка на PR/push в `main` |
| **Артефакты** | DLL из Release-сборки (upload artifacts в Actions) |
| **Релизы** | пока только ручная установка DLL; автоматический release/job — в backlog |
| **Владелец диагностики** | Стейхолдер |

## Текущее состояние (до Actions)

- Локально: `dotnet build -c Release` ([`README.md`](../../README.md))
- PR checks: нет (protected branches не включены)

## Планируемый workflow (черновик требований)

1. Trigger: `push` / `pull_request` на `main` (и feature-ветки при PR)
2. Job: setup .NET 9 SDK → restore → `dotnet build -c Release`
3. Upload artifact: `PoiskKinoMetadataPlugin.dll` (и при необходимости зависимости из `bin/Release/net9.0/`)

## Диагностика

- **Логи:** вкладка Actions в GitHub; локально — вывод `dotnet build`
- **Rerun:** Re-run job в GitHub Actions UI

## Связанное

- [DevOps](../engineering/devops.md)
