// @see core/window/graphics/SkinContainer.as

using System;
using System.Linq;
using System.Xml.Linq;

using Vortex.Core.Utils;
using Vortex.Core.Window.Graphics.Renderer;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Graphics;

/// @see core/window/graphics/SkinContainer.as
public class SkinContainer : ISkinContainer, IDisposable
{
    private const uint MAX_STYLE_COUNT = 100;

    protected static readonly int[] StatesByRenderPriority = [64, 32, 16, 8, 4, 2, 1, 0];

    private Dictionary<uint, ISkinRenderer?[]> _renderers;
    private Dictionary<uint, DefaultAttStruct?[]> _defaults;
    private Dictionary<uint, XElement?[]> _layouts;
    private Dictionary<uint, string?[]> _intents;

    /// @see SkinContainer.as::SkinContainer
    public SkinContainer()
    {
        _renderers = new Dictionary<uint, ISkinRenderer?[]>();
        _defaults = new Dictionary<uint, DefaultAttStruct?[]>();
        _layouts = new Dictionary<uint, XElement?[]>();
        _intents = new Dictionary<uint, string?[]>();
    }

    public bool Disposed { get; private set; }

    /// @see SkinContainer.as::dispose
    public void Dispose()
    {
        _renderers = null!;
        _defaults = null!;
        _layouts = null!;
        _intents = null!;
        Disposed = true;
    }

    /// @see SkinContainer.as::addSkinRenderer
    public void AddSkinRenderer
    (
        uint type,
        uint style,
        string? intent,
        ISkinRenderer renderer,
        XElement? layoutXml,
        DefaultAttStruct? defaults
    )
    {
        // AS3 arrays auto-grow on index assignment; ensure capacity here.
        int requiredLength = (int)style + 1;

        Class3540.EnsureArray(_renderers, type, requiredLength, (int)MAX_STYLE_COUNT);
        _renderers[type][style] = renderer;

        Class3540.EnsureArray(_defaults, type, requiredLength, (int)MAX_STYLE_COUNT);
        _defaults[type][style] = defaults;

        Class3540.EnsureArray(_layouts, type, requiredLength, (int)MAX_STYLE_COUNT);
        _layouts[type][style] = layoutXml;

        Class3540.EnsureArray(_intents, type, requiredLength, (int)MAX_STYLE_COUNT);
        _intents[type][style] = !string.IsNullOrEmpty(intent) ? intent : style.ToString();
    }

    /// @see SkinContainer.as::getSkinRendererByTypeAndStyle
    public ISkinRenderer? GetSkinRendererByTypeAndStyle(uint type, uint style)
    {
        if (!_renderers.TryGetValue(type, out ISkinRenderer?[]? array))
        {
            return null;
        }
        ISkinRenderer? renderer = style < array.Length ? array[style] : null;
        if (renderer == null && style != 0)
        {
            renderer = array[0];
        }
        return renderer;
    }

    /// @see SkinContainer.as::skinRendererExists
    public bool SkinRendererExists(uint type, uint style)
    {
        return _renderers.TryGetValue(type, out ISkinRenderer?[]? array)
               && style < array.Length
               && array[style] != null;
    }

    /// @see SkinContainer.as::getDefaultAttributesByTypeAndStyle
    public DefaultAttStruct? GetDefaultAttributesByTypeAndStyle(uint type, uint style)
    {
        if (!_defaults.TryGetValue(type, out DefaultAttStruct?[]? array))
        {
            return null;
        }
        DefaultAttStruct? defaults = style < array.Length ? array[style] : null;
        if (defaults == null && style != 0)
        {
            defaults = array[0];
        }
        return defaults;
    }

    /// @see SkinContainer.as::getWindowLayoutByTypeAndStyle
    public XElement? GetWindowLayoutByTypeAndStyle(uint type, uint style)
    {
        if (!_layouts.TryGetValue(type, out XElement?[]? array))
        {
            return null;
        }

        XElement? layout = (style < array.Length ? array[style] : null) ?? array[0];

        return layout;
    }

    /// @see SkinContainer.as::getIntentByTypeAndStyle
    public string? GetIntentByTypeAndStyle(uint type, uint style)
    {
        if (!_intents.TryGetValue(type, out string?[]? array))
        {
            return null;
        }
        return style < array.Length ? array[style] : null;
    }

    /// @see SkinContainer.as::getTheActualState
    public uint GetTheActualState(uint type, uint style, uint state)
    {
        ISkinRenderer? renderer = GetSkinRendererByTypeAndStyle(type, style);

        if (renderer != null)
        {
            return (
                from priority in StatesByRenderPriority
                where (state & (uint)priority) == (uint)priority
                where renderer.IsStateDrawable((uint)priority)
                select (uint)priority).FirstOrDefault();
        }

        return 0;
    }
}
