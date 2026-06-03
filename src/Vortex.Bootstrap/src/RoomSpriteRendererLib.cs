namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/RoomSpriteRendererLib.as
public sealed partial class RoomSpriteRendererLib : SimpleApplication
{
    /// @see WIN63-202111081545-75921380-Source-main/src/RoomSpriteRendererLib.as::requiredClasses
    public static string[] requiredClasses => ["RoomRendererFactoryBootstrap", "IIDRoomRendererFactory"];

    /// @see WIN63-202111081545-75921380-Source-main/src/RoomSpriteRendererLib.as::manifest
    /// TODO(as3-port): Map embedded manifest class/resource equivalent.
    public static object? manifest => null;
}
