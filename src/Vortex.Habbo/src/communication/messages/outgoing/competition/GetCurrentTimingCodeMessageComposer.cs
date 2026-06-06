using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Competition;

/// @see com.sulake.habbo.communication.messages.outgoing.competition.GetCurrentTimingCodeMessageComposer
public class GetCurrentTimingCodeMessageComposer(string slotConfig) : IMessageComposer
{
    public void Dispose() { }

    /// @see GetCurrentTimingCodeMessageComposer.as::getMessageArray
    public List<object> GetMessageArray()
    {
        return [slotConfig];
    }
}
