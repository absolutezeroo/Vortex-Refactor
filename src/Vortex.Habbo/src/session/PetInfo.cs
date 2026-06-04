// @see com.sulake.habbo.session.PetInfo

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.PetInfo
public class PetInfo : IPetInfo
{
    /// @see PetInfo.as::get/set petId
    public int petId { get; set; }

    /// @see PetInfo.as::get/set level
    public int level { get; set; }

    /// @see PetInfo.as::get/set levelMax
    public int levelMax { get; set; }

    /// @see PetInfo.as::get/set experience
    public int experience { get; set; }

    /// @see PetInfo.as::get/set experienceMax
    public int experienceMax { get; set; }

    /// @see PetInfo.as::get/set energy
    public int energy { get; set; }

    /// @see PetInfo.as::get/set energyMax
    public int energyMax { get; set; }

    /// @see PetInfo.as::get/set nutrition
    public int nutrition { get; set; }

    /// @see PetInfo.as::get/set nutritionMax
    public int nutritionMax { get; set; }

    /// @see PetInfo.as::get/set ownerId
    public int ownerId { get; set; }

    /// @see PetInfo.as::get/set ownerName
    public string ownerName { get; set; } = "";

    /// @see PetInfo.as::get/set respect
    public int respect { get; set; }

    /// @see PetInfo.as::get/set age
    public int age { get; set; }

    /// @see PetInfo.as::get/set breedId
    public int breedId { get; set; }

    /// @see PetInfo.as::get/set hasFreeSaddle
    public bool hasFreeSaddle { get; set; }

    /// @see PetInfo.as::get/set isRiding
    public bool isRiding { get; set; }

    /// @see PetInfo.as::get/set canBreed
    public bool canBreed { get; set; }

    /// @see PetInfo.as::get/set canHarvest
    public bool canHarvest { get; set; }

    /// @see PetInfo.as::get/set canRevive
    public bool canRevive { get; set; }

    /// @see PetInfo.as::get/set rarityLevel
    public int rarityLevel { get; set; }

    /// @see PetInfo.as::get/set skillTresholds (AS3 Array → IReadOnlyList<int>)
    public IReadOnlyList<int> skillTresholds { get; set; } = [];

    /// @see PetInfo.as::get/set accessRights
    public int accessRights { get; set; }

    /// @see PetInfo.as::get/set maxWellBeingSeconds
    public int maxWellBeingSeconds { get; set; }

    /// @see PetInfo.as::get/set remainingWellBeingSeconds
    public int remainingWellBeingSeconds { get; set; }

    /// @see PetInfo.as::get/set remainingGrowingSeconds
    public int remainingGrowingSeconds { get; set; }

    /// @see PetInfo.as::get/set hasBreedingPermission
    public bool hasBreedingPermission { get; set; }

    /// @see PetInfo.as::get adultLevel (fixed at 7 per source)
    public int adultLevel { get; } = 7;
}
