# Vortex.OnBoardingHcUI

Lightweight UI toolkit for onboarding screens. Provides localized, themed controls with 9-slice scaling. Independent of the window system.

## SDK

`Microsoft.NET.Sdk` — References `GodotSharp`/`Godot.SourceGenerators` 4.6.1.
Depends on: `Vortex.Core`

## Key Classes

- **`LoaderUI`** (~429 lines) — Static factory: `CreateTextField`, `CreateButton`, `CreateFrame`, `CreateBalloon`, `CreateScale9Shape`. Layout helpers (`LineUpHorizontally`, `AlignAnchors`). Font loading (Ubuntu variants). Color conversion.
- **`Button`** (~412 lines) — Interactive button with 5 visual states (default, pressed, inactive, editing, rollover). Caption, icon, background layers. Fires `Pressed` event.
- **`ColouredButton`** — Button subclass with color variants (RED, GREEN, YELLOW). Overrides background/pressed/inactive paths.
- **`InputField`** — Text input dialog with title, prompt, caption, subcaption. Supports password masking. Fires `Changed`/`KeyDown` events.
- **`NineSplitSprite`** — 9-slice renderer. 13 static presets (BALLOON_HIGHLIGHTED, FRAME, INPUT_FIELD, etc.). Creates `StyleBoxTexture`/`NinePatchRect`.
- **`LocalizedSprite`** — Base for localized controls. Static `LocalizationResolver` delegate for `${key}` interpolation.
- **`LocalizedTextField`** — Label with `${key}` pattern support.
- **`IUIContext`** — Interface: `Stage` (viewport), `DebugText` (debug label).

## Directory Structure

```
src/
  IUIContext.cs
  LoaderUI.cs              Static UI factories
  LocalizedSprite.cs       Localized control base
  LocalizedTextField.cs    Localized label
  Button.cs                Interactive button (5 states)
  ColouredButton.cs        Color variants (RED/GREEN/YELLOW)
  InputField.cs            Text input dialog
  NineSplitSprite.cs       9-slice renderer (13 presets)
```

## Pattern

Direct `Control`/`Node` hierarchy — no Component-Context DI, no window system. Consumed by `Vortex.Login` for login flow UI.