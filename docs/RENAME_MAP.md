# Rename Map

Append-only ledger of de-obfuscation renames. Maps the original obfuscated AS3 name to the
canonical C# name. See `docs/FIDELITY_BOUNDARY.md` for why these renames are allowed and required.

## Rules

- **Append-only.** One row per symbol. Supersede rather than rewrite.
- **`@see` stays.** Code keeps pointing at the obfuscated AS3 path; this table resolves it.
- **Status discipline.**
  - `confirmed` — justified by an existing doc-comment, an implementor + member set, or a
    source-dump signature. Cited in `Evidence`. Safe to rename now.
  - `proposed` — inferred from a base interface or weak signal only. **Commit the row, but do not
    rename in code until confirmed against the `PRODUCTION` deobfuscation dump.**
- **Rename symbol *and* file.** A row is "done" only when both are renamed.

## Format

`| AS3 (obfuscated) | C# canonical | Kind | @see | Status | Evidence / Notes |`

> Namespace note: Core-level `WindowType/WindowParam/WindowStyle` live in `Vortex.Core.Window.Enum`
> and do **not** collide with the already-named `HabboWindowType/HabboWindowParam/HabboWindowStyle`
> in `Vortex.Habbo`.

---

## Core — `graphics/`

| AS3 | C# canonical | Kind | @see | Status | Evidence |
|---|---|---|---|---|---|
| `class_3354` | `IWindowRenderer` | Interface | `core/window/graphics/class_3354.as` | confirmed | Doc "Window renderer interface"; implemented by `WindowRenderer`. |
| `class_3655` | `WindowInvalidation` | Enum `[Flags]` | `core/window/graphics/class_3655.as` | confirmed | Doc "Invalidation flags"; powers of two. Convert `static class`→`[Flags] enum`, values unchanged. |
| `class_3725` | `WindowSkinXmlParser` | Type (static) | `core/window/graphics/class_3725.as` | confirmed | Doc "Skin description XML parser". |

## Core — `motion/`

| AS3 | C# canonical | Kind | @see | Status | Evidence |
|---|---|---|---|---|---|
| `class_3596` | `MotionManager` | Type (static) | `core/window/motion/class_3596.as` | confirmed | Symbol already renamed. **File still `class_3596.cs` — rename pending.** |

## Core — `enum/`

| AS3 | C# canonical | Kind | @see | Status | Evidence |
|---|---|---|---|---|---|
| `class_3409` | `WindowType` | Enum/consts | `core/window/enum/class_3409.as` | confirmed | Doc "Window type ID constants. Maps to controller classes via `Classes`". |
| `class_3459` | `WindowParam` | Enum `[Flags]` | `core/window/enum/class_3459.as` | confirmed | Doc "Window parameter bit flags (behavior, scaling, mouse, alignment)". |
| `class_3466` | `WindowState` | Enum `[Flags]` | `core/window/enum/class_3466.as` | confirmed | Doc "Window state bit flags". |
| `class_3487` | `WindowStyle` | Enum `[Flags]` | `core/window/enum/class_3487.as` | confirmed | Doc "Window style flags". |
| `class_3519` | `MouseAreaFilter` | Enum/consts | `core/window/enum/class_3519.as` | confirmed | Doc "Mouse listener area filter types". |
| `class_3549` | `MouseCursorType` | Enum/consts | `core/window/enum/class_3549.as` | confirmed | Doc "Mouse cursor/pointer type constants". |
| `class_3555` | `LinkTarget` | Consts (string) | `core/window/enum/class_3555.as` | confirmed | Doc "Window/link target string constants (HTML link targets)". |
| `class_3588` | `WindowDirection` | Consts (string) | `core/window/enum/class_3588.as` | confirmed | Doc "Direction string constants". Named `Window…` to disambiguate from Habbo `ArrowPivot`. |

## Core — `dynamicstyle/`

| AS3 | C# canonical | Kind | @see | Status | Evidence |
|---|---|---|---|---|---|
| `class_3622` | `DynamicStyleRegistry` | Type (static) | `core/window/dynamicstyle/class_3622.as` | confirmed | Doc "Static style registry … lifted_hover, brightness_and_shadow_under". Pairs with `DynamicStyle`. |

## Core — `components/` (window contract interfaces)

