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
| `habbo/toolbar/*` (whole package) | `habbo/toolbar/*` | 1 | blocks-render | AS3 has 38 classes; port has 5 (`HabboToolbar`, enums, event, interface). Entire content layer absent. | Port the missing classes (see inventory below), starting with `BottomBarLeft`. |
| `HabboToolbar.SetIconBitmap` | `habbo.toolbar.HabboToolbar::setIconBitmap` | 5 | blocks-render (icons) | Body stores to a dict then `// TODO(assets): push bitmap…`. Also: **no caller anywhere** in `src`. AS3 forwards to `BottomBarLeft.setIconBitmap`. | Port `BottomBarLeft` (which owns icon binding via `assets`); have it call a real `SetIconBitmap`. |
| `HabboToolbar.CreateToolbarView` | `habbo.toolbar.HabboToolbar` (`BottomBarLeft` ctor) | 1 | blocks-render | Port calls `_windowManager.BuildFromXml` directly. AS3 delegates to `new BottomBarLeft(this, _windowManager, assets, events)` which builds **and** binds icons. The bar draws; icon population is gone. | Reintroduce a `BottomBarLeft` equivalent with `assets` threaded in. |
| `SkinParserUtil.Parse(xml, container)` | `habbo/window/utils/class_3503.as::parse` | 2 | blocks-render | AS3 signature is `parse(XML, IAssetLibrary, SkinContainer)`; it binds bitmaps via `assets.getAssetByName(...)` (lines ~104, ~123). Port dropped the `IAssetLibrary` arg and routes lookups through global `HabboAssetResolver`. | Restore the `IAssetLibrary` parameter, or prove `HabboAssetResolver` resolves the identical component-scoped names. |
| Skin render path (renderers + parser) | `BitmapSkinRenderer`, `TextSkinRenderer`, `SkinRenderer`, `class_3503`, `class_3725` | 3 | needs-confirmation | 19 AS3 window/toolbar files thread `IAssetLibrary` explicitly; the port uses it in only 3 and resolves via global `HabboAssetResolver` throughout the render path. Scope-fragile where asset names are component-scoped. | Audit each: confirm the global resolves the right names, or restore threading. |
| `HabboWindowManagerComponent` skin load | `HabboWindowManagerComponent.as::initComponent` | 4 | hides failures | AS3: `parse(asset.content as XML, ...)` with no guard → throws on missing asset. Port: `if (elementDescXml != null)` + 3-way fallback → silent empty `SkinContainer`. | Make the required lookup throw (`ThrowIfNull`), mirroring AS3. See `FIDELITY_BOUNDARY` fail-loud rule. |

## Missing-class inventory — `habbo/toolbar` (33 of 38)

Bar / structure: `BottomBarLeft`, `BottomBackgroundBorder`, `ToolbarView`, `ExtensionView`,
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
