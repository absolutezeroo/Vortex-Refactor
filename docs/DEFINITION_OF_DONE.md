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

## Communication Layer (If Applicable)
- [ ] `Manager`, `Outgoing`, `Incoming`, `Parser` structures match `docs/COMMUNICATION_EXAMPLES.md`.
- [ ] Message field order and types match source exactly.
- [ ] IDs are correctly registered in `src/Vortex.Habbo/communication/HabboMessages.cs`.

## Validation
- [ ] `dotnet build Vortex.sln -c Debug` passes.
- [ ] `dotnet test Vortex.sln` executed (or explicitly documented if no tests).
- [ ] Smoke run completed on `scenes/Vortex.tscn` with no new runtime errors.

## PR Readiness
- [ ] Diff is scoped and minimal.
- [ ] PR description includes source references and verification steps.
