namespace Vortex.Bundle;

/// <summary>
/// Constants defining the .vortex binary bundle format.
/// </summary>
public static class VortexBundleFormat
{
    /// <summary>"VRTX" magic bytes (0x56 0x52 0x54 0x58).</summary>
    public static ReadOnlySpan<byte> Magic => "VRTX"u8;

    public const ushort CurrentVersion = 1;

    public const int HeaderSize = 16;
    public const int TocEntrySize = 12;

    // --- Header flags ---
    public const ushort FLAG_HAS_SHADOWS = 1 << 0;
    public const ushort FLAG_WEBP_IMAGES = 1 << 2;

    // --- Section IDs ---
    public const ushort SECTION_STRING_TABLE = 0x01;
    public const ushort SECTION_ASSETS = 0x02;
    public const ushort SECTION_ALIASES = 0x03;
    public const ushort SECTION_VISUALIZATION = 0x04;
    public const ushort SECTION_LOGIC = 0x05;
    public const ushort SECTION_ANIMATION = 0x06;
    public const ushort SECTION_PALETTES = 0x07;
    public const ushort SECTION_SPRITESHEET_META = 0x08;
    public const ushort SECTION_SPRITESHEET_IMAGE = 0x09;
    public const ushort SECTION_RAW_DATA = 0x0A;

    /// <summary>Sentinel value for "no string" / null reference in string table lookups.</summary>
    public const uint NULL_STRING = 0xFFFFFFFF;
}
