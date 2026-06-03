namespace Vortex.Habbo.Room.Events;

using Vortex.Room.Events;
using Vortex.Room.Object;

/// @see com.sulake.habbo.room.events.RoomObjectBadgeAssetEvent
public class RoomObjectBadgeAssetEvent(string type, IRoomObject? obj, string badgeId, bool groupBadge = true)
    : RoomObjectEvent(type, obj)
{
    public const string LOAD_BADGE = "ROGBE_LOAD_BADGE";

    public string BadgeId => badgeId;

    public bool GroupBadge => groupBadge;
}
