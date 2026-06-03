using Vortex.Habbo.Room.Messages;
using Vortex.Room.Messages;
using Vortex.Room.Object;
using Vortex.Room.Object.Logic;

namespace Vortex.Habbo.Room.Object.Logic;

/// @see com.sulake.habbo.room.object.logic.room.RoomTileCursorLogic
public class RoomTileCursorLogic : ObjectLogicBase
{
    private const int STATE_ENABLED = 0;
    private const int STATE_DISABLED = 1;
    private const int STATE_SHOW_TILE_HEIGHT = 6;

    private string? _lastSourceEventId;
    private bool _hiddenOnPurpose;

    public override void Initialize(System.Xml.Linq.XElement? xml)
    {
        if (Object == null)
        {
            return;
        }

        IRoomObjectModelController model = Object.ModelController;

        model.SetNumber("furniture_alpha_multiplier", 1);
        Object.SetState(STATE_DISABLED, 0);
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage message)
    {
        if (message is not RoomObjectTileCursorUpdateMessage cursorMsg)
        {
            return;
        }

        if (_lastSourceEventId != null && _lastSourceEventId == cursorMsg.SourceEventId)
        {
            return;
        }

        if (cursorMsg.ToggleVisibility)
        {
            _hiddenOnPurpose = !_hiddenOnPurpose;
        }

        base.ProcessUpdateMessage(message);

        if (Object != null)
        {
            if (_hiddenOnPurpose || !cursorMsg.Visible)
            {
                Object.SetState(STATE_DISABLED, 0);
            }
            else
            {
                double height = cursorMsg.Height;
                Object.ModelController.SetNumber("tile_cursor_height", height);

                int state = height > 0.8 ? STATE_SHOW_TILE_HEIGHT : STATE_ENABLED;
                Object.SetState(state, 0);
            }
        }

        _lastSourceEventId = cursorMsg.SourceEventId;
    }
}
