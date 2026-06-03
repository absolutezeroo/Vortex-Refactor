using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.FurnitureAliasesMessageEventParser
public class FurnitureAliasesMessageEventParser : IMessageParser
{
    private readonly List<(string Name, string Alias)> _aliases = [];

    public int AliasCount => _aliases.Count;

    public string? GetName(int index)
    {
        return index >= 0 && index < _aliases.Count ? _aliases[index].Name : null;
    }

    public string? GetAlias(int index)
    {
        return index >= 0 && index < _aliases.Count ? _aliases[index].Alias : null;
    }

    public bool Flush()
    {
        _aliases.Clear();
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        _aliases.Clear();
        int count = param1.ReadInteger();
        for (int i = 0; i < count; i++)
        {
            string name = param1.ReadString();
            string alias = param1.ReadString();
            _aliases.Add((name, alias));
        }
        return true;
    }
}
