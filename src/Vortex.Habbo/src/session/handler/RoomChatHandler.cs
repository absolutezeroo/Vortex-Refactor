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
            return;
        connection.AddMessageEvent(new ChatMessageEvent(OnRoomChat));
        connection.AddMessageEvent(new WhisperMessageEvent(OnRoomWhisper));
        connection.AddMessageEvent(new ShoutMessageEvent(OnRoomShout));
        // TODO(as3-port): RespectNotificationMessageEvent not yet ported
        // TODO(as3-port): PetRespectNotificationEvent not yet ported
        // TODO(as3-port): PetSupplementedNotificationEvent not yet ported
        connection.AddMessageEvent(new FloodControlMessageEvent(OnFloodControl));
        // TODO(as3-port): HandItemReceivedMessageEvent not yet ported
        connection.AddMessageEvent(new RemainingMutePeriodEvent(OnRemainingMutePeriod));
    }

    /// @see RoomChatHandler.as::onRoomChat
    private void OnRoomChat(IMessageEvent ev)
    {
        if (listener?.events == null)
            return;
        var chatEv = ev as ChatMessageEvent;
        if (chatEv == null)
            return;
        var parser = chatEv.parser as ChatMessageEventParser;
        if (parser == null)
            return;
        var session = listener.GetSession(currentRoomId);
        if (session == null)
            return;
        if (parser.TrackingId != -1)
            session.ReceivedChatWithTrackingId(parser.TrackingId);
        listener.events.DispatchEvent(new RoomSessionChatEvent(
            RoomSessionChatEvent.ROOM_SESSION_CHAT_EVENT, session,
            parser.UserId, parser.Text, RoomSessionChatEvent.CHAT_TYPE_SPEAK, parser.StyleId, parser.Links));
    }

    /// @see RoomChatHandler.as::onRoomWhisper
    private void OnRoomWhisper(IMessageEvent ev)
    {
        if (listener?.events == null)
            return;
        var whisperEv = ev as WhisperMessageEvent;
        if (whisperEv == null)
            return;
        var parser = whisperEv.parser as ChatMessageEventParser;
        if (parser == null)
            return;
        var session = listener.GetSession(currentRoomId);
        if (session == null)
            return;
        listener.events.DispatchEvent(new RoomSessionChatEvent(
            RoomSessionChatEvent.ROOM_SESSION_CHAT_EVENT, session,
            parser.UserId, parser.Text, RoomSessionChatEvent.CHAT_TYPE_WHISPER, parser.StyleId, parser.Links));
    }

    /// @see RoomChatHandler.as::onRoomShout
    private void OnRoomShout(IMessageEvent ev)
    {
        if (listener?.events == null)
            return;
        var shoutEv = ev as ShoutMessageEvent;
        if (shoutEv == null)
            return;
        var parser = shoutEv.parser as ChatMessageEventParser;
        if (parser == null)
            return;
        var session = listener.GetSession(currentRoomId);
        if (session == null)
            return;
        listener.events.DispatchEvent(new RoomSessionChatEvent(
            RoomSessionChatEvent.ROOM_SESSION_CHAT_EVENT, session,
            parser.UserId, parser.Text, RoomSessionChatEvent.CHAT_TYPE_SHOUT, parser.StyleId, parser.Links));
    }

    /// @see RoomChatHandler.as::onFloodControl
    private void OnFloodControl(IMessageEvent ev)
    {
        if (listener?.events == null)
            return;
        var parser = (ev as MessageEvent)?.parser as FloodControlMessageEventParser;
        var session = listener.GetSession(currentRoomId);
        if (session == null)
            return;
        listener.events.DispatchEvent(new RoomSessionChatEvent(
            RoomSessionChatEvent.ROOM_SESSION_FLOODCONTROL_EVENT, session,
            -1, parser?.Seconds.ToString() ?? "0", 0, 0));
    }

    /// @see RoomChatHandler.as::onRemainingMutePeriod
    private void OnRemainingMutePeriod(IMessageEvent ev)
    {
        if (listener?.events == null)
            return;
        var parser = (ev as MessageEvent)?.parser as RemainingMutePeriodEventParser;
        if (parser == null)
            return;
        var session = listener.GetSession(currentRoomId);
        if (session == null)
            return;
        listener.events.DispatchEvent(new RoomSessionChatEvent(
            RoomSessionChatEvent.ROOM_SESSION_CHAT_EVENT, session,
            session.ownUserRoomId, "", RoomSessionChatEvent.CHAT_TYPE_MUTE_REMAINING, 1,
            null, parser.SecondsRemaining));
    }
}
