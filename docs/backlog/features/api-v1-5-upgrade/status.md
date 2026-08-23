# Status — `api-v1-5-upgrade`

Единая точка правды по стадии для Оркестратора. Обновляйте при каждом handoff (см. `.opencode/skills/role-handoff/SKILL.md`).

## Текущее состояние

| Поле | Значение |
|------|----------|
| **Стадия** | `qa` → pass; готово к `finalization` |
| **Owner** | `Orchestrator` |
| **Worktree** | `/home/alex/src/my/jellyfin-metadata-plugin-api-v15` |
| **Ветка** | `feature/api-v1-5-upgrade` |
| **PR/MR** | _нет_ |
| **Commit со ссылкой на PR/MR** | _нет_ |
| **Блокеры** | _нет_ |

### Классификация (intake)

- **Тип:** новая фича, чисто техническая — миграция с deprecated API v1.4 на актуальное v1.5.
- **Постановка Стейхолдера:** проанализировать код и `api-docs/documentation.yaml`, найти использование устаревшего API и выполнить апгрейд.
- **Первичная фактура Оркестратора:**
  - `PoiskKinoApiClient.cs:81` → `GET /v1.4/movie/search` (deprecated) — актуальный аналог `/v1.5/movie/search`.
  - `PoiskKinoApiClient.cs:202` → `GET /v1.4/movie/{id}` (deprecated) — актуальный аналог `/v1.5/movie/{id}`.
  - `PoiskKinoApiClient.cs:320` → `GET /v1.5/season` — уже на актуальной версии.
  - Затронуты модели `Models/PoiskKino*V1_4*.cs` (`SearchMovieResponseDtoV1_4`, `MovieDtoV1_4`, `SeasonDocsResponseDtoV1_4` и связанные), юнит-тесты с фикстурами v1.4 JSON.
  - В спеке v1.5 — курсорная пагинация (`next`/`prev`/`hasNext`) и обновлённые схемы DTO; demo/free тариф ограничен 10 страницами пагинации.
  - Требование `api-docs/AGENTS.md`: опираться на https://api.poiskkino.dev/llms.txt (прочитан).

### Стадии (справочно)

`intake` → `analysis` → `analysis-review` → `backlog-paused` или `architecture` → `tech-decomposition` → `development` → `devops-check` → `tech-lead-review` → `qa` → `rework` → `finalization` → `pr-mr-ready` → `stakeholder-review` → `rework` или `accepted`

`analysis-review` — обязательная остановка после Аналитика: Стейхолдер решает продолжать сейчас или оставить фичу в backlog.

## Задачи реализации

| ID | Файл | Роль / стек | Статус |
|----|------|-------------|--------|
| 001 | [001-migrate-api-client-v15.md](tasks/001-migrate-api-client-v15.md) | developer-dotnet / ApiClient URL + тесты пути | **done** (commit `73ea86f`) |
| 002 | [002-rename-models-cleanup.md](tasks/002-rename-models-cleanup.md) | developer-dotnet / переименование моделей, чистка, мёртвый код | **done** (commit `8f724dd`) |
| 003 | [003-provider-year-filter.md](tasks/003-provider-year-filter.md) | developer-dotnet / локальный фильтр года по ADR-0001 + тесты | **done** (commit `1abee81`) |

Порядок выполнения строго последовательный: 001 → 002 → 003, один worktree, без task-branches.

**Примечание по исполнению:** вызов субагента `developer-csharp` через Task tool заблокирован окружением (`subagent depth limit reached`). По fallback-правилу постановки задачи выполнены самим Техлидом в роли разработчика C#/.NET с соблюдением AC каждой задачи; каждый шаг принят Техлидом после прогона `dotnet test`. Для будущих фич рекомендуется поднять `subagent_depth`.

## DevOps impact check

Проверено Техлидом 2026-08-23 после accept задач 001–003 (файлы: `.github/workflows/ci.yaml`, `release.yaml`, код плагина):

| Вопрос | Результат |
|--------|-----------|
| Новые env vars / secrets | **нет** (API-ключ хранится в конфиге плагина как раньше; URL — не секрет) |
| Новые интеграции / порты / очереди / storage / jobs | **нет** (те же 3 эндпоинта api.poiskkino.dev, только версии путей v1.4→v1.5) |
| Docker / compose / CI/CD / deploy / Helm / K8s / observability | **нет** (`ci.yaml`/`release.yaml` не менялись: dotnet restore/build/test покрывают изменения; workflow-триггеры и шаги актуальны) |
| DevOps-задача | **не нужна** |

