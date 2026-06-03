namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

/// @see com.sulake.habbo.communication.messages.incoming.room.engine.class_1668
public class UserMessageData(int roomIndex)
{
    public const string MALE = "M";
    public const string FEMALE = "F";

    private bool _readOnly;

    public int RoomIndex => roomIndex;
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public int Dir { get; set; }
    public int UserType { get; set; }
    public string Name { get; set; } = "";
    public string Sex { get; set; } = "";
    public string Figure { get; set; } = "";
    public string Custom { get; set; } = "";
    public int WebID { get; set; }
    public string GroupID { get; set; } = "";
    public int GroupStatus { get; set; }
    public string GroupName { get; set; } = "";
    public string SubType { get; set; } = "";
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = "";
    public int RarityLevel { get; set; }
    public bool HasSaddle { get; set; }
    public bool IsRiding { get; set; }
    public bool CanBreed { get; set; }
    public bool CanHarvest { get; set; }
    public bool CanRevive { get; set; }
    public bool HasBreedingPermission { get; set; }
    public int PetLevel { get; set; }
    public string PetPosture { get; set; } = "";
    public int AchievementScore { get; set; }
    public bool IsModerator { get; set; }
    public List<int>? BotSkills { get; set; }

    public void SetReadOnly()
    {
        _readOnly = true;
    }
}
