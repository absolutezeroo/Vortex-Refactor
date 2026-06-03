// @see core/window/graphics/ISkinContainer.as

using System;
using System.Xml.Linq;

using Vortex.Core.Window.Graphics.Renderer;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Graphics;

/// @see core/window/graphics/ISkinContainer.as
public interface ISkinContainer : IDisposable
{
    bool Disposed { get; }

    new void Dispose();

    ISkinRenderer? GetSkinRendererByTypeAndStyle(uint type, uint style);

    bool SkinRendererExists(uint type, uint style);

    DefaultAttStruct? GetDefaultAttributesByTypeAndStyle(uint type, uint style);

    XElement? GetWindowLayoutByTypeAndStyle(uint type, uint style);

    string? GetIntentByTypeAndStyle(uint type, uint style);

    uint GetTheActualState(uint type, uint style, uint state);
}
