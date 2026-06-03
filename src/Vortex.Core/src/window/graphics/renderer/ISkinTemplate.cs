// @see core/window/graphics/renderer/ISkinTemplate.as

using Godot;

using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Graphics.Renderer;

/// @see core/window/graphics/renderer/ISkinTemplate.as
public interface ISkinTemplate : IChildEntityCollection, IChildEntity
{
    Image? Asset { get; }

    void Dispose();
}
