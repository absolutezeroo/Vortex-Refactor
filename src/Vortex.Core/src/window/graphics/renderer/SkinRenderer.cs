// @see core/window/graphics/renderer/SkinRenderer.as

using System;
using System.Linq;

using Godot;

namespace Vortex.Core.Window.Graphics.Renderer;

/// @see core/window/graphics/renderer/SkinRenderer.as
public class SkinRenderer : ISkinRenderer, IDisposable
{
    protected static readonly Dictionary<string, (int x, int y)> ETCHING_POSITION = new()
    {
        ["top-left"] = (-1, -1),
        ["top"] = (0, -1),
        ["top-right"] = (1, -1),
        ["left"] = (-1, 0),
        ["right"] = (1, 0),
        ["bottom-left"] = (-1, 1),
        ["bottom"] = (0, 1),
        ["bottom-right"] = (1, 1),
    };

    protected Dictionary<string, ISkinTemplate> _templatesByName;
    protected Dictionary<uint, ISkinTemplate> _stateToTemplate;
    protected Dictionary<string, ISkinLayout> _layoutsByName;
    protected Dictionary<uint, ISkinLayout> _stateToLayout;

    /// @see SkinRenderer.as::SkinRenderer
    public SkinRenderer(string name)
    {
        Name = name;
        _templatesByName = new Dictionary<string, ISkinTemplate>();
        _stateToTemplate = new Dictionary<uint, ISkinTemplate>();
        _layoutsByName = new Dictionary<string, ISkinLayout>();
        _stateToLayout = new Dictionary<uint, ISkinLayout>();
    }

    public string Name { get; }

    public bool Disposed { get; }

    /// @see SkinRenderer.as::parse
    public virtual void Parse(System.Xml.Linq.XElement? skinXml, System.Xml.Linq.XElement? xmlStates, Func<string, Image?>? assetResolver)
    {
    }

    /// @see SkinRenderer.as::dispose
    public virtual void Dispose()
    {
        if (Disposed)
        {
            return;
        }

        foreach (ISkinLayout layout in _layoutsByName.Values)
        {
            layout.Dispose();
        }

        _layoutsByName = null!;
        _stateToLayout = null!;

        foreach (ISkinTemplate template in _templatesByName.Values)
        {
            template.Dispose();
        }

        _templatesByName = null!;
        _stateToTemplate = null!;
    }

    /// @see SkinRenderer.as::draw
    public virtual void Draw(IWindow window, Image buffer, Rect2 region, uint state, bool flag) { }

    /// @see SkinRenderer.as::isStateDrawable
    public virtual bool IsStateDrawable(uint state)
    {
        return false;
    }

    /// @see SkinRenderer.as::getLayoutByState
    public ISkinLayout? GetLayoutByState(uint state)
    {
        _stateToLayout.TryGetValue(state, out ISkinLayout? layout);

        return layout;
    }

    /// @see SkinRenderer.as::registerLayoutForRenderState
    public void RegisterLayoutForRenderState(uint state, string layoutName)
    {
        if (!_layoutsByName.TryGetValue(layoutName, out ISkinLayout? layout))
        {
            throw new Exception($"Layout \"{layoutName}\" not found in renderer!");
        }

        _stateToLayout[state] = layout;
    }

    /// @see SkinRenderer.as::removeLayoutFromRenderState
    public void RemoveLayoutFromRenderState(uint state)
    {
        _stateToLayout.Remove(state);
    }

    /// @see SkinRenderer.as::hasLayoutForState
    public bool HasLayoutForState(uint state)
    {
        return _stateToLayout.ContainsKey(state);
    }

    /// @see SkinRenderer.as::getTemplateByState
    public ISkinTemplate? GetTemplateByState(uint state)
    {
        _stateToTemplate.TryGetValue(state, out ISkinTemplate? template);

        return template;
    }

    /// @see SkinRenderer.as::registerTemplateForRenderState
    public void RegisterTemplateForRenderState(uint state, string templateName)
    {
        if (!_templatesByName.TryGetValue(templateName, out ISkinTemplate? template))
        {
            throw new Exception($"Template \"{templateName}\" not found in renderer!");
        }

        _stateToTemplate[state] = template;
    }

    /// @see SkinRenderer.as::removeTemplateFromRenderState
    public void RemoveTemplateFromRenderState(uint state)
    {
        _stateToTemplate.Remove(state);
    }

    /// @see SkinRenderer.as::hasTemplateForState
    public bool HasTemplateForState(uint state)
    {
        return _stateToTemplate.ContainsKey(state);
    }

    /// @see SkinRenderer.as::addLayout
    public ISkinLayout AddLayout(ISkinLayout layout)
    {
        _layoutsByName[layout.Name] = layout;

        return layout;
    }

    /// @see SkinRenderer.as::getLayoutByName
    public ISkinLayout? GetLayoutByName(string name)
    {
        _layoutsByName.TryGetValue(name, out ISkinLayout? layout);

        return layout;
    }

    /// @see SkinRenderer.as::removeLayout
    public ISkinLayout? RemoveLayout(ISkinLayout layout)
    {
        if (!_layoutsByName.ContainsKey(layout.Name))
        {
            return null;
        }

        // Remove from state mappings
        List<uint> toRemove = (from kvp in _stateToLayout where kvp.Value == layout select kvp.Key).ToList();

        foreach (uint state in toRemove)
        {
            RemoveLayoutFromRenderState(state);
        }

        _layoutsByName.Remove(layout.Name);

        return layout;
    }

    /// @see SkinRenderer.as::addTemplate
    public ISkinTemplate AddTemplate(ISkinTemplate template)
    {
        _templatesByName[((Utils.IChildEntity)template).Name] = template;

        return template;
    }

    /// @see SkinRenderer.as::getTemplateByName
    public ISkinTemplate? GetTemplateByName(string name)
    {
        _templatesByName.TryGetValue(name, out ISkinTemplate? template);

        return template;
    }

    /// @see SkinRenderer.as::removeTemplate
    public ISkinTemplate? RemoveTemplate(ISkinTemplate template)
    {
        string name = ((Utils.IChildEntity)template).Name;

        if (!_templatesByName.ContainsKey(name))
        {
            return null;
        }

        List<uint> toRemove = (from kvp in _stateToTemplate where kvp.Value == template select kvp.Key).ToList();

        foreach (uint state in toRemove)
        {
            RemoveTemplateFromRenderState(state);
        }

        _templatesByName.Remove(name);

        return template;
    }
}
