using System.Xml.Linq;

using Vortex.Habbo.Room.Messages;
using Vortex.Room.Messages;
using Vortex.Room.Object;
using Vortex.Room.Object.Logic;

namespace Vortex.Habbo.Room.Object.Logic;

/// @see com.sulake.habbo.room.object.logic.room.SelectionArrowLogic
public class SelectionArrowLogic : ObjectLogicBase
{
    public override void Initialize(XElement? xml)
    {
        if (Object == null)
        {
            return;
        }

        IRoomObjectModelController model = Object.ModelController;

        model.SetNumber("furniture_alpha_multiplier", 1);
        Object.SetState(1, 0);
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage message)
    {
        base.ProcessUpdateMessage(message);

        if (message is RoomObjectVisibilityUpdateMessage visMsg)
        {
            switch (visMsg.Type)
            {
                case RoomObjectVisibilityUpdateMessage.ENABLED:
                    Object?.SetState(0, 0);
                    break;
                case RoomObjectVisibilityUpdateMessage.DISABLED:
                    Object?.SetState(1, 0);
                    break;
            }
        }
    }
}
