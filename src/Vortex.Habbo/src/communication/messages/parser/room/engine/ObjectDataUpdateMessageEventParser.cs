using System.Globalization;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Room;
using Vortex.Habbo.Room.Object.Data;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.ObjectDataUpdateMessageEventParser
public class ObjectDataUpdateMessageEventParser : IMessageParser
{
    public int Id { get; private set; }
    public int State { get; private set; }
    public IStuffData Data { get; private set; } = new LegacyStuffData();

    public bool Flush()
    {
        State = 0;
        Data = new LegacyStuffData();
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        Id = int.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
        Data = ObjectDataParseHelper.ParseStuffData(param1);
        if (double.TryParse(Data.GetLegacyString(), NumberStyles.Float, CultureInfo.InvariantCulture, out _))
        {
            State = int.Parse(Data.GetLegacyString(), CultureInfo.InvariantCulture);
        }
        return true;
    }
}
