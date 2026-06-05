# Known Adaptations

Track intentional Godot/C# adaptations that do not change source behavior.

## Allowed Adaptation Categories
- API replacement: Flash/AS3 API mapped to Godot/.NET equivalent
- Lifecycle translation: AS3 init/update/dispose mapped to Godot lifecycle hooks
- Type/runtime translation: collections, numeric types, nullability, async boundaries

## Entry Template
Use one entry per adaptation:

```md
## [Area] Short Title
- Source reference:
- Target file:
- Original behavior:
- Godot/C# adaptation:
- Why behavior is preserved:
- Risk level (Low/Medium/High):
```

## Rules
- Do not log feature additions here; only technical adaptations.
- If adaptation affects protocol shape or ordering, also update rationale from `docs/SOURCE_CONFLICTS.md` in PR.
- Keep entries concise and evidence-based.

## [Landing View] Godot Window Activation
- Source reference: `WIN63-202111081545-75921380-Source-main/src/com/sulake/habbo/friendbar/landingview/HabboLandingView.as`, `WidgetContainerLayout.as`
- Target file: `src/Vortex.Habbo/src/friendbar/landingview/WidgetContainerLayout.cs`
- Original behavior: `HabboLandingView` initializes on `NavigatorSettingsEvent` when `roomIdToEnter <= 0`, builds the configured Landing View XML, and displays it on the desktop.
- Godot/C# adaptation: The local window context can attach `BuildFromXml` results to the desktop during construction; the layout verifies that parentage and only attaches explicitly if a window is still detached.
- Why behavior is preserved: The same XML-driven window is displayed on the same desktop layer; only the engine-specific attachment step is explicit.
- Risk level (Low/Medium/High): Low

## [Window Manager] Viewport Resize Signal Replaces Flash Stage Resize Event
- Source reference: `WIN63-202407091256-704579380-Source-main/core/window/WindowContext.as::stageResizedHandler`
- Target file: `src/Vortex.Habbo/src/window/HabboWindowManagerComponent.cs`, `src/Vortex.Core/src/window/WindowContext.cs`
- Original behavior: AS3 `WindowContext` registers `stage.addEventListener("resize", stageResizedHandler)` in its constructor; `stageResizedHandler` reads `stage.stageWidth/stageHeight` and updates the desktop window bounds.
- Godot/C# adaptation: `HabboWindowManagerComponent.EnsureInitialized` connects `Viewport.SizeChanged` → `OnViewportSizeChanged`, which calls `WindowContext.StageResizedHandler(null)` on each context. The signal is also fired immediately after connection to correct any bounds set before the viewport reported a valid size.
- Why behavior is preserved: Desktop bounds update on window resize exactly as in the AS3 source; the initial correction covers Godot's late viewport size availability.
- Risk level (Low/Medium/High): Low

## [Landing View] Godot Window Layer Mapping
- Source reference: `WIN63-202111081545-75921380-Source-main/src/com/sulake/habbo/friendbar/landingview/layout/WidgetContainerLayout.as::activate`
- Target file: `src/Vortex.Habbo/src/friendbar/landingview/HabboLandingView.cs`
- Original behavior: AS3 builds the Landing View on `getDesktop(0)`.
- Godot/C# adaptation: The local `HabboWindowManagerComponent` uses context layer `1` as its active/default window layer, so Landing View uses that layer for construction and desktop attachment.
- Why behavior is preserved: The view is still shown on the main client desktop; the numeric layer is mapped to the active Godot window context.
- Risk level (Low/Medium/High): Low

## [Landing View] Missing Dynamic Layout Data Fallback
- Source reference: `WIN63-202111081545-75921380-Source-main/src/com/sulake/habbo/friendbar/landingview/layout/WidgetContainerLayout.as::getLayout`
- Target file: `src/Vortex.Habbo/src/friendbar/landingview/WidgetContainerLayout.cs`
- Original behavior: If `landing.view.layoutxml` is absent, the AS3 client uses `landing_view_default_dynamic_layout`.
- Godot/C# adaptation: When no dynamic slot widget configuration is present, the port falls back to `landing_view_generic_reception` so the locally extracted Hotel View can render instead of an empty dynamic shell.
- Why behavior is preserved: Real external configuration still takes precedence through `landing.view.layoutxml`; the fallback only covers incomplete local configuration data during the port.
- Risk level (Low/Medium/High): Medium
