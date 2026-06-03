using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.BuildersClubPlacementWarningMessageEventParser
public class BuildersClubPlacementWarningMessageEventParser : IMessageParser
{
    public const int FLOOR_TYPE = 0;
    public const int WALL_TYPE = 1;

    public int TypeCode { get; private set; }
    public int PageId { get; private set; }
    public int OfferId { get; private set; }
    public string ExtraParam { get; private set; } = "";
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Direction { get; private set; }
    public string? WallLocation { get; private set; }

    public bool Flush()
    {
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        TypeCode = param1.ReadInteger();
        PageId = param1.ReadInteger();
        OfferId = param1.ReadInteger();
        ExtraParam = param1.ReadString();
        if (TypeCode == FLOOR_TYPE)
        {
            X = param1.ReadInteger();
            Y = param1.ReadInteger();
            Direction = param1.ReadInteger();
        }
        else
        {
            WallLocation = param1.ReadString();
        }
        return true;
    }
}
