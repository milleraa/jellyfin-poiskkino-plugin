# Stack-specific standards

Подробные инженерные соглашения по каждому стеку. Общие правила остаются в [`../coding-standards.md`](../coding-standards.md), а этот раздел уточняет версии, layout, линтеры, тесты и локальные договорённости.

## Стеки

- [C# / .NET](dotnet.md)
- [Go](go.md)
- [TypeScript](typescript.md)
- [React](react.md)
- [PostgreSQL](postgresql.md)

## Как обновлять

- Заполняйте документы после bootstrap или при появлении реального решения в проекте.
- Не храните здесь секреты, URL окружений и доступы — для этого есть [`../../environments/README.md`](../../environments/README.md).
- Если стек добавляется впервые, создайте новый `<stack>.md`, роль `.cursor/agents/developer-<stack>.md` и правило `.cursor/rules/<stack>-developer.mdc`.
