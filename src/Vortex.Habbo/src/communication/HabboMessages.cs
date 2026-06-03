using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Availability;
using Vortex.Habbo.Communication.Messages.Incoming.Error;
using Vortex.Habbo.Communication.Messages.Incoming.Handshake;
using Vortex.Habbo.Communication.Messages.Incoming.Help;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Action;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Chat;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Furniture;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Layout;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Permissions;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Pets;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Session;
using Vortex.Habbo.Communication.Messages.Incoming.Users;
using Vortex.Habbo.Communication.Messages.Outgoing.Handshake;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Engine;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Layout;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Session;
using Vortex.Habbo.Communication.Messages.Outgoing.Tracking;

namespace Vortex.Habbo.Communication;

public class HabboMessages : IMessageConfiguration
{
    public HabboMessages()
    {
        // --- Handshake / System ---
        events[2853] = typeof(UserObjectEvent);
        events[1417] = typeof(IdentityAccountsEvent);
        events[2323] = typeof(AuthenticationOkMessageEvent);
        events[3126] = typeof(MaintenanceStatusMessageEvent);
        events[3504] = typeof(LoginFailedHotelClosedMessageEvent);
        events[3777] = typeof(CompleteDiffieHandshakeEvent);
        events[3974] = typeof(UniqueMachineIdEvent);
        events[4000] = typeof(DisconnectReasonEvent);
        events[598] = typeof(GenericErrorEvent);
        events[657] = typeof(ErrorReportEvent);
        events[658] = typeof(PingMessageEvent);
        events[771] = typeof(InitDiffieHandshakeEvent);

        // --- Room Session ---
        events[1195] = typeof(RoomReadyMessageEvent);
        events[631] = typeof(OpenConnectionMessageEvent);
        events[2529] = typeof(CloseConnectionMessageEvent);
        events[1522] = typeof(FlatAccessibleMessageEvent);
        events[2207] = typeof(CantConnectMessageEvent);
        events[1618] = typeof(GamePlayerValueMessageEvent);
        events[305] = typeof(YouArePlayingGameMessageEvent);
        events[1704] = typeof(YouAreSpectatorMessageEvent);
        events[2111] = typeof(YouAreNotSpectatorMessageEvent);
        events[3352] = typeof(RoomQueueStatusMessageEvent);
        events[1727] = typeof(RoomForwardMessageEvent);
        events[1009] = typeof(HanditemConfigurationMessageEvent);

        // --- Room Engine: Objects ---
        events[171] = typeof(ObjectsMessageEvent);
        events[2792] = typeof(ObjectAddMessageEvent);
        events[1217] = typeof(ObjectUpdateMessageEvent);
        events[2181] = typeof(ObjectRemoveMessageEvent);
        events[3458] = typeof(ObjectDataUpdateMessageEvent);
        events[2513] = typeof(ObjectsDataUpdateMessageEvent);
        events[32] = typeof(ObjectRemoveMultipleMessageEvent);
        events[3314] = typeof(ObjectRemoveConfirmMessageEvent);

        // --- Room Engine: Items ---
        events[3683] = typeof(ItemsMessageEvent);
        events[2866] = typeof(ItemAddMessageEvent);
        events[805] = typeof(ItemUpdateMessageEvent);
        events[1133] = typeof(ItemRemoveMessageEvent);
        events[93] = typeof(ItemDataUpdateMessageEvent);
        events[316] = typeof(ItemsStateUpdateMessageEvent);

        // --- Room Engine: Users ---
        events[2471] = typeof(UsersMessageEvent);
        events[2711] = typeof(UserChangeMessageEvent);
        events[2435] = typeof(UserRemoveMessageEvent);
        events[2377] = typeof(UserUpdateMessageEvent);

        // --- Room Engine: Misc ---
        events[190] = typeof(SlideObjectBundleMessageEvent);
        events[648] = typeof(RoomPropertyMessageEvent);
        events[619] = typeof(RoomVisualizationSettingsEvent);
        events[2340] = typeof(FurnitureAliasesMessageEvent);
        events[1669] = typeof(RoomEntryInfoMessageEvent);
        events[3907] = typeof(SpecialRoomEffectMessageEvent);
        events[3778] = typeof(HeightMapMessageEvent);
        events[1945] = typeof(FloorHeightMapMessageEvent);
        events[2337] = typeof(HeightMapUpdateMessageEvent);
        events[2781] = typeof(FavoriteMembershipUpdateMessageEvent);
        events[3000] = typeof(WiredMovementsMessageEvent);
        events[3140] = typeof(BuildersClubPlacementWarningMessageEvent);

        // --- Room Action ---
        events[2392] = typeof(AvatarEffectMessageEvent);
        events[2877] = typeof(CarryObjectMessageEvent);
        events[3717] = typeof(DanceMessageEvent);
        events[927] = typeof(ExpressionMessageEvent);
        events[1170] = typeof(SleepMessageEvent);
        events[3726] = typeof(UseObjectMessageEvent);

        // --- Room Chat ---
        events[1646] = typeof(ChatMessageEvent);
        events[3869] = typeof(ShoutMessageEvent);
        events[3117] = typeof(WhisperMessageEvent);
        events[640] = typeof(UserTypingMessageEvent);
        events[1351] = typeof(FloodControlMessageEvent);
        events[3893] = typeof(RemainingMutePeriodEvent);
        events[683] = typeof(RoomChatSettingsMessageEvent);
        events[38] = typeof(RoomFilterSettingsMessageEvent);

        // --- Room Permissions ---
        events[3835] = typeof(YouAreControllerMessageEvent);
        events[66] = typeof(YouAreNotControllerMessageEvent);
        events[2584] = typeof(YouAreOwnerMessageEvent);

        // --- Room Layout ---
        events[3546] = typeof(RoomEntryTileMessageEvent);
        events[2898] = typeof(RoomOccupiedTilesMessageEvent);

        // --- Room Pets ---
        events[192] = typeof(PetExperienceEvent);
        events[1390] = typeof(PetFigureUpdateEvent);

        // --- Room Furniture ---
        events[638] = typeof(DiceValueMessageEvent);
        events[1573] = typeof(OneWayDoorStatusMessageEvent);
        events[1963] = typeof(RoomDimmerPresetsMessageEvent);
        events[1906] = typeof(CustomStackingHeightUpdateMessageEvent);
        events[311] = typeof(PresentOpenedMessageEvent);

        // --- Help / Guide Session ---
        events[836] = typeof(GuideSessionStartedMessageEvent);
        events[1459] = typeof(GuideSessionEndedMessageEvent);
        events[2384] = typeof(GuideSessionErrorMessageEvent);

        // --- Users ---
        events[1304] = typeof(IgnoreResultMessageEvent);
        events[1247] = typeof(GroupDetailsChangedMessageEvent);
        events[2092] = typeof(HabboGroupBadgesMessageEvent);

        // --- Handshake / System Composers ---
        composers[1113] = typeof(DisconnectMessageComposer);
        composers[1390] = typeof(UniqueIdMessageComposer);
        composers[2297] = typeof(EventLogMessageComposer);
        composers[2331] = typeof(PongMessageComposer);
        composers[245] = typeof(InfoRetrieveMessageComposer);
        composers[2616] = typeof(CompleteDiffieHandshakeMessageComposer);
        composers[4000] = typeof(ClientHelloMessageComposer);
        composers[53] = typeof(SsoTicketMessageComposer);
        composers[586] = typeof(InitDiffieHandshakeMessageComposer);

        // --- Room Engine Composers ---
        composers[778] = typeof(MoveAvatarMessageComposer);
        composers[2378] = typeof(MoveObjectMessageComposer);
        composers[2478] = typeof(MoveWallItemMessageComposer);
        composers[2735] = typeof(PlaceObjectMessageComposer);
        composers[3238] = typeof(PickupObjectMessageComposer);
        composers[3226] = typeof(UseFurnitureMessageComposer);
        composers[696] = typeof(UseWallItemMessageComposer);
        composers[1935] = typeof(GetHeightMapMessageComposer);
        composers[1411] = typeof(GetFurnitureAliasesMessageComposer);
        composers[757] = typeof(ClickFurniMessageComposer);
        composers[638] = typeof(RemoveItemMessageComposer);
        composers[2942] = typeof(SetObjectDataMessageComposer);
        composers[754] = typeof(SetItemDataMessageComposer);
        composers[2165] = typeof(GetItemDataMessageComposer);

        // --- Room Session Composers ---
        composers[46] = typeof(OpenFlatConnectionMessageComposer);
        composers[2722] = typeof(QuitMessageComposer);
        composers[1330] = typeof(ChangeQueueMessageComposer);

        // --- Room Layout Composers ---
        composers[16] = typeof(GetOccupiedTilesMessageComposer);
        composers[2476] = typeof(GetRoomEntryTileMessageComposer);
    }

    public Dictionary<int, Type> events { get; } = new();

    public Dictionary<int, Type> composers { get; } = new();
}
