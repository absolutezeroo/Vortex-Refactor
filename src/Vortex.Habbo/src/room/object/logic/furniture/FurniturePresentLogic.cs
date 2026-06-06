namespace Vortex.Habbo.Room.Object.Logic;

using System.Xml.Linq;

using Events;
using Messages;
using Data;
using Vortex.Room.Events;
using Vortex.Room.Messages;
using Vortex.Room.Utils;

/// @see com.sulake.habbo.room.object.logic.furniture.FurniturePresentLogic
public class FurniturePresentLogic : FurnitureLogic
{
    private const string MESSAGE = "MESSAGE";
    private const string PRODUCT_CODE = "PRODUCT_CODE";
    private const string EXTRA_PARAM = "EXTRA_PARAM";
    private const string PURCHASER_NAME = "PURCHASER_NAME";
    private const string PURCHASER_FIGURE = "PURCHASER_FIGURE";
    private const string TRUSTED_SENDER = "TRUSTED_SENDER";

    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectWidgetRequestEvent.PRESENT,
            RoomObjectFurnitureActionEvent.CURSOR_REQUEST_BUTTON,
            RoomObjectFurnitureActionEvent.CURSOR_REQUEST_ARROW,
        ];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void Initialize(XElement? xml)
    {
        base.Initialize(xml);

        if (xml == null || Object == null)
        {
            return;
        }

        XElement? particleSystems = xml.Element("particlesystems");

        if (particleSystems != null)
        {
            Object.ModelController.SetString("furniture_fireworks_data", particleSystems.ToString());
        }
    }

    public override void MouseEvent(RoomSpriteMouseEvent mouseEvent, IRoomGeometry geometry)
    {
        switch (mouseEvent.Type)
        {
            case "rollOver":
                DispatchEvent(new RoomObjectFurnitureActionEvent(
                    RoomObjectFurnitureActionEvent.CURSOR_REQUEST_BUTTON, Object));
                break;
            case "rollOut":
                DispatchEvent(new RoomObjectFurnitureActionEvent(
                    RoomObjectFurnitureActionEvent.CURSOR_REQUEST_ARROW, Object));
                break;
        }
        base.MouseEvent(mouseEvent, geometry);
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is not RoomObjectDataUpdateMessage dataMsg || Object == null)
        {
            return;
        }

        if (dataMsg.Data is not MapStuffData mapData)
        {
            return;
        }

        string? msg = mapData.GetValue(MESSAGE);

        if (msg != null)
        {
            Object.ModelController.SetString("furniture_data", msg);
        }

        string? typeId = mapData.GetValue(PRODUCT_CODE);

        if (typeId != null)
        {
            Object.ModelController.SetString("furniture_type_id", typeId);
        }

        string? purchaserName = mapData.GetValue(PURCHASER_NAME);

        if (purchaserName != null)
        {
            Object.ModelController.SetString("furniture_purchaser_name", purchaserName);
        }

        string? purchaserFigure = mapData.GetValue(PURCHASER_FIGURE);

        if (purchaserFigure != null)
        {
            Object.ModelController.SetString("furniture_purchaser_figure", purchaserFigure);
        }

        string? trustedSender = mapData.GetValue(TRUSTED_SENDER);

        if (trustedSender != null)
        {
            Object.ModelController.SetString("furniture_trusted_sender", trustedSender);
        }
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.PRESENT, Object));
    }
}
