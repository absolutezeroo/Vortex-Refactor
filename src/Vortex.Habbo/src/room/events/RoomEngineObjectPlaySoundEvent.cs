namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomEngineObjectPlaySoundEvent
public class RoomEngineObjectPlaySoundEvent(string type,
    int roomId,
    int objectId,
    int category,
    string soundId,
    double pitch = 1.0)
    : RoomEngineObjectEvent(type, roomId, objectId, category)
{
    public const string PLAY_SOUND = "REPSE_PLAY_SOUND";
    public const string PLAY_SOUND_AT_PITCH = "REPSE_PLAY_SOUND_AT_PITCH";

    public string SoundId { get; } = soundId;

    public double Pitch { get; } = pitch;
}
