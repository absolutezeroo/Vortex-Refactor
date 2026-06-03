using System.Xml.Linq;

using Vortex.Bundle.Data;
using Vortex.Bundle.Converter.Convert;

namespace Vortex.Bundle.Converter.Extract;

/// <summary>
/// Parses assets.xml from a Habbo SWF into AssetEntry[] and AliasEntry[].
/// @see nitro-converter-main/src/common/mapping/mappers/asset/AssetMapper.ts
/// </summary>
public static class AssetsXmlParser
{
    public static (AssetEntry[] Assets, AliasEntry[] Aliases) Parse(XDocument doc, StringTableBuilder strings)
    {
        List<AssetEntry> assets = new();
        List<AliasEntry> aliases = new();

        XElement? root = doc.Root;
        if (root == null)
        {
            return ([], []);
        }

        // <assets> or <object><assets> or <manifest><library><assets>
        XElement assetsEl = root.Name.LocalName == "assets"
            ? root
            : root.Element("assets")
              ?? root.Element("library")?.Element("assets")
              ?? root;

        foreach (XElement assetEl in assetsEl.Elements("asset"))
        {
            string? name = (string?)assetEl.Attribute("name");
            if (name == null)
            {
                continue;
            }

            // Skip shadow and 32px icon sprites (nitro convention)
            if (name.StartsWith("sh_") || name.Contains("_32_"))
            {
                continue;
            }

            string? source = (string?)assetEl.Attribute("source");

            // Offsets: try x/y attributes first (furniture format),
            // then <param key="offset" value="x,y" /> children (avatar/manifest format)
            int x = (int?)assetEl.Attribute("x") ?? 0;
            int y = (int?)assetEl.Attribute("y") ?? 0;
            if (x == 0 && y == 0)
            {
                (x, y) = ParseParamOffset(assetEl);
            }

            bool flipH = ParseBool(assetEl.Attribute("flipH"));
            bool flipV = ParseBool(assetEl.Attribute("flipV"));
            bool usesPalette = ParseBool(assetEl.Attribute("usesPalette"));

            // If source is present, this is an alias (referencing another asset)
            if (source != null)
            {
                byte aliasFlags = 0;
                if (flipH)
                {
                    aliasFlags |= AliasEntry.FLAG_FLIP_H;
                }
                if (flipV)
                {
                    aliasFlags |= AliasEntry.FLAG_FLIP_V;
                }

                aliases.Add(new AliasEntry
                {
                    NameIndex = strings.Add(name), LinkIndex = strings.Add(source), Flags = aliasFlags,
                });
            }
            else
            {
                byte flags = 0;
                if (flipH)
                {
                    flags |= AssetEntry.FLAG_FLIP_H;
                }
                if (flipV)
                {
                    flags |= AssetEntry.FLAG_FLIP_V;
                }
                if (usesPalette)
                {
                    flags |= AssetEntry.FLAG_USES_PALETTE;
                }

                assets.Add(new AssetEntry
                {
                    NameIndex = strings.Add(name),
                    OffsetX = (short)x,
                    OffsetY = (short)y,
                    SourceIndex = VortexBundleFormat.NULL_STRING,
                    Flags = flags,
                });
            }
        }

        return (assets.ToArray(), aliases.ToArray());
    }

    /// <summary>
    /// Parses offset from avatar manifest format: &lt;param key="offset" value="-33,-31" /&gt;
    /// </summary>
    private static (int X, int Y) ParseParamOffset(XElement assetEl)
    {
        foreach (XElement param in assetEl.Elements("param"))
        {
            if ((string?)param.Attribute("key") == "offset")
            {
                string? value = (string?)param.Attribute("value");
                if (value != null)
                {
                    string[] parts = value.Split(',');
                    if (parts.Length == 2
                        && int.TryParse(parts[0], out int px)
                        && int.TryParse(parts[1], out int py))
                    {
                        return (px, py);
                    }
                }
            }
        }
        return (0, 0);
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
