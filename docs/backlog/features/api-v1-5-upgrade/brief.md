# Brief — `api-v1-5-upgrade`

## Цель

Перевести плагин с deprecated эндпоинтов ПоискКино API v1.4 (`movie/search`, `movie/{id}`) на актуальные v1.5, сохранив текущее поведение метаданных Jellyfin.

## Scope (in)

- Замена URL в `PoiskKinoApiClient` на `/v1.5/movie/search` и `/v1.5/movie/{id}`.
- Обновление комментариев моделей и тестовых фикстур, явные тесты пути запроса.
- Косметика имён/мёртвого кода — по решению Стейхолдера (см. analysis.md §8).

## Out of scope

- Переход на курсорный `GET /v1.5/movie` с фильтрами; изменения кэша/rate-limit/UI; прочие домены API.

## Стейкхолдеры

См. [stakeholders](../../../product/stakeholders.md).

Детальный анализ: [analysis.md](analysis.md).
