---
name: backlog-refinement
description: Проводит refinement эпика или крупной темы: уточняет scope, риски, зависимости, готовит материал для фич в backlog. Используй когда Стейхолдер принёс большую тему, нужно разбить на фичи или обновить epics.md и stories.
disable-model-invocation: true
---

# Backlog refinement

## Входы

- `docs/backlog/epics.md`, продукт `docs/product/requirements.md`.
- Сообщение Стейхолдера или Оркестратора.

## Действия

1. Зафиксируй цель refinement в `docs/backlog/README.md` или в комментарии к epic (кратко).
2. Для каждой выделенной фичи: имя папки `kebab-case`, связь с epic, риски, зависимости.
3. Создай или обнови записи в `epics.md`; при готовности создай папку `docs/backlog/features/<name>/` из шаблона `feature-template`.
4. Выставь в новой фиче `status.md`: стадия `intake`, owner `Orchestrator`.

## Выход

Список созданных/обновлённых фич и ссылок для Оркестратора.
