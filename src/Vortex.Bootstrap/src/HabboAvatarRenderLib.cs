namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/HabboAvatarRenderLib.as
public sealed partial class HabboAvatarRenderLib : SimpleApplication
{
    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAvatarRenderLib.as::requiredClasses
    public static string[] requiredClasses => ["AvatarRenderManagerBootstrap", "IIDAvatarRenderManager"];

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAvatarRenderLib.as::manifest
    /// TODO(as3-port): Map embedded manifest class/resource equivalent.
    public static object? manifest => null;
}
