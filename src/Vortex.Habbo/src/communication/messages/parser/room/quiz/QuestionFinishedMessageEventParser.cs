// @see com.sulake.habbo.communication.messages.parser.room.quiz.QuestionFinishedParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Quiz;

/// @see com.sulake.habbo.communication.messages.parser.room.quiz.QuestionFinishedParser
public class QuestionFinishedMessageEventParser : IMessageParser
{
    public int QuestionId { get; private set; }
    public Dictionary<string, int>? AnswerCounts { get; private set; }

    public bool Flush()
    {
        QuestionId = 0;
        AnswerCounts = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source — QuestionFinishedParser
        QuestionId = param1.ReadInteger();
        int count = param1.ReadInteger();
        Dictionary<string, int> counts = new Dictionary<string, int>(count);
        for (int i = 0; i < count; i++)
        {
            string key = param1.ReadString();
            int val = param1.ReadInteger();
            counts[key] = val;
        }
        AnswerCounts = counts;
        return true;
    }
}
