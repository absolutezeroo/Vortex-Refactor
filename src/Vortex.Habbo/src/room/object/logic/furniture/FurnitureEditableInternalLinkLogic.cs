namespace Vortex.Habbo.Room.Object.Logic;

using System.Xml.Linq;

using Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureEditableInternalLinkLogic
public class FurnitureEditableInternalLinkLogic : FurnitureMultiStateLogic
{
    private bool _startState;
    private int _frameCounter;

    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectWidgetRequestEvent.INTERNAL_LINK];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void Initialize(XElement? xml)
    {
        base.Initialize(xml);

        if (xml == null)
        {
            return;
        }

        XElement? action = xml.Element("action");

        if (action == null)
        {
            return;
        }

        string? startStateStr = (string?)action.Attribute("startState");

        _startState = startStateStr == "1";
    }

    public override void Update(int time)
    {
        base.Update(time);

        if (!_startState || _frameCounter < 0)
        {
            return;
        }

        _frameCounter++;

        if (_frameCounter < 20)
        {
            return;
        }

        SetAnimationState(1);

        _frameCounter = -1;
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        SetAnimationState(0);
        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.INTERNAL_LINK, Object));
    }

    private void SetAnimationState(int state)
    {
        Object?.ModelController.SetNumber("furniture_automatic_state_index", state);
    }
}
