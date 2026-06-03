// @see core/window/graphics/class_3725.as

using System;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Utils;
using Vortex.Core.Window.Graphics.Renderer;

namespace Vortex.Core.Window.Graphics;

/// <summary>
/// Skin description XML parser. Builds renderer/layout/template structures from XML.
/// </summary>
/// @see core/window/graphics/class_3725.as
public static class Class3725
{
    public const string WINDOW_STATE_DEFAULT = "default";
    public const string WINDOW_STATE_ACTIVE = "active";
    public const string WINDOW_STATE_FOCUSED = "focused";
    public const string WINDOW_STATE_HOVERING = "hovering";
    public const string WINDOW_STATE_SELECTED = "selected";
    public const string WINDOW_STATE_PRESSED = "pressed";
    public const string WINDOW_STATE_DISABLED = "disabled";
    public const string WINDOW_STATE_LOCKED = "locked";

    /// @see class_3725.as::parseSkinDescription
    public static void ParseSkinDescription
    (
        XElement skinXml, XElement? xmlStates,
        ISkinRenderer renderer, string? rendererName, Func<string, Image?>? assetResolver
    )
    {
        // 1. Parse <variables> → substitution dictionary
        Dictionary<string, string> variables = new();
        XElement? variablesEl = skinXml.Element("variables");

        if (variablesEl != null)
        {
            IEnumerable<XElement> variableElements = variablesEl.Elements("variable");
            Class3540.ParseVariableList(variableElements, variables);
        }

        // 2. Parse <templates> → BitmapSkinTemplate instances
        Dictionary<string, BitmapSkinTemplate> templateDict = new();
        XElement? templatesEl = skinXml.Element("templates");

        if (templatesEl != null)
        {
            ParseTemplateList(renderer, templatesEl, templateDict, variables, assetResolver);
        }

        // 3. Parse <layouts>
        Dictionary<string, SkinLayout> layoutDict = new();
        XElement? layoutsEl = skinXml.Element("layouts");

        if (layoutsEl != null)
        {
            if (rendererName == null)
            {
                ParseLayoutList(renderer, layoutsEl, layoutDict, variables);
            }
            else
            {
                foreach (XElement layoutXml in layoutsEl.Elements("layout"))
                {
                    if (layoutXml.Attribute("name")?.Value != rendererName)
                    {
                        continue;
                    }

                    SkinLayout layout = ParseLayout(layoutXml, variables);
                    layoutDict[rendererName] = layout;
                    renderer.AddLayout(layout);

                    break;
                }
            }
        }

        // 4. Parse <states>
        // If xmlStates has no elements, fall back to skin XML's own <states>
        XElement? statesEl;

        if (xmlStates is { HasElements: true })
        {
            statesEl = xmlStates;
        }
        else
        {
            statesEl = skinXml.Element("states");
        }

        if (statesEl == null)
        {
            return;
        }

        if (rendererName == null)
        {
            ParseRenderStateList(renderer, statesEl, layoutDict, variables);
        }
        else
        {
            foreach (XElement stateXml in statesEl.Elements("state"))
            {
                string stateLayout = ResolveVariable(stateXml.Attribute("layout")?.Value, variables) ?? "";

                if (stateLayout == rendererName)
                {
                    ParseState(renderer, stateXml, layoutDict, variables);
                }
            }
        }
    }

    /// @see class_3725.as::parseLayout
    public static SkinLayout ParseLayout(XElement layoutXml, Dictionary<string, string> variables)
    {
        string name = layoutXml.Attribute("name")?.Value ?? "";
        bool transparent = layoutXml.Attribute("transparent")?.Value == "true";
        string blendMode = layoutXml.Attribute("blendMode")?.Value ?? "";
        SkinLayout layout = new(name, transparent, blendMode);

        XElement? entitiesEl = layoutXml.Element("entities");
        if (entitiesEl != null)
        {
            foreach (XElement entityXml in entitiesEl.Elements("entity"))
            {
                SkinLayoutEntity? entity = ParseLayoutEntity(entityXml, variables);
                if (entity != null)
                {
                    layout.AddChild(entity);
                }
            }
        }

        return layout;
    }

