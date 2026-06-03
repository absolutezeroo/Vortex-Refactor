namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar figure (clothing/looks) change with gender and race info.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarFigureUpdateMessage
public class RoomObjectAvatarFigureUpdateMessage(string figure, string? gender = null, string? race = null, bool isRiding = false)
    : RoomObjectUpdateStateMessage
{
    public string Figure => figure;
    public string? Gender => gender;
    public string? Race => race;
    public bool IsRiding => isRiding;
}
