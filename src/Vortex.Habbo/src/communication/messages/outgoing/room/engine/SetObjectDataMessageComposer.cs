using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Engine;

/// @see com.sulake.habbo.communication.messages.outgoing.room.engine.SetObjectDataMessageComposer
public class SetObjectDataMessageComposer(int objectId, Dictionary<string, string> data) : IMessageComposer
{
    public void Dispose() { }

    public List<object> GetMessageArray()
    {
        List<object> result = new()
        {
            objectId, data.Count * 2,
        };
        foreach (KeyValuePair<string, string> kvp in data)
        {
            result.Add(kvp.Key);
            result.Add(kvp.Value);
        }
        return result;
    }
}
