# Task 005 — Полная санитизация `userinfo` в `Warning`-логе базового URL (rework после приёмки)

## Метаданные

- **ID:** 005
- **Feature / Hotfix:** [docs/backlog/features/eu-api-endpoint/](../)
- **Назначено:** `developer-csharp`
- **Стек / infra-область:** C# / .NET 10, xUnit + Moq; только `PoiskKinoApiClient` (приватный метод логирования) и тесты
- **Статус:** `done` (см. [Результат выполнения](#результат-выполнения))
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

## Результат выполнения

**Статус:** `done`. Коммиты в ветке `feature/eu-api-endpoint` (без push):

| Коммит | Содержимое |
|--------|------------|
| `bf2f6bc` — `fix(api): вырезать весь userinfo до последнего @ в логе` | `PoiskKinoApiClient.RedactUserInfo` + тесты |
| `docs(eu-api-endpoint): отметить задачу 005 выполненной` | этот файл, [tasks/README.md](README.md), [status.md](../status.md) |

### Как исправлен `RedactUserInfo`

Было: `value.IndexOf('@', authorityStart)` — первый `@` в authority; хвост пароля с `@` оставался в логе.
Стало (правила 1–5 задачи):

1. authority — от `://` (или от начала строки) до первого из `/`, `?`, `#` или до конца строки (`authorityEnd`);
2. guard `authorityEnd <= authorityStart` — пустой authority (`https://`, `https:///x`) возвращается без изменений и без исключений;
3. `value.LastIndexOf('@', authorityEnd - 1, authorityEnd - authorityStart)` — вырезается всё от начала authority до **последнего** `@` в authority включительно (пароль может содержать `@`), схема сохраняется;
4. `@` только в пути/query/fragment — значение возвращается без изменений;
5. порядок операций прежний: `TruncateForLog(RedactUserInfo(raw), 200)`; метод остался `private static`, публичный API, `ApiBaseUrlResolver`, конфигурация, провайдеры, `configPage.html` и документация не трогались.

Поведение после фикса: `https://admin:p@ss@api.poiskkino.dev` → в `Warning` `https://api.poiskkino.dev`; `admin:p@ss@api.poiskkino.dev` → `api.poiskkino.dev`; `https://admin:p@ss@eu-api.poiskkino.dev:8443` → `https://eu-api.poiskkino.dev:8443`. Правило резолвера «userinfo → fallback с причиной» не менялось (меняется только текст лога).

### Тесты (`ApiClient/PoiskKinoApiClientBaseUrlTests.cs`)

| Тест | Что проверяет |
|------|----------------|
| `Constructor_UserInfoInBaseUrl_RedactedInWarning` — 6 строк теории | 3 прежних кейса (`sup3r-secret`) без правки ожиданий + 3 новых с `@` в пароле (`https://admin:p@ss@api.poiskkino.dev`, `admin:p@ss@api.poiskkino.dev`, `https://admin:p@ss@eu-api.poiskkino.dev:8443`). Ассерты: в сообщении нет секрета, нет хвоста (`ss@` / `secret`), есть точное санитизированное значение (`'https://api.poiskkino.dev'` / `'https://eu-api.poiskkino.dev:8443'` / `'api.poiskkino.dev'`), `Warning` ровно один |
| `Constructor_AtSignOutsideAuthority_LoggedUnchanged` — 3 строки | `https://api.poiskkino.dev/path@x`, `?email=a@b`, `#a@b`: `Warning` один, в сообщении значение **без изменений** (redaction не съедает путь/query/fragment), исключений нет |
| `Constructor_DegenerateUserInfoInputs_DoNotThrow` — 8 строк | `null`, `""`, `https://`, `https:///x`, `@`, `@@@`, `://@`, `https://@` — пустой/вырожденный authority, никаких исключений |

Прежние тесты (в т. ч. `ApiKey_NeverAppearsInLogs`, усечение до 200 символов) прошли без правок.

### Проверки

- `dotnet build PoiskKinoMetadataPlugin.slnx -c Release --no-incremental` — 0 ошибок, предупреждения только baseline `CS1591` (122), новых нет.
- `dotnet test PoiskKinoMetadataPlugin.slnx -c Release` — **281/281 passed** (было 267, +14 кейсов), failed 0, skipped 0.
- `git status` после коммита кода — изменены только `PoiskKinoApiClient.cs` и `ApiClient/PoiskKinoApiClientBaseUrlTests.cs` (AC-7).

### Замечания для Техлида

1. **`LastIndexOf(char, startIndex, count)`** используется с диапазоном только внутри authority (`startIndex = authorityEnd - 1`, `count = authorityEnd - authorityStart`); guard на пустой authority обязателен и покрыт тестом `https:///x`/`https://@`.
2. **Регистр и экранирование:** санитизация работает по сырой строке (без `Uri`-парсинга), поэтому не зависит от того, примет ли `Uri` такое значение — это важно для диагностики невалидных входов.
3. **Что не менялось:** формат сообщения `Invalid API base URL '{Raw}' (truncated to 200 chars), falling back to '{Default}': {Reason}`, второе усечение до 200 символов, `Information`-строка, а также нормализация/fallback в `ApiBaseUrlResolver` (ADR-0003).
4. **Остаточный риск (принят):** если администратор вставит в поле ключ вместо URL (`https://<api-key>@host`), он попадёт в лог как часть нормализованного `Information`-адреса — резолвер получает только `ApiBaseUrl`, связи с `ApiKey` нет; вне scope фичи.
