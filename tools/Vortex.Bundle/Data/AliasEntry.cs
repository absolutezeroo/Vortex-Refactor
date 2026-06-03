namespace Vortex.Bundle.Data;

/// <summary>
/// A single alias entry (9 bytes on disk).
/// Maps one asset name to another (the "link").
/// </summary>
public struct AliasEntry
{
    /// <summary>String table index for the alias name.</summary>
    public uint NameIndex;

    /// <summary>String table index for the target asset name.</summary>
    public uint LinkIndex;

    /// <summary>
    /// Bit 0: flipH, Bit 1: flipV.
    /// </summary>
    public byte Flags;

    public const byte FLAG_FLIP_H = 1 << 0;
    public const byte FLAG_FLIP_V = 1 << 1;

    public bool FlipH => (Flags & FLAG_FLIP_H) != 0;
    public bool FlipV => (Flags & FLAG_FLIP_V) != 0;
}
