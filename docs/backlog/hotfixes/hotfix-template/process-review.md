# Process review — `<class-key>` — `<case-name>`

> Создавайте только для `candidate`; этот review не блокирует ordinary rework и не меняет workflow автоматически.

## Status

- **State:** `candidate` | `approved-for-separate-pr` | `rejected` | `superseded` | `inconclusive`
- **Owner:** _роль / человек_
- **Created:** `YYYY-MM-DD`
- **Decision / reviewer:** _link / pending_

## Classification

- **Affected role:** _роль_
- **Violated contract or gate:** _конкретный contract/gate_
- **Classification:** `implementation` | `process`
- **Class key:** `<role> + <contract/gate> + <classification>`

## Evidence

| Case | PR/MR / feedback evidence | Why comparable |
|---|---|---|
| _current case_ | _link_ | _reason_ |
| _prior independent case_ | _link_ | _reason_ |

## Triage

- **Trigger result:** `candidate`
- **Suppression checked:** `none` | _reason and evidence_
- **Ordinary rework:** _link to status.md_; **not blocked**

## Root-cause hypothesis

_Фальсифицируемое объяснение повторяемой проблемы; явно укажите неопределённость._

## Proposal

- **Proposed change:** _не применяется автоматически_
- **Affected files / roles:** _точные пути и владельцы_
- **Expected improvement:** _наблюдаемое поведение_
- **Safety constraints preserved:** _roles, gates, QA, branch discipline, Finalizer, PR-only_

## Regression set and verdict

| Evidence case | Expected behavior after change | Result |
|---|---|---|
| _link_ | _behavior_ | `pass` / `fail` / `not-run` |

- **Verdict:** `quality improvement demonstrated` | `inconclusive` | `safety regression`
- **Recommendation:** `separate PR/MR` | `do not change process`

Без regression evidence verdict — `inconclusive`, а recommendation — `do not change process`. При положительном verdict создайте отдельную process-change feature branch и human-reviewed PR/MR; не изменяйте исходный rework автоматически.
