namespace Vortex.Habbo.Room.Object.Logic;

using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Messages;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureJukeboxLogic
public class FurnitureJukeboxLogic : FurnitureMultiStateLogic
{
    private bool _disposeRequested;
    private bool _initialized;
    private int _lastState;

    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectFurnitureActionEvent.JUKEBOX_INIT,
            RoomObjectFurnitureActionEvent.JUKEBOX_START,
            RoomObjectFurnitureActionEvent.JUKEBOX_MACHINE_STOP,
            RoomObjectFurnitureActionEvent.JUKEBOX_DISPOSE,
            RoomObjectWidgetRequestEvent.JUKEBOX_PLAYLIST_EDITOR,
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
                RequestPlayList();
                break;
            case 0:
                RequestStopPlaying();
                break;
        }

        _lastState = state;
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.JUKEBOX_PLAYLIST_EDITOR, Object));
        DispatchEvent(new RoomObjectStateChangeEvent(
            RoomObjectStateChangeEvent.ROOM_OBJECT_STATE_CHANGE, Object, -1));
    }

    private void RequestInit()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectFurnitureActionEvent(
            RoomObjectFurnitureActionEvent.JUKEBOX_INIT, Object));
    }

    private void RequestPlayList()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectFurnitureActionEvent(
            RoomObjectFurnitureActionEvent.JUKEBOX_START, Object));
    }

    private void RequestStopPlaying()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectFurnitureActionEvent(
            RoomObjectFurnitureActionEvent.JUKEBOX_MACHINE_STOP, Object));
    }

    private void RequestDispose()
    {
        if (_disposeRequested || Object == null)
        {
            return;
        }

        _disposeRequested = true;

        DispatchEvent(new RoomObjectFurnitureActionEvent(
            RoomObjectFurnitureActionEvent.JUKEBOX_DISPOSE, Object));
    }
}
