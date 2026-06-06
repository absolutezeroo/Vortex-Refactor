# Definition Of Done

A change is ready only if all items below are true.

## Source Integrity
- [ ] Logic comes from source dumps defined in `CONTEXT.md`.
- [ ] No invented gameplay/business behavior.
- [ ] `@see` source references are present for ported methods/classes.

## Code Quality
- [ ] Follows `docs/STYLEGUIDE.md` (naming, nullability, async, braces, file layout).
- [ ] Uses modern C# features where they improve clarity and safety.
- [ ] No manual edits of generated `*.uid` unless required and justified.
- [ ] No decompiler artifacts in touched identifiers (class_NNNN, var_NNNN, paramN).
- [ ] De-obfuscation renames recorded in docs/RENAME_MAP.md (symbol + file renamed).
- [ ] @see anchors preserved on renamed symbols.
- [ ] Required lookups throw on absence (no guards/fallbacks the AS3 lacks) — FIDELITY_BOUNDARY.
- [ ] Tier 1 vs Tier 2 respected: semantic changes go through SOURCE_CONFLICTS/KNOWN_ADAPTATIONS;
  representation changes do not.

## Communication Layer (If Applicable)
- [ ] `Manager`, `Outgoing`, `Incoming`, `Parser` structures match `docs/COMMUNICATION_EXAMPLES.md`.
- [ ] Message field order and types match source exactly.
- [ ] IDs are correctly registered in `src/Vortex.Habbo/communication/HabboMessages.cs`.

## Validation
- [ ] `dotnet build Vortex.sln -c Debug` passes.
- [ ] `dotnet test Vortex.sln` executed (or explicitly documented if no tests).
- [ ] Smoke run completed on `scenes/Vortex.tscn` with no new runtime errors.
- [ ] Protocol-affecting changes covered by a passing golden test (captured-packet fixture).

## PR Readiness
- [ ] Diff is scoped and minimal.
- [ ] PR description includes source references and verification steps.
