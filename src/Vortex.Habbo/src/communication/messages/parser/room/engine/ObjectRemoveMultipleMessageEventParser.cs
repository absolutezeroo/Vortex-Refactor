using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.ObjectRemoveMultipleMessageEventParser
public class ObjectRemoveMultipleMessageEventParser : IMessageParser
{
    private readonly List<int> _ids = [];

    public IReadOnlyList<int> Ids => _ids;
    public int PickerId { get; private set; }

    public bool Flush() { _ids.Clear(); PickerId = 0; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        _ids.Clear();
        int count = param1.ReadInteger();
        for (int i = 0; i < count; i++)
        {
            _ids.Add(param1.ReadInteger());
        }
        PickerId = param1.ReadInteger();
        return true;
    }
}
