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

## [Launcher] Full-Rect Root Control
- Source reference: `WIN63-202111081545-75921380-Source-main/src/com/sulake/habbo/HabboAirMain.as`
- Target file: `scenes/Vortex.tscn`
- Original behavior: The Flash stage owns the full client viewport and the Habbo root renders into that full stage area.
- Godot/C# adaptation: The Godot root `Control` uses full-rect anchors and the launcher script path points to the active `HabboAir.cs` location.
- Why behavior is preserved: The client root receives the same full viewport area; only Godot scene anchoring and script resource mapping are engine-specific.
- Risk level (Low/Medium/High): Low

## [Window Resources] HTTP Bitmap Loading
- Source reference: `WIN63-202407091256-704579380-Source-main/core/window/graphics/renderer/ResourceManager.as::retrieveAsset`
- Target file: `src/Vortex.Habbo/src/window/ResourceManager.cs`, `src/Vortex.Core/src/assets/loaders/BitmapFileLoader.cs`
- Original behavior: AS3 retrieves bitmap assets from the loaded asset library by resolved asset name; if the resolved name is an HTTP(S) URL, it calls `assets.loadAssetFromFile` with that URL.
- Godot/C# adaptation: `ResourceManager` keeps the same library-first and HTTP(S)-URL loading flow, while `BitmapFileLoader` maps Flash `Loader.load(URLRequest)` to .NET HTTP download plus Godot image decoding.
- Why behavior is preserved: `${image.library.url}` resources remain configuration-interpolated URLs loaded through the asset library; only Flash's bitmap decoder and event transport are replaced by Godot/.NET equivalents.
- Risk level (Low/Medium/High): Low

## [Launcher] Deferred RemoveChild on TreeExited
- Source reference: `WIN63-202111081545-75921380-Source-main/src/HabboAir.as::dispose`
- Target file: `src/Vortex.Launcher/src/HabboAir.cs`
- Original behavior: AS3 `dispose()` calls `parent.removeChild(this)` synchronously when `HabboAirMain` is removed.
- Godot/C# adaptation: `Dispose()` uses `CallDeferred(Node.MethodName.RemoveChild, this)` instead of a direct `RemoveChild`. The `TreeExited` signal fires while the scene tree is mid-operation; a direct `RemoveChild` call at that point triggers Godot's "parent node is busy" error.
- Why behavior is preserved: The node is still removed from its parent; only the timing shifts to the next idle frame, which is equivalent to Flash's deferred display list removal.
- Risk level: Low

## [Window Manager] Skin Init Asset Library Fallback
- Source reference: `WIN63-202407091256-704579380-Source-main/habbo/window/HabboWindowManagerComponent.as::initComponent`
- Target file: `src/Vortex.Habbo/src/window/HabboWindowManagerComponent.cs`
- Original behavior: `initComponent` receives a non-null `IAssetLibrary` via constructor and calls `findAssetByName("habbo_element_description_xml")` from the component's pre-populated local storage.
- Godot/C# adaptation: Bootstrap passes `null` as the `assets` constructor parameter; the component falls back to `HabboFileSystemAssetLibrary` (same adapter used by `WindowSystemCreation`). Element-description XML is read from local storage first; if not preloaded, it is fetched from the library via `GetAssetByName`.
- Why behavior is preserved: The same XML is parsed by the same `SkinParserUtil.Parse` call with the same `IAssetLibrary` contract. Bitmap and skin-XML asset lookups within the parse all route through `HabboFileSystemAssetLibrary`, which delegates to `HabboAssetResolver`. Throw on truly-missing element-description is preserved.
- Risk level: Low

## [Session Data] HTTP Text Loading
- Source reference: `WIN63-202407091256-704579380-Source-main/habbo/session/SessionDataManager.as::initFurnitureData`, `FurnitureDataParser.as::loadData`, `ProductDataParser.as::ProductDataParser`
- Target file: `src/Vortex.Habbo/src/session/SessionDataManager.cs`, `src/Vortex.Habbo/src/session/furniture/FurnitureDataParser.cs`, `src/Vortex.Habbo/src/session/product/ProductDataParser.cs`
- Original behavior: AS3 waits for configuration completion, reads `furnidata.load.url` and `productdata.load.url`, then loads those text assets through `AssetLibrary.loadAssetFromFile`.
- Godot/C# adaptation: The parsers use `TextFileLoader` to fetch the configured URL synchronously and dispatch the same ready events after parsing.
- Why behavior is preserved: Data source, parse format selection, ready events, and listener flow remain AS3-equivalent; only Flash's async asset transport is mapped to the existing C# loader.
- Risk level (Low/Medium/High): Medium
