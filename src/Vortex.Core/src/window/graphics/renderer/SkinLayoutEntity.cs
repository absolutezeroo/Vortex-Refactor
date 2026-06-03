// @see core/window/graphics/renderer/SkinLayoutEntity.as

using System;

using Godot;

using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Graphics.Renderer;

/// @see core/window/graphics/renderer/SkinLayoutEntity.as
public class SkinLayoutEntity : IChildEntity, IDisposable
{
    public const uint SCALE_TYPE_FIXED = 0;
    public const uint SCALE_TYPE_MOVE = 1;
    public const uint SCALE_TYPE_STRECH = 2;
    public const uint SCALE_TYPE_TILED = 4;
    public const uint SCALE_TYPE_CENTER = 8;

    public uint Color { get; set; }
    public uint Blend { get; set; }
    public uint ScaleH { get; set; }
    public uint ScaleV { get; set; }
    public Rect2 Region { get; set; }
    public bool Colorize { get; set; }

    /// @see SkinLayoutEntity.as::SkinLayoutEntity
    public SkinLayoutEntity(uint id, string name)
    {
        Id = id;
        Name = name;
    }

    public uint Id { get; }

    public string Name { get; }

    public static string[]? Tags => null;

    /// @see SkinLayoutEntity.as::dispose
    public void Dispose()
    {
        Region = default;
    }
}
