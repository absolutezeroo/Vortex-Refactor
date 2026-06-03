using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Layout;

/// @see com.sulake.habbo.communication.messages.parser.room.layout.RoomOccupiedTilesMessageParser
public class RoomOccupiedTilesMessageEventParser : IMessageParser
{
    private readonly List<(int X, int Y)> _occupiedTiles = [];

    public int TileCount => _occupiedTiles.Count;

    public (int X, int Y)? GetTile(int index)
    {
        return index >= 0 && index < _occupiedTiles.Count ? _occupiedTiles[index] : null;
    }

    public bool Flush()
    {
        _occupiedTiles.Clear();
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        _occupiedTiles.Clear();
        int count = param1.ReadInteger();
        for (int i = 0; i < count; i++)
        {
            int x = param1.ReadInteger();
            int y = param1.ReadInteger();
            _occupiedTiles.Add((x, y));
        }
        return true;
    }
}
