// @see com.sulake.habbo.communication.messages.parser.room.poll.PollContentsParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Poll;

/// @see com.sulake.habbo.communication.messages.parser.room.poll.PollContentsParser
public class PollContentsMessageEventParser : IMessageParser
{
    public int Id { get; private set; }
    public string? StartMessage { get; private set; }
    public string? EndMessage { get; private set; }
    public int NumQuestions { get; private set; }
    public object[]? QuestionArray { get; private set; }
    public bool NpsPoll { get; private set; }

    public bool Flush()
    {
        Id = 0;
        StartMessage = null;
        EndMessage = null;
        NumQuestions = 0;
        QuestionArray = null;
        NpsPoll = false;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify full wire format from AS3 source — PollContentsParser has complex question sub-parsing
        Id = param1.ReadInteger();
        StartMessage = param1.ReadString();
        EndMessage = param1.ReadString();
        NumQuestions = param1.ReadInteger();
        QuestionArray = new object[NumQuestions]; // TODO(as3-port): parse individual question objects
        NpsPoll = param1.ReadBoolean();
        return true;
    }
}
