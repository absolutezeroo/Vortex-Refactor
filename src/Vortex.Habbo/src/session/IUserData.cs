// @see com.sulake.habbo.session.IUserData

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.IUserData
public interface IUserData
{
    /// @see IUserData.as::get roomObjectId
    int roomObjectId { get; }

    /// @see IUserData.as::get/set name
    string name { get; set; }

    /// @see IUserData.as::get/set custom
    string custom { get; set; }

    /// @see IUserData.as::get/set achievementScore
    int achievementScore { get; set; }

    /// @see IUserData.as::get type
    int type { get; }

    /// @see IUserData.as::get/set figure
    string figure { get; set; }

    /// @see IUserData.as::get/set sex
    string sex { get; set; }

    /// @see IUserData.as::get/set groupID
    string groupID { get; set; }

    /// @see IUserData.as::get groupStatus
    int groupStatus { get; }

    /// @see IUserData.as::get/set groupName
    string groupName { get; set; }

    /// @see IUserData.as::get webID
    int webID { get; }

    /// @see IUserData.as::get/set ownerId
    int ownerId { get; set; }

    /// @see IUserData.as::get/set ownerName
    string ownerName { get; set; }

    /// @see IUserData.as::get/set rarityLevel
    int rarityLevel { get; set; }

    /// @see IUserData.as::get/set hasSaddle
    bool hasSaddle { get; set; }

    /// @see IUserData.as::get/set isRiding
    bool isRiding { get; set; }

    /// @see IUserData.as::get/set canBreed
    bool canBreed { get; set; }

    /// @see IUserData.as::get/set canHarvest
    bool canHarvest { get; set; }

    /// @see IUserData.as::get/set canRevive
    bool canRevive { get; set; }

    /// @see IUserData.as::get/set hasBreedingPermission
    bool hasBreedingPermission { get; set; }

    /// @see IUserData.as::get/set petLevel
    int petLevel { get; set; }

    /// @see IUserData.as::get/set botSkills (AS3 Array → IReadOnlyList<int>)
    IReadOnlyList<int>? botSkills { get; set; }

    /// @see IUserData.as::get/set botSkillData (AS3 Array → IReadOnlyList<object>)
    IReadOnlyList<object>? botSkillData { get; set; }

    /// @see IUserData.as::get/set isModerator
    bool isModerator { get; set; }
}
