using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Poll;

/// @see com.sulake.habbo.communication.messages.outgoing.room.poll.PollAnswerComposer
public class PollAnswerComposer(int pollId, int questionId, IReadOnlyList<string> answers) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        // TODO(as3-port): verify payload order from AS3 source
        var result = new List<object> { pollId, questionId, answers.Count };
        foreach (string answer in answers)
        {
            result.Add(answer);
        }

        return result;
    }
}
