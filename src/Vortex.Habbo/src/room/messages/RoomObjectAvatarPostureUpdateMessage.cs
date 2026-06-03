namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar posture change (stand, sit, lay, walk) with optional parameter.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarPostureUpdateMessage
public class RoomObjectAvatarPostureUpdateMessage(string postureType, string parameter = "") : RoomObjectUpdateStateMessage
{
    public string PostureType => postureType;
    public string Parameter => parameter;
}
