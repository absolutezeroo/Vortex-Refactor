// @see core/window/graphics/renderer/SkinTemplateEntity.as

using Godot;

namespace Vortex.Core.Window.Graphics.Renderer;

/// @see core/window/graphics/renderer/SkinTemplateEntity.as
public class SkinTemplateEntity : ISkinTemplateEntity
{
    /// @see SkinTemplateEntity.as::SkinTemplateEntity
    public SkinTemplateEntity(string name, string type, uint id, Rect2 region)
    {
        Id = id;
        Name = name;
        Type = type;
        Region = region;
    }

    public uint Id { get; }

    public string Name { get; }

    public string Type { get; }

    public Rect2 Region { get; }
}