Вывод: DevOps-check закрыт без назначения задачи.

## QA

| Дата | Результат | Ссылка на прогон |
|------|-------------|------------------|
| 2026-08-23 | **pass** | [qa.md](qa.md) · запись в [test-runs](../../../qa/test-runs.md) |

## PR/MR feedback / rework

| Поле | Значение |
|------|----------|
| **Feedback source** | _чат / PR/MR comments / CI_ |
| **Feedback summary** | _нет_ |
| **Rework route** | _Analyst / Architect / Tech Lead / DevOps / QA_ |
| **Worktree restored from branch** | _нет / да_ |
| **Rework tasks** | _ссылки на tasks/_ |

## Changelog (handoff)

### 2026-08-23 — QA: валидация пройдена (pass) → готово к finalization

- Чеклист Техлида закрыт полностью ([qa.md](qa.md)): `/v1.4` в коде нет (только допустимые ссылки на имена схем спеки в XML-doc); пути запросов покрыты assertion'ами (`/v1.5/movie/search`, `/v1.5/movie/{id}`); фильтр года по ADR-0001 — все 4 сценария (фильм точное совпадение, сериал releaseYears+fallback по Year, fallback на неотфильтрованный список с debug-log, отсутствие года) покрыты юнит-тестами; маппинг метаданных не менялся (дифф `0cf9460..HEAD`: только переименование типа и вставка вызова `SearchYearFilter.Apply`).
- Прогоны QA самостоятельно: `dotnet test` — **160 passed / 0 failed**; `dotnet build` — 0 ошибок, предупреждения идентичны baseline main (`00900d1`): 61× CS1591, новых нет.
- Решение по инструментарию: Playwright неприменим — backend/.NET-плагин без собственного UI (согласовано с [test-strategy](../../../qa/test-strategy.md)); проверка через юнит-тесты + статический анализ + диффы.
- Ограничение QA: smoke с реальным API-ключом не выполнялся (ключа нет) — риск strict-валидации `&year=` на сервере v1.5 остаётся открытым; план изоляции зафиксирован в ADR-0001. Интеграционная проверка в живом Jellyfin не проводилась.
- Дефектов не найдено. Handoff → **Оркестратор**: стадия `finalization`, owner `Orchestrator`.

### 2026-08-23 — Техлид: реализация принята, DevOps-check закрыт → готово к QA

- Декомпозиция на 3 последовательные задачи ([tasks/](tasks/)); исполнение — fallback в роли developer-dotnet (см. примечание в таблице задач: субагенты заблокированы окружением).
- **001** (`73ea86f`): `/v1.4/movie/search` и `/v1.4/movie/{id}` → `/v1.5/...`; `/v1.5/season` не тронут; в тестах клиента добавлены assertion фактических путей запросов (регрессия на v1.4 теперь падает).
- **002** (`8f724dd`): `PoiskKinoMovieDtoV1_4` → `PoiskKinoMovieDto` (модель, клиент, 3 провайдера, тесты); XML-doc моделей больше не называют v1.4 целевой версией (допустимы ссылки на имена схем спеки); удалён мёртвый `Models/PoiskKinoSeasonResponse.cs`; комментарии фикстур `TestJsonData.cs` обновлены, JSON не менялся.
- **003** (`1abee81`): локальная фильтрация по году по [ADR-0001](../../../architecture/decisions/ADR-0001-search-year-filter.md) через `SearchYearFilter`: фильмы — точное совпадение `Year`; сериалы — попадание в `releaseYears` (null-границы открыты) или совпадение с `Year`; пустой результат после фильтра → fallback на неотфильтрованный список с debug-log; год не указан → без фильтрации. Фильтрация в провайдерах, `ApiClient` без бизнес-правил; кэш хранит сырой ответ.
- Обновлён [overview.md «Интеграции»](../../../architecture/overview.md) (обязательство architecture.md §5).
- Тесты: `dotnet test` — **160 passed / 0 failed** (+15 новых: пути запросов, фильтр года). Сборка без новых предупреждений.
- Tech-lead review: **accept**. AC analysis.md §5 проверены grep'ом (`/v1.4` в коде нет, кроме пояснения в doc-comment ADR-контекста).
- DevOps impact check: влияния нет, задача не назначалась (таблица выше).
- Handoff → **Оркестратор**: стадия `qa`, owner `Orchestrator`. PR/MR не открывался, push не выполнялся.

