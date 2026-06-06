using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Availability;
using Vortex.Habbo.Communication.Messages.Incoming.Avatar;
using Vortex.Habbo.Communication.Messages.Incoming.Competition;
using Vortex.Habbo.Communication.Messages.Incoming.Error;
using Vortex.Habbo.Communication.Messages.Incoming.Handshake;
using Vortex.Habbo.Communication.Messages.Incoming.Help;
using Vortex.Habbo.Communication.Messages.Incoming.Navigator;
using Vortex.Habbo.Communication.Messages.Incoming.Preferences;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Action;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Chat;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Data;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Furniture;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Layout;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Permissions;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Pets;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Poll;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Quiz;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Session;
using Vortex.Habbo.Communication.Messages.Incoming.Users;
using Vortex.Habbo.Communication.Messages.Outgoing.Handshake;
using Vortex.Habbo.Communication.Messages.Outgoing.Competition;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Action;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Chat;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Engine;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Furniture;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Layout;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Moderation;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Pets;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Poll;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Session;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Users;
using Vortex.Habbo.Communication.Messages.Outgoing.Tracking;

namespace Vortex.Habbo.Communication;

// IDs aligned to Turbo Revision20260112 (server/Turbo.Revisions/Revision20260112/Headers.cs)
// events[] = server→client (MessageComposer in Headers.cs)
// composers[] = client→server (MessageEvent in Headers.cs)
public class HabboMessages : IMessageConfiguration
{
    public HabboMessages()
    {
        // --- Handshake / System ---
        events[2305] = typeof(UserObjectEvent);
        events[2585] = typeof(IdentityAccountsEvent);
        events[3014] = typeof(AuthenticationOkMessageEvent);
        events[184]  = typeof(MaintenanceStatusMessageEvent);
        events[2761] = typeof(LoginFailedHotelClosedMessageEvent);
        events[3034] = typeof(CompleteDiffieHandshakeEvent);
        events[744]  = typeof(FigureUpdateMessageEvent); // FigureUpdateComposer = 744
        events[836]  = typeof(UniqueMachineIdEvent);    // UniqueMachineIDComposer = 836
        events[4000] = typeof(DisconnectReasonEvent);
        events[195]  = typeof(GenericErrorEvent);
        events[1790] = typeof(ErrorReportEvent);
        events[2449] = typeof(PingMessageEvent);
        events[2334] = typeof(InitDiffieHandshakeEvent);

        // --- Room Session ---
        events[2244] = typeof(RoomReadyMessageEvent);
        events[1915] = typeof(OpenConnectionMessageEvent);
        events[3241] = typeof(CloseConnectionMessageEvent);
        events[3731] = typeof(FlatAccessibleMessageEvent);
        events[671]  = typeof(CantConnectMessageEvent);
        events[2309] = typeof(GamePlayerValueMessageEvent);
        events[677]  = typeof(YouArePlayingGameMessageEvent);
        events[3216] = typeof(YouAreSpectatorMessageEvent);
        events[1856] = typeof(YouAreNotSpectatorMessageEvent);
        events[1111] = typeof(RoomQueueStatusMessageEvent);
        events[3678] = typeof(RoomForwardMessageEvent);
        events[3768] = typeof(HanditemConfigurationMessageEvent);

        // --- Room Engine: Objects ---
        events[3997] = typeof(ObjectsMessageEvent);
        events[3829] = typeof(ObjectAddMessageEvent);
        events[970]  = typeof(ObjectUpdateMessageEvent);
        events[2963] = typeof(ObjectRemoveMessageEvent);
        events[391]  = typeof(ObjectDataUpdateMessageEvent);
        events[653]  = typeof(ObjectsDataUpdateMessageEvent);
        events[1065] = typeof(ObjectRemoveMultipleMessageEvent);
        events[3631] = typeof(ObjectRemoveConfirmMessageEvent);

        // --- Room Engine: Items ---
        events[3255] = typeof(ItemsMessageEvent);
        events[2579] = typeof(ItemAddMessageEvent);
        events[934]  = typeof(ItemUpdateMessageEvent);
        events[1903] = typeof(ItemRemoveMessageEvent);
        events[2402] = typeof(ItemStateUpdateMessageEvent);
        events[2028] = typeof(ItemDataUpdateMessageEvent);
        events[627]  = typeof(ItemsStateUpdateMessageEvent);

        // --- Room Engine: Users ---
        events[1835] = typeof(UsersMessageEvent);
        events[3173] = typeof(UserChangeMessageEvent);
        events[833]  = typeof(UserRemoveMessageEvent);
        events[534]  = typeof(UserUpdateMessageEvent);

        // --- Room Engine: Misc ---
        events[369]  = typeof(SlideObjectBundleMessageEvent);
        events[639]  = typeof(RoomPropertyMessageEvent);
        events[562]  = typeof(RoomVisualizationSettingsEvent);
        events[234]  = typeof(FurnitureAliasesMessageEvent);
        events[1120] = typeof(RoomEntryInfoMessageEvent);
        events[3161] = typeof(SpecialRoomEffectMessageEvent);
        events[1721] = typeof(HeightMapMessageEvent);
        events[2724] = typeof(FloorHeightMapMessageEvent);
        events[3175] = typeof(HeightMapUpdateMessageEvent);
        events[233]  = typeof(FavoriteMembershipUpdateMessageEvent);
        events[1998] = typeof(WiredMovementsMessageEvent);
        events[90]   = typeof(BuildersClubPlacementWarningMessageEvent);

        // --- Room Action ---
        events[2555] = typeof(AvatarEffectMessageEvent);
        events[1104] = typeof(CarryObjectMessageEvent);
        events[2910] = typeof(DanceMessageEvent);
        events[1783] = typeof(ExpressionMessageEvent);
        events[3524] = typeof(SleepMessageEvent);
        events[2833] = typeof(UseObjectMessageEvent);

        // --- Room Chat ---
        events[1264] = typeof(ChatMessageEvent);
        events[3310] = typeof(ShoutMessageEvent);
        events[492]  = typeof(WhisperMessageEvent);
        events[2514] = typeof(UserTypingMessageEvent);
        events[1356] = typeof(FloodControlMessageEvent);
        events[2623] = typeof(RemainingMutePeriodEvent);
        events[3156] = typeof(RoomChatSettingsMessageEvent);
        events[1328] = typeof(RoomFilterSettingsMessageEvent);

        // --- Room Permissions ---
        events[168]  = typeof(YouAreControllerMessageEvent);
        events[3352] = typeof(YouAreNotControllerMessageEvent);
        events[2791] = typeof(YouAreOwnerMessageEvent);

        // --- Room Layout ---
        events[2777] = typeof(RoomEntryTileMessageEvent);
        events[1373] = typeof(RoomOccupiedTilesMessageEvent);

        // --- Room Pets ---
        events[2466] = typeof(PetExperienceEvent);
        events[3016] = typeof(PetFigureUpdateEvent);
        events[1079] = typeof(PetInfoMessageEvent);
        events[3811] = typeof(PetCommandsMessageEvent);
        events[3240] = typeof(PetPlacingErrorEvent);
        events[1644] = typeof(PetBreedingResultEvent);
        events[95]   = typeof(PetBreedingEvent);
        events[249]  = typeof(PetStatusUpdateEvent);
        events[2560] = typeof(PetLevelUpdateEvent);
        events[730]  = typeof(ConfirmBreedingRequestEvent);
        events[999]  = typeof(ConfirmBreedingResultEvent);
        events[1127] = typeof(NestBreedingSuccessEvent);
        events[1428] = typeof(OpenPetPackageRequestedMessageEvent);
        events[3835] = typeof(OpenPetPackageResultMessageEvent);

        // --- Room Engine: User extras ---
        events[1968] = typeof(HabboUserBadgesMessageEvent);
        events[2505] = typeof(DoorbellMessageEvent);
        events[906]  = typeof(UserNameChangedMessageEvent);
        events[1775] = typeof(BotErrorEvent);
        events[1515] = typeof(NewFriendRequestEvent);

        // --- Room Chat extras ---
        events[486]  = typeof(RespectNotificationMessageEvent);
        events[2652] = typeof(PetRespectNotificationEvent);
        events[2204] = typeof(PetSupplementedNotificationEvent);
        events[2144] = typeof(HandItemReceivedMessageEvent);

        // --- Room Session extras ---
        events[2007] = typeof(FlatAccessDeniedMessageEvent);

        // --- Room Data ---
        events[2582] = typeof(GetGuestRoomResultEvent);

        // --- Session Data ---
        events[3337] = typeof(UserRightsMessageEvent);           // @see HabboMessages.as: _events[1416] = class_143
        events[782] = typeof(NoobnessLevelMessageEvent);        // @see HabboMessages.as: _events[1916] = NoobnessLevelMessageEvent
        events[995] = typeof(AvailabilityStatusMessageEvent);   // @see HabboMessages.as: _events[3449] = AvailabilityStatusMessageEvent
        events[2082] = typeof(AccountPreferencesMessageEvent);   // @see HabboMessages.as: _events[2641] = class_219
        events[2981]  = typeof(AccountSafetyLockMessageEvent);    // @see HabboMessages.as: _events[654]  = class_217
        events[3401]  = typeof(EmailStatusMessageEvent);          // @see HabboMessages.as: _events[712]  = class_547

        // --- Navigator ---
        events[3969] = typeof(NavigatorSettingsEvent);

        // --- Competition ---
        events[1179] = typeof(CurrentTimingCodeMessageEvent);

        // --- Room Poll ---
        events[1808] = typeof(PollContentsEvent);
        events[2078] = typeof(PollOfferEvent);
        events[2085] = typeof(PollErrorEvent);

        // --- Room Quiz ---
        events[18]   = typeof(QuestionEvent);
        events[1073] = typeof(QuestionAnsweredEvent);
        events[1219] = typeof(QuestionFinishedEvent);

        // --- Room Furniture ---
        events[2659] = typeof(DiceValueMessageEvent);
        events[1940] = typeof(OneWayDoorStatusMessageEvent);
        events[2355] = typeof(RoomDimmerPresetsMessageEvent);
        events[2552] = typeof(CustomStackingHeightUpdateMessageEvent);
        events[3064] = typeof(PresentOpenedMessageEvent);

        // --- Help / Guide Session ---
        events[1196] = typeof(GuideSessionStartedMessageEvent);
        events[843]  = typeof(GuideSessionEndedMessageEvent);
        events[2576] = typeof(GuideSessionErrorMessageEvent);

        // --- Users ---
        events[2293] = typeof(IgnoreResultMessageEvent);
        events[894]  = typeof(GroupDetailsChangedMessageEvent);
        events[1995] = typeof(HabboGroupBadgesMessageEvent);

        // --- Handshake / System Composers ---
        composers[1863] = typeof(DisconnectMessageComposer);
        composers[2920] = typeof(UniqueIdMessageComposer);
        composers[849]  = typeof(EventLogMessageComposer);
        composers[2134] = typeof(PongMessageComposer);
        composers[3241] = typeof(InfoRetrieveMessageComposer);
        composers[1517] = typeof(CompleteDiffieHandshakeMessageComposer);
        composers[4000] = typeof(ClientHelloMessageComposer);
        composers[749]  = typeof(SsoTicketMessageComposer);
        composers[3644] = typeof(InitDiffieHandshakeMessageComposer);

        // --- Room Engine Composers ---
        composers[144]  = typeof(MoveAvatarMessageComposer);
        composers[2828] = typeof(MoveObjectMessageComposer);
        composers[656]  = typeof(MoveWallItemMessageComposer);
        composers[3258] = typeof(PlaceObjectMessageComposer);
        composers[443]  = typeof(PickupObjectMessageComposer);
        composers[1675] = typeof(UseFurnitureMessageComposer);
        composers[1540] = typeof(UseWallItemMessageComposer);
        // GetHeightMapMessageComposer removed in Revision20260112 (id = -1)
        composers[205]  = typeof(GetFurnitureAliasesMessageComposer);
        composers[3284] = typeof(ClickFurniMessageComposer);
        composers[2880] = typeof(RemoveItemMessageComposer);
        composers[3445] = typeof(SetObjectDataMessageComposer);
        composers[2914] = typeof(SetItemDataMessageComposer);
        composers[204]  = typeof(GetItemDataMessageComposer);

        // --- Room Session Composers ---
        composers[329]  = typeof(OpenFlatConnectionMessageComposer);
        composers[1949] = typeof(QuitMessageComposer);
        composers[1047] = typeof(ChangeQueueMessageComposer);

        // --- Room Chat Composers ---
        composers[641]  = typeof(ChatMessageComposer);
        composers[521]  = typeof(Game2GameChatMessageComposer);
        composers[2286] = typeof(ShoutMessageComposer);
        composers[2317] = typeof(WhisperMessageComposer);
        composers[2994] = typeof(TypingStartMessageComposer);
        composers[2811] = typeof(TypingStopMessageComposer);

        // --- Room Action Composers ---
        composers[3447] = typeof(AvatarExpressionMessageComposer);
        composers[524]  = typeof(SignMessageComposer);
        composers[3420] = typeof(DanceMessageComposer);
        composers[3927] = typeof(ChangePostureMessageComposer);
        composers[3706] = typeof(ChangeMottoMessageComposer);

        // --- Room Moderation Composers ---
        composers[906]  = typeof(KickUserMessageComposer);
        composers[1702] = typeof(BanUserWithDurationMessageComposer);
        composers[2706] = typeof(MuteUserMessageComposer);
        composers[3628] = typeof(UnmuteUserMessageComposer);
        composers[355]  = typeof(AssignRightsMessageComposer);
        composers[2976] = typeof(RemoveRightsMessageComposer);
        composers[732]  = typeof(LetUserInMessageComposer);
        composers[197]  = typeof(AmbassadorAlertMessageComposer);

        // --- Room Pets Composers ---
        composers[418]  = typeof(RemovePetFromFlatMessageComposer);
        composers[1626] = typeof(MountPetMessageComposer);
        composers[3798] = typeof(TogglePetRidingPermissionMessageComposer);
        composers[949]  = typeof(RemoveSaddleFromPetMessageComposer);
        composers[2025] = typeof(HarvestPetMessageComposer);
        composers[2198] = typeof(CompostPlantMessageComposer);
        composers[2457] = typeof(GetPetCommandsMessageComposer);
        composers[3327] = typeof(CustomizePetWithFurniComposer);
        composers[760]  = typeof(OpenPetPackageMessageComposer);
        composers[1400] = typeof(TogglePetBreedingPermissionMessageComposer);

        // --- Room Poll Composers ---
        composers[1773] = typeof(PollStartComposer);
        composers[3929] = typeof(PollRejectComposer);
        composers[706]  = typeof(PollAnswerComposer);

        // --- Room Furniture Composers ---
        composers[1793] = typeof(GetRoomDimmerPresetsMessageComposer);
        composers[1802] = typeof(RoomDimmerSavePresetMessageComposer);
        composers[3707] = typeof(RoomDimmerChangeStateMessageComposer);
        composers[2243] = typeof(CreditFurniRedeemMessageComposer);
        composers[2358] = typeof(PresentOpenMessageComposer);

        // --- Room Users Composers ---
        composers[1946] = typeof(VisitUserMessageComposer);
        composers[3485] = typeof(PeerUsersClassificationMessageComposer);
        composers[3735] = typeof(RoomUsersClassificationMessageComposer);
        composers[470]  = typeof(NewUserExperienceScriptProceedComposer);
        composers[520]  = typeof(SetClothingChangeDataMessageComposer);

        // --- Room Layout Composers ---
        composers[3831] = typeof(GetOccupiedTilesMessageComposer);
        composers[1149] = typeof(GetRoomEntryTileMessageComposer);

        // --- Competition Composers ---
        composers[1332] = typeof(GetCurrentTimingCodeMessageComposer);
    }

    public Dictionary<int, Type> events { get; } = new();

    public Dictionary<int, Type> composers { get; } = new();
}
