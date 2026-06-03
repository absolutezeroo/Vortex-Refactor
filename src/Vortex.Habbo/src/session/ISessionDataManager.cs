// @see com.sulake.habbo.session.ISessionDataManager

using Godot;

using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;
using Vortex.Habbo.Session.Furniture;
using Vortex.Habbo.Session.Product;

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.ISessionDataManager
public interface ISessionDataManager : IUnknown
{
    /// @see ISessionDataManager.as::get events
    EventDispatcherWrapper? events { get; }

    // --- System state ---
    bool systemOpen { get; }
    bool systemShutDown { get; }
    bool isAuthenticHabbo { get; }
    bool hasSecurity(int level);
    int topSecurityLevel { get; }

    // --- Club / membership ---
    int clubLevel { get; }
    bool hasVip { get; }
    bool hasClub { get; }
    bool isNoob { get; }
    bool isRealNoob { get; }

    // --- User identity ---
    int userId { get; }
    string userName { get; }
    string realName { get; }
    string figure { get; }
    string gender { get; }
    bool nameChangeAllowed { get; }
    bool isAnyRoomController { get; }
    bool isAmbassador { get; }
    bool isEmailVerified { get; }
    bool isRoomCameraFollowDisabled { get; }
    int uiFlags { get; }
    string? mysteryBoxColor { get; }
    string? mysteryKeyColor { get; }

    // --- Respect ---
    int respectLeft { get; }
    int petRespectLeft { get; }
    void GiveStarGem(int userId);
    void GiveRespectFailed();
    void GivePetRespect(int petId);

    // --- Safety ---
    bool IsAccountSafetyLocked();

    // --- Ignored users ---
    bool IsIgnored(string userName);
    void IgnoreUser(string userName);
    void UnignoreUser(string userName);

    // --- Badge images ---
    Image? GetBadgeImage(string badgeId);
    Image? GetBadgeSmallImage(string badgeId);
    string? GetBadgeImageAssetName(string badgeId);
    string? GetBadgeImageSmallAssetName(string badgeId);
    Image? RequestBadgeImage(string badgeId);
    BadgeInfo? GetBadgeImageWithInfo(string badgeId);
    string? GetGroupBadgeId(int groupId);
    Image? GetGroupBadgeImage(string badgeId);
    Image? GetGroupBadgeSmallImage(string badgeId);
    string? GetGroupBadgeAssetName(string badgeId);
    string? GetGroupBadgeSmallAssetName(string badgeId);

    // --- Furniture / product data ---
    IProductData? GetProductData(string code);
    IFurnitureData? GetFloorItemData(int typeId);
    IFurnitureData[]? GetFloorItemsDataByCategory(int category);
    IFurnitureData? GetWallItemData(int typeId);
    IFurnitureData? GetFloorItemDataByName(string className, int index = 0);
    IFurnitureData? GetWallItemDataByName(string className, int index = 0);
    bool LoadProductData(IProductDataListener? listener = null);
    void GetFurniData(IFurniDataListener listener);
    void AddProductsReadyEventListener(IProductDataListener listener);
    void RemoveFurniDataListener(IFurniDataListener listener);

    // --- Perks ---
    bool perksReady { get; }
    bool IsPerkAllowed(string perk);
    string GetPerkErrorMessage(string perk);

    // --- UI preferences ---
    void SetRoomCameraFollowDisabled(bool disabled);
    void SetFriendBarState(bool open);
    void SetRoomToolsState(bool open);

    // --- Vault / rewards ---
    void GetCreditVaultStatus();
    void GetIncomeRewardStatus();
    void WithdrawCreditVault();
    void ClaimReward(int rewardId);

    // --- Room actions ---
    void OpenHabboHomePage(int userId, string name);
    void PickAllFurniture(int roomId);
    void EjectAllFurniture(int roomId, string ownerName);
    void EjectPets(int roomId);
    void PickAllBuilderFurniture(int roomId);
    void SendSpecialCommandMessage(string command);

    // --- Talent track ---
    string? currentTalentTrack { get; }
}
