namespace Vortex.Habbo.Toolbar.Events;

/// @see com.sulake.habbo.toolbar.events.HabboToolbarEvent
public class HabboToolbarEvent
{
    public const string TOOLBAR_CLICK = "HTE_TOOLBAR_CLICK";
    public const string GROUP_ROOM_INFO_CLICK = "HTE_GROUP_ROOM_INFO_CLICK";
    public const string RESIZED = "HTE_RESIZED";
    public const string CAMERA_TOGGLE = "HTE_CAMERA_TOGGLE";

    public const int CAMERA_ORIGIN_ROOM_TOOL = 0;
    public const int CAMERA_ORIGIN_CHAT = 1;
    public const int CAMERA_ORIGIN_EIW_MAKE_OWN = 2;
    public const int CAMERA_ORIGIN_TOOLBAR = 3;

    public string type { get; }
    public int iconId { get; }
    public string? iconName { get; }

    public HabboToolbarEvent(string type, int iconId = -1, string? iconName = null)
    {
        this.type = type;
        this.iconId = iconId;
        this.iconName = iconName;
    }
}
