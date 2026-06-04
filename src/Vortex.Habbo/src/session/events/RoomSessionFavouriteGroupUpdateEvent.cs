// @see com.sulake.habbo.session.events.RoomSessionFavouriteGroupUpdateEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionFavouriteGroupUpdateEvent
public class RoomSessionFavouriteGroupUpdateEvent : RoomSessionEvent
{
    public const string FAVOURITE_GROUP_UPDATE = "rsfgue_favourite_group_update";

    /// @see RoomSessionFavouriteGroupUpdateEvent.as::RoomSessionFavouriteGroupUpdateEvent
    /// Note: AS3 arg order is (session, roomIndex, habboGroupId, status, habboGroupName, ...)
    public RoomSessionFavouriteGroupUpdateEvent(IRoomSession session, int roomIndex, int habboGroupId,
        int status, string habboGroupName, bool openLandingPage = false)
        : base(FAVOURITE_GROUP_UPDATE, session, openLandingPage)
    {
        this.roomIndex = roomIndex;
        this.habboGroupId = habboGroupId;
        this.status = status;
        this.habboGroupName = habboGroupName;
    }

    /// @see RoomSessionFavouriteGroupUpdateEvent.as::get roomIndex
    public int roomIndex { get; }

    /// @see RoomSessionFavouriteGroupUpdateEvent.as::get habboGroupId
    public int habboGroupId { get; }

    /// @see RoomSessionFavouriteGroupUpdateEvent.as::get habboGroupName
    public string habboGroupName { get; }

    /// @see RoomSessionFavouriteGroupUpdateEvent.as::get status
    public int status { get; }
}
