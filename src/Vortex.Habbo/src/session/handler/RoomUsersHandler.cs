// @see com.sulake.habbo.session.handler.RoomUsersHandler

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Action;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Pets;
using Vortex.Habbo.Communication.Messages.Parser.Room.Action;
using Vortex.Habbo.Communication.Messages.Parser.Room.Engine;
using Vortex.Habbo.Communication.Messages.Parser.Room.Pets;
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
        {
            return;
        }

        connection.AddMessageEvent(new UsersMessageEvent(OnUsers));
        connection.AddMessageEvent(new UserRemoveMessageEvent(OnUserRemove));
        connection.AddMessageEvent(new HabboUserBadgesMessageEvent(OnUserBadges));
        connection.AddMessageEvent(new DoorbellMessageEvent(OnDoorbell));
        connection.AddMessageEvent(new UserChangeMessageEvent(OnUserChange));
        connection.AddMessageEvent(new UserNameChangedMessageEvent(OnUserNameChanged));
        connection.AddMessageEvent(new PetInfoMessageEvent(OnPetInfo));
        connection.AddMessageEvent(new PetCommandsMessageEvent(OnPetCommands));
        connection.AddMessageEvent(new PetPlacingErrorEvent(OnPetPlacingError));
        connection.AddMessageEvent(new PetFigureUpdateEvent(OnPetFigureUpdate));
        connection.AddMessageEvent(new PetBreedingResultEvent(OnPetBreedingResult));
        connection.AddMessageEvent(new PetBreedingEvent(OnPetBreeding));
        connection.AddMessageEvent(new PetStatusUpdateEvent(OnPetStatusUpdate));
        connection.AddMessageEvent(new PetLevelUpdateEvent(OnPetLevelUpdate));
        connection.AddMessageEvent(new ConfirmBreedingRequestEvent(OnConfirmBreedingRequest));
        connection.AddMessageEvent(new ConfirmBreedingResultEvent(OnConfirmBreedingResult));
        connection.AddMessageEvent(new NestBreedingSuccessEvent(OnNestBreedingSuccess));
        connection.AddMessageEvent(new BotErrorEvent(OnBotError));
        connection.AddMessageEvent(new NewFriendRequestEvent(OnNewFriendRequest));
        connection.AddMessageEvent(new DanceMessageEvent(OnDance));
        connection.AddMessageEvent(new FavoriteMembershipUpdateMessageEvent(OnFavoriteMembershipUpdate));
    }

    /// @see RoomUsersHandler.as::onUsers
    private void OnUsers(IMessageEvent ev)
    {
        var usersEv = ev as UsersMessageEvent;
        if (usersEv == null)
        {
            return;
        }

        var parser = usersEv.parser as UsersMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        var addedUsers = new List<IUserData>();
        for (int i = 0; i < parser.UserCount; i++)
        {
            var msg = parser.GetUser(i);
            if (msg == null)
            {
                continue;
            }

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
            {
                addedUsers.Add(userData);
            }

            session.userDataManager.SetUserData(userData);
        }
        listener?.events?.DispatchEvent(new RoomSessionUserDataUpdateEvent(session, addedUsers));
    }

    /// @see RoomUsersHandler.as::onUserRemove
    private void OnUserRemove(IMessageEvent ev)
    {
        var removeEv = ev as UserRemoveMessageEvent;
        if (removeEv == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        var parser = removeEv.parser as UserRemoveMessageEventParser;
        if (parser == null)
        {
            return;
        }

        session.userDataManager.RemoveUserDataByRoomIndex(parser.Id);
    }

    /// @see RoomUsersHandler.as::onUserBadges
    private void OnUserBadges(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as HabboUserBadgesMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionUserBadgesEvent(session, parser.UserId, parser.Badges));
    }

    /// @see RoomUsersHandler.as::onDoorbell
    private void OnDoorbell(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as DoorbellMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionDoorbellEvent(RoomSessionDoorbellEvent.DOORBELL, session, parser.UserName ?? ""));
    }

    /// @see RoomUsersHandler.as::onUserChange
    private void OnUserChange(IMessageEvent ev)
    {
        var changeEv = ev as UserChangeMessageEvent;
        if (changeEv == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        if (changeEv.id >= 0)
        {
            session.userDataManager.UpdateFigure(changeEv.id, changeEv.figure, changeEv.sex, false, false);
            session.userDataManager.UpdateCustom(changeEv.id, changeEv.customInfo);
            session.userDataManager.UpdateAchievementScore(changeEv.id, changeEv.achievementScore);
            listener?.events?.DispatchEvent(new RoomSessionUserFigureUpdateEvent(
                session, changeEv.id, changeEv.figure, changeEv.sex, changeEv.customInfo, changeEv.achievementScore));
        }
    }

    /// @see RoomUsersHandler.as::onUserNameChanged
    private void OnUserNameChanged(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as UserNameChangedMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        var userData = session.userDataManager.GetUserDataByIndex(parser.RoomIndex);
        if (userData != null)
        {
            userData.name = parser.NewName ?? userData.name;
        }
    }

    /// @see RoomUsersHandler.as::onPetInfo
    private void OnPetInfo(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as PetInfoMessageEventParser;
        if (parser?.PetInfo == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionPetInfoUpdateEvent(session, parser.PetInfo));
    }

    /// @see RoomUsersHandler.as::onPetCommands
    private void OnPetCommands(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as PetCommandsMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionPetCommandsUpdateEvent(
            session, parser.PetId, parser.AllCommands, parser.EnabledCommands));
    }

    /// @see RoomUsersHandler.as::onPetPlacingError
    private void OnPetPlacingError(IMessageEvent ev)
    {
        // TODO(as3-port): dispatch RoomSessionErrorMessageEvent with pet placing error code once verified
        _ = ev;
    }

    /// @see RoomUsersHandler.as::onPetFigureUpdate
    private void OnPetFigureUpdate(IMessageEvent ev)
    {
        var petEv = ev as PetFigureUpdateEvent;
        if (petEv == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        var parser = petEv.parser as PetFigureUpdateMessageEventParser;
        if (parser == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionPetFigureUpdateEvent(
            session, parser.petId, parser.figureData?.figureString ?? ""));
    }

    /// @see RoomUsersHandler.as::onPetBreedingResult
    private void OnPetBreedingResult(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as PetBreedingResultMessageEventParser;
        if (parser?.OwnResult == null || parser.OtherResult == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionPetBreedingResultEvent(
            session, parser.OwnResult, parser.OtherResult));
    }

    /// @see RoomUsersHandler.as::onPetBreeding
    private void OnPetBreeding(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as PetBreedingMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionPetBreedingEvent(
            session, parser.State, parser.OwnPetId, parser.OtherPetId));
    }

    /// @see RoomUsersHandler.as::onPetStatusUpdate
    private void OnPetStatusUpdate(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as PetStatusUpdateMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionPetStatusUpdateEvent(
            session, parser.PetId, parser.CanBreed, parser.CanHarvest, parser.CanRevive, parser.HasBreedingPermission));
    }

    /// @see RoomUsersHandler.as::onPetLevelUpdate
    private void OnPetLevelUpdate(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as PetLevelUpdateMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionPetLevelUpdateEvent(session, parser.PetId, parser.Level));
    }

    /// @see RoomUsersHandler.as::onConfirmBreedingRequest
    private void OnConfirmBreedingRequest(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as ConfirmBreedingRequestMessageEventParser;
        if (parser?.Pet1 == null || parser.Pet2 == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionConfirmPetBreedingEvent(
            session, parser.NestId, parser.Pet1, parser.Pet2, parser.RarityCategories, parser.ResultPetTypeId));
    }

    /// @see RoomUsersHandler.as::onConfirmBreedingResult
    private void OnConfirmBreedingResult(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as ConfirmBreedingResultMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionConfirmPetBreedingResultEvent(
            session, parser.BreedingNestStuffId, parser.Result));
    }

    /// @see RoomUsersHandler.as::onNestBreedingSuccess
    private void OnNestBreedingSuccess(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as NestBreedingSuccessMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionNestBreedingSuccessEvent(session, parser.PetId, parser.RarityCategory));
    }

    /// @see RoomUsersHandler.as::onBotError
    private void OnBotError(IMessageEvent ev)
    {
        // TODO(as3-port): dispatch RoomSessionErrorMessageEvent with bot error code once verified
        _ = ev;
    }

    /// @see RoomUsersHandler.as::onNewFriendRequest
    private void OnNewFriendRequest(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as NewFriendRequestMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionFriendRequestEvent(
            session, parser.RequestId, parser.UserId, parser.UserName ?? ""));
    }

    /// @see RoomUsersHandler.as::onDance
    private void OnDance(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as DanceMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionDanceEvent(session, parser.UserId, parser.DanceStyle));
    }

    /// @see RoomUsersHandler.as::onFavoriteMembershipUpdate
    private void OnFavoriteMembershipUpdate(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as FavoriteMembershipUpdateMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        var userData = session.userDataManager.GetUserDataByIndex(parser.RoomIndex);
        if (userData == null)
        {
            return;
        }

        userData.groupID = parser.HabboGroupId.ToString();
        userData.groupName = parser.HabboGroupName;
        listener?.events?.DispatchEvent(new RoomSessionFavouriteGroupUpdateEvent(
            session, parser.RoomIndex, parser.HabboGroupId, parser.Status, parser.HabboGroupName));
    }
}
