namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/HabboConfigurationCom.as
public sealed partial class HabboConfigurationCom : SimpleApplication
{
    /// @see WIN63-202111081545-75921380-Source-main/src/HabboConfigurationCom.as::requiredClasses
    public static string[] requiredClasses => ["HabboConfigurationManagerBootstrap", "IIDHabboConfigurationManager", "HabboConfigurationManager"];
}
