namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar expression/emote change (wave, laugh, etc.).
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarExpressionUpdateMessage
public class RoomObjectAvatarExpressionUpdateMessage(int expressionType = -1) : RoomObjectUpdateStateMessage
{
    public int ExpressionType => expressionType;
}
