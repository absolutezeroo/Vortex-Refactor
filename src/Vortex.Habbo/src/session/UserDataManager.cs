// @see com.sulake.habbo.session.UserDataManager

using Vortex.Core.Communication.Connection;

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.UserDataManager
public class UserDataManager : IUserDataManager
{
    private const int TYPE_USER = 1;
    private const int TYPE_PET = 2;

    private readonly Dictionary<int, Dictionary<int, IUserData>> _byTypeAndWebId = new();
    private readonly Dictionary<int, IUserData> _byRoomIndex = new();
    private readonly Dictionary<int, IReadOnlyList<string>> _badges = new();
    private IConnection? _connection;

    /// @see UserDataManager.as::set connection
    public IConnection? connection
    {
        set => _connection = value;
    }

    /// @see UserDataManager.as::dispose
    public void Dispose()
    {
        _connection = null;
        _byTypeAndWebId.Clear();
        _byRoomIndex.Clear();
        _badges.Clear();
    }

    /// @see UserDataManager.as::getUserData
    public IUserData? GetUserData(int userId)
    {
        return GetUserDataByType(userId, TYPE_USER);
    }

    /// @see UserDataManager.as::getUserDataByType
    public IUserData? GetUserDataByType(int userId, int type)
    {
        if (!_byTypeAndWebId.TryGetValue(type, out Dictionary<int, IUserData>? typeMap))
        {
            return null;
        }

        typeMap.TryGetValue(userId, out IUserData? data);
        return data;
    }

    /// @see UserDataManager.as::getUserDataByIndex
    public IUserData? GetUserDataByIndex(int roomObjectId)
    {
        _byRoomIndex.TryGetValue(roomObjectId, out IUserData? data);
        return data;
    }

    /// @see UserDataManager.as::getUserDataByName
    public IUserData? GetUserDataByName(string name)
    {
        foreach (IUserData data in _byRoomIndex.Values)
        {
            if (data.name == name)
            {
                return data;
            }
        }
        return null;
    }

    /// @see UserDataManager.as::getUserBadges
    public IReadOnlyList<string>? GetUserBadges(int userId)
    {
        // TODO(as3-port): GetSelectedBadgesMessageComposer not yet ported — skipping connection.Send
        _badges.TryGetValue(userId, out IReadOnlyList<string>? badges);
        return badges ?? [];
    }

    /// @see UserDataManager.as::removeUserDataByRoomIndex
    public void RemoveUserDataByRoomIndex(int roomObjectId)
    {
        if (!_byRoomIndex.Remove(roomObjectId, out IUserData? data))
        {
            return;
        }

        if (_byTypeAndWebId.TryGetValue(data.type, out Dictionary<int, IUserData>? typeMap))
        {
            typeMap.Remove(data.webID);
        }
    }

    /// @see UserDataManager.as::setUserBadges
    public void SetUserBadges(int userId, IReadOnlyList<string> badges)
    {
        _badges[userId] = badges;
    }

    /// @see UserDataManager.as::setUserData
    public void SetUserData(IUserData userData)
    {
        if (userData == null)
        {
            return;
        }

        RemoveUserDataByRoomIndex(userData.roomObjectId);
        if (!_byTypeAndWebId.TryGetValue(userData.type, out Dictionary<int, IUserData>? typeMap))
        {
            typeMap = new Dictionary<int, IUserData>();
            _byTypeAndWebId[userData.type] = typeMap;
        }
        typeMap[userData.webID] = userData;
        _byRoomIndex[userData.roomObjectId] = userData;
    }

    /// @see UserDataManager.as::updateFigure
    public void UpdateFigure(int roomObjectId, string figure, string sex, bool hasSaddle, bool isRiding)
    {
        IUserData? data = GetUserDataByIndex(roomObjectId);
        if (data == null)
        {
            return;
        }

        data.figure = figure;
        data.sex = sex;
        data.hasSaddle = hasSaddle;
        data.isRiding = isRiding;
    }

    /// @see UserDataManager.as::updatePetLevel
    public void UpdatePetLevel(int roomObjectId, int level)
    {
        IUserData? data = GetUserDataByIndex(roomObjectId);
        if (data != null)
        {
            data.petLevel = level;
        }
    }

    /// @see UserDataManager.as::updatePetBreedingStatus
    public void UpdatePetBreedingStatus(int roomObjectId, bool canBreed, bool canHarvest, bool canRevive, bool hasBreedingPermission)
    {
        IUserData? data = GetUserDataByIndex(roomObjectId);
        if (data == null)
        {
            return;
        }

        data.canBreed = canBreed;
        data.canHarvest = canHarvest;
        data.canRevive = canRevive;
        data.hasBreedingPermission = hasBreedingPermission;
    }

    /// @see UserDataManager.as::updateCustom
    public void UpdateCustom(int roomObjectId, string custom)
    {
        IUserData? data = GetUserDataByIndex(roomObjectId);
        if (data != null)
        {
            data.custom = custom;
        }
    }

    /// @see UserDataManager.as::updateAchievementScore
    public void UpdateAchievementScore(int roomObjectId, int score)
    {
        IUserData? data = GetUserDataByIndex(roomObjectId);
        if (data != null)
        {
            data.achievementScore = score;
        }
    }

    /// @see UserDataManager.as::updateNameByIndex
    public void UpdateNameByIndex(int roomObjectId, string name)
    {
        IUserData? data = GetUserDataByIndex(roomObjectId);
        if (data != null)
        {
            data.name = name;
        }
    }

    /// @see UserDataManager.as::getPetUserData
    public IUserData? GetPetUserData(int petId)
    {
        return GetUserDataByType(petId, TYPE_PET);
    }

    /// @see UserDataManager.as::getRentableBotUserData
    public IUserData? GetRentableBotUserData(int botId)
    {
        return GetUserDataByType(botId, 4);
    }

    /// @see UserDataManager.as::requestPetInfo
    public void RequestPetInfo(int petId)
    {
        IUserData? data = GetPetUserData(petId);
        // TODO(as3-port): GetPetInfoMessageComposer not yet ported
        _ = data;
    }

    /// @see UserDataManager.as::getAllUserIds
    public IReadOnlyList<int> GetAllUserIds()
    {
        List<int> ids = new List<int>();
        foreach (IUserData data in _byRoomIndex.Values)
        {
            ids.Add(data.webID);
        }

        return ids;
    }
}
