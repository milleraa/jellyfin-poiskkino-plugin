# Status — `api-v1-5-upgrade`

Единая точка правды по стадии для Оркестратора. Обновляйте при каждом handoff (см. `.opencode/skills/role-handoff/SKILL.md`).

## Текущее состояние

| Поле | Значение |
|------|----------|
| **Стадия** | `tech-decomposition` |
| **Owner** | `Orchestrator` → handoff Техлиду |
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
| — | — | — | — |

## DevOps impact check

| Вопрос | Результат |
|--------|-----------|
| Новые env vars / secrets | _нет / да_ |
| Новые интеграции / порты / очереди / storage / jobs | _нет / да_ |
| Docker / compose / CI/CD / deploy / Helm / K8s / observability | _нет / да_ |
| DevOps-задача | _не нужна / ссылка_ |

## QA

| Дата | Результат | Ссылка на прогон |
|------|-------------|------------------|
| — | — | [test-runs](../../../qa/test-runs.md) |

## PR/MR feedback / rework

| Поле | Значение |
|------|----------|
| **Feedback source** | _чат / PR/MR comments / CI_ |
| **Feedback summary** | _нет_ |
| **Rework route** | _Analyst / Architect / Tech Lead / DevOps / QA_ |
| **Worktree restored from branch** | _нет / да_ |
| **Rework tasks** | _ссылки на tasks/_ |

## Changelog (handoff)

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