    /// @see class_3725.as::parseLayoutList
    public static void ParseLayoutList
    (
        ISkinRenderer renderer, XElement layoutsXml,
        Dictionary<string, SkinLayout> layoutDict, Dictionary<string, string> variables
    )
    {
        foreach (XElement layoutXml in layoutsXml.Elements("layout"))
        {
            SkinLayout layout = ParseLayout(layoutXml, variables);
            string name = layoutXml.Attribute("name")?.Value ?? "";
            layoutDict[name] = layout;
            renderer.AddLayout(layout);
        }
    }

    /// @see class_3725.as::parseLayoutEntity
    public static SkinLayoutEntity ParseLayoutEntity(XElement entityXml, Dictionary<string, string> variables)
    {
        string? idStr = ResolveVariable(entityXml.Attribute("id")?.Value, variables);
        uint id = string.IsNullOrEmpty(idStr) ? 0 : uint.Parse(idStr);

        string? nameStr = ResolveVariable(entityXml.Attribute("name")?.Value, variables);
        string name = nameStr ?? "";

        SkinLayoutEntity entity = new(id, name);

        string colorizeStr = entityXml.Attribute("colorize")?.Value ?? "";
        entity.Colorize = colorizeStr is "" or "true";

        // Color
        XElement? colorEl = entityXml.Element("color");
        if (colorEl != null)
        {
            string? colorVal = ResolveVariable(colorEl.Value, variables);
            entity.Color = string.IsNullOrEmpty(colorVal) ? 0 : uint.Parse(colorVal);
        }

        // Blend
        XElement? blendEl = entityXml.Element("blend");
        if (blendEl != null)
        {
            string? blendVal = ResolveVariable(blendEl.Value, variables);
            entity.Blend = string.IsNullOrEmpty(blendVal) ? uint.MaxValue : uint.Parse(blendVal);
        }
        else
        {
            entity.Blend = uint.MaxValue;
        }

        // Scale
        XElement? scaleEl = entityXml.Element("scale");
        if (scaleEl != null)
        {
            string? hStr = ResolveVariable(scaleEl.Attribute("horizontal")?.Value, variables);
            entity.ScaleH = ParseScaleType(hStr);

            string? vStr = ResolveVariable(scaleEl.Attribute("vertical")?.Value, variables);
            entity.ScaleV = ParseScaleType(vStr);
        }

        // Region
        XElement? regionEl = entityXml.Element("region");
        if (regionEl != null)
        {
            XElement? rectEl = regionEl.Element("Rectangle");
            if (rectEl != null)
            {
                float x = Class3540.SafeParseFloat(ResolveVariable(rectEl.Attribute("x")?.Value, variables));
                float y = Class3540.SafeParseFloat(ResolveVariable(rectEl.Attribute("y")?.Value, variables));
                float w = Class3540.SafeParseFloat(ResolveVariable(rectEl.Attribute("width")?.Value, variables));
                float h = Class3540.SafeParseFloat(ResolveVariable(rectEl.Attribute("height")?.Value, variables));
                entity.Region = new Rect2(x, y, w, h);
            }
        }

        return entity;
    }

    /// @see class_3725.as::parseLayoutEntityList
    public static void ParseLayoutEntityList
    (
        ISkinRenderer renderer, SkinLayout layout,
        IEnumerable<XElement> entityElements, Dictionary<string, string> variables
    )
    {
        foreach (XElement entityXml in entityElements)
        {
            SkinLayoutEntity? entity = ParseLayoutEntity(entityXml, variables);
            if (entity != null)
            {
                layout.AddChild(entity);
            }
        }
    }

    /// @see class_3725.as::parseTemplateList
    public static void ParseTemplateList
    (
        ISkinRenderer renderer, XElement templatesXml,
        Dictionary<string, BitmapSkinTemplate> templateDict,
        Dictionary<string, string> variables, Func<string, Image?>? assetResolver
    )
    {
        foreach (XElement templateXml in templatesXml.Elements("template"))
        {
            string name = ResolveVariable(templateXml.Attribute("name")?.Value, variables) ?? "";
            string assetName = ResolveVariable(templateXml.Attribute("asset")?.Value, variables) ?? "";

            Image? asset = assetResolver?.Invoke(assetName);

            BitmapSkinTemplate template = new(name, asset);

            XElement? entitiesEl = templateXml.Element("entities");
            if (entitiesEl != null)
            {
                ParseTemplateEntityList(renderer, template, entitiesEl.Elements("entity"), variables);
            }

            templateDict[name] = template;
            renderer.AddTemplate(template);
        }
    }

