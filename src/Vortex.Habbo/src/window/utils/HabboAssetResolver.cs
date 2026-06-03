// @see habbo/window/utils/class_3503.as (asset resolution logic)

using System.IO;
using System.Xml.Linq;

using Godot;

namespace Vortex.Habbo.Window.Utils;

/// <summary>
/// Resolves AS3-style asset names to filesystem paths and loads images/XML.
/// Convention: "habbo_blue_skin_png" → "assets/images/habbo_blue_skin.png"
///             "habbo_skin_frame_xml" → "data/layouts/habbo_skin_frame.xml"
/// </summary>
public static class HabboAssetResolver
{
    private static readonly Dictionary<string, Image?> _imageCache = new();

    /// <summary>
    /// Loads a PNG image asset by AS3-style name (e.g. "habbo_blue_skin_png").
    /// </summary>
    public static Image? LoadImageAsset(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            return null;
        }

        if (_imageCache.TryGetValue(assetName, out Image? cached))
        {
            return cached;
        }

        // Strip "_png" suffix and resolve to filesystem path
        string fileName = assetName;

        if (fileName.EndsWith("_png"))
        {
            fileName = fileName[..^4];
        }

        string resPath = $"res://assets/images/{fileName}.png";
        string? absPath = ProjectSettings.GlobalizePath(resPath);

        Image? image = null;

        if (File.Exists(absPath))
        {
            image = new Image();
            Error err = image.Load(absPath);

            if (err != Error.Ok)
            {
                GD.PrintErr($"[HabboAssetResolver] Failed to load image: {absPath} (error: {err})");
                image = null;
            }
        }

        _imageCache[assetName] = image;
        return image;
    }

    /// <summary>
    /// Loads a text asset by AS3-style name (e.g. "text_styles_css").
    /// Maps "text_styles_css" → "res://data/text_styles.css".
    /// </summary>
    public static string? LoadTextAsset(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            return null;
        }

        string fileName = assetName;

        if (fileName.EndsWith("_css"))
        {
            fileName = fileName[..^4];
        }

        string resPath = $"res://data/{fileName}.css";
        string? absPath = ProjectSettings.GlobalizePath(resPath);

        if (!File.Exists(absPath))
        {
            return null;
        }

        try
        {
            return File.ReadAllText(absPath);
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"[HabboAssetResolver] Failed to load text asset: {absPath} — {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Loads an XML asset by AS3-style name (e.g. "habbo_skin_frame_xml").
    /// </summary>
    public static XElement? LoadXmlAsset(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            return null;
        }

        // Strip "_xml" suffix and resolve to filesystem path
        string fileName = assetName;

        if (fileName.EndsWith("_xml"))
        {
            fileName = fileName[..^4];
        }

        string resPath = $"res://data/layouts/{fileName}.xml";
        string? absPath = ProjectSettings.GlobalizePath(resPath);

        if (!File.Exists(absPath))
        {
            return null;
        }

        try
        {
            return XElement.Load(absPath);
        }
        catch (System.Xml.XmlException e)
        {
            GD.PrintErr($"[HabboAssetResolver] Malformed XML: {absPath} — {e.Message}");
            return null;
        }
    }
}
