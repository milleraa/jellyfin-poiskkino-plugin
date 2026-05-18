# Stack notes

*Источники: [`PoiskKinoMetadataPlugin.csproj`](../../PoiskKinoMetadataPlugin.csproj), структура репозитория (bootstrap 2026-05-18).*

Индекс стеков, используемых в этом репозитории. Подробные соглашения — в [`stacks/`](stacks/).

## Активные стеки в v1

| Стек | Статус | Примечание |
|------|--------|------------|
| [C# / .NET](stacks/dotnet.md) | **активен** | единственный прикладной стек; один проект `PoiskKinoMetadataPlugin.csproj` |
| [Go](stacks/go.md) | не в v1 | — |
| [TypeScript](stacks/typescript.md) | не в v1 | — |
| [React](stacks/react.md) | не в v1 | embedded HTML в плагине, не React SPA |
| [PostgreSQL](stacks/postgresql.md) | не в v1 | нет БД в проекте |

**Тип репозитория:** монорепо не используется — один .NET plugin assembly.

## Новый стек

1. Создайте `docs/engineering/stacks/<stack>.md`.
2. Добавьте роль `.cursor/agents/developer-<stack>.md`.
3. Добавьте правило `.cursor/rules/<stack>-developer.mdc` с подходящими `globs`.
4. Добавьте ссылку в [`stacks/README.md`](stacks/README.md) и в таблицу выше.
