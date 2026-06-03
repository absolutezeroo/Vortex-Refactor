namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/RoomManagerLib.as
public sealed partial class RoomManagerLib : SimpleApplication
{
    /// @see WIN63-202111081545-75921380-Source-main/src/RoomManagerLib.as::requiredClasses
    public static string[] requiredClasses => ["RoomManagerBootstrap", "IIDRoomManager"];

    /// @see WIN63-202111081545-75921380-Source-main/src/RoomManagerLib.as::manifest
    /// TODO(as3-port): Map embedded manifest class/resource equivalent.
    public static object? manifest => null;
}
