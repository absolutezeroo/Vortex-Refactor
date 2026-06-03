// @see habbo/window/utils/class_3503.as

using System;
using System.Xml.Linq;

using Godot;

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

    /// @see habbo/window/utils/class_3503.as::parse
    public static void Parse(XElement elementDescription, SkinContainer skinContainer)
    {
        // 1. Build lookup tables
        Dictionary<string, uint> nameToType = new();
        Dictionary<uint, string> typeToName = new();

        TypeCodeTable.FillTables(nameToType, typeToName);

        Dictionary<string, uint> nameToState = new();
        Dictionary<uint, string> stateToName = new();

        StateCodeTable.FillTables(nameToState, stateToName);

        // 2. Renderer type → factory map
        Dictionary<string, Func<string?, ISkinRenderer>> rendererFactories = new()
        {
            [RENDERER_TYPE_SKIN] = name => new BitmapSkinRenderer(name ?? ""),
            [RENDERER_TYPE_BITMAP] = name => new BitmapDataRenderer(name ?? ""),
            [RENDERER_TYPE_FILL] = name => new FillSkinRenderer(name ?? ""),
            [RENDERER_TYPE_TEXT] = name => new TextSkinRenderer(name ?? ""),
            [RENDERER_TYPE_LABEL] = name => new LabelRenderer(name ?? ""),
            [RENDERER_TYPE_SHAPE] = name => new ShapeSkinRenderer(name ?? ""),
            [RENDERER_TYPE_UNKNOWN] = name => new SkinRenderer(name ?? ""),
            [RENDERER_TYPE_NULL] = name => new NullSkinRenderer(name ?? ""),
        };

        // 3. Iterate <window> elements
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
            if (!nameToType.TryGetValue(typeName, out uint typeId))
            {
                continue;
            }

            if (!uint.TryParse(styleStr, out uint style))
            {
                style = 0;
            }

            // Create renderer from factory
            if (!rendererFactories.TryGetValue(rendererType, out Func<string?, ISkinRenderer>? factory))
            {
                continue;
            }

            ISkinRenderer renderer = factory(layoutName);

            // Load skin XML asset and parse via renderer
            XElement? skinXml = string.IsNullOrEmpty(assetName) ? null : HabboAssetResolver.LoadXmlAsset(assetName);
            XElement? inlineStatesForParse = inlineStates;

            renderer.Parse(skinXml, inlineStatesForParse, HabboAssetResolver.LoadImageAsset);

            // Build DefaultAttStruct
            DefaultAttStruct defaults = new()
            {
                Threshold = ParseUintAttr(windowEl, "treshold", 10),
                Background = windowEl.Attribute("background")?.Value == "true",
                Blend = ParseFloatAttr(windowEl, "blend", 1.0f),
                Color = ParseUintAttr(windowEl, "color", 0xFFFFFF),
                WidthMin = ParseIntAttr(windowEl, "width_min", int.MinValue),
                WidthMax = ParseIntAttr(windowEl, "width_max", int.MaxValue),
                HeightMin = ParseIntAttr(windowEl, "height_min", int.MinValue),
                HeightMax = ParseIntAttr(windowEl, "height_max", int.MaxValue),
            };

            // Resolve optional window_layout XML
            XElement? windowLayoutXml = null;

            if (!string.IsNullOrEmpty(windowLayoutName))
            {
                windowLayoutXml = HabboAssetResolver.LoadXmlAsset(windowLayoutName);
            }

            // Register in SkinContainer
            skinContainer.AddSkinRenderer(typeId, style, intent, renderer, windowLayoutXml, defaults);
            parsedCount++;
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
