namespace Vortex.Room.Utils;

/// <summary>
/// Room ID utility for preview room detection.
/// </summary>
/// @see com.sulake.room.utils.RoomId
public static class RoomId
{
    private const int PREVIEW_ROOM_ID_BASE = 2147418112;

    public static int MakeRoomPreviewerId(int id)
    {
        return (id & 0xFFFF) + PREVIEW_ROOM_ID_BASE;
    }

    public static bool IsRoomPreviewerId(int id)
    {
        return id >= PREVIEW_ROOM_ID_BASE;
    }
}
