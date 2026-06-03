// @see com.sulake.habbo.session.SessionDataManager

using System.Linq;

using Godot;

using Vortex.Core.Communication.Messages;
using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;
using Vortex.Habbo.Communication;
using Vortex.Habbo.Communication.Messages.Incoming.Handshake;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;
using Vortex.Habbo.Session.Events;
using Vortex.Habbo.Session.Furniture;
using Vortex.Habbo.Session.Product;
using Vortex.IID;

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.SessionDataManager
public class SessionDataManager : Component, ISessionDataManager
{
    // --- User identity ---
    private int _userId;
    private string _userName = "";
    private string _figure = "";
    private string _gender = "";
    private string _realName = "";

    // --- Respect ---
    private int _respectLeft;
    private int _petRespectLeft;

    // --- Status flags ---
    private bool _systemOpen;
    private bool _systemShutDown;
    private bool _isAuthenticHabbo;
    private bool _nameChangeAllowed = true;
    private bool _isAmbassador;
    private bool _isEmailVerified;
    private bool _isRoomCameraFollowDisabled;
    private bool _isAccountSafetyLocked;
    private int _uiFlags;
    private string? _mysteryBoxColor;
    private string? _mysteryKeyColor;
    private string? _currentTalentTrack;

    // --- Club / security ---
    private int _clubLevel;
    private int _topSecurityLevel;

    // --- Injected managers (via ComponentDependency) ---
    private object? _communicationObj;
    private object? _windowManagerObj;
    private object? _localizationObj;
    private object? _roomSessionManagerObj;

    // --- Sub-managers ---
    private BadgeImageManager? _badgeImageManager;
    private HabboGroupInfoManager? _groupInfoManager;
    private IgnoredUsersManager? _ignoredUsersManager;
    private PerkManager? _perkManager;
    private FurnitureDataParser? _furniDataParser;
    private ProductDataParser? _productDataParser;

    // --- Data stores ---
    private readonly Dictionary<int, IFurnitureData> _floorItems = new();
    private readonly Dictionary<int, IFurnitureData> _wallItems = new();
    private readonly Dictionary<string, List<int>> _classNameMap = new();
    private readonly Dictionary<string, IProductData> _products = new();

    // --- Furni data listeners ---
    private readonly List<IFurniDataListener> _furniDataListeners = new();
    private bool _furniDataReady;
    private readonly List<IProductDataListener> _productDataListeners = new();
    private bool _productDataReady;

    // --- Wired message events (for removal on dispose) ---
    private IMessageEvent? _userObjectEvent;
    private IMessageEvent? _userChangeEvent;

    // TODO(communication): Wire once ported:
    // private IMessageEvent? _userRightsEvent;           // sets clubLevel, topSecurityLevel
    // private IMessageEvent? _figureUpdateEvent;          // updates avatar figure
    // private IMessageEvent? _noobnessLevelEvent;         // sets isNoob flags
    // private IMessageEvent? _changeUserNameResultEvent;  // username change result
    // private IMessageEvent? _accountSafetyLockEvent;     // sets _isAccountSafetyLocked
    // private IMessageEvent? _emailStatusEvent;           // sets _isEmailVerified
    // private IMessageEvent? _catalogPublishedEvent;      // catalog availability
    // private IMessageEvent? _accountPreferencesEvent;    // uiFlags, camera follow, toolbar state
    // private IMessageEvent? _userNameChangedEvent;       // propagates UserNameUpdateEvent
    // private IMessageEvent? _inClientLinkEvent;          // in-client link navigation
    // private IMessageEvent? _petRespectFailedEvent;      // increments _petRespectLeft
    // private IMessageEvent? _mysteryBoxKeysEvent;        // sets _mysteryBoxColor/_mysteryKeyColor
    // private IMessageEvent? _availabilityStatusEvent;    // sets _systemOpen/_systemShutDown

