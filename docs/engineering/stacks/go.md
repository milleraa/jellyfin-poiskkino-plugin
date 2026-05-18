# Go standards

**Статус:** не используется в v1.

*Источник: в репозитории нет `go.mod` / Go-исходников (bootstrap 2026-05-18).*

## Runtime и tooling

- **TODO:** минимальная версия Go.
- **TODO:** `gofmt`, `go vet`, golangci-lint или другой набор проверок.
- **TODO:** команды `test`, `lint`, `build`.

## Layout

- **TODO:** layout репозитория: `cmd/`, `internal/`, `pkg/`, `api/`, `migrations/`.
- **TODO:** правила именования пакетов и файлов.

## Context и ошибки

- **TODO:** политика передачи `context.Context`.
- **TODO:** error wrapping, sentinel errors, typed errors.
- **TODO:** где допустимы `panic` и `recover`.

## Тестирование

- **TODO:** unit/integration/e2e разделение.
- **TODO:** test fixtures, mocks/fakes, работа с БД.

## Связанные правила Cursor

- `.cursor/rules/go-developer.mdc`
- `.cursor/agents/developer-go.md`
