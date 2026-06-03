// @see core/window/graphics/renderer/ISkinRenderer.as

using System;
using System.Xml.Linq;

using Godot;

namespace Vortex.Core.Window.Graphics.Renderer;

/// @see core/window/graphics/renderer/ISkinRenderer.as
public interface ISkinRenderer
{
    string Name { get; }

    bool Disposed { get; }

    void Parse(XElement? skinXml, XElement? xmlStates, Func<string, Image?>? assetResolver);

    void Draw(IWindow window, Image buffer, Rect2 region, uint state, bool flag);

    bool IsStateDrawable(uint state);

    void Dispose();

    ISkinLayout AddLayout(ISkinLayout layout);

    ISkinLayout? GetLayoutByName(string name);

    ISkinLayout? RemoveLayout(ISkinLayout layout);

    ISkinLayout? GetLayoutByState(uint state);

    void RegisterLayoutForRenderState(uint state, string layoutName);

    void RemoveLayoutFromRenderState(uint state);

    bool HasLayoutForState(uint state);

    ISkinTemplate AddTemplate(ISkinTemplate template);

    ISkinTemplate? GetTemplateByName(string name);

    ISkinTemplate? RemoveTemplate(ISkinTemplate template);

    ISkinTemplate? GetTemplateByState(uint state);

    void RegisterTemplateForRenderState(uint state, string templateName);

    void RemoveTemplateFromRenderState(uint state);

    bool HasTemplateForState(uint state);
}
