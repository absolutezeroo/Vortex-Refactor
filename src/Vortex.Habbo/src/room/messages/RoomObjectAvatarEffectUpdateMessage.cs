namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar effect (particle/overlay) with optional activation delay.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarEffectUpdateMessage
public class RoomObjectAvatarEffectUpdateMessage(int effect = 0, int delayMilliSeconds = 0) : RoomObjectUpdateStateMessage
{
    public int Effect => effect;
    public int DelayMilliSeconds => delayMilliSeconds;
}
