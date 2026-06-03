namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomEngineAreaHideStateWidgetEvent
public class RoomEngineAreaHideStateWidgetEvent(int roomId, int objectId, int category, bool isOn)
    : RoomEngineToWidgetEvent(UPDATE_STATE_AREA_HIDE, roomId, objectId, category)
{
    public const string UPDATE_STATE_AREA_HIDE = "RETWE_UPDATE_STATE_AREA_HIDE";

    public bool IsOn { get; } = isOn;
}
