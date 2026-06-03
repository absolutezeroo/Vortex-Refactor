namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomEngineDimmerStateEvent
public class RoomEngineDimmerStateEvent(int roomId,
    int objectId,
    int state,
    int presetId,
    int effectId,
    uint color,
    uint brightness)
    : RoomEngineEvent(CYCLED, roomId)
{
    public const string CYCLED = "REDSE_ROOM_COLOR";

    public int ObjectId { get; } = objectId;

    public int State { get; } = state;

    public int PresetId { get; } = presetId;

    public int EffectId { get; } = effectId;

    public uint Color { get; } = color;

    public uint Brightness { get; } = brightness;
}
