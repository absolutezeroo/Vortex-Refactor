using Vortex.Room.Object;

namespace Vortex.Habbo.Room.Utils;

/// @see com.sulake.habbo.room.utils.RoomObjectBadgeImageAssetListener
public class RoomObjectBadgeImageAssetListener(IRoomObjectController obj, bool groupBadge)
{
    public IRoomObjectController Object { get; } = obj;

    public bool GroupBadge { get; } = groupBadge;
}
