# CI/CD

*Источники: bootstrap 2026-05-18; реализовано 2026-06-30.*

| Поле | Значение |
|------|----------|
| **Платформа** | GitHub Actions |
| **CI файл** | `.github/workflows/ci.yaml` |
| **Release файл** | `.github/workflows/release.yaml` |
| **Цель CI** | `dotnet build -c Release` + `dotnet test` на push/PR в `main` |
| **Цель Release** | `dotnet publish` → zip → GitHub Release по тегу `v*` |
| **Версионирование** | календарное (например `v2026.06.30`) |
| **Обязательные checks** | успешная сборка и тесты на PR/push в `main` |
| **Владелец диагностики** | Стейхолдер |

## Workflow CI (`.github/workflows/ci.yaml`)

1. **Trigger:** `push` / `pull_request` на `main` (игнорируя `*.md` и `docs/`)
2. **Steps:** setup .NET 9 SDK → `dotnet restore` → `dotnet build -c Release --no-restore` → `dotnet test --no-restore --verbosity normal`

## Workflow Release (`.github/workflows/release.yaml`)

1. **Trigger:** push тега `v*` (например `v2026.06.30`)
2. **Steps:**
   - `dotnet publish` с версией из тега
   - Упаковка DLL + PDB + XML в zip
   - Вычисление md5 и sha256
   - Создание GitHub Release

## Релизный архив

Содержит всё необходимое для ручной установки:
- `PoiskKinoMetadataPlugin.dll` — плагин
- `PoiskKinoMetadataPlugin.pdb` — отладочные символы
- `PoiskKinoMetadataPlugin.xml` — документация API

### Установка

1. Скачать архив из GitHub Release
2. Распаковать в `/var/lib/jellyfin/plugins/PoiskKinoMetadataPlugin/` (Linux) или `%ProgramData%\Jellyfin\Server\plugins\PoiskKinoMetadataPlugin\` (Windows)
3. Перезапустить Jellyfin

## Диагностика

- **Логи:** вкладка Actions в GitHub; локально — вывод `dotnet build`
- **Rerun:** Re-run job в GitHub Actions UI
- **Release:** вкладка Releases на GitHub

## История

| Дата | Изменение |
|------|-----------|
| 2026-06-30 | Созданы CI и Release workflow. Исправлен csproj (ExcludeAssets runtime). |

## Связанное

- [DevOps](../engineering/devops.md)
- [Git remotes](git-remotes.md)
