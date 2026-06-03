namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/HabboRoomObjectLogicLib.as
public sealed partial class HabboRoomObjectLogicLib : SimpleApplication
{
    /// @see WIN63-202111081545-75921380-Source-main/src/HabboRoomObjectLogicLib.as::requiredClasses
    public static string[] requiredClasses => ["RoomObjectFactoryBootstrap", "IIDRoomObjectFactory"];

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboRoomObjectLogicLib.as::manifest
    /// TODO(as3-port): Map embedded manifest class/resource equivalent.
    public static object? manifest => null;
}
