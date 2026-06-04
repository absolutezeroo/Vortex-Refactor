// @see com.sulake.habbo.session.UserData

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.UserData
public class UserData : IUserData
{
    /// @see UserData.as::UserData
    public UserData(int roomObjectId)
    {
        this.roomObjectId = roomObjectId;
    }

    /// @see UserData.as::get roomObjectId
    public int roomObjectId { get; }

    /// @see UserData.as::get/set name
    public string name { get; set; } = "";

    /// @see UserData.as::get/set type (setter not in IUserData interface)
    public int type { get; set; } = 0;

    /// @see UserData.as::get/set sex
    public string sex { get; set; } = "";

    /// @see UserData.as::get/set figure
    public string figure { get; set; } = "";

    /// @see UserData.as::get/set custom
    public string custom { get; set; } = "";

    /// @see UserData.as::get/set achievementScore
    public int achievementScore { get; set; }

    /// @see UserData.as::get/set webID (setter not in IUserData interface)
    public int webID { get; set; } = 0;

    /// @see UserData.as::get/set groupID
    public string groupID { get; set; } = "";

    /// @see UserData.as::get/set groupStatus (setter not in IUserData interface)
    public int groupStatus { get; set; } = 0;

    /// @see UserData.as::get/set groupName
    public string groupName { get; set; } = "";

    /// @see UserData.as::get/set ownerId
    public int ownerId { get; set; } = 0;

    /// @see UserData.as::get/set ownerName
    public string ownerName { get; set; } = "";

    /// @see UserData.as::get/set rarityLevel
    public int rarityLevel { get; set; } = 0;

    /// @see UserData.as::get/set hasSaddle
    public bool hasSaddle { get; set; }

    /// @see UserData.as::get/set isRiding
    public bool isRiding { get; set; }

    /// @see UserData.as::get/set canBreed
    public bool canBreed { get; set; }

    /// @see UserData.as::get/set canHarvest
    public bool canHarvest { get; set; }

    /// @see UserData.as::get/set canRevive
    public bool canRevive { get; set; }

    /// @see UserData.as::get/set hasBreedingPermission
    public bool hasBreedingPermission { get; set; }

    /// @see UserData.as::get/set petLevel
    public int petLevel { get; set; } = 0;

    /// @see UserData.as::get/set botSkills (AS3 Array → IReadOnlyList<int>)
    public IReadOnlyList<int>? botSkills { get; set; }

    /// @see UserData.as::get/set botSkillData (AS3 Array → IReadOnlyList<object>)
    public IReadOnlyList<object>? botSkillData { get; set; }

    /// @see UserData.as::get/set isModerator
    public bool isModerator { get; set; }
}
