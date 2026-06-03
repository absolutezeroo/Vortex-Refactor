namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomEngineRoomAdEvent
public class RoomEngineRoomAdEvent(string type, int roomId, int objectId, int category)
    : RoomEngineObjectEvent(type, roomId, objectId, category)
{
    public const string FURNI_CLICK = "RERAE_FURNI_CLICK";
    public const string FURNI_DOUBLE_CLICK = "RERAE_FURNI_DOUBLE_CLICK";
    public const string TOOLTIP_SHOW = "RERAE_TOOLTIP_SHOW";
    public const string TOOLTIP_HIDE = "RERAE_TOOLTIP_HIDE";
}
