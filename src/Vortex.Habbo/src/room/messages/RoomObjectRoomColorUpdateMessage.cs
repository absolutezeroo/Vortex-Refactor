using Vortex.Room.Messages;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Room background color and lighting change.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectRoomColorUpdateMessage
public class RoomObjectRoomColorUpdateMessage(string type, uint color, int light, bool bgOnly)
    : RoomObjectUpdateMessage(null, null)
{
    public const string BACKGROUND_COLOR = "RORCUM_BACKGROUND_COLOR";

    public string Type => type;
    public uint Color => color;
    public int Light => light;
    public bool BgOnly => bgOnly;
}
