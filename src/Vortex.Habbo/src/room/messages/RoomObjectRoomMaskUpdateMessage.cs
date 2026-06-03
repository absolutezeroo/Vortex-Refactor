using Vortex.Room.Messages;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Room plane mask add/remove (doors, windows, holes).
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectRoomMaskUpdateMessage
public class RoomObjectRoomMaskUpdateMessage
(
    string type,
    string maskId,
    string? maskType = null,
    IVector3d? maskLocation = null,
    string maskCategory = "window"
)
    : RoomObjectUpdateMessage(null, null)
{
    public const string ADD_MASK = "RORMUM_ADD_MASK";
    public const string REMOVE_MASK = "RORMUM_REMOVE_MASK";
    public const string MASK_TYPE_DOOR = "door";
    public const string MASK_CATEGORY_WINDOW = "window";
    public const string MASK_CATEGORY_HOLE = "hole";

    public string Type => type;
    public string MaskId => maskId;
    public string? MaskType => maskType;
    public IVector3d? MaskLocation => maskLocation;
    public string MaskCategory => maskCategory;
}
