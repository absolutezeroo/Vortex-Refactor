namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomEngineObjectSamplePlaybackEvent
public class RoomEngineObjectSamplePlaybackEvent(string type,
    int roomId,
    int objectId,
    int category,
    int sampleId,
    double pitch = 1.0)
    : RoomEngineObjectEvent(type, roomId, objectId, category)
{
    public const string ROOM_OBJECT_INITIALIZED = "REOSPE_ROOM_OBJECT_INITIALIZED";
    public const string ROOM_OBJECT_DISPOSED = "REOSPE_ROOM_OBJECT_DISPOSED";
    public const string PLAY_SAMPLE = "REOSPE_PLAY_SAMPLE";
    public const string CHANGE_PITCH = "REOSPE_CHANGE_PITCH";

    public int SampleId { get; } = sampleId;

    public double Pitch { get; } = pitch;
}
