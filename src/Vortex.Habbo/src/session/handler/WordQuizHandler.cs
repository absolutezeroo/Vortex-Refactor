// @see com.sulake.habbo.session.handler.WordQuizHandler

using Vortex.Core.Communication.Connection;
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
            return;
        // TODO(as3-port): QuestionEvent not yet ported
        // TODO(as3-port): QuestionAnsweredEvent not yet ported
        // TODO(as3-port): QuestionFinishedEvent not yet ported
    }

    // TODO(as3-port): onQuestionStatus — QuestionParser (pollId, question, duration, pollType, questionId)
    //   → dispatch RoomSessionWordQuizEvent("RWPUW_NEW_QUESTION", session, pollId)

    // TODO(as3-port): onQuestionAnsweredEvent — QuestionAnsweredParser (userId, value, answerCounts)
    //   → dispatch RoomSessionWordQuizEvent("RWPUW_QUESTION_ANSWERED", session, userId)

    // TODO(as3-port): onQuestionFinishedEvent — QuestionFinishedParser (questionId, answerCounts)
    //   → dispatch RoomSessionWordQuizEvent("RWPUW_QUESION_FINSIHED", session)
}
