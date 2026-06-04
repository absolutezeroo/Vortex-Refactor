// @see com.sulake.habbo.communication.messages.incoming.room.pets.PetBreedingResultData

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Pets;

/// @see com.sulake.habbo.communication.messages.incoming.room.pets.PetBreedingResultData
public class PetBreedingResultData
{
    /// @see PetBreedingResultData.as::PetBreedingResultData
    public PetBreedingResultData(int stuffId, int classId, string productCode, int userId,
        string userName, int rarityLevel, bool hasMutation)
    {
        this.stuffId = stuffId;
        this.classId = classId;
        this.productCode = productCode;
        this.userId = userId;
        this.userName = userName;
        this.rarityLevel = rarityLevel;
        this.hasMutation = hasMutation;
    }

    /// @see PetBreedingResultData.as::get stuffId
    public int stuffId { get; }

    /// @see PetBreedingResultData.as::get classId
    public int classId { get; }

    /// @see PetBreedingResultData.as::get productCode
    public string productCode { get; }

    /// @see PetBreedingResultData.as::get userId
    public int userId { get; }

    /// @see PetBreedingResultData.as::get userName
    public string userName { get; }

    /// @see PetBreedingResultData.as::get rarityLevel
    public int rarityLevel { get; }

    /// @see PetBreedingResultData.as::get hasMutation
    public bool hasMutation { get; }
}
