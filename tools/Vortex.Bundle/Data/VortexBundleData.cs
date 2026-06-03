namespace Vortex.Bundle.Data;

/// <summary>
/// Top-level container for all data read from a .vortex bundle.
/// </summary>
public sealed class VortexBundleData
{
    public ushort Version { get; set; }

    public ushort Flags { get; set; }

    public StringTable? StringTable { get; set; }

    public AssetEntry[]? Assets { get; set; }

    public AliasEntry[]? Aliases { get; set; }

    public VisualizationData[]? Visualizations { get; set; }

    public LogicData? Logic { get; set; }

    public AnimationData[]? Animations { get; set; }

    public PaletteData[]? Palettes { get; set; }

    public SpritesheetMeta? SpritesheetMeta { get; set; }

    public byte[]? SpritesheetImage { get; set; }

    public Dictionary<string, byte[]>? RawData { get; set; }

    public bool HasShadows => (Flags & VortexBundleFormat.FLAG_HAS_SHADOWS) != 0;

    public bool UsesWebP => (Flags & VortexBundleFormat.FLAG_WEBP_IMAGES) != 0;
}
