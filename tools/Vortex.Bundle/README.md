# Vortex.Bundle

Binary asset bundle format (`.vortex` files) for packing textures, animations, palettes, and logic into a single file. Used by avatar rendering and UI systems.

## SDK

`Microsoft.NET.Sdk` — .NET 9.0.
**No external dependencies.** Fully standalone.

## Bundle Format

```
Header (16 bytes):
  Magic: "VRTX" (4 bytes)
  Version: ushort
  Flags: ushort
  TocOffset: uint
  TocCount: uint

ToC Entries (12 bytes each):
  SectionId: ushort
  Reserved: ushort
  Offset: uint
  Length: uint

Sections (variable):
  0x01  STRING_TABLE        String pool (read first, all sections reference by index)
  0x02  ASSETS              AssetEntry[] (name, offset, source, flip flags)
  0x03  ALIASES             AliasEntry[] (asset name aliases)
  0x04  VISUALIZATION       VisualizationData[] (geometry/hierarchy)
  0x05  LOGIC               LogicData (single object)
  0x06  ANIMATION           AnimationData[] (frame definitions)
  0x07  PALETTES            PaletteData[] (color palettes)
  0x08  SPRITESHEET_META    SpritesheetMeta
  0x09  SPRITESHEET_IMAGE   Raw spritesheet bytes
```

**Flags:** `FLAG_HAS_SHADOWS (1<<0)`, `FLAG_WEBP_IMAGES (1<<2)`
**Encoding:** Little-endian. NULL_STRING sentinel = `0xFFFFFFFF`.

## Key Classes

- **`VortexBundleReader`** — Reads `.vortex` bundles. String table parsed first, then sections. Optional: skip spritesheet image for metadata-only access.
- **`VortexBundleWriter`** — Writes `.vortex` bundles. Builds section buffers, calculates layout, writes header → ToC → sections.
- **`VortexBundleData`** — Container for all sections. Computed: `HasShadows`, `UsesWebP`.
- **`VortexBundleFormat`** — Static constants (magic, version, section IDs, flags).
- **`StringTable`** — Immutable string lookup by index.

## Directory Structure

```
Data/
  VortexBundleFormat.cs     Constants
  VortexBundleData.cs       Data container
  StringTable.cs            String pool
  TocEntry.cs               Table of contents entry
  AssetEntry.cs             Sprite reference (offset, flip flags)
  AliasEntry.cs             Asset alias mapping
  VisualizationData.cs      Geometry structures
  LogicData.cs              Logic data
  AnimationData.cs          Frame definitions
  PaletteData.cs            Color palettes
  SpritesheetMeta.cs        Spritesheet metadata
  FrameData.cs              Frame definition
IO/
  VortexBundleReader.cs     Reader (~200 lines)
  VortexBundleWriter.cs     Writer (~200 lines)
```