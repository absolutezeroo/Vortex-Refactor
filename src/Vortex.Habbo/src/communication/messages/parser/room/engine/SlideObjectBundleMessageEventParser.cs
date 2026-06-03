using System.Globalization;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.SlideObjectBundleMessageEventParser
public class SlideObjectBundleMessageEventParser : IMessageParser
{
    private readonly List<SlideObjectMessageData> _objectList = [];

    public int Id { get; private set; } = -1;
    public SlideObjectMessageData? Avatar { get; private set; }
    public IReadOnlyList<SlideObjectMessageData> ObjectList => _objectList;

    public bool Flush()
    {
        Id = -1;
        Avatar = null;
        _objectList.Clear();
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        double oldX = (double)param1.ReadInteger();
        double oldY = (double)param1.ReadInteger();
        double newX = (double)param1.ReadInteger();
        double newY = (double)param1.ReadInteger();
        int objectCount = param1.ReadInteger();
        _objectList.Clear();
        for (int i = 0; i < objectCount; i++)
        {
            int objectId = param1.ReadInteger();
            double oldZ = double.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
            double newZ = double.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
            Vector3d loc = new(oldX, oldY, oldZ);
            Vector3d target = new(newX, newY, newZ);
            _objectList.Add(new SlideObjectMessageData(objectId, loc, target));
        }
        Id = param1.ReadInteger();
        if (param1.bytesAvailable == 0)
        {
            return true;
        }
        int moveTypeId = param1.ReadInteger();
        switch (moveTypeId)
        {
            case 0:
                break;
            case 1:
                {
                    int avatarId = param1.ReadInteger();
                    double oldZ = double.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
                    double newZ = double.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
                    Avatar = new SlideObjectMessageData(avatarId, new Vector3d(oldX, oldY, oldZ), new Vector3d(newX, newY, newZ),
                        SlideObjectMessageData.MOVE);
                    break;
                }
            case 2:
                {
                    int avatarId = param1.ReadInteger();
                    double oldZ = double.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
                    double newZ = double.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
                    Avatar = new SlideObjectMessageData(avatarId, new Vector3d(oldX, oldY, oldZ), new Vector3d(newX, newY, newZ),
                        SlideObjectMessageData.SLIDE);
                    break;
                }
        }
        return true;
    }
}