Замечания для QA:
1. Smoke-тест с реальным API-ключом: `GET /v1.5/movie/search?query=...&year=...` обязан вернуть 200 (риск strict-валидации неизвестных параметров v1.5, ADR-0001 §Последствия); при отказе — план изоляции: убрать `&year=` из URL (одна строка + один тест).
2. Identify с указанным годом: фильм находится по точному году; сериал — по диапазону лет выхода; при расхождении годов источников должен срабатывать fallback (в логе debug-запись).
3. Регрессия маппинга метаданных: название/год/описание/рейтинги/постеры/персоны/жанры не должны измениться (логика маппинга не трогалась).

### 2026-08-23 — Архитектор: дизайн готов (`architecture.md` + ADR-0001)

- Создан [architecture.md](architecture.md): целевая схема вызовов (все запросы на `/v1.5/*`), изменения по слоям (ApiClient, модели, провайдеры, тесты), совместимость/риски, план отката.
- **Решение по `year`** (вопрос Стейхолдера закрыт, [ADR-0001](../../../architecture/decisions/ADR-0001-search-year-filter.md)): параметр `year` был и остаётся **недокументированным query-параметром** `movie/search` — его нет в спеке ни v1.4, ни v1.5; это не «поиск по полю ответа». Решение — гибрид: сохранить передачу `&year=` в URL + добавить детерминированную локальную фильтрацию результатов по году в провайдерах (фильмы — точное совпадение `Year`; сериалы — попадание в `releaseYears`; fallback на неотфильтрованный список при пустом результате). Год из «Распознать» Jellyfin теперь учитывается независимо от поведения сервера. Вариант B (`GET /v1.5/movie`) отвергнут — вне scope.
- Учтены решения Стейхолдера: переименование моделей `PoiskKinoMovieDtoV1_4` → `PoiskKinoMovieDto` (да), удаление мёртвого `Models/PoiskKinoSeasonResponse.cs` (да).
- Ключевые риски для Техлида: smoke-тест `/v1.5/movie/search?...&year=` с реальным ключом (strict-валидация неизвестных параметров в v1.5); llms.txt не индексирует v1.5-маршруты поиска; обязательный assertion пути `/v1.5/…` в тестах клиента.
- DevOps impact: нет.
- Handoff → **Оркестратор**: стадия `tech-decomposition`, декомпозиция — Техлид.

### 2026-08-23 — Стейхолдер: gate `analysis-review` пройден, продолжаем

- Решения по открытым вопросам analysis.md §8:
  1. Переименование моделей `*V1_4` → **да**.
  2. Параметр `year` в search → **передан Архитектору на проверку по спеке v1.5**: Стейхолдер сообщает, что параметра больше нет; уточнить — это фильтр поиска или поле ответа. Контекст: в Jellyfin при «Распознать» пользователь указывает год, и он должен продолжать участвовать в поиске метаданных.
  3. Удаление мёртвого `PoiskKinoSeasonResponse.cs` → **да**.
- Handoff → **Архитектор** (`.opencode/agents/architect.md`).

### 2026-08-23 — Аналитик: анализ завершён (`analysis.md`)

- Заполнен [analysis.md](analysis.md): инвентаризация deprecated-использований, сравнение контрактов v1.4/v1.5, влияние на модели/провайдеры/тесты, AC, риски.
- Ключевой вывод: v1.5 `movie/search` и `movie/{id}` возвращают **те же схемы**, что и v1.4 (`SearchMovieResponseDtoV1_4`, `MovieDtoV1_4`) — функционально меняются только пути запросов; модели и маппинг Jellyfin не затрагиваются.
- Live-проба API подтвердила существование v1.5-маршрутов (401 без ключа); риск: llms.txt их не индексирует — рекомендован smoke-тест с реальным ключом.
- 3 открытых вопроса Стейхолдеру с рекомендациями (analysis.md §8): переименование моделей `*V1_4`, судьба недокументированного параметра `year`, удаление мёртвого `PoiskKinoSeasonResponse.cs`.
- Рекомендация Аналитика: `continue`.
- Handoff → **Оркестратор**: обязательная остановка на решении Стейхолдера (`analysis-review`).

### 2026-08-23 — создан шаблон

- Создана папка фичи из шаблона.
- Оркестратор: intake, классификация (техническая фича), ветка `feature/api-v1-5-upgrade`, worktree `/home/alex/src/my/jellyfin-metadata-plugin-api-v15`.
- Handoff → **Аналитик** (`.opencode/agents/analyst.md`): подготовить `analysis.md` по миграции v1.4 → v1.5.
