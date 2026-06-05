// @see habbo/window/utils/class_3503.as (asset resolution logic)

using System;
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

        string fileName = ResolveImageFileName(assetName);

        string[] candidates =
        [
            fileName,
            Path.GetFileName(fileName),
        ];

        Image? image = null;

        foreach (string candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            string resPath = candidate.StartsWith("res://", StringComparison.Ordinal)
                ? candidate
                : $"res://assets/images/{candidate}";

            if (ResourceLoader.Exists(resPath))
            {
                Texture2D? texture = ResourceLoader.Load<Texture2D>(resPath);

                if (texture != null)
                {
                    image = texture.GetImage();

                    if (image != null)
                    {
                        break;
                    }
                }
            }

            string? absPath = ProjectSettings.GlobalizePath(resPath);

            if (string.IsNullOrEmpty(absPath) || !File.Exists(absPath))
            {
                continue;
            }

            image = new Image();
            Error err = image.Load(absPath);

            if (err != Error.Ok)
            {
                GD.PrintErr($"[HabboAssetResolver] Failed to load image: {absPath} (error: {err})");
                image = null;
                continue;
            }

            break;
        }

        _imageCache[assetName] = image;
        return image;
    }

    private static string ResolveImageFileName(string assetName)
    {
        string fileName = assetName.Replace('\\', '/');

        fileName = fileName.Replace("${image.library.url}", string.Empty, StringComparison.Ordinal);

        if (Uri.TryCreate(fileName, UriKind.Absolute, out Uri? uri) &&
            (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            fileName = uri.AbsolutePath.TrimStart('/');
        }

        if (fileName.EndsWith("_png", StringComparison.Ordinal))
        {
            fileName = fileName[..^4] + ".png";
        }
        else if (!fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            fileName += ".png";
        }

        return fileName;
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
