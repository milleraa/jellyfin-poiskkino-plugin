# Reject learning log

Нормализованный append-only журнал ссылок на reject cases. Он не заменяет `status.md`, не хранит секреты/PII/raw logs и не является автоматическим trigger.

| Date | Case | PR/MR / evidence | Class key | Triage | Process review |
|---|---|---|---|---|---|
| — | — | — | — | — | — |

## Правила записи

- `Case` ссылается на `status.md` feature/hotfix.
- `Class key` следует [правилам learning](README.md#evidence-и-class-key).
- `Triage`: `not-triggered`, `suppressed` или `candidate`.
- Для `suppressed` укажите причину в case-level `status.md`.
- Для `candidate` добавьте ссылку на `process-review.md`.
- Не учитывайте одну evidence-ссылку дважды; исправления добавляйте новой датированной записью или `superseded`.
