namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/HabboTrackingLib.as
public sealed partial class HabboTrackingLib : SimpleApplication
{
    /// @see WIN63-202111081545-75921380-Source-main/src/HabboTrackingLib.as::requiredClasses
    public static string[] requiredClasses => ["HabboTrackingBootstrap", "IIDHabboTracking"];

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboTrackingLib.as::manifest
    /// TODO(as3-port): Map embedded manifest class/resource equivalent.
    public static object? manifest => null;
}
