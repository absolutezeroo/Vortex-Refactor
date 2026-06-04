// @see com.sulake.habbo.session.IUserDataManager

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.IUserDataManager
public interface IUserDataManager
{
    /// @see IUserDataManager.as::setUserData
    void SetUserData(IUserData userData);

    /// @see IUserDataManager.as::getUserData
    IUserData? GetUserData(int userId);

    /// @see IUserDataManager.as::getUserDataByType
    IUserData? GetUserDataByType(int userId, int type);

    /// @see IUserDataManager.as::getUserDataByIndex
    IUserData? GetUserDataByIndex(int roomObjectId);

    /// @see IUserDataManager.as::getUserDataByName
    IUserData? GetUserDataByName(string name);

    /// @see IUserDataManager.as::getUserBadges (AS3 Array → IReadOnlyList<string>)
    IReadOnlyList<string>? GetUserBadges(int userId);

    /// @see IUserDataManager.as::removeUserDataByRoomIndex
    void RemoveUserDataByRoomIndex(int roomObjectId);

    /// @see IUserDataManager.as::setUserBadges
    void SetUserBadges(int userId, IReadOnlyList<string> badges);

    /// @see IUserDataManager.as::updateFigure
    void UpdateFigure(int userId, string figure, string sex, bool hasSaddle, bool isRiding);

    /// @see IUserDataManager.as::updatePetLevel
    void UpdatePetLevel(int userId, int level);

    /// @see IUserDataManager.as::updatePetBreedingStatus
    void UpdatePetBreedingStatus(int userId, bool canBreed, bool canHarvest, bool canRevive, bool hasBreedingPermission);

    /// @see IUserDataManager.as::updateCustom
    void UpdateCustom(int userId, string custom);

    /// @see IUserDataManager.as::updateAchievementScore
    void UpdateAchievementScore(int userId, int score);

    /// @see IUserDataManager.as::updateNameByIndex
    void UpdateNameByIndex(int roomObjectId, string name);

    /// @see IUserDataManager.as::getPetUserData
    IUserData? GetPetUserData(int petId);

    /// @see IUserDataManager.as::getRentableBotUserData
    IUserData? GetRentableBotUserData(int botId);

    /// @see IUserDataManager.as::requestPetInfo
    void RequestPetInfo(int petId);

    /// @see IUserDataManager.as::getAllUserIds (AS3 Array → IReadOnlyList<int>)
    IReadOnlyList<int> GetAllUserIds();
}
