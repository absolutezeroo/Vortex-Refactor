# Project Context

## Goal
This repository ports Habbo client logic to Godot 4.6 + C# (`net8.0`) while preserving original behavior.

## Source Of Truth
- `sources/WIN63-202407091256-704579380-Source-main/`: primary source for all application logic.
- `sources/WIN63-202111081545-75921380-Source-main/`: use only for XML-related porting and these files: `HabboAirMain.as`, `Habbo.as`, `HabboLoadingScreen.as`, `IHabboLoadingScreen.as`.
- `sources/PRODUCTION-201611291003-338511768-Source-main/`: deobfuscation fallback when code exists there and is missing or unclear in newer dumps.

## Porting Rule
Do not invent gameplay or business logic. Behavior must come from source dumps above.

Allowed exceptions are technical adaptations required by Godot/C#:
- API mapping from AS3/Flash to Godot/.NET
- lifecycle/event wiring
- type-system and runtime integration

## Active Code Areas
- `scripts/`: C# implementation
- `scenes/`: Godot scenes (`scenes/Vortex.tscn` main scene)
- `data/manifests/`: XML manifests
- `docs/`: migration and status notes
