namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/HabboLocalizationCom.as
public sealed partial class HabboLocalizationCom : SimpleApplication
{
    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLocalizationCom.as::requiredClasses
    public static string[] requiredClasses =>
    [
        "HabboLocalizationManagerBootstrap", "IIDCoreLocalizationManager", "IIDHabboLocalizationManager", "ICoreLocalizationManager",
        "IHabboLocalizationManager", "HabboLocalizationManager",
    ];
}
