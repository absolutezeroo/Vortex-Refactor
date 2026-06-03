using System.Text;
using System.Xml.Linq;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using Vortex.Bundle.Converter.Extract;
using Vortex.Bundle.Converter.Pack;
using Vortex.Bundle.Converter.Swf;
using Vortex.Bundle.Data;

namespace Vortex.Bundle.Converter.Convert;

/// <summary>
/// Orchestrates the full SWF → .vortex conversion pipeline.
/// Steps: parse SWF → extract images + XML → parse XML → pack sprites → build bundle → write.
/// </summary>
public sealed class ConvertPipeline
{
    public static ConvertResult Convert(string swfPath, string outputPath, ConvertOptions? options = null)
    {
        options ??= new ConvertOptions();
        ConvertResult result = new()
        {
            InputPath = swfPath, OutputPath = outputPath,
        };

        try
        {
            // 1. Parse SWF
            SwfFile swf = SwfReader.Read(swfPath);
            result.DocumentClass = swf.Tags.OfType<SymbolClassTag>()
                                      .SelectMany(t => t.Symbols)
                                      .FirstOrDefault(s => s.CharId == 0).ClassName ?? "unknown";

            // 2. Extract images + XML
            HabboSwfExtractor extractor = new(swf);
            extractor.Extract();

            result.ImageCount = extractor.Images.Count;
            result.XmlDocuments = extractor.XmlDocuments.Keys.ToList();

            // 3. Build string table and parse XML sections
            StringTableBuilder strings = new();
            VortexBundleBuilder builder = new(strings);
            ushort flags = 0;

            // Assets + Aliases
            // Try "assets" XML first (furniture), fall back to "manifest" (avatar SWFs)
            if (!extractor.XmlDocuments.TryGetValue("assets", out XDocument? assetsXml))
            {
                extractor.XmlDocuments.TryGetValue("manifest", out assetsXml);
            }

            if (assetsXml != null)
            {
                (AssetEntry[] assets, AliasEntry[] aliases) = AssetsXmlParser.Parse(assetsXml, strings);
                builder.SetAssets(assets);
                builder.SetAliases(aliases);

                // Palettes (embedded in assets XML)
                PaletteData[] palettes = PaletteXmlParser.Parse(assetsXml, strings);
                if (palettes.Length > 0)
                {
                    builder.SetPalettes(palettes);
                }
            }

            // Visualization
            if (extractor.XmlDocuments.TryGetValue("visualization", out XDocument? vizXml))
            {
                VisualizationData[] vizData = VisualizationXmlParser.Parse(vizXml, strings);
                builder.SetVisualizations(vizData);
            }

            // Logic
            if (extractor.XmlDocuments.TryGetValue("logic", out XDocument? logicXml))
            {
                LogicData logicData = LogicXmlParser.Parse(logicXml, strings);
                builder.SetLogic(logicData);
            }

            // Animation (figure parts)
            if (extractor.XmlDocuments.TryGetValue("animation", out XDocument? animXml))
            {
                AnimationData[] animData = AnimationXmlParser.Parse(animXml, strings);
                if (animData.Length > 0)
                {
                    builder.SetAnimations(animData);
                }
            }

            // Raw XML blobs — preserves original XML documents (e.g. room-specific
            // <wallData>/<floorData> that can't be reconstructed from structured data)
            if (extractor.XmlDocuments.Count > 0)
            {
                Dictionary<string, byte[]> rawData = new();

                foreach ((string typeName, XDocument doc) in extractor.XmlDocuments)
                {
                    rawData[typeName] = Encoding.UTF8.GetBytes(doc.ToString());
                }

                builder.SetRawData(rawData);
            }

            // 4. Pack sprites into spritesheet
            if (extractor.Images.Count > 0)
            {
                bool useWebP = options.UseWebP;
                (SpritesheetMeta meta, byte[] imageBytes) = SpritesheetPacker.Pack(extractor.Images, strings, useWebP);
                builder.SetSpritesheet(meta, imageBytes);
                result.AtlasWidth = meta.Width;
                result.AtlasHeight = meta.Height;
                result.FrameCount = meta.Frames.Length;

                if (useWebP)
                {
                    flags |= VortexBundleFormat.FLAG_WEBP_IMAGES;
                }
            }

            builder.SetFlags(flags);

            // 5. Write bundle
            builder.WriteToFile(outputPath);

            result.Success = true;
            result.OutputSize = new FileInfo(outputPath).Length;
            result.StringCount = strings.Count;

            // Dispose extracted images
            foreach (Image<Rgba32> img in extractor.Images.Values)
            {
                img.Dispose();
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Error = ex.Message;
        }

        return result;
    }
}

public sealed class ConvertOptions
{
    public bool UseWebP { get; set; } = true;
}

public sealed class ConvertResult
{
    public required string InputPath { get; set; }
    public required string OutputPath { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? DocumentClass { get; set; }
    public int ImageCount { get; set; }
    public List<string> XmlDocuments { get; set; } = [];
    public int StringCount { get; set; }
    public int AtlasWidth { get; set; }
    public int AtlasHeight { get; set; }
    public int FrameCount { get; set; }
    public long OutputSize { get; set; }
}
