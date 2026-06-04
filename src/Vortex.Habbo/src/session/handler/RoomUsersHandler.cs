// @see com.sulake.habbo.session.handler.RoomUsersHandler

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Action;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Pets;
using Vortex.Habbo.Communication.Messages.Parser.Room.Action;
using Vortex.Habbo.Communication.Messages.Parser.Room.Engine;
using Vortex.Habbo.Session.Events;

namespace Vortex.Habbo.Session.Handler;

/// @see com.sulake.habbo.session.handler.RoomUsersHandler
public class RoomUsersHandler : BaseHandler
{
    /// @see RoomUsersHandler.as::RoomUsersHandler
    public RoomUsersHandler(IConnection? connection, IRoomHandlerListener listener)
        : base(connection, listener)
    {
        if (connection == null)
            return;
        connection.AddMessageEvent(new UsersMessageEvent(OnUsers));
        connection.AddMessageEvent(new UserRemoveMessageEvent(OnUserRemove));
        // TODO(as3-port): HabboUserBadgesMessageEvent not yet ported
        // TODO(as3-port): DoorbellMessageEvent not yet ported
        connection.AddMessageEvent(new UserChangeMessageEvent(OnUserChange));
        // TODO(as3-port): UserNameChangedMessageEvent not yet ported
        // TODO(as3-port): PetInfoMessageEvent not yet ported
        // TODO(as3-port): PetCommandsMessageEvent not yet ported
        // TODO(as3-port): PetPlacingErrorEvent not yet ported
        connection.AddMessageEvent(new PetFigureUpdateEvent(OnPetFigureUpdate));
        // TODO(as3-port): PetBreedingResultEvent not yet ported
        // TODO(as3-port): PetBreedingEvent (inventory/pets) not yet ported
        // TODO(as3-port): PetStatusUpdateEvent not yet ported
        // TODO(as3-port): PetLevelUpdateEvent not yet ported
        // TODO(as3-port): ConfirmBreedingRequestEvent not yet ported
        // TODO(as3-port): ConfirmBreedingResultEvent not yet ported
        // TODO(as3-port): NestBreedingSuccessEvent not yet ported
        // TODO(as3-port): BotErrorEvent not yet ported
        // TODO(as3-port): NewFriendRequestEvent not yet ported
        connection.AddMessageEvent(new DanceMessageEvent(OnDance));
        connection.AddMessageEvent(new FavoriteMembershipUpdateMessageEvent(OnFavoriteMembershipUpdate));
    }

    /// @see RoomUsersHandler.as::onUsers
    private void OnUsers(IMessageEvent ev)
    {
        var usersEv = ev as UsersMessageEvent;
        if (usersEv == null)
            return;
        var parser = usersEv.parser as UsersMessageEventParser;
        if (parser == null)
            return;
        var session = listener?.GetSession(currentRoomId);
        if (session == null)
            return;
        var addedUsers = new List<IUserData>();
        for (int i = 0; i < parser.UserCount; i++)
        {
            var msg = parser.GetUser(i);
            if (msg == null)
                continue;
            var userData = new UserData(msg.RoomIndex)
            {
                name = msg.Name,
                custom = msg.Custom,
                achievementScore = msg.AchievementScore,
                figure = msg.Figure,
                type = msg.UserType,
                webID = msg.WebID,
                groupID = msg.GroupID,
                groupName = msg.GroupName,
                groupStatus = msg.GroupStatus,
                sex = msg.Sex,
                ownerId = msg.OwnerId,
                ownerName = msg.OwnerName,
                rarityLevel = msg.RarityLevel,
                hasSaddle = msg.HasSaddle,
                isRiding = msg.IsRiding,
                canBreed = msg.CanBreed,
                canHarvest = msg.CanHarvest,
                canRevive = msg.CanRevive,
                hasBreedingPermission = msg.HasBreedingPermission,
                petLevel = msg.PetLevel,
                botSkills = msg.BotSkills,
                isModerator = msg.IsModerator,
            };
            if (session.userDataManager.GetUserData(msg.RoomIndex) == null)
                addedUsers.Add(userData);
            session.userDataManager.SetUserData(userData);
        }
        listener?.events?.DispatchEvent(new RoomSessionUserDataUpdateEvent(session, addedUsers));
    }

    /// @see RoomUsersHandler.as::onUserRemove
    private void OnUserRemove(IMessageEvent ev)
    {
        var removeEv = ev as UserRemoveMessageEvent;
        if (removeEv == null)
            return;
        var session = listener?.GetSession(currentRoomId);
        if (session == null)
            return;
        var parser = removeEv.parser as UserRemoveMessageEventParser;
        if (parser == null)
            return;
        session.userDataManager.RemoveUserDataByRoomIndex(parser.Id);
    }

    /// @see RoomUsersHandler.as::onUserChange
    private void OnUserChange(IMessageEvent ev)
    {
        var changeEv = ev as UserChangeMessageEvent;
        if (changeEv == null)
            return;
        var session = listener?.GetSession(currentRoomId);
        if (session == null)
            return;
        if (changeEv.id >= 0)
        {
            session.userDataManager.UpdateFigure(changeEv.id, changeEv.figure, changeEv.sex, false, false);
            session.userDataManager.UpdateCustom(changeEv.id, changeEv.customInfo);
            session.userDataManager.UpdateAchievementScore(changeEv.id, changeEv.achievementScore);
            listener?.events?.DispatchEvent(new RoomSessionUserFigureUpdateEvent(
                session, changeEv.id, changeEv.figure, changeEv.sex, changeEv.customInfo, changeEv.achievementScore));
        }
    }

    /// @see RoomUsersHandler.as::onPetFigureUpdate
    private void OnPetFigureUpdate(IMessageEvent ev)
    {
        var petEv = ev as PetFigureUpdateEvent;
        if (petEv == null)
            return;
        var session = listener?.GetSession(currentRoomId);
        if (session == null)
            return;
        // TODO(as3-port): PetFigureUpdateMessageParser — access figureData.figureString, roomIndex, petId, hasSaddle, isRiding
        // Once PetFigureUpdateMessageParser is verified, wire figure update and dispatch RoomSessionPetFigureUpdateEvent
    }

    /// @see RoomUsersHandler.as::onDance
    private void OnDance(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as DanceMessageEventParser;
        if (parser == null)
            return;
        var session = listener?.GetSession(currentRoomId);
        if (session == null)
            return;
        listener?.events?.DispatchEvent(new RoomSessionDanceEvent(session, parser.UserId, parser.DanceStyle));
    }

    /// @see RoomUsersHandler.as::onFavoriteMembershipUpdate
    private void OnFavoriteMembershipUpdate(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as FavoriteMembershipUpdateMessageEventParser;
        if (parser == null)
            return;
        var session = listener?.GetSession(currentRoomId);
        if (session == null)
            return;
        var userData = session.userDataManager.GetUserDataByIndex(parser.RoomIndex);
        if (userData == null)
            return;
        userData.groupID = parser.HabboGroupId.ToString();
        userData.groupName = parser.HabboGroupName;
        listener?.events?.DispatchEvent(new RoomSessionFavouriteGroupUpdateEvent(
            session, parser.RoomIndex, parser.HabboGroupId, parser.Status, parser.HabboGroupName));
    }
}
