// @see com.sulake.habbo.session.RoomSession

using System;

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.RoomSettings;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Session;

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.RoomSession
public class RoomSession : IRoomSession
{
    private const int CHAT_LAG_WARNING_LIMIT = 2500;

    private IConnection? _connection;
    private string _state = "RSE_CREATED";
    private IMessageComposer? _openConnectionComposer;
    private readonly UserDataManager _userDataManager;

    // @see RoomSession.as::_SafeStr_3711 (pending chat tracking Map)
    private readonly Dictionary<int, long> _pendingChatMessages = new();
    private int _chatMessageIndex = 0;

    /// @see RoomSession.as::RoomSession
    public RoomSession()
    {
        _userDataManager = new UserDataManager();
    }

    /// @see RoomSession.as::set connection
    public IConnection? connection
    {
        set
        {
            if (value == null)
                return;
            _connection = value;
            _userDataManager.connection = value;
        }
    }

    /// @see RoomSession.as::dispose
    public void Dispose()
    {
        _connection = null;
        _userDataManager.Dispose();
        _openConnectionComposer = null;
        _pendingChatMessages.Clear();
    }

    /// @see RoomSession.as::get roomId
    public int roomId { get; set; } = 0;

    /// @see RoomSession.as::get/set roomPassword (not on IRoomSession, used by RoomSessionManager)
    public string roomPassword { get; set; } = "";

    /// @see RoomSession.as::get/set roomResources
    public string roomResources { get; set; } = "";

    /// @see RoomSession.as::get/set openConnectionComposer (not on IRoomSession, used by RoomSessionManager)
    public IMessageComposer? openConnectionComposer
    {
        get => _openConnectionComposer;
        set => _openConnectionComposer = value;
    }

    /// @see RoomSession.as::get state
    public string state => _state;

    /// @see RoomSession.as::get/set isGameSession
    public bool isGameSession { get; set; } = false;

    /// @see RoomSession.as::get/set roomModerationSettings
    public RoomModerationSettings? roomModerationSettings { get; set; }

    /// @see RoomSession.as::start
    public bool Start()
    {
        if (state == "RSE_CREATED" && _connection != null)
        {
            _state = "RSE_STARTED";
            if (_openConnectionComposer != null)
                return SendPredefinedOpenConnection();
            return SendOpenFlatConnectionMessage();
        }
        return false;
    }

    /// @see RoomSession.as::reset
    public void Reset(int newRoomId)
    {
        if (newRoomId != roomId)
        {
            roomId = newRoomId;
            _areBotsAllowed = false;
            _roomControllerLevel = 0;
            _tradeMode = 0;
            _isSpectatorMode = false;
        }
    }

    private bool SendOpenFlatConnectionMessage()
    {
        if (_connection == null)
            return false;
        _connection.Send(new OpenFlatConnectionMessageComposer(roomId, roomPassword));
        return true;
    }

    private bool SendPredefinedOpenConnection()
    {
        if (_connection == null)
            return false;
        _connection.Send(_openConnectionComposer!);
        _openConnectionComposer = null;
        return true;
    }

    /// @see RoomSession.as::sendChatMessage
    public void SendChatMessage(string text, int styleId = 0)
    {
        // TODO(as3-port): Game2GameChatMessageComposer + ChatMessageComposer not yet ported
        _ = text; _ = styleId;
    }

    /// @see RoomSession.as::sendChangeMottoMessage
    public void SendChangeMottoMessage(string motto)
    {
        // TODO(as3-port): ChangeMottoMessageComposer not yet ported
        _ = motto;
    }

    /// @see RoomSession.as::sendShoutMessage
    public void SendShoutMessage(string text, int styleId = 0)
    {
        // TODO(as3-port): ShoutMessageComposer not yet ported
        _ = text; _ = styleId;
    }

    /// @see RoomSession.as::sendWhisperMessage
    public void SendWhisperMessage(string recipientName, string text, int styleId = 0)
    {
        // TODO(as3-port): WhisperMessageComposer not yet ported
        _ = recipientName; _ = text; _ = styleId;
    }

