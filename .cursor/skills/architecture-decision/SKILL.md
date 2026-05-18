---
name: architecture-decision
description: Помогает оформить архитектурное решение: границы, варианты, последствия, ссылка на ADR. Используй когда Архитектор фиксирует значимое решение или спор между подходами.
disable-model-invocation: true
---

# Architecture decision

## Входы

- `docs/architecture/decisions/adr-template.md`, `docs/architecture/overview.md`.
- Папка фичи: `architecture.md`, `status.md`.

## Действия

1. Сформулируй проблему и критерии выбора (производительность, стоимость, сложность, риски).
2. Перечисли варианты с плюсами/минусами; выбери рекомендуемый.
3. Создай файл `docs/architecture/decisions/NNNN-title.md` по шаблону ADR.
4. В `architecture.md` фичи — диаграмма в тексте (mermaid по желанию) и ссылка на ADR.
5. Обнови `docs/history/decisions-log.md` одной строкой со ссылкой на ADR.

## Выход

Путь к ADR и обновлённый `status.md` (стадия `architecture`).
