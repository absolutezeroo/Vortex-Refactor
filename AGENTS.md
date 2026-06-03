# Repository Guidelines

## Project Structure & Module Organization
`scripts/` contains the C# implementation (`scripts/bootstrap/`, `scripts/core/`, `scripts/habbo/`, `scripts/iid/`). `scenes/` contains Godot scenes (`scenes/Vortex.tscn` main scene). `data/manifests/` stores XML manifests and `docs/` tracks migration status.

For source-of-truth hierarchy and porting constraints, read `CONTEXT.md`.

## Build, Test, and Development Commands
- `dotnet restore Vortex.sln`: restore dependencies.
- `dotnet build Vortex.sln -c Debug`: build C# assemblies.
- `dotnet test Vortex.sln`: run tests (no dedicated test project yet).
- `godot --path .`: open/run project (or Godot 4.6, `F5`).

## Coding Style & Naming Conventions
Use `docs/STYLEGUIDE.md` as the authoritative C# style reference (naming, nullability, async, file layout, and porting-oriented rules).
For communication-layer additions, use `docs/COMMUNICATION_EXAMPLES.md` as the required baseline for `Manager`, `Outgoing`, `Incoming`, and `Parser` structures.

## Workflow References
Follow these documents explicitly:
- `CONTEXT.md`: source-of-truth hierarchy and "no invented logic" rule.
- `docs/PORTING_WORKFLOW.md`: mandatory implementation flow.
- `docs/DEFINITION_OF_DONE.md`: merge gate checklist.
- `docs/SOURCE_CONFLICTS.md`: resolution policy for divergent dumps.
- `docs/COMMUNICATION_CHECKLIST.md`: protocol-layer verification.
- `docs/KNOWN_ADAPTATIONS.md`: tracked engine-level deviations.
- `docs/PR_TEMPLATE.md`: required PR structure.

## Testing Guidelines
Prefer small deterministic tests for parser/protocol/utility logic. For runtime or scene changes, smoke-run `scenes/Vortex.tscn` and check Godot output for new errors. If you add tests, place them in `tests/` and include them in `Vortex.sln`.

## Commit & Pull Request Guidelines
Recent history follows Conventional Commit-style messages, usually scoped:
- `feat(core): ...`
- `refactor(runtime): ...`

Use imperative, specific subjects (avoid `Fix`). PRs must include summary, touched paths/modules, verification steps (build/run/test), and screenshots for UI changes.

## Security & Configuration Tips
Never commit secrets, SSO tokens, or crash-log payloads. Keep generated artifacts (`.godot/`, `obj/`, `bin/`, `TestResults/`) untracked per `.gitignore`.
