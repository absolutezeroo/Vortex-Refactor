using Vortex.Room.Messages;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Group badge asset loaded notification for furniture display.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectGroupBadgeUpdateMessage
public class RoomObjectGroupBadgeUpdateMessage(string badgeId, string assetName) : RoomObjectUpdateMessage(null, null)
{
    public const string BADGE_LOADED = "ROGBUM_BADGE_LOADED";

    public string BadgeId => badgeId;
    public string AssetName => assetName;
}
