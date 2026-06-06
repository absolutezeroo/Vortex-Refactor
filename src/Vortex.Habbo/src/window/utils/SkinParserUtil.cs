// @see habbo/window/utils/class_3503.as

using System;
using System.Collections.Frozen;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Window.Graphics;
using Vortex.Core.Window.Graphics.Renderer;
using Vortex.Core.Window.Utils;

namespace Vortex.Habbo.Window.Utils;

/// <summary>
/// Main skin loader. Parses habbo_element_description.xml and populates a SkinContainer
/// with renderer instances, templates, layouts, and default attributes.
/// </summary>
/// @see habbo/window/utils/class_3503.as
public static class SkinParserUtil
{
    private const string RENDERER_TYPE_SKIN = "skin";
    private const string RENDERER_TYPE_BITMAP = "bitmap";
    private const string RENDERER_TYPE_FILL = "fill";
    private const string RENDERER_TYPE_TEXT = "text";
    private const string RENDERER_TYPE_LABEL = "label";
    private const string RENDERER_TYPE_SHAPE = "shape";
    private const string RENDERER_TYPE_UNKNOWN = "unknown";
    private const string RENDERER_TYPE_NULL = "null";

    // Build-once, read-many dispatch tables. FrozenDictionary gives faster lookups on hot paths
    // (GetSkinRendererByTypeAndStyle is called every skin apply). See docs/MODERNIZATION_CATALOG.md.
    private static readonly FrozenDictionary<string, uint> _nameToType;
    private static readonly FrozenDictionary<string, Func<string?, ISkinRenderer>> _rendererFactories;

    static SkinParserUtil()
    {
        Dictionary<string, uint> nameToType = new();
        TypeCodeTable.FillTables(nameToType);
        _nameToType = nameToType.ToFrozenDictionary();

        // TODO(as3-port): nameToState is built by class_3503.as but inline state parsing
        // is not ported yet; add when skin state XML attributes are implemented.

        _rendererFactories = new Dictionary<string, Func<string?, ISkinRenderer>>
        {
            [RENDERER_TYPE_SKIN]     = name => new BitmapSkinRenderer(name ?? ""),
            [RENDERER_TYPE_BITMAP]   = name => new BitmapDataRenderer(name ?? ""),
            [RENDERER_TYPE_FILL]     = name => new FillSkinRenderer(name ?? ""),
            [RENDERER_TYPE_TEXT]     = name => new TextSkinRenderer(name ?? ""),
            [RENDERER_TYPE_LABEL]    = name => new LabelRenderer(name ?? ""),
            [RENDERER_TYPE_SHAPE]    = name => new ShapeSkinRenderer(name ?? ""),
            [RENDERER_TYPE_UNKNOWN]  = name => new SkinRenderer(name ?? ""),
            [RENDERER_TYPE_NULL]     = name => new NullSkinRenderer(name ?? ""),
        }.ToFrozenDictionary();
    }

