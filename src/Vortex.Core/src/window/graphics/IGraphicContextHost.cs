// @see core/window/graphics/IGraphicContextHost.as

namespace Vortex.Core.Window.Graphics;

/// @see core/window/graphics/IGraphicContextHost.as
public interface IGraphicContextHost
{
    string name { get; }
    IGraphicContext? GetGraphicContext(bool create);
    bool HasGraphicsContext();
}
