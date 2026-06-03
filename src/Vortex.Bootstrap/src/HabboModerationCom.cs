namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/HabboModerationCom.as
public sealed partial class HabboModerationCom : SimpleApplication
{
    /// @see WIN63-202111081545-75921380-Source-main/src/HabboModerationCom.as::requiredClasses
    public static string[] requiredClasses => ["ModerationManagerBootstrap", "IIDHabboModeration"];
}
