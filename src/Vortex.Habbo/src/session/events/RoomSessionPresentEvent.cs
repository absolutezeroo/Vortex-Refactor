// @see com.sulake.habbo.session.events.RoomSessionPresentEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionPresentEvent
public class RoomSessionPresentEvent : RoomSessionEvent
{
    public const string ROOM_SESSION_PRESENT_OPENED = "RSPE_PRESENT_OPENED";

    /// @see RoomSessionPresentEvent.as::RoomSessionPresentEvent
    public RoomSessionPresentEvent(string type, IRoomSession session, int classId, string itemType,
        string? productCode, int placedItemId, string placedItemType, bool placedInRoom,
        string? petFigureString, bool openLandingPage = false)
        : base(type, session, openLandingPage)
    {
        this.classId = classId;
        this.itemType = itemType;
        this.productCode = productCode;
        this.placedItemId = placedItemId;
        this.placedItemType = placedItemType;
        this.placedInRoom = placedInRoom;
        this.petFigureString = petFigureString;
    }

    /// @see RoomSessionPresentEvent.as::get classId
    public int classId { get; }

    /// @see RoomSessionPresentEvent.as::get itemType
    public string itemType { get; }

    /// @see RoomSessionPresentEvent.as::get productCode
    public string? productCode { get; }

    /// @see RoomSessionPresentEvent.as::get placedItemId
    public int placedItemId { get; }

    /// @see RoomSessionPresentEvent.as::get placedItemType
    public string placedItemType { get; }

    /// @see RoomSessionPresentEvent.as::get placedInRoom
    public bool placedInRoom { get; }

    /// @see RoomSessionPresentEvent.as::get petFigureString
    public string? petFigureString { get; }
}
