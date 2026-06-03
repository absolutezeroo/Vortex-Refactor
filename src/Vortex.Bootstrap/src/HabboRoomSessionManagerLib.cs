namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/HabboRoomSessionManagerLib.as
public sealed partial class HabboRoomSessionManagerLib : SimpleApplication
{
    /// @see WIN63-202111081545-75921380-Source-main/src/HabboRoomSessionManagerLib.as::requiredClasses
    public static string[] requiredClasses => ["RoomSessionManagerBootstrap", "IIDHabboRoomSessionManager"];

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboRoomSessionManagerLib.as::manifest
    /// TODO(as3-port): Map embedded manifest class/resource equivalent.
    public static object? manifest => null;
}
