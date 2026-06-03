namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/HabboRoomObjectVisualizationLib.as
public sealed partial class HabboRoomObjectVisualizationLib : SimpleApplication
{
    /// @see WIN63-202111081545-75921380-Source-main/src/HabboRoomObjectVisualizationLib.as::requiredClasses
    public static string[] requiredClasses => ["RoomObjectVisualizationFactoryBootstrap", "IIDRoomObjectVisualizationFactory"];

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboRoomObjectVisualizationLib.as::manifest
    /// TODO(as3-port): Map embedded manifest class/resource equivalent.
    public static object? manifest => null;
}
