namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar gained experience points (pet leveling).
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarExperienceUpdateMessage
public class RoomObjectAvatarExperienceUpdateMessage(int gainedExperience) : RoomObjectUpdateStateMessage
{
    public int GainedExperience => gainedExperience;
}
