namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar playing game state change.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarPlayingGameMessage
public class RoomObjectAvatarPlayingGameMessage(bool isPlayingGame = false) : RoomObjectUpdateStateMessage
{
    public bool IsPlayingGame => isPlayingGame;
}
