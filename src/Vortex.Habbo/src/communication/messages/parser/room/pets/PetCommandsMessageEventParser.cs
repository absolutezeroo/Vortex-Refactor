// @see com.sulake.habbo.communication.messages.parser.room.pets.PetCommandsMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

/// @see com.sulake.habbo.communication.messages.parser.room.pets.PetCommandsMessageParser
public class PetCommandsMessageEventParser : IMessageParser
{
    public int PetId { get; private set; }
    public IReadOnlyList<int> AllCommands { get; private set; } = [];
    public IReadOnlyList<int> EnabledCommands { get; private set; } = [];

    public bool Flush()
    {
        PetId = 0;
        AllCommands = [];
        EnabledCommands = [];
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        PetId = param1.ReadInteger();
        int allCount = param1.ReadInteger();
        var all = new List<int>(allCount);
        for (int i = 0; i < allCount; i++)
        {
            all.Add(param1.ReadInteger());
        }

        AllCommands = all;
        int enabledCount = param1.ReadInteger();
        var enabled = new List<int>(enabledCount);
        for (int i = 0; i < enabledCount; i++)
        {
            enabled.Add(param1.ReadInteger());
        }

        EnabledCommands = enabled;
        return true;
    }
}
