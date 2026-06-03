namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomEngineSoundMachineEvent
public class RoomEngineSoundMachineEvent(string type, int roomId, int objectId, int category)
    : RoomEngineObjectEvent(type, roomId, objectId, category)
{
    public const string SOUND_MACHINE_INIT = "ROSM_SOUND_MACHINE_INIT";
    public const string SOUND_MACHINE_SWITCHED_ON = "ROSM_SOUND_MACHINE_SWITCHED_ON";
    public const string SOUND_MACHINE_SWITCHED_OFF = "ROSM_SOUND_MACHINE_SWITCHED_OFF";
    public const string SOUND_MACHINE_DISPOSE = "ROSM_SOUND_MACHINE_DISPOSE";
    public const string JUKEBOX_INIT = "ROSM_JUKEBOX_INIT";
    public const string JUKEBOX_SWITCHED_ON = "ROSM_JUKEBOX_SWITCHED_ON";
    public const string JUKEBOX_SWITCHED_OFF = "ROSM_JUKEBOX_SWITCHED_OFF";
    public const string JUKEBOX_DISPOSE = "ROSM_JUKEBOX_DISPOSE";
}
