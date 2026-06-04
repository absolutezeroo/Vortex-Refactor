// @see com.sulake.habbo.session.IRoomSession

using Vortex.Habbo.Communication.Messages.Incoming.RoomSettings;

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.IRoomSession
public interface IRoomSession
{
    /// @see IRoomSession.as::get roomId
    int roomId { get; }

    /// @see IRoomSession.as::get roomResources
    string roomResources { get; }

    /// @see IRoomSession.as::get state
    string state { get; }

    /// @see IRoomSession.as::start
    bool Start();

    /// @see IRoomSession.as::sendChatMessage
    void SendChatMessage(string text, int styleId = 0);

    /// @see IRoomSession.as::sendChangeMottoMessage
    void SendChangeMottoMessage(string motto);

    /// @see IRoomSession.as::sendShoutMessage
    void SendShoutMessage(string text, int styleId = 0);

    /// @see IRoomSession.as::sendWhisperMessage
    void SendWhisperMessage(string recipientName, string text, int styleId = 0);

    /// @see IRoomSession.as::sendChatTypingMessage
    void SendChatTypingMessage(bool isTyping);

    /// @see IRoomSession.as::sendAvatarExpressionMessage
    void SendAvatarExpressionMessage(int expression);

    /// @see IRoomSession.as::sendSignMessage
    void SendSignMessage(int sign);

    /// @see IRoomSession.as::sendDanceMessage
    void SendDanceMessage(int danceStyle);

    /// @see IRoomSession.as::sendChangePostureMessage
    void SendChangePostureMessage(int posture);

    /// @see IRoomSession.as::sendCreditFurniRedeemMessage
    void SendCreditFurniRedeemMessage(int stuffId);

    /// @see IRoomSession.as::sendPresentOpenMessage
    void SendPresentOpenMessage(int stuffId);

    /// @see IRoomSession.as::sendOpenPetPackageMessage
    void SendOpenPetPackageMessage(int objectId, string petName);

    /// @see IRoomSession.as::sendRoomDimmerGetPresetsMessage
    void SendRoomDimmerGetPresetsMessage();

    /// @see IRoomSession.as::sendRoomDimmerSavePresetMessage
    void SendRoomDimmerSavePresetMessage(int presetId, int type, uint color, int brightness, bool apply);

    /// @see IRoomSession.as::sendRoomDimmerChangeStateMessage
    void SendRoomDimmerChangeStateMessage();

    /// @see IRoomSession.as::sendConversionPoint
    void SendConversionPoint(string category, string type, string action, string? label = null, int value = 0);

    /// @see IRoomSession.as::sendPollStartMessage
    void SendPollStartMessage(int pollId);

    /// @see IRoomSession.as::sendPollRejectMessage
    void SendPollRejectMessage(int pollId);

    /// @see IRoomSession.as::sendPollAnswerMessage
    void SendPollAnswerMessage(int pollId, int questionId, IReadOnlyList<string> answers);

    /// @see IRoomSession.as::sendPeerUsersClassificationMessage
    void SendPeerUsersClassificationMessage(string classification);

    /// @see IRoomSession.as::sendRoomUsersClassificationMessage
    void SendRoomUsersClassificationMessage(string classification);

    /// @see IRoomSession.as::sendVisitFlatMessage
    void SendVisitFlatMessage(int roomId);

    /// @see IRoomSession.as::sendVisitUserMessage
    void SendVisitUserMessage(string userName);

    /// @see IRoomSession.as::ambassadorAlert
    void AmbassadorAlert(int userId);

    /// @see IRoomSession.as::kickUser
    void KickUser(int userId);

    /// @see IRoomSession.as::banUserWithDuration
    void BanUserWithDuration(int userId, string duration);

    /// @see IRoomSession.as::muteUser
    void MuteUser(int userId, int minutes);

    /// @see IRoomSession.as::unmuteUser
    void UnmuteUser(int userId);

    /// @see IRoomSession.as::assignRights
    void AssignRights(int userId);

    /// @see IRoomSession.as::removeRights
    void RemoveRights(int userId);

    /// @see IRoomSession.as::letUserIn
    void LetUserIn(string userName, bool letIn);

    /// @see IRoomSession.as::pickUpPet
    void PickUpPet(int petId);

    /// @see IRoomSession.as::mountPet
    void MountPet(int petId);

    /// @see IRoomSession.as::togglePetRidingPermission
    void TogglePetRidingPermission(int petId);

    /// @see IRoomSession.as::dismountPet
    void DismountPet(int petId);

    /// @see IRoomSession.as::removeSaddleFromPet
    void RemoveSaddleFromPet(int petId);

    /// @see IRoomSession.as::requestPetCommands
    void RequestPetCommands(int petId);

    /// @see IRoomSession.as::useProductForPet
    void UseProductForPet(int stuffId, int petId);

    /// @see IRoomSession.as::plantSeed
    void PlantSeed(int stuffId);

    /// @see IRoomSession.as::quit
    void Quit();

    /// @see IRoomSession.as::changeQueue
    void ChangeQueue(int queueId);

    /// @see IRoomSession.as::sendUpdateClothingChangeFurniture
    void SendUpdateClothingChangeFurniture(int stuffId, string figure, string figureType);

    /// @see IRoomSession.as::receivedChatWithTrackingId
    void ReceivedChatWithTrackingId(int trackingId);

    /// @see IRoomSession.as::get/set ownUserRoomId
    int ownUserRoomId { get; set; }

    /// @see IRoomSession.as::get/set isRoomOwner
    bool isRoomOwner { get; set; }

    /// @see IRoomSession.as::get/set roomControllerLevel
    int roomControllerLevel { get; set; }

    /// @see IRoomSession.as::get/set isGuildRoom
    bool isGuildRoom { get; set; }

    /// @see IRoomSession.as::get isNoobRoom
    bool isNoobRoom { get; }

    /// @see IRoomSession.as::set doorMode
    int doorMode { set; }

    /// @see IRoomSession.as::get/set tradeMode
    int tradeMode { get; set; }

    /// @see IRoomSession.as::get isPrivateRoom
    bool isPrivateRoom { get; }

    /// @see IRoomSession.as::get userDataManager
    IUserDataManager userDataManager { get; }

    /// @see IRoomSession.as::get/set isSpectatorMode
    bool isSpectatorMode { get; set; }

    /// @see IRoomSession.as::get/set arePetsAllowed
    bool arePetsAllowed { get; set; }

    /// @see IRoomSession.as::get areBotsAllowed
    bool areBotsAllowed { get; }

    /// @see IRoomSession.as::get/set isUserDecorating
    bool isUserDecorating { get; set; }

    /// @see IRoomSession.as::get isGameSession
    bool isGameSession { get; }

    /// @see IRoomSession.as::get/set roomModerationSettings
    RoomModerationSettings? roomModerationSettings { get; set; }

    /// @see IRoomSession.as::get/set isNuxNotComplete
    bool isNuxNotComplete { get; set; }

    /// @see IRoomSession.as::harvestPet
    void HarvestPet(int petId);

    /// @see IRoomSession.as::togglePetBreedingPermission
    void TogglePetBreedingPermission(int petId);

    /// @see IRoomSession.as::compostPlant
    void CompostPlant(int stuffId);

    /// @see IRoomSession.as::sendScriptProceed
    void SendScriptProceed();

    /// @see IRoomSession.as::trackEventLogOncePerSession
    void TrackEventLogOncePerSession(string category, string action, string label);
}
