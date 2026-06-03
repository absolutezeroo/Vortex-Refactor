namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/CoreCommunicationFrameworkLib.as
public sealed partial class CoreCommunicationFrameworkLib : SimpleApplication
{
    /// @see WIN63-202111081545-75921380-Source-main/src/CoreCommunicationFrameworkLib.as::requiredClasses
    public static string[] requiredClasses => ["CoreCommunicationManagerBootstrap", "IIDCoreCommunicationManager"];

    /// @see WIN63-202111081545-75921380-Source-main/src/CoreCommunicationFrameworkLib.as::manifest
    /// TODO(as3-port): Map embedded manifest class/resource equivalent.
    public static object? manifest => null;
}
