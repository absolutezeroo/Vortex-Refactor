namespace Vortex.Bundle.Data;

/// <summary>
/// Palette data for color-swappable assets (e.g. clothing with different colors).
/// </summary>
public sealed class PaletteData
{
    public uint Id { get; set; }

    /// <summary>String table index for source reference.</summary>
    public uint SourceIndex { get; set; }

    public bool Master { get; set; }

    public ushort Breed { get; set; }

    public ushort ColorTag { get; set; }

    /// <summary>String table index for primary color name.</summary>
    public uint Color1Index { get; set; }

    /// <summary>String table index for secondary color name.</summary>
    public uint Color2Index { get; set; }

    /// <summary>String table indices for additional tags.</summary>
    public uint[] TagIndices { get; set; } = [];

    /// <summary>Raw RGB entries (3 bytes each: R, G, B).</summary>
    public PaletteColor[] Colors { get; set; } = [];
}

public struct PaletteColor
{
    public byte R;
    public byte G;
    public byte B;
}
