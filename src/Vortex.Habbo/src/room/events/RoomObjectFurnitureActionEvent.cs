using Vortex.Room.Events;
using Vortex.Room.Object;

namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomObjectFurnitureActionEvent
public class RoomObjectFurnitureActionEvent(string type, IRoomObject? obj) : RoomObjectEvent(type, obj)
{
    public const string DICE_OFF = "ROFCAE_DICE_OFF";
    public const string DICE_ACTIVATE = "ROFCAE_DICE_ACTIVATE";
    public const string USE_HABBOWHEEL = "ROFCAE_USE_HABBOWHEEL";
    public const string STICKIE = "ROFCAE_STICKIE";
    public const string ENTER_ONEWAYDOOR = "ROFCAE_ENTER_ONEWAYDOOR";
    public const string SOUND_MACHINE_INIT = "ROFCAE_SOUND_MACHINE_INIT";
    public const string SOUND_MACHINE_START = "ROFCAE_SOUND_MACHINE_START";
    public const string SOUND_MACHINE_STOP = "ROFCAE_SOUND_MACHINE_STOP";
    public const string SOUND_MACHINE_DISPOSE = "ROFCAE_SOUND_MACHINE_DISPOSE";
    public const string JUKEBOX_INIT = "ROFCAE_JUKEBOX_INIT";
    public const string JUKEBOX_START = "ROFCAE_JUKEBOX_START";
    public const string JUKEBOX_MACHINE_STOP = "ROFCAE_JUKEBOX_MACHINE_STOP";
    public const string JUKEBOX_DISPOSE = "ROFCAE_JUKEBOX_DISPOSE";
    public const string CURSOR_REQUEST_BUTTON = "ROFCAE_MOUSE_BUTTON";
    public const string CURSOR_REQUEST_ARROW = "ROFCAE_MOUSE_ARROW";
}