    /// @see RoomSession.as::sendChatTypingMessage
    public void SendChatTypingMessage(bool isTyping)
    {
        // TODO(as3-port): _SafeStr_52 (typing start) and _SafeStr_45 (typing stop) composers not yet ported
        _ = isTyping;
    }

    /// @see RoomSession.as::sendAvatarExpressionMessage
    public void SendAvatarExpressionMessage(int expression)
    {
        // TODO(as3-port): AvatarExpressionMessageComposer not yet ported
        _ = expression;
    }

    /// @see RoomSession.as::sendSignMessage
    public void SendSignMessage(int sign)
    {
        // TODO(as3-port): SignMessageComposer not yet ported
        _ = sign;
    }

    /// @see RoomSession.as::sendDanceMessage
    public void SendDanceMessage(int danceStyle)
    {
        // TODO(as3-port): DanceMessageComposer not yet ported
        _ = danceStyle;
    }

    /// @see RoomSession.as::sendChangePostureMessage
    public void SendChangePostureMessage(int posture)
    {
        // TODO(as3-port): ChangePostureMessageComposer not yet ported
        _ = posture;
    }

    /// @see RoomSession.as::sendCreditFurniRedeemMessage
    public void SendCreditFurniRedeemMessage(int stuffId)
    {
        // TODO(as3-port): CreditFurniRedeemMessageComposer not yet ported
        _ = stuffId;
    }

    /// @see RoomSession.as::sendPresentOpenMessage
    public void SendPresentOpenMessage(int stuffId)
    {
        // TODO(as3-port): PresentOpenMessageComposer not yet ported
        _ = stuffId;
    }

    /// @see RoomSession.as::sendOpenPetPackageMessage
    public void SendOpenPetPackageMessage(int objectId, string petName)
    {
        // TODO(as3-port): OpenPetPackageMessageComposer not yet ported
        _ = objectId; _ = petName;
    }

    /// @see RoomSession.as::sendRoomDimmerGetPresetsMessage
    public void SendRoomDimmerGetPresetsMessage()
    {
        // TODO(as3-port): _SafeStr_37 (GetRoomDimmerPresetsMessageComposer) not yet ported
    }

    /// @see RoomSession.as::sendRoomDimmerSavePresetMessage
    public void SendRoomDimmerSavePresetMessage(int presetId, int type, uint color, int brightness, bool apply)
    {
        // TODO(as3-port): RoomDimmerSavePresetMessageComposer not yet ported
        _ = presetId; _ = type; _ = color; _ = brightness; _ = apply;
    }

    /// @see RoomSession.as::sendRoomDimmerChangeStateMessage
    public void SendRoomDimmerChangeStateMessage()
    {
        // TODO(as3-port): _SafeStr_34 (RoomDimmerChangeStateMessageComposer) not yet ported
    }

    /// @see RoomSession.as::sendConversionPoint
    public void SendConversionPoint(string category, string type, string action, string? label = null, int value = 0)
    {
        // TODO(as3-port): EventLogMessageComposer not yet ported
        _ = category; _ = type; _ = action; _ = label; _ = value;
    }

    /// @see RoomSession.as::sendPollStartMessage
    public void SendPollStartMessage(int pollId)
    {
        // TODO(as3-port): PollStartComposer not yet ported
        _ = pollId;
    }

    /// @see RoomSession.as::sendPollRejectMessage
    public void SendPollRejectMessage(int pollId)
    {
        // TODO(as3-port): PollRejectComposer not yet ported
        _ = pollId;
    }

    /// @see RoomSession.as::sendPollAnswerMessage
    public void SendPollAnswerMessage(int pollId, int questionId, IReadOnlyList<string> answers)
    {
        // TODO(as3-port): PollAnswerComposer not yet ported
        _ = pollId; _ = questionId; _ = answers;
    }

    /// @see RoomSession.as::sendPeerUsersClassificationMessage
    public void SendPeerUsersClassificationMessage(string classification)
    {
        // TODO(as3-port): PeerUsersClassificationMessageComposer not yet ported
        _ = classification;
    }

    /// @see RoomSession.as::sendRoomUsersClassificationMessage
    public void SendRoomUsersClassificationMessage(string classification)
    {
        // TODO(as3-port): RoomUsersClassificationMessageComposer not yet ported
        _ = classification;
    }

