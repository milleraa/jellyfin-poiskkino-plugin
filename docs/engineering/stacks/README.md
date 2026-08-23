# Stack-specific standards

Подробные инженерные соглашения по каждому стеку. Общие правила остаются в [`../coding-standards.md`](../coding-standards.md), а этот раздел уточняет версии, layout, линтеры, тесты и локальные договорённости.

## Стеки

- [C# / .NET](dotnet.md)
- [Go](go.md)
- [TypeScript](typescript.md)
- [React](react.md)
- [PostgreSQL](postgresql.md)

## Актуальная документация (внешние источники)

В каждом `<stack>.md` — ссылки на **официальные** и регулярно обновляемые источники. Их используют агенты (`webfetch`, `scout`) и люди при заполнении TODO после bootstrap.

| Стек | Главный источник |
|------|------------------|
| C# / .NET | [Microsoft Learn — .NET](https://learn.microsoft.com/dotnet/) |
| Go | [go.dev — Documentation](https://go.dev/doc/) |
| TypeScript | [TypeScript Handbook](https://www.typescriptlang.org/docs/) |
| React | [react.dev](https://react.dev/) |
| PostgreSQL | [PostgreSQL Documentation](https://www.postgresql.org/docs/current/) |

**DeepWiki MCP** (бесплатно, публичные GitHub-репо) включён в [`opencode.json`](../../../opencode.json). В каждом `<stack>.md` — таблица рекомендуемых `owner/repo`; см. [README.md](../../../README.md#mcp-серверы).

## Как обновлять

- Заполняйте документы после bootstrap или при появлении реального решения в проекте.
- Не храните здесь секреты, URL окружений и доступы — для этого есть [`../../environments/README.md`](../../environments/README.md).
- Если стек добавляется впервые, создайте новый `<stack>.md` и роль `.opencode/agents/developer-<stack>.md` со встроенными стековыми правилами; добавьте секцию **Актуальная документация** по образцу соседних стеков.
