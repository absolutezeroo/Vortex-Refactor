// @see com.sulake.habbo.session.handler.PollHandler

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Poll;
using Vortex.Habbo.Communication.Messages.Parser.Room.Poll;
using Vortex.Habbo.Session.Events;

namespace Vortex.Habbo.Session.Handler;

/// @see com.sulake.habbo.session.handler.PollHandler
public class PollHandler : BaseHandler
{
    /// @see PollHandler.as::PollHandler
    public PollHandler(IConnection? connection, IRoomHandlerListener listener)
        : base(connection, listener)
    {
        if (connection == null)
        {
            return;
        }

        connection.AddMessageEvent(new PollContentsEvent(OnPollContents));
        connection.AddMessageEvent(new PollOfferEvent(OnPollOffer));
        connection.AddMessageEvent(new PollErrorEvent(OnPollError));
    }

    /// @see PollHandler.as::onPollOfferEvent
    private void OnPollOffer(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as PollOfferMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        var pollEvent = new RoomSessionPollEvent(RoomSessionPollEvent.OFFER, session, parser.Id)
        {
            headline = parser.Headline,
            summary = parser.Summary,
        };
        listener?.events?.DispatchEvent(pollEvent);
    }

    /// @see PollHandler.as::onPollErrorEvent
    private void OnPollError(IMessageEvent ev)
    {
        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionPollEvent(RoomSessionPollEvent.ERROR, session, -1));
    }

    /// @see PollHandler.as::onPollContentsEvent
    private void OnPollContents(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as PollContentsMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        var pollEvent = new RoomSessionPollEvent(RoomSessionPollEvent.CONTENT, session, parser.Id)
        {
            startMessage  = parser.StartMessage ?? "",
            endMessage    = parser.EndMessage ?? "",
            numQuestions  = parser.NumQuestions,
            questionArray = parser.QuestionArray,
            npsPoll       = parser.NpsPoll,
        };
        listener?.events?.DispatchEvent(pollEvent);
    }
}