    /// @see SessionDataManager.as::SessionDataManager
    public SessionDataManager(IContext param1, uint param2 = 0, object? param3 = null)
        : base(param1, param2 | COMPONENT_FLAG_DISPOSABLE, param3)
    {
        RegisterInterface(new IIDSessionDataManager(), this);
    }

    // --- IUnknown / Component ---

    /// @see SessionDataManager.as::get dependencies
    protected override IList<ComponentDependency> dependencies =>
        new List<ComponentDependency>(base.dependencies)
        {
            new(new IIDHabboCommunicationManager(), p => _communicationObj = p, true),
            new(new IIDHabboWindowManager(),        p => _windowManagerObj = p, false),
            new(new IIDHabboLocalizationManager(),  p => _localizationObj = p, false),
            new(new IIDHabboRoomSessionManager(),   p => _roomSessionManagerObj = p, false),
        };

    /// @see SessionDataManager.as::initComponent
    protected override void InitComponent()
    {
        _furniDataParser = new FurnitureDataParser(_floorItems, _wallItems, _classNameMap);
        _furniDataParser.events.AddEventListener(FurnitureDataParser.READY, OnFurniDataReady);

        _productDataParser = new ProductDataParser(_products);
        _productDataParser.events.AddEventListener(ProductDataParser.READY, OnProductDataReady);

        _badgeImageManager = new BadgeImageManager(events);

        // Configure badge URLs from configuration
        // TODO(config): Use actual ICoreConfiguration once IIDHabboConfigurationManager is injected.
        // _badgeImageManager.Configure(GetProperty("image.library.url"), GetProperty("group.badge.url"));

        _groupInfoManager = new HabboGroupInfoManager(this);
        _ignoredUsersManager = new IgnoredUsersManager(this);
        _ignoredUsersManager.InitIgnoreList();
        _perkManager = new PerkManager(this);

        if (communication != null)
        {
            _userObjectEvent = communication.AddHabboConnectionMessageEvent(
                new UserObjectEvent(OnUserObject));
            _userChangeEvent = communication.AddHabboConnectionMessageEvent(
                new UserChangeMessageEvent(OnUserChange));

            // TODO(communication): Register remaining events once ported (see field TODO block above).
        }
    }

    /// @see SessionDataManager.as::dispose
    public override void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (communication != null)
        {
            if (_userObjectEvent != null)  { communication.RemoveHabboConnectionMessageEvent(_userObjectEvent); _userObjectEvent = null; }
            if (_userChangeEvent != null)  { communication.RemoveHabboConnectionMessageEvent(_userChangeEvent); _userChangeEvent = null; }
            // TODO(communication): Remove remaining events when wired.
        }

        _perkManager?.Dispose();
        _ignoredUsersManager?.Dispose();
        _groupInfoManager?.Dispose();
        _badgeImageManager?.Dispose();
        _furniDataParser?.Dispose();
        _productDataParser?.Dispose();

        _furniDataListeners.Clear();
        _productDataListeners.Clear();
        _floorItems.Clear();
        _wallItems.Clear();
        _classNameMap.Clear();
        _products.Clear();

