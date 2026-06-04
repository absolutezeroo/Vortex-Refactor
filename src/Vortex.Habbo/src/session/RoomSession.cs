// @see com.sulake.habbo.session.RoomSession

using System;

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.RoomSettings;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Action;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Chat;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Furniture;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Moderation;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Pets;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Poll;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Session;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Users;

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
            {
                return;
            }

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
            {
                return SendPredefinedOpenConnection();
            }

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
        {
            return false;
        }

        _connection.Send(new OpenFlatConnectionMessageComposer(roomId, roomPassword));
        return true;
    }

    private bool SendPredefinedOpenConnection()
    {
        if (_connection == null)
        {
            return false;
        }

        _connection.Send(_openConnectionComposer!);
        _openConnectionComposer = null;
        return true;
    }

    /// @see RoomSession.as::sendChatMessage
    public void SendChatMessage(string text, int styleId = 0)
    {
        if (_connection == null)
        {
            return;
        }

        if (isGameSession)
        {
            _connection.Send(new Game2GameChatMessageComposer(text));
        }
        else
        {
            _connection.Send(new ChatMessageComposer(text, styleId, _chatMessageIndex));
        }

        _pendingChatMessages[_chatMessageIndex] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _chatMessageIndex++;
    }

    /// @see RoomSession.as::sendChangeMottoMessage
    public void SendChangeMottoMessage(string motto)
    {
        _connection?.Send(new ChangeMottoMessageComposer(motto));
    }

    /// @see RoomSession.as::sendShoutMessage
    public void SendShoutMessage(string text, int styleId = 0)
    {
        _connection?.Send(new ShoutMessageComposer(text, styleId));
    }

    /// @see RoomSession.as::sendWhisperMessage
    public void SendWhisperMessage(string recipientName, string text, int styleId = 0)
    {
        _connection?.Send(new WhisperMessageComposer(recipientName, text, styleId));
    }

    /// @see RoomSession.as::sendChatTypingMessage
    public void SendChatTypingMessage(bool isTyping)
    {
        if (_connection == null)
        {
            return;
        }

        if (isTyping)
        {
            _connection.Send(new TypingStartMessageComposer());
        }
        else
        {
            _connection.Send(new TypingStopMessageComposer());
        }
    }

    /// @see RoomSession.as::sendAvatarExpressionMessage
    public void SendAvatarExpressionMessage(int expression)
    {
        _connection?.Send(new AvatarExpressionMessageComposer(expression));
    }

    /// @see RoomSession.as::sendSignMessage
    public void SendSignMessage(int sign)
    {
        _connection?.Send(new SignMessageComposer(sign));
    }

    /// @see RoomSession.as::sendDanceMessage
    public void SendDanceMessage(int danceStyle)
    {
        _connection?.Send(new DanceMessageComposer(danceStyle));
    }

    /// @see RoomSession.as::sendChangePostureMessage
    public void SendChangePostureMessage(int posture)
    {
        _connection?.Send(new ChangePostureMessageComposer(posture));
    }

    /// @see RoomSession.as::sendCreditFurniRedeemMessage
    public void SendCreditFurniRedeemMessage(int stuffId)
    {
        _connection?.Send(new CreditFurniRedeemMessageComposer(stuffId));
    }

    /// @see RoomSession.as::sendPresentOpenMessage
    public void SendPresentOpenMessage(int stuffId)
    {
        _connection?.Send(new PresentOpenMessageComposer(stuffId));
    }

    /// @see RoomSession.as::sendOpenPetPackageMessage
    public void SendOpenPetPackageMessage(int objectId, string petName)
    {
        _connection?.Send(new OpenPetPackageMessageComposer(objectId, petName));
    }

    /// @see RoomSession.as::sendRoomDimmerGetPresetsMessage
    public void SendRoomDimmerGetPresetsMessage()
    {
        _connection?.Send(new GetRoomDimmerPresetsMessageComposer());
    }

    /// @see RoomSession.as::sendRoomDimmerSavePresetMessage
    public void SendRoomDimmerSavePresetMessage(int presetId, int type, uint color, int brightness, bool apply)
    {
        _connection?.Send(new RoomDimmerSavePresetMessageComposer(presetId, type, color, brightness, apply));
    }

    /// @see RoomSession.as::sendRoomDimmerChangeStateMessage
    public void SendRoomDimmerChangeStateMessage()
    {
        _connection?.Send(new RoomDimmerChangeStateMessageComposer());
    }

    /// @see RoomSession.as::sendConversionPoint
    public void SendConversionPoint(string category, string type, string action, string? label = null, int value = 0)
    {
        // TODO(as3-port): EventLogMessageComposer — payload format not yet verified from AS3 source
        _ = category; _ = type; _ = action; _ = label; _ = value;
    }

    /// @see RoomSession.as::sendPollStartMessage
    public void SendPollStartMessage(int pollId)
    {
        _connection?.Send(new PollStartComposer(pollId));
    }

    /// @see RoomSession.as::sendPollRejectMessage
    public void SendPollRejectMessage(int pollId)
    {
        _connection?.Send(new PollRejectComposer(pollId));
    }

    /// @see RoomSession.as::sendPollAnswerMessage
    public void SendPollAnswerMessage(int pollId, int questionId, IReadOnlyList<string> answers)
    {
        _connection?.Send(new PollAnswerComposer(pollId, questionId, answers));
    }

    /// @see RoomSession.as::sendPeerUsersClassificationMessage
    public void SendPeerUsersClassificationMessage(string classification)
    {
        _connection?.Send(new PeerUsersClassificationMessageComposer(classification));
    }

    /// @see RoomSession.as::sendRoomUsersClassificationMessage
    public void SendRoomUsersClassificationMessage(string classification)
    {
        _connection?.Send(new RoomUsersClassificationMessageComposer(classification));
    }

    /// @see RoomSession.as::sendVisitFlatMessage
    public void SendVisitFlatMessage(int roomId)
    {
        _connection?.Send(new OpenFlatConnectionMessageComposer(roomId));
    }

    /// @see RoomSession.as::sendVisitUserMessage
    public void SendVisitUserMessage(string userName)
    {
        _connection?.Send(new VisitUserMessageComposer(userName));
    }

    /// @see RoomSession.as::ambassadorAlert
    public void AmbassadorAlert(int userId)
    {
        _connection?.Send(new AmbassadorAlertMessageComposer(userId));
    }

    /// @see RoomSession.as::kickUser
    public void KickUser(int userId)
    {
        _connection?.Send(new KickUserMessageComposer(userId));
    }

    /// @see RoomSession.as::banUserWithDuration
    public void BanUserWithDuration(int userId, string duration)
    {
        _connection?.Send(new BanUserWithDurationMessageComposer(userId, duration));
    }

    /// @see RoomSession.as::muteUser
    public void MuteUser(int userId, int minutes)
    {
        _connection?.Send(new MuteUserMessageComposer(userId, minutes));
    }

    /// @see RoomSession.as::unmuteUser
    public void UnmuteUser(int userId)
    {
        _connection?.Send(new UnmuteUserMessageComposer(userId));
    }

    /// @see RoomSession.as::assignRights
    public void AssignRights(int userId)
    {
        _connection?.Send(new AssignRightsMessageComposer(userId));
    }

    /// @see RoomSession.as::removeRights
    public void RemoveRights(int userId)
    {
        _connection?.Send(new RemoveRightsMessageComposer(userId));
    }

    /// @see RoomSession.as::letUserIn
    public void LetUserIn(string userName, bool letIn)
    {
        _connection?.Send(new LetUserInMessageComposer(userName, letIn));
    }

    /// @see RoomSession.as::pickUpPet
    public void PickUpPet(int petId)
    {
        _connection?.Send(new RemovePetFromFlatMessageComposer(petId));
    }

    /// @see RoomSession.as::mountPet
    public void MountPet(int petId)
    {
        _connection?.Send(new MountPetMessageComposer(petId, true));
    }

    /// @see RoomSession.as::togglePetRidingPermission
    public void TogglePetRidingPermission(int petId)
    {
        _connection?.Send(new TogglePetRidingPermissionMessageComposer(petId));
    }

    /// @see RoomSession.as::dismountPet
    public void DismountPet(int petId)
    {
        _connection?.Send(new MountPetMessageComposer(petId, false));
    }

    /// @see RoomSession.as::removeSaddleFromPet
    public void RemoveSaddleFromPet(int petId)
    {
        _connection?.Send(new RemoveSaddleFromPetMessageComposer(petId));
    }

    /// @see RoomSession.as::harvestPet
    public void HarvestPet(int petId)
    {
        _connection?.Send(new HarvestPetMessageComposer(petId));
    }

    /// @see RoomSession.as::compostPlant
    public void CompostPlant(int stuffId)
    {
        _connection?.Send(new CompostPlantMessageComposer(stuffId));
    }

    /// @see RoomSession.as::requestPetCommands
    public void RequestPetCommands(int petId)
    {
        _connection?.Send(new GetPetCommandsMessageComposer(petId));
    }

    /// @see RoomSession.as::useProductForPet
    public void UseProductForPet(int stuffId, int petId)
    {
        _connection?.Send(new CustomizePetWithFurniComposer(stuffId, petId));
    }

    /// @see RoomSession.as::plantSeed
    public void PlantSeed(int stuffId)
    {
        // TODO(as3-port): UseFurnitureMessageComposer — verify correct use for planting seed
        _ = stuffId;
    }

    /// @see RoomSession.as::sendScriptProceed
    public void SendScriptProceed()
    {
        _connection?.Send(new NewUserExperienceScriptProceedComposer());
    }

    /// @see RoomSession.as::quit
    public void Quit()
    {
        _connection?.Send(new QuitMessageComposer());
    }

    /// @see RoomSession.as::changeQueue
    public void ChangeQueue(int queueId)
    {
        _connection?.Send(new ChangeQueueMessageComposer(queueId));
    }

    /// @see RoomSession.as::sendUpdateClothingChangeFurniture
    public void SendUpdateClothingChangeFurniture(int stuffId, string figure, string figureType)
    {
        _connection?.Send(new SetClothingChangeDataMessageComposer(stuffId, figure, figureType));
    }

    /// @see RoomSession.as::receivedChatWithTrackingId
    public void ReceivedChatWithTrackingId(int trackingId)
    {
        if (!_pendingChatMessages.Remove(trackingId, out long sentAt))
        {
            return;
        }

        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if ((now - sentAt) > CHAT_LAG_WARNING_LIMIT)
        {
            // TODO(as3-port): IHabboTracking.chatLagDetected not yet ported
        }
    }

    /// @see RoomSession.as::togglePetBreedingPermission
    public void TogglePetBreedingPermission(int petId)
    {
        _connection?.Send(new TogglePetBreedingPermissionMessageComposer(petId));
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
            {
                _roomControllerLevel = value;
            }
            else
            {
                _roomControllerLevel = 0;
            }
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
