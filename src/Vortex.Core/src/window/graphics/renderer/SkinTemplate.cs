// @see core/window/graphics/renderer/SkinTemplate.as

using System;

using Godot;

using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Graphics.Renderer;

/// @see core/window/graphics/renderer/SkinTemplate.as
public class SkinTemplate : ChildEntityArray<SkinTemplateEntity>, ISkinTemplate, IDisposable
{
    protected string _name;
    protected Image? _asset;

    /// @see SkinTemplate.as::SkinTemplate
    public SkinTemplate(string name, Image? asset)
    {
        _name = name;
        _asset = asset;
    }

    public uint Id => 0;
    string IChildEntity.Name => _name;
    public virtual Image? Asset => _asset;

    /// @see SkinTemplate.as::dispose
    public void Dispose()
    {
        uint count = (uint)NumChildren;
        for (uint i = 0;
             i < count;
             i++)
        {
            RemoveChildAt(0);
        }
        _asset = null;
        _name = null!;
    }
}
