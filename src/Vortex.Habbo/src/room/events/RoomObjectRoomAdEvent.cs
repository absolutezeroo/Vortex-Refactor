namespace Vortex.Habbo.Room.Events;

using Vortex.Room.Events;
using Vortex.Room.Object;

/// @see com.sulake.habbo.room.events.RoomObjectRoomAdEvent
public class RoomObjectRoomAdEvent(string type, IRoomObject? obj, string imageUrl = "", string clickUrl = "")
    : RoomObjectEvent(type, obj)
{
    public const string ROOM_AD_LOAD_IMAGE = "RORAE_ROOM_AD_LOAD_IMAGE";
    public const string ROOM_AD_FURNI_CLICK = "RORAE_ROOM_AD_FURNI_CLICK";
    public const string ROOM_AD_FURNI_DOUBLE_CLICK = "RORAE_ROOM_AD_FURNI_DOUBLE_CLICK";
    public const string ROOM_AD_TOOLTIP_SHOW = "RORAE_ROOM_AD_TOOLTIP_SHOW";
    public const string ROOM_AD_TOOLTIP_HIDE = "RORAE_ROOM_AD_TOOLTIP_HIDE";

    public string ImageUrl => imageUrl;

    public string ClickUrl => clickUrl;
}