| AS3 | C# canonical | Kind | @see | Status | Evidence |
|---|---|---|---|---|---|
| `class_3463` | `IBackgroundWindow` | Interface | `core/window/components/class_3463.as` | confirmed | Implemented by `BackgroundController`. |
| `class_3542` | `ICloseButtonWindow` | Interface | `core/window/components/class_3542.as` | confirmed | Implemented by `CloseButtonController`; extends `IInteractiveWindow`. |
| `class_3592` | `IBubbleWindow` | Interface | `core/window/components/class_3592.as` | confirmed | Members `Direction/PointerOffset/Margins/Content` → speech bubble; pairs with `BubbleController`. |
| `class_3651` | `IBoxSizerWindow` | Interface | `core/window/components/class_3651.as` | confirmed | Implemented by `BoxSizerController`; `SetHorizontal/VerticalPadding/Spacing/Vertical`. |
| `class_3404` | `IHtmlTextWindow` | Interface | `core/window/components/class_3404.as` | proposed | Member `InitializeLinkStyle()` → HTML text with links (`HTMLTextController`). Confirm. |
| `class_3493` | `IScrollableContainerWindow` | Interface | `core/window/components/class_3493.as` | proposed | Members `ScrollbarOffsetX/Y`. Confirm role vs `IScrollableWindow`. |
| `class_3502` | `IFrameWindow` | Interface | `core/window/components/class_3502.as` | proposed | Members `Title`, `Controls`. Titled frame; confirm vs `3514`. |
| `class_3514` | `IHeaderedWindow` | Interface | `core/window/components/class_3514.as` | proposed | Members `Title/Header/Content/Margins`. Confirm naming vs `3502`. |
| `class_3398` | `ISelectableButtonWindow` | Interface | `core/window/components/class_3398.as` | proposed | Extends `ISelectableWindow`, no added members. Confirm vs `SelectableButtonController`. |
| `class_3424` | `IPasswordFieldWindow` | Interface | `core/window/components/class_3424.as` | proposed | Extends `ITextFieldWindow`. Likely password field. Confirm. |
| `class_3437` | _TBD_ | Interface | `core/window/components/class_3437.as` | proposed | Extends `IWindowContainer`, no members. Insufficient signal — confirm against dump. |
| `class_3489` | `IContainerButtonWindow` | Interface | `core/window/components/class_3489.as` | proposed | Extends `IInteractiveWindow, IWindowContainer`. Confirm vs `ContainerButtonController`. |
| `class_3492` | _TBD_ | Interface | `core/window/components/class_3492.as` | proposed | Extends `IWindowContainer`, no members. Insufficient signal — confirm against dump. |

## Core — `services/`, `tools/`, `utils/`

| AS3 | C# canonical | Kind | @see | Status | Evidence |
|---|---|---|---|---|---|
| `class_3739` | `IWindowService` | Interface | `core/window/services/class_3739.as` | proposed | Members `DisposeService()`, `Begin/End(IWindow)`. Likely base service contract; confirm vs named services. |
| `class_3394` | `IProfilerView` | Interface | `core/window/tools/class_3394.as` | proposed | Member `Visible`; `tools/` dir alongside `ProfilerOutput`. Confirm. |
| `class_3441` | _TBD_ | Interface | `core/window/utils/class_3441.as` | proposed | Extends `IClass3348` (outside window tree). Resolve the pair against dump. |
| `class_3562` | _TBD_ | Interface | `core/window/utils/class_3562.as` | proposed | Member `Visible` only. Insufficient signal — confirm against dump. |

## Habbo — `toolbar/`

| AS3 | C# canonical | Kind | @see | Status | Evidence |
|---|---|---|---|---|---|
| `class_3491` | `ICurrencyIndicator` | Interface | `…/toolbar/extensions/purse/class_3491.as` | confirmed | Members `Dispose()`, `window`, `RegisterUpdateEvents(object?)` — currency indicator contract for purse area. |
| `class_3571` | `ExtensionFixedSlotsEnum` | Consts (int) | `…/toolbar/class_3571.as` | confirmed | `SLOT_PURSE=0`, `SLOT_SETTINGS=1` — fixed slot indices for extension view. |
| `class_3422` | `WingCodeEnum` | Consts (string) | `…/toolbar/class_3422.as` | confirmed | `Social/Group/Quest/Game` wing codes for toolbar wings. |

## Habbo — `enum/`

