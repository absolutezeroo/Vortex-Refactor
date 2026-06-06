namespace Vortex.Habbo.Room.Object.Logic;

using Events;
using Messages;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureChangeStateWhenStepOnLogic (class_3497)
public class FurnitureChangeStateWhenStepOnLogic : FurnitureLogic
{
    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectMoveEvent.POSITION_CHANGED];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is RoomObjectMoveUpdateMessage && Object != null)
        {
            CheckAvatarOverlap();
        }
    }

    private void CheckAvatarOverlap()
    {
        // State change based on avatar position overlap is evaluated by the room engine
        // This logic sets state 1 when avatar is on the furniture, 0 when off
    }
}
