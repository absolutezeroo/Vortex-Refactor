using System.Globalization;
using System.Xml.Linq;

using Vortex.Bundle.Data;
using Vortex.Bundle.Converter.Convert;

namespace Vortex.Bundle.Converter.Extract;

/// <summary>
/// Parses palette data from assets.xml (palettes are embedded alongside assets).
/// @see nitro-converter-main/src/common/mapping/mappers/asset/AssetMapper.ts (palette section)
/// </summary>
public static class PaletteXmlParser
{
    public static PaletteData[] Parse(XDocument doc, StringTableBuilder strings)
    {
        List<PaletteData> result = new();
        XElement? root = doc.Root;
        if (root == null)
        {
            return [];
        }

        // Palettes are found under <assets><palette> or <object><palettes><palette>
        XElement palettesEl = root.Element("palettes")
                              ?? root.Element("assets")?.Element("palettes")
                              ?? root;

        foreach (XElement palEl in palettesEl.Elements("palette"))
        {
            PaletteData pal = new()
            {
                Id = ParseUInt32(palEl.Attribute("id")),
                SourceIndex = OptString(palEl, "source", strings),
                Master = ParseBool(palEl.Attribute("master")),
                Breed = ParseUInt16(palEl.Attribute("breed")),
                ColorTag = ParseUInt16(palEl.Attribute("colorTag")),
                Color1Index = OptString(palEl, "color1", strings),
                Color2Index = OptString(palEl, "color2", strings),
            };

            // Tags
            List<uint> tags = new();
            foreach (XElement tagEl in palEl.Elements("tag"))
            {
                string? tagName = tagEl.Value;
                if (tagName != null)
                {
                    tags.Add(strings.Add(tagName));
                }
            }
            pal.TagIndices = tags.ToArray();

            // Colors (RGB entries)
            List<PaletteColor> colors = new();
            foreach (XElement cEl in palEl.Elements("color"))
            {
                string? hex = cEl.Value ?? (string?)cEl.Attribute("value");
                if (hex == null)
                {
                    continue;
                }
                hex = hex.TrimStart('#');

                if (hex.Length >= 6 && uint.TryParse(hex[..6], NumberStyles.HexNumber, null, out uint rgb))
                {
                    colors.Add(new PaletteColor
                    {
                        R = (byte)((rgb >> 16) & 0xFF), G = (byte)((rgb >> 8) & 0xFF), B = (byte)(rgb & 0xFF),
                    });
                }
            }
            pal.Colors = colors.ToArray();

            result.Add(pal);
        }

        return result.ToArray();
    }

    private static uint OptString(XElement el, string attrName, StringTableBuilder strings)
    {
        string? val = (string?)el.Attribute(attrName);
        return val != null ? strings.Add(val) : VortexBundleFormat.NULL_STRING;
    }

    private static ushort ParseUInt16(XAttribute? attr)
    {
        return attr != null && ushort.TryParse(attr.Value, out ushort v) ? v : (ushort)0;
    }

    private static uint ParseUInt32(XAttribute? attr)
    {
        return attr != null && uint.TryParse(attr.Value, out uint v) ? v : 0u;
    }

    private static bool ParseBool(XAttribute? attr)
    {
        if (attr == null)
        {
            return false;
        }
        string val = attr.Value.ToLowerInvariant();
        return val is "1" or "true";
    }
}
