# Testing

- Пирамида тестов: unit → integration → e2e по необходимости.
- UI и web-flows: [Playwright](../qa/playwright.md).
- Evidence: [test-runs](../qa/test-runs.md).

Правила QA встроены в `.opencode/agents/qa-playwright.md`.

## Актуальная документация

| Уровень / инструмент | Источник |
|----------------------|----------|
| Пирамида тестов (концепция) | [Martin Fowler — Test Pyramid](https://martinfowler.com/bliki/TestPyramid.html) |
| Playwright (e2e, UI) | [Playwright Documentation](https://playwright.dev/docs/intro) |
| Playwright — CI, reporters | [CI environments](https://playwright.dev/docs/ci), [Reporters](https://playwright.dev/docs/test-reporters) |
| .NET testing | [Unit testing in .NET](https://learn.microsoft.com/dotnet/core/testing/) |
| Go testing | [Package testing](https://pkg.go.dev/testing) |
| TypeScript / React (unit, component) | [Vitest](https://vitest.dev/guide/), [React Testing Library](https://testing-library.com/docs/react-testing-library/intro/) |
| PostgreSQL (integration с БД) | [PostgreSQL Documentation](https://www.postgresql.org/docs/current/) — см. также [`stacks/postgresql.md`](stacks/postgresql.md) |

## DeepWiki MCP (репозитории)

Включён в [`opencode.json`](../../opencode.json). См. также стековые таблицы в [`stacks/`](stacks/).

| `owner/repo` | Когда использовать |
|--------------|-------------------|
| [`microsoft/playwright`](https://deepwiki.com/microsoft/playwright) | E2E, fixtures, trace, CI |
| [`vitest-dev/vitest`](https://deepwiki.com/vitest-dev/vitest) | Unit/integration (TS) |
| [`xunit/xunit`](https://deepwiki.com/xunit/xunit) | Unit (.NET) |
| [`stretchr/testify`](https://deepwiki.com/stretchr/testify) | Unit (Go) |
| [`testing-library/react-testing-library`](https://deepwiki.com/testing-library/react-testing-library) | Component tests |

Опционально: [Context7 MCP](https://github.com/upstash/context7) — см. [README.md](../../README.md#mcp-серверы).
