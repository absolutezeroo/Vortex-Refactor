# Porting Workflow

## Objective
Port AS3 behavior to Godot/C# with zero invented business logic and minimal engine adaptation.

## Required Input
- Source mapping from `CONTEXT.md`
- Target C# file path in `src/`
- Related communication patterns from `docs/COMMUNICATION_EXAMPLES.md` when applicable

## Step-by-Step Process
1. Identify source file and method in the correct dump hierarchy.
2. Copy behavior semantics first (conditions, state changes, field order, event flow).
3. Apply only Godot/C# adaptations (API mapping, lifecycle hooks, type conversion).
4. Add `@see` comments pointing to original source path/method.
5. Keep naming and layout aligned with `docs/STYLEGUIDE.md`.
6. For communication code, register IDs in `src/Vortex.Habbo/communication/HabboMessages.cs`.
7. Validate build and runtime.

## Validation Commands
- `dotnet build Vortex.sln -c Debug`
- `dotnet test Vortex.sln`
- `godot --path .` then run `scenes/Vortex.tscn`

## Output Expectations
- Deterministic behavior parity with source dump
- No protocol field/order drift
- No added gameplay or product logic
