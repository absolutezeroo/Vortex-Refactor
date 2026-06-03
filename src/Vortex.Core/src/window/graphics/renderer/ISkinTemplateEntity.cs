// @see core/window/graphics/renderer/ISkinTemplateEntity.as

using Godot;

using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Graphics.Renderer;

/// @see core/window/graphics/renderer/ISkinTemplateEntity.as
public interface ISkinTemplateEntity : IChildEntity
{
    string Type { get; }

    Rect2 Region { get; }
}
