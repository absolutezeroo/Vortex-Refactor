using Vortex.Bundle.Data;
using Vortex.Bundle.IO;

namespace Vortex.Bundle.Converter.Convert;

/// <summary>
/// Assembles a complete .vortex bundle from collected data sections.
/// </summary>
public sealed class VortexBundleBuilder(StringTableBuilder strings)
{
    private AssetEntry[]? _assets;
    private AliasEntry[]? _aliases;
    private VisualizationData[]? _visualizations;
    private LogicData? _logic;
    private AnimationData[]? _animations;
    private PaletteData[]? _palettes;
    private SpritesheetMeta? _spritesheetMeta;
    private byte[]? _spritesheetImage;
    private Dictionary<string, byte[]>? _rawData;
    private ushort _flags;

    public VortexBundleBuilder SetAssets(AssetEntry[] assets)
    {
        _assets = assets;
        return this;
    }
    public VortexBundleBuilder SetAliases(AliasEntry[] aliases)
    {
        _aliases = aliases;
        return this;
    }
    public VortexBundleBuilder SetVisualizations(VisualizationData[] vizs)
    {
        _visualizations = vizs;
        return this;
    }
    public VortexBundleBuilder SetLogic(LogicData logic)
    {
        _logic = logic;
        return this;
    }
    public VortexBundleBuilder SetAnimations(AnimationData[] anims)
    {
        _animations = anims;
        return this;
    }
    public VortexBundleBuilder SetPalettes(PaletteData[] palettes)
    {
        _palettes = palettes;
        return this;
    }
    public VortexBundleBuilder SetSpritesheet(SpritesheetMeta meta, byte[] imageBytes)
    {
        _spritesheetMeta = meta;
        _spritesheetImage = imageBytes;
        return this;
    }

    public VortexBundleBuilder SetRawData(Dictionary<string, byte[]> rawData)
    {
        foreach (string name in rawData.Keys)
        {
            strings.Add(name);
        }

        _rawData = rawData;
        return this;
    }

    public VortexBundleBuilder SetFlags(ushort flags)
    {
        _flags = flags;
        return this;
    }

    /// <summary>
    /// Writes the assembled .vortex bundle to the given stream.
    /// </summary>
    public void WriteTo(Stream output)
    {
        VortexBundleData data = new()
        {
            Version = VortexBundleFormat.CurrentVersion,
            Flags = _flags,
            Assets = _assets,
            Aliases = _aliases,
            Visualizations = _visualizations,
            Logic = _logic,
            Animations = _animations,
            Palettes = _palettes,
            SpritesheetMeta = _spritesheetMeta,
            SpritesheetImage = _spritesheetImage,
            RawData = _rawData,
        };

        VortexBundleWriter writer = new();
        VortexBundleWriter.Write(output, data, strings.ToArray());
    }

    /// <summary>
    /// Writes the assembled .vortex bundle to a file.
    /// </summary>
    public void WriteToFile(string path)
    {
        using FileStream fs = File.Create(path);
        WriteTo(fs);
    }
}