| AS3 | C# canonical | Kind | @see | Status | Evidence |
|---|---|---|---|---|---|
| `class_3417` | `ArrowOrientation` | Consts (int) | `…/window/enum/class_3417.as` | confirmed | `ARROW_HORIZONTAL=0`, `ARROW_VERTICAL=1`. |
| `class_3550` | `BadgeType` | Consts (string) | `…/window/enum/class_3550.as` | confirmed | `NORMAL/GROUP/PERK` — Habbo badge categories. |
| `class_3719` | `ProgressIndicatorMode` | Consts (string) | `…/window/enum/class_3719.as` | confirmed | `POSITION/PROGRESS`; matches `ProgressIndicatorWidget.Mode`. |
| `class_3821` | `ArrowPivot` | Consts (string) | `…/window/enum/class_3821.as` | confirmed | `UP_LEFT…DOWN_RIGHT`; matches `BalloonWidget.ArrowPivot`. |
| `class_3759` | `RunningNumberXmlNames` | Consts (string) | `…/window/enum/class_3759.as` | proposed | `VALUE_ELEMENT_NAME="count"`. Confirm scope. |
| `class_3827` | `SeparatorStyle` | Consts (string) | `…/window/enum/class_3827.as` | proposed | `FLAT/ETCHED`. Likely separator/border bevel; confirm vs `SeparatorWidget`. |

## Habbo — `utils/`

| AS3 | C# canonical | Kind | @see | Status | Evidence |
|---|---|---|---|---|---|
| `class_3822` | `RoomUserCountColorResolver` | Type (static) | `…/window/utils/class_3822.as` | confirmed | Doc "room user count display color based on occupancy percentage". |

## Habbo — `widgets/` (widget contract interfaces + registry)

| AS3 | C# canonical | Kind | @see | Status | Evidence |
|---|---|---|---|---|---|
| `class_3474` | `WidgetType` | Type (registry) | `…/window/widgets/class_3474.as` | confirmed | Doc "widget type registry … used by `IWidgetFactory.CreateWidget()`". |
| `class_3528` | `IProgressIndicatorWidget` | Interface | `…/widgets/class_3528.as` | confirmed | Implemented by `ProgressIndicatorWidget`; `Size/Position/Mode`. |
| `class_3530` | `IPixelLimitWidget` | Interface | `…/widgets/class_3530.as` | confirmed | Implemented by `PixelLimitWidget`; `Limit`. |
| `class_3563` | `IHoverBitmapWidget` | Interface | `…/widgets/class_3563.as` | confirmed | Implemented by `HoverBitmapWidget`; `BitmapWrapper`. |
| `class_3570` | `ICountdownWidget` | Interface | `…/widgets/class_3570.as` | confirmed | Implemented by `CountdownWidget`; `ColorStyle/Running/Digits/Seconds`. |
| `class_3583` | `IBalloonWidget` | Interface | `…/widgets/class_3583.as` | confirmed | Implemented by `BalloonWidget`; `ArrowPivot/ArrowDisplacement`. |
| `class_3614` | `IUpdatingTimeStampWidget` | Interface | `…/widgets/class_3614.as` | confirmed | Implemented by `UpdatingTimeStampWidget`; `Reset()`. |
| `class_3618` | `IFurnitureImageWidget` | Interface | `…/widgets/class_3618.as` | confirmed | Implemented by `FurnitureImageWidget`; `Scale/Direction`. |
| `class_3654` | `IPetImageWidget` | Interface | `…/widgets/class_3654.as` | confirmed | Implemented by `PetImageWidget`; `Scale/Direction/PetWidth/PetHeight`. |

---

## WindowContext fields (`src/Vortex.Core/src/window/WindowContext.cs`)

| AS3 | C# canonical | Kind | @see | Status | Evidence |
|---|---|---|---|---|---|
| `var_1836` | `_renderer` | Field (`IWindowRenderer`) | `WindowContext.as::var_1836` | confirmed | Receives `AddToRenderQueue`/`Render`. |
| `var_4604` | `_eventProcessorState` | Field | `WindowContext.as::var_4604` | confirmed | Type already named. |
| `var_1750` | `_desktop` | Field (`IDesktopWindow`) | `WindowContext.as::var_1750` | confirmed | Type already named. |
| `var_2890` | `_resourceManager` | Field (`IResourceManager`) | `WindowContext.as::var_2890` | confirmed | Type already named. |
| `var_3528` | `_windowParser` | Field (`WindowParser`) | `WindowContext.as::var_3528` | confirmed | Type already named. |
| `var_3190` | `_serviceManager` | Field | `WindowContext.as::var_3190` | confirmed | Used as `var_3190 is ServiceManager`. **Tighten field type `object`→`ServiceManager`/interface.** |
| `var_1896` | `_updating` | Field (`bool`) | `WindowContext.as` | proposed | `true` during `Update`, `false` after — re-entrancy guard. Confirm intent. |

---

## How To Add A Row

1. Touch a file containing an obfuscated name.
2. Determine the canonical name (doc-comment, implementor + members, or PRODUCTION dump).
3. Justified → `confirmed`; rename symbol + file via the IDE, keep `@see`, add the row.
4. Unsure → add a `proposed` row, leave code as-is, confirm later.
