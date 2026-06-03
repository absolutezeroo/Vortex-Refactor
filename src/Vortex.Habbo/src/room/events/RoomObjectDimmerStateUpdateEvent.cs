namespace Vortex.Habbo.Room.Events;

using Vortex.Room.Events;
using Vortex.Room.Object;

/// @see com.sulake.habbo.room.events.RoomObjectDimmerStateUpdateEvent
public class RoomObjectDimmerStateUpdateEvent(IRoomObject? obj,
    int state,
    int presetId,
    int effectId,
    uint color,
    int brightness)
    : RoomObjectEvent(DIMMER_STATE, obj)
{
    public const string DIMMER_STATE = "RODSUE_DIMMER_STATE";

    public int State => state;

    public int PresetId => presetId;

    public int EffectId => effectId;

    public uint Color => color;

    public int Brightness => brightness;
}
