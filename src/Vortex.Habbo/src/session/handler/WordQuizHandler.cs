// @see com.sulake.habbo.session.handler.WordQuizHandler

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Quiz;
using Vortex.Habbo.Communication.Messages.Parser.Room.Quiz;
using Vortex.Habbo.Session.Events;

namespace Vortex.Habbo.Session.Handler;

/// @see com.sulake.habbo.session.handler.WordQuizHandler
public class WordQuizHandler : BaseHandler
{
    /// @see WordQuizHandler.as::WordQuizHandler
    public WordQuizHandler(IConnection? connection, IRoomHandlerListener listener)
        : base(connection, listener)
    {
        if (connection == null)
        {
            return;
        }

        connection.AddMessageEvent(new QuestionEvent(OnQuestionStatus));
        connection.AddMessageEvent(new QuestionAnsweredEvent(OnQuestionAnswered));
        connection.AddMessageEvent(new QuestionFinishedEvent(OnQuestionFinished));
    }

    /// @see WordQuizHandler.as::onQuestionStatus
    private void OnQuestionStatus(IMessageEvent ev)
    {
        QuestionMessageEventParser? parser = (ev as MessageEvent)?.parser as QuestionMessageEventParser;
        if (parser == null)
        {
            return;
        }

        IRoomSession? session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        RoomSessionWordQuizEvent quizEvent = new RoomSessionWordQuizEvent(RoomSessionWordQuizEvent.NEW_QUESTION, session, parser.PollId)
        {
            pollId     = parser.PollId,
            pollType   = parser.PollType,
            questionId = parser.QuestionId,
            duration   = parser.Duration,
            question   = parser.Question,
        };
        listener?.events?.DispatchEvent(quizEvent);
    }

    /// @see WordQuizHandler.as::onQuestionAnsweredEvent
    private void OnQuestionAnswered(IMessageEvent ev)
    {
        QuestionAnsweredMessageEventParser? parser = (ev as MessageEvent)?.parser as QuestionAnsweredMessageEventParser;
        if (parser == null)
        {
            return;
        }

        IRoomSession? session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        RoomSessionWordQuizEvent quizEvent = new RoomSessionWordQuizEvent(RoomSessionWordQuizEvent.QUESTION_ANSWERED, session, parser.UserId)
        {
            userId       = parser.UserId,
            value        = parser.Value,
            answerCounts = parser.AnswerCounts,
        };
        listener?.events?.DispatchEvent(quizEvent);
    }

    /// @see WordQuizHandler.as::onQuestionFinishedEvent
    private void OnQuestionFinished(IMessageEvent ev)
    {
        QuestionFinishedMessageEventParser? parser = (ev as MessageEvent)?.parser as QuestionFinishedMessageEventParser;
        if (parser == null)
        {
            return;
        }

        IRoomSession? session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        RoomSessionWordQuizEvent quizEvent = new RoomSessionWordQuizEvent(RoomSessionWordQuizEvent.FINISHED, session)
        {
            questionId   = parser.QuestionId,
            answerCounts = parser.AnswerCounts,
        };
        listener?.events?.DispatchEvent(quizEvent);
    }
}
