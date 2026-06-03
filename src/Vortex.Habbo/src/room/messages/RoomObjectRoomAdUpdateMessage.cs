using Godot;

using Vortex.Room.Messages;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Room billboard/ad activation or image load status.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectRoomAdUpdateMessage
public class RoomObjectRoomAdUpdateMessage
(
    string type,
    string asset,
    string clickUrl,
    int objectId = -1,
    Image? bitmapData = null
)
    : RoomObjectUpdateMessage(null, null)
{
    public const string ROOM_AD_ACTIVATE = "RORUM_ROOM_AD_ACTIVATE";
    public const string ROOM_BILLBOARD_IMAGE_LOADED = "RORUM_ROOM_BILLBOARD_IMAGE_LOADED";
    public const string ROOM_BILLBOARD_LOADING_FAILED = "RORUM_ROOM_BILLBOARD_IMAGE_LOADING_FAILED";

    public string Type => type;
    public string Asset => asset;
    public string ClickUrl => clickUrl;
    public int ObjectId => objectId;
    public Image? BitmapData => bitmapData;
}
