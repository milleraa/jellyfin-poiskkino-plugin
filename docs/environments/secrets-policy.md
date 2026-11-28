# Secrets policy

- **Никогда** не коммить пароли, приватные ключи, API keys, connection strings с паролями.
- В репозитории допустимы только **имена** переменных окружения и указания, где настраивать секреты.
- В `docs/` описывайте: какие секреты нужны, кто выдаёт доступ, rotation policy — без значений.

## Bootstrap (2026-05-18), обновлено 2026-08-23

- **Подтверждено при bootstrap:** секреты (API-ключ ПоискКино) **не хранятся в репозитории**; задаются в конфигурации плагина Jellyfin ([`Configuration.cs`](../../PoiskKinoMetadataPlugin/Configuration.cs), Dashboard → Plugins).
- **Ответственный за выдачу API-ключа ПоискКино:** владелец аккаунта на [poiskkino.dev](https://poiskkino.dev) (Стейхолдер / администратор Jellyfin).
- **CI secrets:** N/A — GitHub Actions использует встроенный `GITHUB_TOKEN` (permissions `contents: write` для Release); дополнительные секреты не требуются.
