// @see com.sulake.habbo.communication.messages.parser.room.quiz.QuestionAnsweredParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Quiz;

/// @see com.sulake.habbo.communication.messages.parser.room.quiz.QuestionAnsweredParser
public class QuestionAnsweredMessageEventParser : IMessageParser
{
    public int UserId { get; private set; }
    public string? Value { get; private set; }
    public Dictionary<string, int>? AnswerCounts { get; private set; }

    public bool Flush()
    {
        UserId = 0;
        Value = null;
        AnswerCounts = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source — QuestionAnsweredParser
        UserId = param1.ReadInteger();
        Value = param1.ReadString();
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
