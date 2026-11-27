# Task 005 — Полная санитизация `userinfo` в `Warning`-логе базового URL (rework после приёмки)

## Метаданные

- **ID:** 005
- **Feature / Hotfix:** [docs/backlog/features/eu-api-endpoint/](../)
- **Назначено:** `developer-csharp`
- **Стек / infra-область:** C# / .NET 10, xUnit + Moq; только `PoiskKinoApiClient` (приватный метод логирования) и тесты
- **Статус:** todo
- **Ветка:** `feature/eu-api-endpoint` (worktree `/home/alex/src/my/jellyfin-metadata-plugin`); rework на существующей ветке, task-branch не создавать

## Описание

Исправить дефект в санитизации `userinfo`, добавленной коммитом `1533989` (`fix(api): не логировать userinfo из невалидного базового URL`).

`PoiskKinoApiClient.RedactUserInfo` вырезает всё до **первого** `@` в пределах authority. Если пароль сам содержит `@`, хвост пароля остаётся в `Warning`-логе — то есть цель фикса («пароль не попадает в лог») достигается не всегда:

| Вход | Текущий результат в `Warning` | Ожидание |
|------|-------------------------------|----------|
| `https://admin:p@ss@api.poiskkino.dev` | `https://ss@api.poiskkino.dev` (утечка хвоста пароля) | `https://api.poiskkino.dev` |
| `admin:p@ss@api.poiskkino.dev` (без схемы) | `ss@api.poiskkino.dev` | `api.poiskkino.dev` |
| `https://admin:p@ss@eu-api.poiskkino.dev:8443` | `https://ss@eu-api.poiskkino.dev:8443` | `https://eu-api.poiskkino.dev:8443` |

Причина: `value.IndexOf('@', authorityStart)` находит первый `@`, хотя `userinfo` в authority заканчивается **последним** `@` перед первым `/`, `?` или `#`.

Требуемое поведение `RedactUserInfo`:

1. Authority — участок после `"://"` (или с начала строки, если схемы нет) до первого из `/`, `?`, `#` (или до конца строки).
2. Если в authority есть хотя бы один `@` — вырезается весь участок от начала authority до **последнего** `@` в authority включительно; схема сохраняется.
3. Если `@` есть только за пределами authority (путь/query/fragment) — значение возвращается без изменений (это не credential).
4. Пустой authority (`https:///x`) — без изменений, без исключений.
5. Порядок операций сохраняется: сначала redaction, затем `TruncateForLog(…, 200)`.

Ориентир реализации (не обязателен дословно, поведение — обязательно):

```csharp
var authorityEndIndex = value.IndexOfAny(['/', '?', '#'], authorityStart);
var authorityEnd = authorityEndIndex < 0 ? value.Length : authorityEndIndex;
if (authorityEnd <= authorityStart)
{
    return value;
}

var atIndex = value.LastIndexOf('@', authorityEnd - 1, authorityEnd - authorityStart);
if (atIndex < 0)
{
    return value;
}

var schemePrefix = schemeSeparatorIndex < 0 ? string.Empty : value[..authorityStart];
return string.Concat(schemePrefix, value.AsSpan(atIndex + 1));
```

Тесты — `PoiskKinoMetadataPlugin.UnitTests/ApiClient/PoiskKinoApiClientBaseUrlTests.cs`:

- расширить теорию `Constructor_UserInfoInBaseUrl_RedactedInWarning` кейсами с `@` внутри пароля: `https://admin:p@ss@api.poiskkino.dev`, `admin:p@ss@api.poiskkino.dev`, `https://admin:p@ss@eu-api.poiskkino.dev:8443`. Ассерт: в сообщении нет `p@ss` и нет `ss@`, хост (`poiskkino.dev`) присутствует, `Warning` ровно один;
- новый тест: `https://api.poiskkino.dev/path@x` → значение в `Warning` не изменилось (нет `@` в authority; redaction не должна съедать путь), `Warning` один, исключений нет.

## Acceptance criteria

