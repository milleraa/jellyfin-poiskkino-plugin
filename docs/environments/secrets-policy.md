# Secrets policy

- **Никогда** не коммить пароли, приватные ключи, API keys, connection strings с паролями.
- В репозитории допустимы только **имена** переменных окружения и ссылки на менеджер секретов (Vault, GitLab variables, и т.д.).
- В `docs/` описывайте: какие секреты нужны, кто выдаёт доступ, rotation policy — без значений.

## Пример допустимой записи

`DATABASE_URL` — задаётся в GitLab CI variables, scope: protected, environment: production.

## Bootstrap (2026-05-18)

- **Подтверждено при bootstrap:** секреты (API-ключ ПоискКино) **не хранятся в репозитории**; задаются в конфигурации плагина Jellyfin ([`Configuration.cs`](../../Configuration.cs), Dashboard → Plugins).
- **Ответственный за выдачу API-ключа ПоискКино:** владелец аккаунта на [poiskkino.dev](https://poiskkino.dev) (Стейхолдер / администратор Jellyfin).
- **CI secrets:** N/A (CI не настроен).
