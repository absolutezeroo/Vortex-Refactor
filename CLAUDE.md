# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Vortex ports the Habbo Hotel Flash/AS3 client to Godot 4.6 + C# (net9.0). The core rule: **do not invent gameplay or business logic**. All behavior must come from the AS3 source dumps. Only Godot/C# technical adaptations (API mapping, lifecycle, type system) are allowed.

## Source of Truth Hierarchy

1. `sources/WIN63-202407091256-704579380-Source-main/` — primary for all application logic
2. `sources/WIN63-202111081545-75921380-Source-main/` — only for XML porting and: `HabboAirMain.as`, `Habbo.as`, `HabboLoadingScreen.as`, `IHabboLoadingScreen.as`
3. `sources/PRODUCTION-201611291003-338511768-Source-main/` — deobfuscation fallback when newer dumps are missing or unclear

If sources disagree, use highest-priority source. Lower-priority sources only clarify names/flow/data shape. Never merge contradictory logic.

## Build and Development Commands

```bash
dotnet restore Vortex.sln          # Restore dependencies
dotnet build Vortex.sln -c Debug   # Build C# assemblies
dotnet test Vortex.sln             # Run tests (no dedicated test project yet)
godot --path .                     # Run project in Godot 4.6
```

NuGet uses a local Godot SDK package from `thirdparty/nupkgs/godot-4.6.1` (configured in `NuGet.Config`). The `.nuget/packages` directory is the local package cache.

## Architecture

The solution is split into multiple projects under `src/`:

```
src/
  Vortex/              (main Godot project — Godot.NET.Sdk)
  Vortex.Core/         (runtime, assets, communication, window, localization)
  Vortex.IID/          (~49 marker interfaces for DI)
  Vortex.Habbo/        (Habbo-specific managers, UI, communication)
  Vortex.Bootstrap/    (component wiring into context)
  Vortex.Login/        (login flow UI)
  Vortex.OnBoardingHcUI/ (onboarding UI)
  Vortex.Splash/       (splash screen, no Vortex deps)
  Vortex.Room/         (room engine, scaffold)
tools/
  Vortex.Bundle/       (asset bundle loader)
```

**Dependency graph:**
```
Vortex.Splash (standalone)
Vortex.Core → Vortex.Bundle
Vortex.IID → Core
Vortex.OnBoardingHcUI → Core
Vortex.Habbo → Core, IID
Vortex.Bootstrap → Core, Habbo, IID
Vortex.Login → Core, Habbo, Bootstrap, IID, OnBoardingHcUI
Vortex (main) → all above + Vortex.Bundle
```

Sub-projects use `Microsoft.NET.Sdk` + `PackageReference` to `GodotSharp`/`Godot.SourceGenerators` 4.6.1. Only the main `Vortex.csproj` uses `Godot.NET.Sdk`. Shared build properties are in `src/Directory.Build.props`.

### Component-Context System (`src/Vortex.Core/runtime/`)

The foundation is an AS3-ported component framework with dependency injection:

- **`Component`** — Base class for all manageable entities. Has lifecycle management (lock/unlock states), asset storage, event dispatching, and reference counting with auto-dispose.
- **`ComponentContext`** — Container for Components implementing `IContext`. Manages interface queuing (async dependency resolution) and update receivers.
- **`CoreComponentContext`** — Root context with Godot integration (scene tree, frame updates, hibernation). Parses configuration XML and manages 3 priority levels of update receivers.

**Lifecycle:** Components lock during dependency injection → unlock when all dependencies resolve → `InitComponent()` fires → "INTERNAL_EVENT_UNLOCKED" emitted.

### IID System (`src/Vortex.IID/`)

~49 marker interfaces for type-safe dependency injection. Pattern:

```csharp
public class IIDCoreCommunicationManager : IID { }
```

Components declare dependencies via `ComponentDependency` with an IID type and a setter callback. `QueueInterface()` resolves dependencies recursively through the component hierarchy.

### Bootstrap System (`src/Vortex.Bootstrap/`)

Two patterns for wiring components into the context:

- **Sealed bootstraps** — Minimal wrappers (~15 lines) for core managers (e.g., `CoreCommunicationManagerBootstrap`)
- **`HabboBootstrapComponentBase`** — Abstract base for feature components. Declares provided/required IIDs, handles dependency requirements, converts feature modules into injectable components.

### Communication Layer

```
HabboCommunicationManager (src/Vortex.Habbo/communication/)
  → CoreCommunicationManager (src/Vortex.Core/communication/)
	→ IConnection → SocketConnection (TCP)
	  → IWireFormat (EvaWireFormat) — binary protocol encoding
```

Four required types for any protocol message (see `docs/COMMUNICATION_EXAMPLES.md`):
- **Composer** (`IMessageComposer`) — outgoing message, payload order must match source exactly
- **Event** (`MessageEvent`) — incoming message wrapper with typed parser accessors
- **Parser** (`IMessageParser`) — `Flush()` reset + `Parse()` sequential reads from `IMessageDataWrapper`
- **Registration** — Map message IDs in `src/Vortex.Habbo/communication/HabboMessages.cs`

### Login Flow (`src/Vortex.Login/`)

Multi-screen Godot UI flow: Environment → Login → Avatars → SSO Token. `LoginFlow` implements `ILoginContext`/`ILoginViewer`, creates a `CoreComponentContext` for dependency resolution, and delegates authentication to `ILoginProvider` (e.g., `WebApiLoginProvider`).

### Room Engine (`src/Vortex.Room/`)

Subdirectories: `data/`, `events/`, `messages/`, `object/`, `renderer/`, `utils/`, `exceptions/`.

## Key Directories

- `src/` — All C# projects and source code (see Architecture above)
- `scenes/` — Godot scenes (`Vortex.tscn` is the main scene)
- `data/manifests/` — XML manifests
- `docs/` — Porting workflow, style guide, checklists, known adaptations
- `sources/` — AS3 source dumps (gitignored, local only)
- `tools/` — Build-time utilities (`Vortex.Bundle`)

## Coding Style (`docs/STYLEGUIDE.md`)

- **Naming:** Types/methods/properties `PascalCase`, locals/params `camelCase`, private fields `_camelCase`, constants `UPPER_SNAKE_CASE`
- **Layout:** One public type per file, filename matches type, minimal sorted `using` blocks
- **Rules:** Always use braces for control blocks. Async methods end with `Async` and return `Task`/`Task<T>`. Keep nullable annotations accurate. Prefer `readonly` fields and read-only contracts.
- **Namespaces:** Aligned with folder paths. 4-space indentation.

## Porting Workflow (`docs/PORTING_WORKFLOW.md`)

1. Identify source file in the correct dump hierarchy
2. Copy behavior semantics first (conditions, state changes, field order, event flow)
3. Apply only Godot/C# adaptations
4. Add `@see` comments pointing to original source path/method
5. For communication code, register IDs in `src/Vortex.Habbo/communication/HabboMessages.cs`
6. Validate: build, test, smoke-run `scenes/Vortex.tscn`

## Commit Style

Conventional Commits with scope: `feat(core):`, `refactor(runtime):`, `fix(communication):`, etc. Imperative, specific subjects.

## Mandatory References for Specific Work

- Communication layer changes → `docs/COMMUNICATION_EXAMPLES.md` + `docs/COMMUNICATION_CHECKLIST.md`
- Source conflicts → `docs/SOURCE_CONFLICTS.md`
- Engine adaptations → `docs/KNOWN_ADAPTATIONS.md`
- PR structure → `docs/PR_TEMPLATE.md`
- Merge readiness → `docs/DEFINITION_OF_DONE.md`
