---
name: qa-validation
description: Проводит QA-валидацию после приёмки Техлидом: чеклист, exploratory notes, Playwright при необходимости, запись в qa.md и docs/qa/test-runs.md. Используй когда работа передана в QA или нужно зафиксировать регрессионный прогон.
disable-model-invocation: true
---

# QA validation

## Предусловия

- В `status.md` Техлид отметил приёмку реализации разработчика/DevOps (готовность к QA).

## Действия

1. Пройди `docs/qa/test-strategy.md` и AC из `analysis.md` / задач.
2. Зафиксируй сценарии в `qa.md` (папка фичи): пройдено / провалено, шаги.
3. Добавь запись в `docs/qa/test-runs.md`: дата, ветка/коммит, scope, среда, итог.
4. Обнови `status.md`: либо к Техлиду (`rework`) с списком дефектов, либо к Финализатору.

## Playwright

Если в задаче требуется e2e — добавь/обнови тесты по `docs/qa/playwright.md`; не хардкодь секреты.
