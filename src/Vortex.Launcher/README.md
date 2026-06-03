# Vortex.Launcher

Main entry point and loading screen for the Vortex client. Contains Godot `Control` nodes that bootstrap the application and a window system test harness.

**Note:** This folder has no `.csproj`. Its source files are compiled by the root `Vortex.csproj` via `<Compile Include="src/Vortex.Launcher/src/**/*.cs" />` because Godot requires Node classes in the main assembly.

## Key Classes

- **`HabboAirMain`** (~474 lines) — Main `Control`. Initializes core, configuration, localization, room engine in 3 priority steps. Manages loading screen lifecycle. Wires `CoreEnvironment` external log callbacks in `_Ready()`. Drives `OnExitFrame` in `_Process()`.
- **`HabboAir`** (~490 lines) — Application container managing full initialization flow (config → localization → core → room engine). Wires `HabboAirMain` + loading screen.
- **`HabboLoadingScreen`** (~640 lines) — Loading `Control` with progress bar, splash screen (`PhotoSplashScreen`), version/status text. Bootstraps its own `FakeContext` + `HabboConfigurationManager` + `HabboLocalizationManager` for early asset loading.
- **`FakeContext`** (~100 lines) — Lightweight `IContext` stub for loading screen bootstrap. Holds `AssetLibraryCollection`, `EventDispatcherWrapper`. Avoids full `CoreComponentContext` dependency.
- **`IHabboLoadingScreen`** — Interface: `OnLoadingInitialized`, `OnLoadingError`, `OnLoadingPriority`.
- **`WindowSystemTest`** (~450 lines) — Visual test harness. Lists XML layouts from `data/layouts/`, loads them into WindowContext/WindowRenderer/SkinContainer pipeline. Renders via SubViewport. Tracks render queue, GC count, TrackedImage stats.
- **`GlobalUsings.cs`** — `System.Collections.Generic` + Logger alias.

## Directory Structure

```
src/
  HabboAirMain.cs           Main entry point (3-step init)
  HabboAir.cs               Application container
  HabboLoadingScreen.cs     Loading screen with progress
  FakeContext.cs             Lightweight IContext stub
  IHabboLoadingScreen.cs    Loading screen interface
  WindowSystemTest.cs       Window system test harness
  GlobalUsings.cs           Global using directives
```