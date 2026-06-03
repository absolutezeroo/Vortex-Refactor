namespace Vortex.Habbo.Room.Events;

using Vortex.Room.Object;

/// @see com.sulake.habbo.room.events.RoomObjectSamplePlaybackEvent
public class RoomObjectSamplePlaybackEvent(string type, IRoomObject? obj, int sampleId, double pitch = 1.0)
    : RoomObjectFurnitureActionEvent(type, obj)
{
    public const string ROOM_OBJECT_INITIALIZED = "ROPSPE_ROOM_OBJECT_INITIALIZED";
    public const string ROOM_OBJECT_DISPOSED = "ROPSPE_ROOM_OBJECT_DISPOSED";
    public const string PLAY_SAMPLE = "ROPSPE_PLAY_SAMPLE";
    public const string CHANGE_PITCH = "ROPSPE_CHANGE_PITCH";

    public int SampleId => sampleId;

    public double Pitch => pitch;
}
