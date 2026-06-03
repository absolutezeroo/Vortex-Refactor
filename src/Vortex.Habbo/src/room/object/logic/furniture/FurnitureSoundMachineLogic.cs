namespace Vortex.Habbo.Room.Object.Logic;

using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Messages;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureSoundMachineLogic
public class FurnitureSoundMachineLogic : FurnitureMultiStateLogic
{
    private bool _disposeRequested;
    private bool _initialized;
    private int _lastState;

    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectFurnitureActionEvent.SOUND_MACHINE_INIT,
            RoomObjectFurnitureActionEvent.SOUND_MACHINE_START,
            RoomObjectFurnitureActionEvent.SOUND_MACHINE_STOP,
            RoomObjectFurnitureActionEvent.SOUND_MACHINE_DISPOSE,
        ];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void Dispose()
    {
        RequestDispose();

        base.Dispose();
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is not RoomObjectDataUpdateMessage || Object == null)
        {
            return;
        }

        if (!_initialized)
        {
            RequestInit();

            _initialized = true;
        }

        int state = Object.GetState(0);

        if (state == _lastState)
        {
            return;
        }

        switch (state)
        {
            case 1:
                RequestStart();

                break;
            case 0:
                RequestStop();

                break;
        }

        _lastState = state;
    }

    private void RequestInit()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectFurnitureActionEvent(
            RoomObjectFurnitureActionEvent.SOUND_MACHINE_INIT, Object));
    }

    private void RequestStart()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectFurnitureActionEvent(
            RoomObjectFurnitureActionEvent.SOUND_MACHINE_START, Object));
    }

    private void RequestStop()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectFurnitureActionEvent(
            RoomObjectFurnitureActionEvent.SOUND_MACHINE_STOP, Object));
    }

    private void RequestDispose()
    {
        if (_disposeRequested || Object == null)
        {
            return;
        }

        _disposeRequested = true;

        DispatchEvent(new RoomObjectFurnitureActionEvent(
            RoomObjectFurnitureActionEvent.SOUND_MACHINE_DISPOSE, Object));
    }
}