1. Для всех трёх кейсов с `@` в пароле `Warning` содержит хост и **не содержит** ни `p@ss`, ни `ss@` — пароль не утекает ни в каком фрагменте (AC-9 / риск A6).
2. `@` вне authority (путь, query, fragment) не вырезается: значение в `Warning` остаётся прежним; поведение для `https://api.poiskkino.dev?email=a@b` и `#a@b` не меняется.
3. Существующие 3 кейса теории (`https://admin:sup3r-secret@api.poiskkino.dev`, `admin:sup3r-secret@api.poiskkino.dev`, `https://admin:sup3r-secret@eu-api.poiskkino.dev:8443`) проходят без правки ожиданий.
4. `ApiBaseUrlResolver.cs` не изменён: нормализация/валидация и `UsedFallback`/`FallbackReason` остаются как есть (меняется только текст лога); правило «userinfo → fallback» сохраняется.
5. Усечение до 200 символов по-прежнему применяется после redaction; `ApiKey` в лог не попадает (существующий ассерт зелёный).
6. `RedactUserInfo` не бросает исключений ни на одном входе (пустая строка, `null`, `https://`, `@`, строка из `@`), остаётся `private static`.
7. Изменены только `PoiskKinoMetadataPlugin/PoiskKinoApiClient.cs` и `PoiskKinoMetadataPlugin.UnitTests/ApiClient/PoiskKinoApiClientBaseUrlTests.cs` (+ статусные docs-коммиты); `Configuration.cs`, провайдеры, `configPage.html`, README/overview не трогаются.
8. `dotnet build -c Release` — 0 ошибок, предупреждения только baseline `CS1591`; `dotnet test PoiskKinoMetadataPlugin.slnx -c Release` — все тесты зелёные (267 + новые кейсы).
9. Локальный commit без push.

## Технические заметки

- Дефект найден Техлидом на приёмке (`tech-lead-review`) в коммите `1533989`; остальная реализация задач 001–004 принята, переделывать её не нужно.
- Почему не «логировать только резолвнутый адрес»: ADR-0003 / D-ARCH-5 требуют в `Warning` именно сырое значение (усечённое), чтобы администратор видел, что он ввёл. Поэтому правится санитизация, а не формат сообщения.
- Альтернатива «вырезать всё после `://` до `/`» (authority целиком) отвергается: в логе должен остаться хост, иначе предупреждение бесполезно.
- Пустой authority (`https:///x`, `https://`) — валидный вход для redaction (значение невалидно для резолвера, но лог должен быть безопасным); обязателен guard на `authorityEnd <= authorityStart`, иначе `LastIndexOf` с отрицательным/нулевым диапазоном бросит исключение.
- Тест на «не трогать `@` в пути» фиксирует, что защита не ломает диагностику для невалидных URL с `@` в пути.

## DevOps impact check

- Новые/изменённые переменные окружения или secrets: **нет**
- Новые внешние интеграции, порты, очереди, storage, scheduled jobs: **нет**
- Docker/compose/CI/CD/deploy/Helm/Kubernetes/observability: **нет** (меняется только текст уже существующего `Warning`)
- Итог: DevOps-задача **не нужна**.

## Проверка

```bash
dotnet build -c Release PoiskKinoMetadataPlugin.slnx --no-incremental
dotnet test PoiskKinoMetadataPlugin.slnx -c Release
```

Дополнительно вручную: после теста в логе мок-логгера для `https://admin:p@ss@api.poiskkino.dev` должно быть `Invalid API base URL 'https://api.poiskkino.dev' … falling back to 'https://api.poiskkino.dev': userinfo is not supported in a base URL`.

## Handoff

- После выполнения: локальный commit без push (например, `fix(api): вырезать userinfo до последнего '@' в authority`), обновить этот файл (`done` + результат), [tasks/README.md](README.md), [status.md](../status.md) → стадия `tech-lead-review`, owner `Tech Lead`; сообщить Техлиду результаты build/test и дифф.
