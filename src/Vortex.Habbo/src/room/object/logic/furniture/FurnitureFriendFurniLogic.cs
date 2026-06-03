namespace Vortex.Habbo.Room.Object.Logic;

using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Messages;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureFriendFurniLogic (class_3415)
public class FurnitureFriendFurniLogic : FurnitureMultiStateLogic
{
    private bool _locked;

    public override string? ContextMenu => _locked ? "DUMMY" : "FRIEND_FURNITURE";

    public virtual int EngravingDialogType => 0;

    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectWidgetRequestEvent.FRIEND_FURNITURE_CONFIRM,
            RoomObjectWidgetRequestEvent.FRIEND_FURNITURE_ENGRAVING,
        ];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is not RoomObjectDataUpdateMessage || Object == null)
        {
            return;
        }

        int state = Object.GetState(0);

        _locked = state != 0;
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        if (_locked)
        {
            return;
        }

        Object.ModelController.SetNumber("furniture_friend_furni_engraving_dialog", EngravingDialogType);

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.FRIEND_FURNITURE_ENGRAVING, Object));
    }
}
