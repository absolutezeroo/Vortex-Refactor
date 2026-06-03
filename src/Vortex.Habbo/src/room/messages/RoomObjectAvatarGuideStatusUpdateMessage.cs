namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar guide/helper status indicator change.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarGuideStatusUpdateMessage
public class RoomObjectAvatarGuideStatusUpdateMessage(int guideStatus) : RoomObjectUpdateStateMessage
{
    public int GuideStatus => guideStatus;
}