    /// @see habbo/window/utils/class_3503.as::parse
    public static void Parse(XElement elementDescription, IAssetLibrary assets, SkinContainer skinContainer)
    {
        // @see class_3503.as — image resolver wraps param2:IAssetLibrary for template bitmap lookups
        Func<string, Image?> imageResolver = name => assets.GetAssetByName(name)?.Content as Image;

        // @see class_3503.as — _loc22_: tracks used skin assets for disposal after parsing
        List<IAsset> assetsToDispose = [];

        // Iterate <window> elements
        IEnumerable<XElement> windows = elementDescription.Elements("window");
        uint parsedCount = 0;

        foreach (XElement windowEl in windows)
        {
            string typeName = windowEl.Attribute("type")?.Value ?? "";
            string intent = windowEl.Attribute("intent")?.Value ?? "";
            string styleStr = windowEl.Attribute("style")?.Value ?? "0";
            string assetName = windowEl.Attribute("asset")?.Value ?? "";
            string? layoutName = windowEl.Attribute("layout")?.Value;
            string? windowLayoutName = windowEl.Attribute("window_layout")?.Value;
            string rendererType = windowEl.Attribute("renderer")?.Value ?? "";
            XElement? inlineStates = windowEl.Element("states");

            // Resolve type ID
            if (!_nameToType.TryGetValue(typeName, out uint typeId))
            {
                continue;
            }

            if (!uint.TryParse(styleStr, out uint style))
            {
                style = 0;
            }

            float blend = ParseFloatAttr(windowEl, "blend", 1.0f);
            uint color = ParseUintAttr(windowEl, "color", 0xFFFFFF);

            // Create renderer from factory
            if (!_rendererFactories.TryGetValue(rendererType, out Func<string?, ISkinRenderer>? factory))
            {
                continue;
            }

            ISkinRenderer renderer = factory(layoutName);

            // @see class_3503.as — _loc28_ = param2.getAssetByName(_loc27_); renderer.parse(_loc28_, ...)
            IAsset? skinAsset = string.IsNullOrEmpty(assetName) ? null : assets.GetAssetByName(assetName);

            // AS3 calls renderer.parse(_loc28_, ...) with no null guard; a missing required asset
            // would throw inside parse(). Fail loudly here to surface the error at the right level.
            // Renderers with no asset (empty assetName) are intentional and remain silent.
            if (!string.IsNullOrEmpty(assetName) && skinAsset == null)
            {
                throw new InvalidOperationException(
                    $"SkinParserUtil: required skin asset '{assetName}' not found " +
                    $"(type='{typeName}' renderer='{rendererType}')");
            }

            if (skinAsset != null && !assetsToDispose.Contains(skinAsset))
            {
                assetsToDispose.Add(skinAsset);
            }

            renderer.Parse(skinAsset?.Content as XElement, inlineStates, imageResolver);

            // Build DefaultAttStruct
            DefaultAttStruct defaults = new()
            {
                Threshold = ParseUintAttr(windowEl, "treshold", 10),
                Background = windowEl.Attribute("background")?.Value == "true",
                Blend = blend,
                Color = color,
                WidthMin = ParseIntAttr(windowEl, "width_min", int.MinValue),
                WidthMax = ParseIntAttr(windowEl, "width_max", int.MaxValue),
                HeightMin = ParseIntAttr(windowEl, "height_min", int.MinValue),
                HeightMax = ParseIntAttr(windowEl, "height_max", int.MaxValue),
            };

            // @see class_3503.as — param2.getAssetByName(_loc25_)?.content as XML
            XElement? windowLayoutXml = null;

            if (!string.IsNullOrEmpty(windowLayoutName))
            {
                windowLayoutXml = assets.GetAssetByName(windowLayoutName)?.Content as XElement;
            }

            // Register in SkinContainer
            skinContainer.AddSkinRenderer(typeId, style, intent, renderer, windowLayoutXml, defaults);
            parsedCount++;
        }

        // @see class_3503.as — dispose used skin assets after parsing
        foreach (IAsset a in assetsToDispose)
        {
            a.Dispose();
        }

        GD.Print($"[SkinParserUtil] Parsed {parsedCount} skin renderer entries.");
    }

    private static uint ParseUintAttr(XElement el, string attr, uint defaultValue)
    {
        string? val = el.Attribute(attr)?.Value;

        if (val == null)
        {
            return defaultValue;
        }

        // Handle hex values like "0xff418db0"
        if (val.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return uint.TryParse(val[2..], System.Globalization.NumberStyles.HexNumber, null, out uint hex) ? hex : defaultValue;
        }

        return uint.TryParse(val, out uint result) ? result : defaultValue;
    }

    private static float ParseFloatAttr(XElement el, string attr, float defaultValue)
    {
        string? val = el.Attribute(attr)?.Value;

        if (val == null)
        {
            return defaultValue;
        }

        return float.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture,
            out float result) ? result : defaultValue;
    }

    private static int ParseIntAttr(XElement el, string attr, int defaultValue)
    {
        string? val = el.Attribute(attr)?.Value;

        if (val == null)
        {
            return defaultValue;
        }

        return int.TryParse(val, out int result) ? result : defaultValue;
    }
}
