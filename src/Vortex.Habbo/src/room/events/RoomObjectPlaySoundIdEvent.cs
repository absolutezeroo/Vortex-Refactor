namespace Vortex.Habbo.Room.Events;

using Vortex.Room.Object;

/// @see com.sulake.habbo.room.events.RoomObjectPlaySoundIdEvent
public class RoomObjectPlaySoundIdEvent(string type, IRoomObject? obj, string soundId, double pitch = 1.0)
    : RoomObjectFurnitureActionEvent(type, obj)
{
    public const string PLAY_SOUND = "ROPSIE_PLAY_SOUND";
    public const string PLAY_SOUND_AT_PITCH = "ROPSIE_PLAY_SOUND_AT_PITCH";

    public string SoundId => soundId;

    public double Pitch => pitch;
}
