// @see com.sulake.habbo.session.handler.RoomSessionHandler

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Session;
using Vortex.Habbo.Communication.Messages.Parser.Room.Session;
using Vortex.Habbo.Session.Events;

namespace Vortex.Habbo.Session.Handler;

/// @see com.sulake.habbo.session.handler.RoomSessionHandler
public class RoomSessionHandler : BaseHandler
{
    public const string RS_CONNECTED = "RS_CONNECTED";
    public const string RS_READY = "RS_READY";
    public const string RS_DISCONNECTED = "RS_DISCONNECTED";

    /// @see RoomSessionHandler.as::RoomSessionHandler
    public RoomSessionHandler(IConnection? connection, IRoomHandlerListener listener)
        : base(connection, listener)
    {
        if (connection == null)
        {
            return;
        }

        connection.AddMessageEvent(new OpenConnectionMessageEvent(OnRoomConnected));
        connection.AddMessageEvent(new FlatAccessibleMessageEvent(OnFlatAccessible));
        connection.AddMessageEvent(new RoomReadyMessageEvent(OnRoomReady));
        connection.AddMessageEvent(new CloseConnectionMessageEvent(OnRoomDisconnected));
        connection.AddMessageEvent(new FlatAccessDeniedMessageEvent(OnFlatAccessDenied));
        connection.AddMessageEvent(new RoomQueueStatusMessageEvent(OnRoomQueueStatus));
        connection.AddMessageEvent(new YouAreSpectatorMessageEvent(OnYouAreSpectator));
    }

    /// @see RoomSessionHandler.as::onRoomConnected
    private void OnRoomConnected(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as OpenConnectionMessageEventParser;
        if (parser == null)
        {
            return;
        }

        listener?.SessionUpdate(parser.FlatId, RS_CONNECTED);
    }

    /// @see RoomSessionHandler.as::onFlatAccessible
    private void OnFlatAccessible(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as FlatAccessibleMessageEventParser;
        if (parser == null)
        {
            return;
        }

        string? userName = parser.UserName;
        if (!string.IsNullOrEmpty(userName) && listener?.events != null)
        {
            var session = listener.GetSession(parser.FlatId);
            if (session != null)
            {
                listener.events.DispatchEvent(new RoomSessionDoorbellEvent(RoomSessionDoorbellEvent.ACCEPTED, session, userName));
            }
        }
    }

    /// @see RoomSessionHandler.as::onRoomReady
    private void OnRoomReady(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as RoomReadyMessageEventParser;
        if (parser == null)
        {
            return;
        }

        int roomId = parser.RoomId;
        if (listener == null)
        {
            return;
        }

        listener.SessionReinitialize(roomId, roomId);
        listener.SessionUpdate(roomId, RS_READY);
    }

    /// @see RoomSessionHandler.as::onFlatAccessDenied
    private void OnFlatAccessDenied(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as FlatAccessDeniedMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(parser.FlatId);
        if (session == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionDoorbellEvent(RoomSessionDoorbellEvent.REJECTED, session, parser.Info ?? ""));
    }

    /// @see RoomSessionHandler.as::onRoomDisconnected
    private void OnRoomDisconnected(IMessageEvent ev)
    {
        int roomId = currentRoomId;
        listener?.SessionUpdate(roomId, RS_DISCONNECTED);
    }

    /// @see RoomSessionHandler.as::onRoomQueueStatus
    private void OnRoomQueueStatus(IMessageEvent ev)
    {
        if (listener?.events == null)
        {
            return;
        }

        var parser = (ev as MessageEvent)?.parser as RoomQueueStatusMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener.GetSession(parser.FlatId);
        if (session == null)
        {
            return;
        }

        foreach (int target in parser.GetQueueSetTargets())
        {
            var queueSet = parser.GetQueueSet(target);
            if (queueSet == null)
            {
                continue;
            }

            var queueEvent = new RoomSessionQueueEvent(session, queueSet.Name, queueSet.Target,
                queueSet.Target == parser.ActiveTarget);
            for (int i = 0; i < queueSet.QueueCount; i++)
            {
                var (qName, qSize) = queueSet.GetQueue(i);
                queueEvent.AddQueue(qName, qSize);
            }
            listener.events.DispatchEvent(queueEvent);
        }
    }

    /// @see RoomSessionHandler.as::onYouAreSpectator
    private void OnYouAreSpectator(IMessageEvent ev)
    {
        if (listener == null)
        {
            return;
        }

        var parser = (ev as MessageEvent)?.parser as YouAreSpectatorMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener.GetSession(parser.FlatId);
        if (session == null)
        {
            return;
        }

        session.isSpectatorMode = true;
    }
}
