# Vortex.Splash

Standalone splash screen. Displays random Habbo loading images. Zero external dependencies beyond Godot.

## SDK

`Microsoft.NET.Sdk` — References `GodotSharp`/`Godot.SourceGenerators` 4.6.1.
**No project dependencies.** Fully standalone.

## Key Class

**`PhotoSplashScreen`** — `Control` that builds a 3-layer splash UI:
1. `splash_bg_class.png` — Background
2. `splash_img{1-30}.png` — Random image (1 of 30)
3. `splash_top_class.png` — Top overlay

Images loaded from `res://assets/images/`. Falls back to dark gray `ColorRect` if missing. All elements use `MouseFilter.Ignore`.

## Directory Structure

```
src/
  PhotoSplashScreen.cs    Single file (~80 lines)
```