namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/HabboWindowManagerCom.as
public sealed partial class HabboWindowManagerCom : SimpleApplication
{
    /// @see WIN63-202111081545-75921380-Source-main/src/HabboWindowManagerCom.as::requiredClasses
    public static string[] requiredClasses =>
    [
        "HabboWindowManagerComponentBootstrap", "IIDHabboWindowManager", "IIDCoreWindowManager", "HabboWindowManagerComponent",
        "ICoreLocalizationFrameworkLib", "ICoreWindowFrameworkLib", "CoreWindowFrameworkLib",
    ];
}
