# Source Conflicts Resolution

## Priority Order
1. `sources/WIN63-202407091256-704579380-Source-main/` for all application logic.
2. `sources/WIN63-202111081545-75921380-Source-main/` only for XML and:
`HabboAirMain.as`, `Habbo.as`, `HabboLoadingScreen.as`, `IHabboLoadingScreen.as`.
3. `sources/PRODUCTION-201611291003-338511768-Source-main/` as deobfuscation fallback when newer dumps are missing or unclear.

## Conflict Rule
If sources disagree:
- Keep behavior from highest-priority valid source.
- Use lower-priority source only to clarify missing names/flow/data shape.
- Never merge contradictory logic heuristically.

## Escalation Rule
Escalate to review when:
- field order differs for a protocol message
- return semantics differ in control-critical methods
- event timing/lifecycle differs in a way that changes behavior

## Required Conflict Note (in PR)
Include this block in PR description when a conflict occurred:

```md
### Source Conflict Note
- Primary source file:
- Secondary source file:
- Conflict summary:
- Decision taken:
- Why this matches source-of-truth policy:
```
