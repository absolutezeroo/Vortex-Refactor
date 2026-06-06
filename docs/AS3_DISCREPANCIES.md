# AS3 Discrepancies

Ledger of places where the Godot/C# port **silently dropped load-bearing behavior** from the AS3
source of truth. This is the output target of the audit described below and in
`docs/AS3_AUDIT_PROMPT.md`.

This tracks *semantic* divergences only. Representation differences (names, `[Flags] enum`, typed
overloads, Godot glue) are allowed by `docs/FIDELITY_BOUNDARY.md` and must **not** be listed here.

## Divergence taxonomy

1. **Collapsed/omitted class** — an AS3 class with no port counterpart.
2. **Dropped parameter** — a port ctor/method missing an argument the AS3 has (especially
   `IAssetLibrary`/`assets` threaded for bitmap binding).
3. **Explicit → global substitution** — AS3 resolves via a passed-in `IAssetLibrary`; the port
   reaches for the global/static `HabboAssetResolver`. Equivalent only if the global resolves the
   same component-scoped asset names.
4. **Fail-silent vs fail-loud** — port wraps a required lookup in `if (x != null)` / fallback
   where AS3 assumes presence and throws.
5. **Stub/TODO** standing in for real logic.

## Confirmed findings

| Port symbol | @see AS3 | Type | Severity | Evidence | Fix direction |
|---|---|---|---|---|---|
| `habbo/toolbar/*` (whole package) | `habbo/toolbar/*` | 1 | blocks-render | AS3 has 38 classes; port has 5 (`HabboToolbar`, enums, event, interface). Entire content layer absent. **PARTIAL**: `BottomBarLeft` ported (2025-06-06); 32 other classes remain (see inventory). | Port the remaining classes (see inventory below). |
| `HabboToolbar.SetIconBitmap` | `habbo.toolbar.HabboToolbar::setIconBitmap` | 5 | **RESOLVED** (2025-06-06) | Now delegates to `BottomBarLeft.SetIconBitmap`; AS3 MEMENU-only handling preserved. | — |
| `HabboToolbar.CreateToolbarView` | `habbo.toolbar.HabboToolbar` (`BottomBarLeft` ctor) | 1 | **RESOLVED** (2025-06-06) | `CreateToolbarView` now instantiates `BottomBarLeft(this, _windowManager, assets, events)`. `ResourceManager.RetrieveAsset` gains filesystem fallback when `assets` is null (Godot adaptation). | — |
| `SkinParserUtil.Parse(xml, container)` | `habbo/window/utils/class_3503.as::parse` | 2 | **RESOLVED** (2026-06-06) | AS3 signature `parse(XML, IAssetLibrary, SkinContainer)` restored: `Parse(XElement, IAssetLibrary, SkinContainer)`. Asset lookups (skin XML, template bitmaps, window_layout XML) route through the passed `IAssetLibrary` via `GetAssetByName`. `HabboFileSystemAssetLibrary` wraps `HabboAssetResolver` for `WindowSystemCreation` test harness. | — |
| Skin render path (renderers + parser) | `BitmapSkinRenderer`, `TextSkinRenderer`, `SkinRenderer`, `class_3503`, `class_3725` | 3 | **RESOLVED** (2026-06-06) | `SkinParserUtil.Parse` now threads `IAssetLibrary` as `Func<string, Image?>` delegate into `renderer.Parse` and `Class3725.ParseSkinDescription`, matching AS3's param3:IAssetLibrary flow. Global `HabboAssetResolver` no longer used in the component render path. | — |
| `HabboWindowManagerComponent` skin load | `HabboWindowManagerComponent.as::initComponent` | 4 | **RESOLVED** (2026-06-06) | `EnsureInitialized` now throws `InvalidOperationException` on missing `IAssetLibrary` or missing/empty element-description asset, mirroring AS3's no-guard throw. 3-way silent fallback removed. Asset is removed and disposed after parse, matching `this.removeAsset(asset); asset.dispose()`. | — |

## Missing-class inventory — `habbo/toolbar` (32 of 38)

Bar / structure: `BottomBackgroundBorder`, `ToolbarView`, `ExtensionView`,
`IExtensionView`, `ExtensionViewEvent`, `ToolbarDisplayExtensionIds`,
`class_3422`, `class_3491`, `class_3571`.

Me-menu: `MeMenuController`, `MeMenuNewController`, `MeMenuIconLoader`, `MeMenuNewIconLoader`,
`MeMenuChatSettingsView`, `MeMenuSettingsMenuView`, `MeMenuSoundSettingsView`,
`MeMenuSoundSettingsItem`, `MeMenuSoundSettingsSlider`, `OtherSettingsView`,
`SoundSettingsItem`, `SoundSettingsView`, `SettingsExtension`.

Currency / purse: `CurrencyIndicatorBase`, `SeasonalCurrencyIndicator`, `PurseAreaExtension`,
`PurseClubArea`.

Promo / offers: `CitizenshipVipDiscountPromoExtension`, `CitizenshipVipQuestsPromoExtension`,
`ClubDiscountPromoExtension`, `OfferExtension`, `VideoOfferExtension`.

> `MeMenuIconLoader` / `MeMenuNewIconLoader` are the classes that load me-menu icon bitmaps —
> their absence is why the me-menu icon is also empty.

## Method

Leaf-first within a subsystem. For each port symbol: resolve its `@see`, open both files, compare
signatures / control flow / asset+event threading / init order / throw-vs-guard, classify by the
taxonomy, and record severity (`blocks-render` / `wrong-behavior` / `cosmetic`). Mark
`needs-confirmation` rather than guess. Run the audit with `docs/AS3_AUDIT_PROMPT.md`.

Next scopes after toolbar: the window skin/asset render path, then `core/window` components.