    /// @see RoomSession.as::sendVisitFlatMessage
    public void SendVisitFlatMessage(int roomId)
    {
        _connection?.Send(new OpenFlatConnectionMessageComposer(roomId));
    }

    /// @see RoomSession.as::sendVisitUserMessage
    public void SendVisitUserMessage(string userName)
    {
        // TODO(as3-port): VisitUserMessageComposer not yet ported
        _ = userName;
    }

    /// @see RoomSession.as::ambassadorAlert
    public void AmbassadorAlert(int userId)
    {
        // TODO(as3-port): AmbassadorAlertMessageComposer not yet ported
        _ = userId;
    }

    /// @see RoomSession.as::kickUser
    public void KickUser(int userId)
    {
        // TODO(as3-port): KickUserMessageComposer not yet ported
        _ = userId;
    }

    /// @see RoomSession.as::banUserWithDuration
    public void BanUserWithDuration(int userId, string duration)
    {
        // TODO(as3-port): BanUserWithDurationMessageComposer not yet ported
        _ = userId; _ = duration;
    }

    /// @see RoomSession.as::muteUser
    public void MuteUser(int userId, int minutes)
    {
        // TODO(as3-port): MuteUserMessageComposer not yet ported
        _ = userId; _ = minutes;
    }

    /// @see RoomSession.as::unmuteUser
    public void UnmuteUser(int userId)
    {
        // TODO(as3-port): UnmuteUserMessageComposer not yet ported
        _ = userId;
    }

    /// @see RoomSession.as::assignRights
    public void AssignRights(int userId)
    {
        // TODO(as3-port): AssignRightsMessageComposer not yet ported
        _ = userId;
    }

    /// @see RoomSession.as::removeRights
    public void RemoveRights(int userId)
    {
        // TODO(as3-port): RemoveRightsMessageComposer not yet ported
        _ = userId;
    }

    /// @see RoomSession.as::letUserIn
    public void LetUserIn(string userName, bool letIn)
    {
        // TODO(as3-port): LetUserInMessageComposer not yet ported
        _ = userName; _ = letIn;
    }

    /// @see RoomSession.as::pickUpPet
    public void PickUpPet(int petId)
    {
        // TODO(as3-port): RemovePetFromFlatMessageComposer not yet ported
        _ = petId;
    }

    /// @see RoomSession.as::mountPet
    public void MountPet(int petId)
    {
        // TODO(as3-port): MountPetMessageComposer(petId, true) not yet ported
        _ = petId;
    }

    /// @see RoomSession.as::togglePetRidingPermission
    public void TogglePetRidingPermission(int petId)
    {
        // TODO(as3-port): TogglePetRidingPermissionMessageComposer not yet ported
        _ = petId;
    }

    /// @see RoomSession.as::dismountPet
    public void DismountPet(int petId)
    {
        // TODO(as3-port): MountPetMessageComposer(petId, false) not yet ported
        _ = petId;
    }

    /// @see RoomSession.as::removeSaddleFromPet
    public void RemoveSaddleFromPet(int petId)
    {
        // TODO(as3-port): RemoveSaddleFromPetMessageComposer not yet ported
        _ = petId;
    }

    /// @see RoomSession.as::harvestPet
    public void HarvestPet(int petId)
    {
        // TODO(as3-port): HarvestPetMessageComposer not yet ported
        _ = petId;
    }

    /// @see RoomSession.as::compostPlant
    public void CompostPlant(int stuffId)
    {
        // TODO(as3-port): CompostPlantMessageComposer not yet ported
        _ = stuffId;
    }

    /// @see RoomSession.as::requestPetCommands
    public void RequestPetCommands(int petId)
    {
        // TODO(as3-port): GetPetCommandsMessageComposer not yet ported
        _ = petId;
    }

    /// @see RoomSession.as::useProductForPet
    public void UseProductForPet(int stuffId, int petId)
    {
        // TODO(as3-port): CustomizePetWithFurniComposer not yet ported
        _ = stuffId; _ = petId;
    }

    /// @see RoomSession.as::plantSeed
    public void PlantSeed(int stuffId)
    {
        // TODO(as3-port): UseFurnitureMessageComposer not yet ported
        _ = stuffId;
    }

