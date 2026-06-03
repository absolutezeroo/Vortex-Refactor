using System.Text;
using System.Xml.Linq;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using Vortex.Bundle.Converter.Swf;

namespace Vortex.Bundle.Converter.Extract;

/// <summary>
/// Extracts Habbo asset data (images + XML metadata) from a parsed SWF file.
/// @see nitro-converter-main/src/swf/HabboAssetSWF.ts
/// @see nitro-converter-main/src/swf/SWFUtilities.ts
/// </summary>
public sealed class HabboSwfExtractor
{
    private readonly SwfFile _swf;

    /// <summary>Extracted sprite images keyed by class name.</summary>
    public Dictionary<string, Image<Rgba32>> Images { get; } = new();

    /// <summary>Extracted XML documents keyed by type name (e.g. "assets", "visualization").</summary>
    public Dictionary<string, XDocument> XmlDocuments { get; } = new();

    public string DocumentClass { get; }

    public HabboSwfExtractor(SwfFile swf)
    {
        _swf = swf;
        DocumentClass = FindDocumentClass();
    }

    /// <summary>
    /// Performs full extraction: decodes images and parses XML binary data.
    /// </summary>
    public void Extract()
    {
        ExtractImages();
        ExtractXml();
    }

    private void ExtractImages()
    {
        foreach (SwfTag tag in _swf.Tags)
        {
            switch (tag)
            {
                case DefineBitsLosslessTag
                {
                    ClassName: not null,
                } lossless:
                    {
                        Image<Rgba32> image = SwfImageDecoder.DecodeLossless(lossless);
                        string spriteName = ClassNameToAssetName(lossless.ClassName);
                        Images[spriteName] = image;
                        break;
                    }
                case DefineBitsJpegTag
                {
                    ClassName: not null,
                } jpeg:
                    {
                        Image<Rgba32> image = SwfImageDecoder.DecodeJpeg(jpeg);
                        string spriteName = ClassNameToAssetName(jpeg.ClassName);
                        Images[spriteName] = image;
                        break;
                    }
            }
        }
    }

    private void ExtractXml()
    {
        // XML types and their naming patterns
        string[] xmlTypes = new[]
        {
            "manifest",
            "index",
            "assets",
            "logic",
            "visualization",
            "animation",
        };

        // Track which binary data tags were already matched
        HashSet<DefineBinaryDataTag> matched = new();

        foreach (string xmlType in xmlTypes)
        {
            DefineBinaryDataTag? binaryTag = FindBinaryDataTag(xmlType);
            if (binaryTag == null)
            {
                continue;
            }

            matched.Add(binaryTag);

            try
            {
                string xmlText = Encoding.UTF8.GetString(binaryTag.BinaryData);
                xmlText = RemoveXmlComments(xmlText);
                XDocument doc = XDocument.Parse(xmlText);
                XmlDocuments[xmlType] = doc;
            }
            catch (Exception)
            {
                // Skip malformed XML silently
            }
        }

        // Scan remaining DefineBinaryData tags for XML content.
        // Room content SWFs use {DocClass}_{libName}_{type} naming (e.g.
        // HabboRoomContent_room_visualization) which the fixed-name search misses.
        string prefix = DocumentClass + "_";

        foreach (SwfTag tag in _swf.Tags)
        {
            if (tag is not DefineBinaryDataTag { ClassName: not null } bdt || matched.Contains(bdt))
            {
                continue;
            }

            string assetName = ClassNameToAssetName(bdt.ClassName);

            // Only extract if the asset name ends with a known XML type suffix
            string? resolvedType = null;

            foreach (string xmlType in xmlTypes)
            {
                if (assetName == xmlType || assetName.EndsWith("_" + xmlType, StringComparison.Ordinal))
                {
                    resolvedType = xmlType;
                    break;
                }
            }

            if (resolvedType == null || XmlDocuments.ContainsKey(assetName))
            {
                continue;
            }

            try
            {
                string xmlText = Encoding.UTF8.GetString(bdt.BinaryData);
                xmlText = RemoveXmlComments(xmlText);
                XDocument doc = XDocument.Parse(xmlText);

                // Store under the full asset name (e.g. "room_visualization")
                // so the raw blob preserves the original naming
                XmlDocuments[assetName] = doc;
            }
            catch (Exception)
            {
                // Not valid XML, skip
            }
        }
    }

    /// <summary>
    /// Finds the root symbol class name (charId == 0) which identifies the asset.
    /// </summary>
    private string FindDocumentClass()
    {
        foreach (SwfTag tag in _swf.Tags)
        {
            if (tag is SymbolClassTag sct)
            {
                foreach ((ushort charId, string className) in sct.Symbols)
                {
                    if (charId == 0)
                    {
                        return className;
                    }
                }
            }
        }

        return "unknown";
    }

    /// <summary>
    /// Looks up a binary data tag by XML type name.
    /// Tries both CamelCase and snake_case naming patterns.
    /// @see nitro-converter-main/src/swf/HabboAssetSWF.ts getFullClassName
    /// </summary>
    private DefineBinaryDataTag? FindBinaryDataTag(string typeName)
    {
        // Pattern 1: ClassName_typename (e.g. "throne_manifest")
        string pattern1 = $"{DocumentClass}_{typeName}";

        // Pattern 2: ClassName_ClassName_typename (doubled, used in some older SWFs)
        string pattern2 = $"{DocumentClass}_{DocumentClass}_{typeName}";

        foreach (SwfTag tag in _swf.Tags)
        {
            if (tag is DefineBinaryDataTag
                {
                    ClassName: not null,
                } bdt)
            {
                if (bdt.ClassName == pattern1 || bdt.ClassName == pattern2)
                {
                    return bdt;
                }

                // Try snake_case version
                string snakePattern1 = ToSnakeCase(pattern1);
                string snakePattern2 = ToSnakeCase(pattern2);
                if (bdt.ClassName == snakePattern1 || bdt.ClassName == snakePattern2)
                {
                    return bdt;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Converts a Flash class name (e.g. "throne_64_a_0_0") to an asset name.
    /// Strips the document class prefix if present.
    /// </summary>
    private string ClassNameToAssetName(string className)
    {
        // Remove common prefixes like the package path
        int lastDot = className.LastIndexOf('.');
        if (lastDot >= 0)
        {
            className = className[(lastDot + 1)..];
        }

        // If it starts with the document class prefix + '_', strip it
        string prefix = DocumentClass + "_";
        if (className.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            className = className[prefix.Length..];
        }

        return className;
    }

    private static string ToSnakeCase(string input)
    {
        StringBuilder sb = new(input.Length + 4);
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && !char.IsUpper(input[i - 1]))
                {
                    sb.Append('_');
                }
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    private static string RemoveXmlComments(string xml)
    {
        // Remove <!-- ... --> comments
        int start;
        while ((start = xml.IndexOf("<!--", StringComparison.Ordinal)) >= 0)
        {
            int end = xml.IndexOf("-->", start + 4, StringComparison.Ordinal);
            if (end < 0)
            {
                break;
            }
            xml = string.Concat(xml.AsSpan(0, start), xml.AsSpan(end + 3));
        }
        return xml;
    }
}
