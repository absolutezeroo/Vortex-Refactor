namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/HabboSessionDataManagerLib.as
public sealed partial class HabboSessionDataManagerLib : SimpleApplication
{
    /// @see WIN63-202111081545-75921380-Source-main/src/HabboSessionDataManagerLib.as::requiredClasses
    public static string[] requiredClasses => ["SessionDataManagerBootstrap", "IIDSessionDataManager"];

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboSessionDataManagerLib.as::manifest
    /// TODO(as3-port): Map embedded manifest class/resource equivalent.
    public static object? manifest => null;
}
