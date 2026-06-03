// @see core/window/graphics/renderer/ISkinLayout.as

using Godot;

using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Graphics.Renderer;

/// @see core/window/graphics/renderer/ISkinLayout.as
public interface ISkinLayout : IChildEntityCollection
{
    string Name { get; }

    uint Width { get; }

    uint Height { get; }

    string BlendMode { get; }

    bool Transparent { get; }

    void Dispose();

    bool IsFixedWidth();

    bool IsFixedHeight();

    void GetDefaultRegion(string entityName, ref Rect2 result);
}
