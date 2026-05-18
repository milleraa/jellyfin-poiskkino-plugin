# PostgreSQL standards

**Статус:** не используется в v1.

*Источник: нет миграций, SQL, ORM; состояние только в Jellyfin и in-memory кэше плагина.*

## Версия и tooling

- **TODO:** версия PostgreSQL.
- **TODO:** инструмент миграций.
- **TODO:** способ локального запуска БД и тестовой БД.

## Schema и migrations

- **TODO:** naming для таблиц, колонок, индексов, constraints.
- **TODO:** правила backward-compatible миграций.
- **TODO:** политика seeds/reference data.

## Запросы и производительность

- **TODO:** правила индексов.
- **TODO:** когда требуется `EXPLAIN`.
- **TODO:** транзакции, locks, isolation level.

## Тестирование

- **TODO:** проверка миграций.
- **TODO:** integration tests с БД.

## Связанные правила Cursor

- `.cursor/rules/postgresql-developer.mdc`
- `.cursor/agents/developer-postgresql.md`
