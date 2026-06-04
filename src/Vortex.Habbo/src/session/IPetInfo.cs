// @see com.sulake.habbo.session.IPetInfo

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.IPetInfo
public interface IPetInfo
{
    /// @see IPetInfo.as::get petId
    int petId { get; }

    /// @see IPetInfo.as::get level
    int level { get; }

    /// @see IPetInfo.as::get levelMax
    int levelMax { get; }

    /// @see IPetInfo.as::get experience
    int experience { get; }

    /// @see IPetInfo.as::get experienceMax
    int experienceMax { get; }

    /// @see IPetInfo.as::get energy
    int energy { get; }

    /// @see IPetInfo.as::get energyMax
    int energyMax { get; }

    /// @see IPetInfo.as::get nutrition
    int nutrition { get; }

    /// @see IPetInfo.as::get nutritionMax
    int nutritionMax { get; }

    /// @see IPetInfo.as::get ownerId
    int ownerId { get; }

    /// @see IPetInfo.as::get ownerName
    string ownerName { get; }

    /// @see IPetInfo.as::get respect
    int respect { get; }

    /// @see IPetInfo.as::get age
    int age { get; }

    /// @see IPetInfo.as::get breedId
    int breedId { get; }

    /// @see IPetInfo.as::get hasFreeSaddle
    bool hasFreeSaddle { get; }

    /// @see IPetInfo.as::get isRiding
    bool isRiding { get; }

    /// @see IPetInfo.as::get canBreed
    bool canBreed { get; }

    /// @see IPetInfo.as::get canHarvest
    bool canHarvest { get; }

    /// @see IPetInfo.as::get canRevive
    bool canRevive { get; }

    /// @see IPetInfo.as::get rarityLevel
    int rarityLevel { get; }

    /// @see IPetInfo.as::get skillTresholds (AS3 Array → IReadOnlyList<int>)
    IReadOnlyList<int> skillTresholds { get; }

    /// @see IPetInfo.as::get accessRights
    int accessRights { get; }

    /// @see IPetInfo.as::get maxWellBeingSeconds
    int maxWellBeingSeconds { get; }

    /// @see IPetInfo.as::get remainingWellBeingSeconds
    int remainingWellBeingSeconds { get; }

    /// @see IPetInfo.as::get remainingGrowingSeconds
    int remainingGrowingSeconds { get; }

    /// @see IPetInfo.as::get hasBreedingPermission
    bool hasBreedingPermission { get; }

    /// @see IPetInfo.as::get adultLevel
    int adultLevel { get; }
}