    /// @see RoomSession.as::sendScriptProceed
    public void SendScriptProceed()
    {
        // TODO(as3-port): NewUserExperienceScriptProceedComposer not yet ported
    }

    /// @see RoomSession.as::quit
    public void Quit()
    {
        // TODO(as3-port): _SafeStr_23 (QuitMessageComposer) not yet ported
    }

    /// @see RoomSession.as::changeQueue
    public void ChangeQueue(int queueId)
    {
        if (_connection == null)
            return;
        // TODO(as3-port): ChangeQueueMessageComposer not yet ported
        _ = queueId;
    }

    /// @see RoomSession.as::sendUpdateClothingChangeFurniture
    public void SendUpdateClothingChangeFurniture(int stuffId, string figure, string figureType)
    {
        if (_connection == null)
            return;
        // TODO(as3-port): SetClothingChangeDataMessageComposer not yet ported
        _ = stuffId; _ = figure; _ = figureType;
    }

    /// @see RoomSession.as::receivedChatWithTrackingId
    public void ReceivedChatWithTrackingId(int trackingId)
    {
        if (!_pendingChatMessages.Remove(trackingId, out long sentAt))
            return;
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if ((now - sentAt) > CHAT_LAG_WARNING_LIMIT)
        {
            // TODO(as3-port): IHabboTracking.chatLagDetected not yet ported
        }
    }

    /// @see RoomSession.as::togglePetBreedingPermission
    public void TogglePetBreedingPermission(int petId)
    {
        // TODO(as3-port): TogglePetBreedingPermissionMessageComposer not yet ported
        _ = petId;
    }

    /// @see RoomSession.as::trackEventLogOncePerSession
    public void TrackEventLogOncePerSession(string category, string action, string label)
    {
        // TODO(as3-port): IHabboTracking.trackEventLogOncePerSession not yet ported
        _ = category; _ = action; _ = label;
    }

    /// @see RoomSession.as::get userDataManager
    public IUserDataManager userDataManager => _userDataManager;

    /// @see RoomSession.as::get/set ownUserRoomId
    public int ownUserRoomId { get; set; } = -1;

    private bool _areBotsAllowed = false;

    /// @see RoomSession.as::get/set isRoomOwner (_areBotsAllowed field per source)
    public bool isRoomOwner
    {
        get => _areBotsAllowed;
        set => _areBotsAllowed = value;
    }

    private int _roomControllerLevel = 0;

    /// @see RoomSession.as::get/set roomControllerLevel
    public int roomControllerLevel
    {
        get => _roomControllerLevel;
        set
        {
            if (value >= 0 && value <= 5)
                _roomControllerLevel = value;
            else
                _roomControllerLevel = 0;
        }
    }

    /// @see RoomSession.as::get/set tradeMode
    private int _tradeMode = 0;
    public int tradeMode
    {
        get => _tradeMode;
        set => _tradeMode = value;
    }

    /// @see RoomSession.as::get isPrivateRoom (always true per source)
    public bool isPrivateRoom => true;

    /// @see RoomSession.as::get/set isGuildRoom
    public bool isGuildRoom { get; set; } = false;

    /// @see RoomSession.as::get isNoobRoom (_SafeStr_3710 == 4 per source)
    private int _doorMode;
    public bool isNoobRoom => _doorMode == 4;

    /// @see RoomSession.as::set doorMode (_SafeStr_3710 per source)
    public int doorMode
    {
        set => _doorMode = value;
    }

    private bool _isSpectatorMode = false;

    /// @see RoomSession.as::get/set isSpectatorMode
    public bool isSpectatorMode
    {
        get => _isSpectatorMode;
        set => _isSpectatorMode = value;
    }

    /// @see RoomSession.as::get/set arePetsAllowed
    public bool arePetsAllowed { get; set; } = false;

    /// @see RoomSession.as::get areBotsAllowed (_areBotsAllowed = isRoomOwner per source)
    public bool areBotsAllowed => _areBotsAllowed;

    /// @see RoomSession.as::get/set isUserDecorating
    public bool isUserDecorating { get; set; } = false;

    /// @see RoomSession.as::get/set isNuxNotComplete
    public bool isNuxNotComplete { get; set; } = false;
}