    /// @see class_3725.as::parseTemplateEntityList
    public static void ParseTemplateEntityList
    (
        ISkinRenderer renderer, BitmapSkinTemplate template,
        IEnumerable<XElement> entityElements, Dictionary<string, string> variables
    )
    {
        foreach (XElement entityXml in entityElements)
        {
            string name = ResolveVariable(entityXml.Attribute("name")?.Value, variables) ?? "";
            string type = ResolveVariable(entityXml.Attribute("type")?.Value, variables) ?? "";
            string? idStr = ResolveVariable(entityXml.Attribute("id")?.Value, variables);
            uint id = string.IsNullOrEmpty(idStr) ? 0 : uint.Parse(idStr);

            Rect2 region = default;
            XElement? regionEl = entityXml.Element("region");
            if (regionEl != null)
            {
                XElement? rectEl = regionEl.Element("Rectangle");
                if (rectEl != null)
                {
                    float x = Class3540.SafeParseFloat(ResolveVariable(rectEl.Attribute("x")?.Value, variables));
                    float y = Class3540.SafeParseFloat(ResolveVariable(rectEl.Attribute("y")?.Value, variables));
                    float w = Class3540.SafeParseFloat(ResolveVariable(rectEl.Attribute("width")?.Value, variables));
                    float h = Class3540.SafeParseFloat(ResolveVariable(rectEl.Attribute("height")?.Value, variables));
                    region = new Rect2(x, y, w, h);
                }
            }

            template.AddChild(new BitmapSkinTemplateEntity(name, type, id, region));
        }
    }

    /// @see class_3725.as::parseState
    public static void ParseState
    (
        ISkinRenderer renderer, XElement stateXml,
        Dictionary<string, SkinLayout> layoutDict, Dictionary<string, string> variables
    )
    {
        string stateName = ResolveVariable(stateXml.Attribute("name")?.Value, variables) ?? "";
        string layoutName = ResolveVariable(stateXml.Attribute("layout")?.Value, variables) ?? "";
        string templateName = ResolveVariable(stateXml.Attribute("template")?.Value, variables) ?? "";

        if (!layoutDict.ContainsKey(layoutName))
        {
            throw new Exception($"State {stateName} has invalid layout reference {layoutName}!");
        }

        uint stateValue = ParseStateName(stateName);

        renderer.RegisterLayoutForRenderState(stateValue, layoutName);
        renderer.RegisterTemplateForRenderState(stateValue, templateName);
    }

    /// @see class_3725.as::parseRenderStateList
    public static void ParseRenderStateList
    (
        ISkinRenderer renderer, XElement statesXml,
        Dictionary<string, SkinLayout> layoutDict, Dictionary<string, string> variables
    )
    {
        foreach (XElement stateXml in statesXml.Elements("state"))
        {
            ParseState(renderer, stateXml, layoutDict, variables);
        }
    }

    /// @see class_3725.as (state name → uint mapping)
    public static uint ParseStateName(string stateName)
    {
        return stateName switch
        {
            "default" => 0,
            "active" => 1,
            "focused" => 2,
            "hovering" => 4,
            "selected" => 8,
            "pressed" => 16,
            "disabled" => 32,
            "locked" => 64,
            _ => throw new Exception($"Unknown window state: \"{stateName}\"!"),
        };
    }

    private static uint ParseScaleType(string? value)
    {
        return value?.ToLowerInvariant() switch
        {
            "fixed" => SkinLayoutEntity.SCALE_TYPE_FIXED,
            "move" => SkinLayoutEntity.SCALE_TYPE_MOVE,
            "strech" => SkinLayoutEntity.SCALE_TYPE_STRECH,
            "tiled" => SkinLayoutEntity.SCALE_TYPE_TILED,
            "center" => SkinLayoutEntity.SCALE_TYPE_CENTER,
            _ => SkinLayoutEntity.SCALE_TYPE_FIXED,
        };
    }

    private static string? ResolveVariable(string? value, Dictionary<string, string> variables)
    {
        if (value == null)
        {
            return null;
        }
        if (value.Length > 0 && value[0] == '$')
        {
            string varName = value[1..];
            return variables.GetValueOrDefault(varName, value);
        }
        return value;
    }
}
