using System.Globalization;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.class_1750
public static class ItemDataParseHelper
{
    public static ItemMessageData ParseItemData(IMessageDataWrapper param1)
    {
        int itemId = int.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
        int typeId = param1.ReadInteger();
        string location = param1.ReadString();
        string dataStr = param1.ReadString();
        int secondsToExpiration = param1.ReadInteger();
        int usagePolicy = param1.ReadInteger();
        int ownerId = param1.ReadInteger();

        int state = 0;
        if (double.TryParse(dataStr, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
        {
            state = int.Parse(dataStr, CultureInfo.InvariantCulture);
        }

        ItemMessageData item;
        if (location.StartsWith(':'))
        {
            item = new ItemMessageData(itemId, typeId, false);
            string[] parts = location.Split(' ');
            if (parts.Length >= 3)
            {
                string wallPart = parts[0];
                string localPart = parts[1];
                string dirPart = parts[2];
                if (wallPart.Length > 3 && localPart.Length > 2)
                {
                    wallPart = wallPart[3..];
                    localPart = localPart[2..];
                    string[] wallCoords = wallPart.Split(',');
                    if (wallCoords.Length >= 2)
                    {
                        string[] localCoords = localPart.Split(',');
                        if (localCoords.Length >= 2)
                        {
                            item.WallX = int.Parse(wallCoords[0], CultureInfo.InvariantCulture);
                            item.WallY = int.Parse(wallCoords[1], CultureInfo.InvariantCulture);
                            item.LocalX = int.Parse(localCoords[0], CultureInfo.InvariantCulture);
                            item.LocalY = int.Parse(localCoords[1], CultureInfo.InvariantCulture);
                            item.Dir = dirPart;
                            item.Data = dataStr;
                            item.State = state;
                        }
                    }
                }
            }
        }
        else
        {
            item = new ItemMessageData(itemId, typeId, true);
            string[] parts = location.Split(' ');
            if (parts.Length >= 2)
            {
                string dirStr = parts[0];
                dirStr = dirStr is "rightwall" or "frontwall" ? "r" : "l";
                string coordStr = parts[1];
                string[] coords = coordStr.Split(',');
                if (coords.Length >= 3)
                {
                    double y = double.Parse(coords[0], CultureInfo.InvariantCulture);
                    double z = double.Parse(coords[1], CultureInfo.InvariantCulture);
                    item.Y = y;
                    item.Z = z;
                    item.Dir = dirStr;
                    item.Data = dataStr;
                    item.State = state;
                }
            }
        }

        item.UsagePolicy = usagePolicy;
        item.OwnerId = ownerId;
        item.SecondsToExpiration = secondsToExpiration;
        return item;
    }
}
