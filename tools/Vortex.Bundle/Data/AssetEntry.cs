namespace Vortex.Bundle.Data;

/// <summary>
/// A single asset entry (13 bytes on disk).
/// References a sprite region by source name, with pixel offsets and flags.
/// </summary>
public struct AssetEntry
{
    /// <summary>String table index for the asset name.</summary>
    public uint NameIndex;

    /// <summary>X pixel offset for rendering.</summary>
    public short OffsetX;

    /// <summary>Y pixel offset for rendering.</summary>
    public short OffsetY;

    /// <summary>String table index of the source image name. NULL_STRING if self-sourced.</summary>
    public uint SourceIndex;

    /// <summary>
    /// Bit 0: flipH, Bit 1: flipV, Bit 2: usesPalette.
    /// </summary>
    public byte Flags;

    public const byte FLAG_FLIP_H = 1 << 0;
    public const byte FLAG_FLIP_V = 1 << 1;
    public const byte FLAG_USES_PALETTE = 1 << 2;

    public bool FlipH => (Flags & FLAG_FLIP_H) != 0;
    public bool FlipV => (Flags & FLAG_FLIP_V) != 0;
    public bool UsesPalette => (Flags & FLAG_USES_PALETTE) != 0;
}
