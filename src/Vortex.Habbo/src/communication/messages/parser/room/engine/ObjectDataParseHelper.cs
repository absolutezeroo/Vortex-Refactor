using System.Globalization;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;
using Vortex.Habbo.Room;
using Vortex.Habbo.Room.Object.Data;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.class_1642
public static class ObjectDataParseHelper
{
    public static ObjectMessageData? ParseObjectData(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return null;
        }
        int id = param1.ReadInteger();
        ObjectMessageData data = new(id);
        int type = param1.ReadInteger();
        data.Type = type;
        data.X = param1.ReadInteger();
        data.Y = param1.ReadInteger();
        data.Dir = param1.ReadInteger() % 8 * 45;
        data.Z = double.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
        data.SizeZ = double.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
        data.Extra = param1.ReadInteger();
        data.Data = ParseStuffData(param1);
        if (double.TryParse(data.Data.GetLegacyString(), NumberStyles.Float, CultureInfo.InvariantCulture, out _))
        {
            data.State = int.Parse(data.Data.GetLegacyString(), CultureInfo.InvariantCulture);
        }
        data.ExpiryTime = param1.ReadInteger();
        data.UsagePolicy = param1.ReadInteger();
        data.OwnerId = param1.ReadInteger();
        if (type < 0)
        {
            data.StaticClass = param1.ReadString();
        }
        return data;
    }

    public static IStuffData ParseStuffData(IMessageDataWrapper param1)
    {
        int typeId = param1.ReadInteger();
        IStuffData? stuffData = StuffDataFactory.GetStuffDataWrapperForType(typeId);
        stuffData ??= new LegacyStuffData();
        stuffData.InitializeFromIncomingMessage(param1);
        return stuffData;
    }
}