        base.Dispose();
    }

    // --- Internal accessors for sub-managers ---

    internal IHabboCommunicationManager? communication =>
        _communicationObj as IHabboCommunicationManager;

    // --- ISessionDataManager implementation ---

    public bool systemOpen => _systemOpen;
    public bool systemShutDown => _systemShutDown;
    public bool isAuthenticHabbo => _isAuthenticHabbo;
    public bool hasSecurity(int level)
    {
        return _topSecurityLevel >= level;
    }

    public int topSecurityLevel => _topSecurityLevel;

    public int clubLevel => _clubLevel;
    public bool hasVip => _clubLevel >= 2;
    public bool hasClub => _clubLevel > 0;
    public bool isNoob => _clubLevel == 0 && _topSecurityLevel < 2;
    public bool isRealNoob => isNoob && _topSecurityLevel < 2;

    public int userId => _userId;
    public string userName => _userName;
    public string realName => _realName;
    public string figure => _figure;
    public string gender => _gender;
    public bool nameChangeAllowed => _nameChangeAllowed;
    public bool isAnyRoomController => _topSecurityLevel >= 2 || _isAmbassador;
    public bool isAmbassador => _isAmbassador;
    public bool isEmailVerified => _isEmailVerified;
    public bool isRoomCameraFollowDisabled => _isRoomCameraFollowDisabled;
    public int uiFlags => _uiFlags;
    public string? mysteryBoxColor => _mysteryBoxColor;
    public string? mysteryKeyColor => _mysteryKeyColor;
    public string? currentTalentTrack => _currentTalentTrack;
    public bool perksReady => _perkManager?.isReady ?? false;

    public int respectLeft => _respectLeft;
    public int petRespectLeft => _petRespectLeft;

    public bool IsAccountSafetyLocked()
    {
        return _isAccountSafetyLocked;
    }

    // --- Ignored users ---

    public bool IsIgnored(string userName)
    {
        return _ignoredUsersManager?.IsIgnored(userName) ?? false;
    }

    public void IgnoreUser(string userName)
    {
        _ignoredUsersManager?.IgnoreUser(userName);
    }

    public void UnignoreUser(string userName)
    {
        _ignoredUsersManager?.UnignoreUser(userName);
    }

    // --- Respect ---

    /// @see SessionDataManager.as::giveStarGem
    /// TODO(communication): Send GiveStarGemToUserMessageComposer once ported.
    public void GiveStarGem(int userId)
    {
        // TODO(communication): Send(new GiveStarGemToUserMessageComposer(userId));
    }

    /// @see SessionDataManager.as::giveRespectFailed
    public void GiveRespectFailed()
    {
        _respectLeft++;
    }

    /// @see SessionDataManager.as::givePetRespect
    /// TODO(communication): Send RespectPetMessageComposer once ported.
    public void GivePetRespect(int petId)
    {
        if (_petRespectLeft <= 0)
        {
            return;
        }

        _petRespectLeft--;
        // TODO(communication): Send(new RespectPetMessageComposer(petId));
    }

    // --- Badge images ---

    public Image? GetBadgeImage(string badgeId)
    {
        return _badgeImageManager?.GetBadgeImage(badgeId);
    }

    public Image? GetBadgeSmallImage(string badgeId)
    {
        return _badgeImageManager?.GetSmallBadgeImage(badgeId);
    }

    public string? GetBadgeImageAssetName(string badgeId)
    {
        return _badgeImageManager?.GetBadgeImageAssetName(badgeId);
    }

    public string? GetBadgeImageSmallAssetName(string badgeId)
    {
        return _badgeImageManager?.GetSmallScaleBadgeAssetName(badgeId);
    }

    public Image? RequestBadgeImage(string badgeId)
    {
        return _badgeImageManager?.GetBadgeImage(badgeId, BadgeImageManager.TYPE_NORMAL, false);
    }

    public BadgeInfo? GetBadgeImageWithInfo(string badgeId)
    {
        return _badgeImageManager?.GetBadgeImageWithInfo(badgeId);
    }

    public string? GetGroupBadgeId(int groupId)
    {
        return _groupInfoManager?.GetBadgeId(groupId);
    }

    public Image? GetGroupBadgeImage(string badgeId)
    {
        return _badgeImageManager?.GetBadgeImage(badgeId, BadgeImageManager.TYPE_GROUP);
    }

    public Image? GetGroupBadgeSmallImage(string badgeId)
    {
        return _badgeImageManager?.GetSmallBadgeImage(badgeId, BadgeImageManager.TYPE_GROUP);
    }

    public string? GetGroupBadgeAssetName(string badgeId)
    {
        return _badgeImageManager?.GetBadgeImageAssetName(badgeId, BadgeImageManager.TYPE_GROUP);
    }

    public string? GetGroupBadgeSmallAssetName(string badgeId)
    {
        return _badgeImageManager?.GetSmallScaleBadgeAssetName(badgeId, BadgeImageManager.TYPE_GROUP);
    }

    // --- Furniture / product data ---

    public IProductData? GetProductData(string code)
    {
        return _products.TryGetValue(code, out IProductData? data) ? data : null;
    }

    public IFurnitureData? GetFloorItemData(int typeId)
    {
        return _floorItems.TryGetValue(typeId, out IFurnitureData? data) ? data : null;
    }

    public IFurnitureData[]? GetFloorItemsDataByCategory(int category)
    {
        IFurnitureData[] result = _floorItems.Values
            .Where(f => f.category == category)
            .ToArray();
        return result.Length > 0 ? result : null;
    }

    public IFurnitureData? GetWallItemData(int typeId)
    {
        return _wallItems.TryGetValue(typeId, out IFurnitureData? data) ? data : null;
    }

    public IFurnitureData? GetFloorItemDataByName(string className, int index = 0)
    {
        if (!_classNameMap.TryGetValue(className, out List<int>? ids))
        {
            return null;
        }

        if (index >= ids.Count || ids[index] == 0)
        {
            return null;
        }

        return GetFloorItemData(ids[index]);
    }

    public IFurnitureData? GetWallItemDataByName(string className, int index = 0)
    {
        if (!_classNameMap.TryGetValue(className, out List<int>? ids))
        {
            return null;
        }

        if (index >= ids.Count || ids[index] == 0)
        {
            return null;
        }

        return GetWallItemData(ids[index]);
    }

    /// @see SessionDataManager.as::loadProductData
    public bool LoadProductData(IProductDataListener? listener = null)
    {
        if (_productDataReady)
        {
            listener?.ProductDataReceived();
            return true;
        }

        if (listener != null)
        {
            AddProductsReadyEventListener(listener);
        }

        // TODO(assets): Call _productDataParser.LoadData(url) once URL is available from config.
        return false;
    }

    /// @see SessionDataManager.as::getFurniData
    public void GetFurniData(IFurniDataListener listener)
    {
        if (_furniDataReady)
        {
            listener.FurniDataReceived();
            return;
        }

        if (!_furniDataListeners.Contains(listener))
        {
            _furniDataListeners.Add(listener);
        }
        // TODO(assets): Call _furniDataParser.LoadData(url) once URL is available from config.
    }

    /// @see SessionDataManager.as::addProductsReadyEventListener
    public void AddProductsReadyEventListener(IProductDataListener listener)
    {
        if (!_productDataListeners.Contains(listener))
        {
            _productDataListeners.Add(listener);
        }
    }

    /// @see SessionDataManager.as::removeFurniDataListener
    public void RemoveFurniDataListener(IFurniDataListener listener)
    {
        _furniDataListeners.Remove(listener);
    }

    // --- Perks ---

    public bool IsPerkAllowed(string perk)
    {
        return _perkManager?.IsPerkAllowed(perk) ?? false;
    }

    public string GetPerkErrorMessage(string perk)
    {
        return _perkManager?.GetPerkErrorMessage(perk) ?? "";
    }

    // --- UI preferences ---

    /// @see SessionDataManager.as::setRoomCameraFollowDisabled
    /// TODO(communication): Send SetUIFlagsMessageComposer once ported.
    public void SetRoomCameraFollowDisabled(bool disabled)
    {
        _isRoomCameraFollowDisabled = disabled;
        // TODO(communication): Send(new SetUIFlagsMessageComposer(...));
    }

    /// @see SessionDataManager.as::setFriendBarState
    /// TODO(communication): Send SetUIFlagsMessageComposer once ported.
    public void SetFriendBarState(bool open)
    {
        // TODO(communication): Update _uiFlags bitmask and Send(new SetUIFlagsMessageComposer(_uiFlags));
    }

    /// @see SessionDataManager.as::setRoomToolsState
    /// TODO(communication): Send SetUIFlagsMessageComposer once ported.
    public void SetRoomToolsState(bool open)
    {
        // TODO(communication): Update _uiFlags bitmask and Send(new SetUIFlagsMessageComposer(_uiFlags));
    }

    // --- Vault / rewards ---

    /// TODO(communication): Send vault composers once ported.
    public void GetCreditVaultStatus()   { /* TODO(communication): Send(new CreditVaultStatusMessageComposer()); */ }
    public void GetIncomeRewardStatus()  { /* TODO(communication): Send(new IncomeRewardStatusMessageComposer()); */ }
    public void WithdrawCreditVault()    { /* TODO(communication): Send(new WithdrawCreditVaultMessageComposer()); */ }
    public void ClaimReward(int rewardId){ /* TODO(communication): Send(new IncomeRewardClaimMessageComposer(rewardId)); */ }

    // --- Room actions ---

    /// @see SessionDataManager.as::openHabboHomePage
    /// TODO(window): Open URL via IHabboWindowManager or HabboWebTools once available.
    public void OpenHabboHomePage(int userId, string name)
    {
        // TODO(window): _windowManager.openUrl(HabboWebTools.GetUserHomepageUrl(userId, name));
    }

    /// @see SessionDataManager.as::pickAllFurniture
    /// TODO(window): Show confirmation dialog via IHabboWindowManager before sending composer.
    public void PickAllFurniture(int roomId)
    {
        // TODO(window): Show confirmation, then Send(new PickAllFurnitureMessageComposer(roomId));
    }

    /// @see SessionDataManager.as::ejectAllFurniture
    /// TODO(window): Show confirmation dialog.
    public void EjectAllFurniture(int roomId, string ownerName)
    {
        // TODO(window): Show confirmation, then Send(new EjectAllFurnitureMessageComposer(roomId));
    }

    /// @see SessionDataManager.as::ejectPets
    public void EjectPets(int roomId)
    {
        // TODO(communication): Send(new EjectPetsMessageComposer(roomId));
    }

    /// @see SessionDataManager.as::pickAllBuilderFurniture
    public void PickAllBuilderFurniture(int roomId)
    {
        // TODO(communication): Send(new PickAllBuilderFurnitureMessageComposer(roomId));
    }

    /// @see SessionDataManager.as::sendSpecialCommandMessage
    public void SendSpecialCommandMessage(string command)
    {
        // TODO(communication): Send(new ChatMessageComposer(":" + command, ChatStyleEnum.NORMAL, 0));
    }

    // --- Helpers ---

    /// Send a composer through the communication layer.
    internal void Send(IMessageComposer composer)
    {
        communication?.connection?.Send(composer);
    }

    // --- Private event handlers ---

    /// @see SessionDataManager.as::onUserObject
    private void OnUserObject(IMessageEvent param1)
    {
        UserObjectEvent ev = (UserObjectEvent)param1;
        _userId = ev.id;
        _userName = ev.name;
        _figure = ev.figure;
        _gender = ev.sex;
        // TODO(communication): Also read realName, respectLeft, petRespectLeft,
        // nameChangeAllowed from UserObjectMessageEventParser once extended.
    }

    /// @see SessionDataManager.as::onUserChange (UserChangeMessageEvent)
    private void OnUserChange(IMessageEvent param1)
    {
        UserChangeMessageEvent ev = (UserChangeMessageEvent)param1;

        if (ev.id == _userId)
        {
            _figure = ev.figure;
            _gender = ev.sex;
        }
    }

    /// @see SessionDataManager.as — furni data ready callback
    private void OnFurniDataReady(object? _)
    {
        _furniDataReady = true;
        foreach (IFurniDataListener listener in _furniDataListeners.ToList())
        {
            listener.FurniDataReceived();
        }
        _furniDataListeners.Clear();
    }

    /// @see SessionDataManager.as — product data ready callback
    private void OnProductDataReady(object? _)
    {
        _productDataReady = true;
        foreach (IProductDataListener listener in _productDataListeners.ToList())
        {
            listener.ProductDataReceived();
        }
        _productDataListeners.Clear();
    }
}
