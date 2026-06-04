// @see com.sulake.habbo.communication.messages.parser.room.quiz.QuestionParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Quiz;

/// @see com.sulake.habbo.communication.messages.parser.room.quiz.QuestionParser
public class QuestionMessageEventParser : IMessageParser
{
    public int PollId { get; private set; }
    public Dictionary<object, object>? Question { get; private set; }
    public int Duration { get; private set; }
    public string? PollType { get; private set; }
    public int QuestionId { get; private set; }

    public bool Flush()
    {
        PollId = 0;
        Question = null;
        Duration = 0;
        PollType = null;
        QuestionId = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source — QuestionParser
        PollId = param1.ReadInteger();
        Duration = param1.ReadInteger();
        PollType = param1.ReadString();
        QuestionId = param1.ReadInteger();
        // TODO(as3-port): parse question Dictionary from wire format
        Question = new Dictionary<object, object>();
        return true;
    }
}
