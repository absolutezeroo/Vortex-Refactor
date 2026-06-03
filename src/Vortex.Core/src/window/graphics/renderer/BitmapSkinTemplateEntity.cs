// @see core/window/graphics/renderer/BitmapSkinTemplateEntity.as

using Godot;

namespace Vortex.Core.Window.Graphics.Renderer;

/// @see core/window/graphics/renderer/BitmapSkinTemplateEntity.as
public class BitmapSkinTemplateEntity : SkinTemplateEntity
{
    /// @see BitmapSkinTemplateEntity.as::BitmapSkinTemplateEntity
    public BitmapSkinTemplateEntity(string name, string type, uint id, Rect2 region)
        : base(name, type, id, region)
    {
    }
}
