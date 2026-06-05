// @see com.sulake.habbo.session.handler.RoomChatHandler

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Chat;
using Vortex.Habbo.Communication.Messages.Parser.Room.Chat;
using Vortex.Habbo.Session.Events;

namespace Vortex.Habbo.Session.Handler;

/// @see com.sulake.habbo.session.handler.RoomChatHandler
public class RoomChatHandler : BaseHandler
{
    /// @see RoomChatHandler.as::RoomChatHandler
    public RoomChatHandler(IConnection? connection, IRoomHandlerListener listener)
        : base(connection, listener)
    {
        if (connection == null)
        {
            return;
        }

        connection.AddMessageEvent(new ChatMessageEvent(OnRoomChat));
        connection.AddMessageEvent(new WhisperMessageEvent(OnRoomWhisper));
        connection.AddMessageEvent(new ShoutMessageEvent(OnRoomShout));
        connection.AddMessageEvent(new RespectNotificationMessageEvent(OnRespectNotification));
        connection.AddMessageEvent(new PetRespectNotificationEvent(OnPetRespectNotification));
        connection.AddMessageEvent(new PetSupplementedNotificationEvent(OnPetSupplementedNotification));
        connection.AddMessageEvent(new FloodControlMessageEvent(OnFloodControl));
        connection.AddMessageEvent(new HandItemReceivedMessageEvent(OnHandItemReceived));
        connection.AddMessageEvent(new RemainingMutePeriodEvent(OnRemainingMutePeriod));
    }

    /// @see RoomChatHandler.as::onRoomChat
    private void OnRoomChat(IMessageEvent ev)
    {
        if (listener?.events == null)
        {
            return;
        }

        ChatMessageEvent? chatEv = ev as ChatMessageEvent;
        if (chatEv == null)
        {
            return;
        }

        ChatMessageEventParser? parser = chatEv.parser as ChatMessageEventParser;
        if (parser == null)
        {
            return;
        }

        IRoomSession? session = listener.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        if (parser.TrackingId != -1)
        {
            session.ReceivedChatWithTrackingId(parser.TrackingId);
        }

        listener.events.DispatchEvent(new RoomSessionChatEvent(
            RoomSessionChatEvent.ROOM_SESSION_CHAT_EVENT, session,
            parser.UserId, parser.Text, RoomSessionChatEvent.CHAT_TYPE_SPEAK, parser.StyleId, parser.Links));
    }

    /// @see RoomChatHandler.as::onRoomWhisper
    private void OnRoomWhisper(IMessageEvent ev)
    {
        if (listener?.events == null)
        {
            return;
        }

        WhisperMessageEvent? whisperEv = ev as WhisperMessageEvent;
        if (whisperEv == null)
        {
            return;
        }

        ChatMessageEventParser? parser = whisperEv.parser as ChatMessageEventParser;
        if (parser == null)
        {
            return;
        }

        IRoomSession? session = listener.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener.events.DispatchEvent(new RoomSessionChatEvent(
            RoomSessionChatEvent.ROOM_SESSION_CHAT_EVENT, session,
            parser.UserId, parser.Text, RoomSessionChatEvent.CHAT_TYPE_WHISPER, parser.StyleId, parser.Links));
    }

    /// @see RoomChatHandler.as::onRoomShout
    private void OnRoomShout(IMessageEvent ev)
    {
        if (listener?.events == null)
        {
            return;
        }

        ShoutMessageEvent? shoutEv = ev as ShoutMessageEvent;
        if (shoutEv == null)
        {
            return;
        }

        ChatMessageEventParser? parser = shoutEv.parser as ChatMessageEventParser;
        if (parser == null)
        {
            return;
        }

        IRoomSession? session = listener.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener.events.DispatchEvent(new RoomSessionChatEvent(
            RoomSessionChatEvent.ROOM_SESSION_CHAT_EVENT, session,
            parser.UserId, parser.Text, RoomSessionChatEvent.CHAT_TYPE_SHOUT, parser.StyleId, parser.Links));
    }

    /// @see RoomChatHandler.as::onRespectNotification
    private void OnRespectNotification(IMessageEvent ev)
    {
        // TODO(as3-port): RespectNotificationMessageEvent — dispatch respect UI event once ported
        _ = ev;
    }

    /// @see RoomChatHandler.as::onPetRespectNotification
    private void OnPetRespectNotification(IMessageEvent ev)
    {
        // TODO(as3-port): PetRespectNotificationEvent — dispatch pet respect UI event once ported
        _ = ev;
    }

    /// @see RoomChatHandler.as::onPetSupplementedNotification
    private void OnPetSupplementedNotification(IMessageEvent ev)
    {
        // TODO(as3-port): PetSupplementedNotificationEvent — dispatch pet supplemented UI event once ported
        _ = ev;
    }

    /// @see RoomChatHandler.as::onFloodControl
    private void OnFloodControl(IMessageEvent ev)
    {
        if (listener?.events == null)
        {
            return;
        }

        FloodControlMessageEventParser? parser = (ev as MessageEvent)?.parser as FloodControlMessageEventParser;
        IRoomSession? session = listener.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener.events.DispatchEvent(new RoomSessionChatEvent(
            RoomSessionChatEvent.ROOM_SESSION_FLOODCONTROL_EVENT, session,
            -1, parser?.Seconds.ToString() ?? "0", 0, 0));
    }

    /// @see RoomChatHandler.as::onHandItemReceived
    private void OnHandItemReceived(IMessageEvent ev)
    {
        // TODO(as3-port): HandItemReceivedMessageEvent — dispatch hand item UI event once ported
        _ = ev;
    }

    /// @see RoomChatHandler.as::onRemainingMutePeriod
    private void OnRemainingMutePeriod(IMessageEvent ev)
    {
        if (listener?.events == null)
        {
            return;
        }

        RemainingMutePeriodEventParser? parser = (ev as MessageEvent)?.parser as RemainingMutePeriodEventParser;
        if (parser == null)
        {
            return;
        }

        IRoomSession? session = listener.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener.events.DispatchEvent(new RoomSessionChatEvent(
            RoomSessionChatEvent.ROOM_SESSION_CHAT_EVENT, session,
            session.ownUserRoomId, "", RoomSessionChatEvent.CHAT_TYPE_MUTE_REMAINING, 1,
            null, parser.SecondsRemaining));
    }
}
