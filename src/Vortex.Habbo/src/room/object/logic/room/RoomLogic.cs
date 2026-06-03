using System.Xml.Linq;

using Godot;

using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Messages;
using Vortex.Room.Events;
using Vortex.Room.Messages;
using Vortex.Room.Object;
using Vortex.Room.Object.Logic;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Logic;

/// @see com.sulake.habbo.room.object.logic.room.RoomLogic
public class RoomLogic : ObjectLogicBase
{
    protected RoomPlaneParser? _planeParser = new();
    private RoomPlaneBitmapMaskParser? _planeMaskParser = new();
    private ColorTransitioner? _colorTransitioner = new();
    private bool _floorHoleDirty;

    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_MOVE, RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_CLICK];
        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void Dispose()
    {
        base.Dispose();
        if (_planeParser != null)
        {
            _planeParser.Dispose();
            _planeParser = null;
        }
        if (_planeMaskParser != null)
        {
            _planeMaskParser.Dispose();
            _planeMaskParser = null;
        }
        _colorTransitioner = null;
    }

    public override void Initialize(XElement? xml)
    {
        if (xml == null || Object == null)
        {
            return;
        }

        if (!_planeParser!.InitializeFromXML(xml))
        {
            return;
        }

        IRoomObjectModelController model = Object.ModelController;
        model.SetString("room_plane_xml", xml.ToString());
        model.SetNumber("room_background_color", 0xFFFFFF);
        model.SetNumber("room_floor_visibility", 1);
        model.SetNumber("room_wall_visibility", 1);
        model.SetNumber("room_landscape_visibility", 1);
    }

    public override void Update(int time)
    {
        base.Update(time);
        UpdateBackgroundColor(time);

        if (_floorHoleDirty)
        {
            if (Object != null)
            {
                IRoomObjectModelController model = Object.ModelController;
                XElement xml = _planeParser!.GetXML();
                model.SetString("room_plane_xml", xml.ToString());
                model.SetNumber("room_floor_hole_update_time", time);
                _planeParser.InitializeFromXML(xml);
            }
            _floorHoleDirty = false;
        }
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage message)
    {
        if (message == null || Object == null)
        {
            return;
        }

        IRoomObjectModelController model = Object.ModelController;

        if (message is RoomObjectRoomUpdateMessage roomMsg)
        {
            UpdatePlaneTypes(roomMsg, model);
            return;
        }

        if (message is RoomObjectRoomMaskUpdateMessage maskMsg)
        {
            UpdatePlaneMasks(maskMsg, model);
            return;
        }

        if (message is RoomObjectRoomPlaneVisibilityUpdateMessage visMsg)
        {
            UpdatePlaneVisibilities(visMsg, model);
            return;
        }

        if (message is RoomObjectRoomPlanePropertyUpdateMessage propMsg)
        {
            UpdatePlaneProperties(propMsg, model);
            return;
        }

        if (message is RoomObjectRoomFloorHoleUpdateMessage holeMsg)
        {
            UpdateFloorHoles(holeMsg, model);
        }

        if (message is RoomObjectRoomColorUpdateMessage colorMsg)
        {
            UpdateColors(colorMsg, model);
        }
    }

    public override void MouseEvent(RoomSpriteMouseEvent mouseEvent, IRoomGeometry geometry)
    {
        if (geometry == null || mouseEvent == null || Object == null)
        {
            return;
        }

        IRoomObjectModelController model = Object.ModelController;

        int planeIndex = 0;
        string? spriteTag = mouseEvent.SpriteTag;
        if (spriteTag != null)
        {
            int atIndex = spriteTag.IndexOf('@');
            if (atIndex >= 0)
            {
                int.TryParse(spriteTag[(atIndex + 1)..], out planeIndex);
            }
        }

        if (planeIndex < 1 || planeIndex > _planeParser!.PlaneCount)
        {
            if (mouseEvent.Type == "rollOut")
            {
                model.SetNumber("room_selected_plane", 0);
            }
            return;
        }

        planeIndex -= 1;

        IVector3d? planeLocation = _planeParser.GetPlaneLocation(planeIndex);
        IVector3d? planeLeftSide = _planeParser.GetPlaneLeftSide(planeIndex);
        IVector3d? planeRightSide = _planeParser.GetPlaneRightSide(planeIndex);
        IVector3d? planeNormal = _planeParser.GetPlaneNormalDirection(planeIndex);
        int planeType = _planeParser.GetPlaneType(planeIndex);

        if (planeLocation == null || planeLeftSide == null || planeRightSide == null || planeNormal == null)
        {
            return;
        }

        double leftLength = planeLeftSide.Length;
        double rightLength = planeRightSide.Length;
        if (leftLength == 0 || rightLength == 0)
        {
            return;
        }

        Vector2 screenPoint = new((float)mouseEvent.ScreenX, (float)mouseEvent.ScreenY);
        Vector2? planePos = geometry.GetPlanePosition(screenPoint, planeLocation, planeLeftSide, planeRightSide);
        if (planePos == null)
        {
            model.SetNumber("room_selected_plane", 0);
            return;
        }

        Vector3d pos = Vector3d.Product(planeLeftSide, planePos.Value.X / leftLength)!;
        pos.Add(Vector3d.Product(planeRightSide, planePos.Value.Y / rightLength)!);
        pos.Add(planeLocation);

        double worldX = pos.X;
        double worldY = pos.Y;
        double worldZ = pos.Z;

        if (planePos.Value.X >= 0 && planePos.Value.X < leftLength &&
            planePos.Value.Y >= 0 && planePos.Value.Y < rightLength)
        {
            model.SetNumber("room_selected_x", worldX);
            model.SetNumber("room_selected_y", worldY);
            model.SetNumber("room_selected_z", worldZ);
            model.SetNumber("room_selected_plane", planeIndex + 1);

            RoomObjectEvent? evt = null;

            switch (mouseEvent.Type)
            {
                case "mouseMove":
                case "rollOver":
                case "mouseDown":
                case "click":
                    {
                        string eventType;
                        if (mouseEvent.Type is "mouseMove" or "rollOver")
                        {
                            eventType = RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_MOVE;
                        }
                        else if (mouseEvent.Type == "click")
                        {
                            eventType = RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_CLICK;
                        }
                        else
                        {
                            eventType = RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_DOWN;
                        }

                        if (EventDispatcher != null)
                        {
                            if (planeType == 1)
                            {
                                evt = new RoomObjectTileMouseEvent(
                                    eventType, Object, mouseEvent.EventId,
                                    worldX, worldY, worldZ,
                                    mouseEvent.AltKey, mouseEvent.CtrlKey, mouseEvent.ShiftKey
                                );
                            }
                            else if (planeType is 2 or 3)
                            {
                                double direction = 90;
                                if (planeNormal != null)
                                {
                                    direction = planeNormal.X + 90;
                                    if (direction > 360)
                                    {
                                        direction -= 360;
                                    }
                                }

                                double wallX = planeLeftSide.Length * planePos.Value.X / leftLength;
                                double wallY = planeRightSide.Length * planePos.Value.Y / rightLength;

                                evt = new RoomObjectWallMouseEvent(
                                    eventType, Object, mouseEvent.EventId,
                                    planeLocation, planeLeftSide, planeRightSide,
                                    wallX, wallY, direction
                                );
                            }

                            if (evt != null)
                            {
                                DispatchEvent(evt);
                            }
                        }
                        break;
                    }
            }
            return;
        }

        model.SetNumber("room_selected_plane", 0);
    }

    private void UpdateBackgroundColor(int time)
    {
        if (Object == null)
        {
            return;
        }

        if (_colorTransitioner != null && _colorTransitioner.UpdateColor(time))
        {
            int color = _colorTransitioner.Color;
            IRoomObjectModelController model = Object.ModelController;
            model.SetNumber("room_background_color", color);
        }
    }

    private static void UpdatePlaneTypes(RoomObjectRoomUpdateMessage message, IRoomObjectModelController model)
    {
        switch (message.Type)
        {
            case RoomObjectRoomUpdateMessage.ROOM_FLOOR_UPDATE:
                model.SetString("room_floor_type", message.Value);
                break;
            case RoomObjectRoomUpdateMessage.ROOM_WALL_UPDATE:
                model.SetString("room_wall_type", message.Value);
                break;
            case RoomObjectRoomUpdateMessage.ROOM_LANDSCAPE_UPDATE:
                model.SetString("room_landscape_type", message.Value);
                break;
        }
    }

    private void UpdatePlaneMasks(RoomObjectRoomMaskUpdateMessage message, IRoomObjectModelController model)
    {
        bool changed = false;

        switch (message.Type)
        {
            case RoomObjectRoomMaskUpdateMessage.ADD_MASK:
                {
                    string category = message.MaskCategory == "hole" ? "hole" : "window";
                    _planeMaskParser!.AddMask(message.MaskId, message.MaskType, message.MaskLocation, category);
                    changed = true;
                    break;
                }
            case RoomObjectRoomMaskUpdateMessage.REMOVE_MASK:
                changed = _planeMaskParser!.RemoveMask(message.MaskId);
                break;
        }

        if (changed)
        {
            XElement xml = _planeMaskParser!.GetXML();
            model.SetString("room_plane_mask_xml", xml.ToString());
        }
    }

    private static void UpdatePlaneVisibilities(RoomObjectRoomPlaneVisibilityUpdateMessage message, IRoomObjectModelController model)
    {
        int value = message.Visible ? 1 : 0;

        switch (message.Type)
        {
            case RoomObjectRoomPlaneVisibilityUpdateMessage.FLOOR_VISIBILITY:
                model.SetNumber("room_floor_visibility", value);
                break;
            case RoomObjectRoomPlaneVisibilityUpdateMessage.WALL_VISIBILITY:
                model.SetNumber("room_wall_visibility", value);
                model.SetNumber("room_landscape_visibility", value);
                break;
        }
    }

    private static void UpdatePlaneProperties(RoomObjectRoomPlanePropertyUpdateMessage message, IRoomObjectModelController model)
    {
        switch (message.Type)
        {
            case RoomObjectRoomPlanePropertyUpdateMessage.FLOOR_THICKNESS:
                model.SetNumber("room_floor_thickness", message.Value);
                break;
            case RoomObjectRoomPlanePropertyUpdateMessage.WALL_THICKNESS:
                model.SetNumber("room_wall_thickness", message.Value);
                break;
        }
    }

    private void UpdateFloorHoles(RoomObjectRoomFloorHoleUpdateMessage message, IRoomObjectModelController model)
    {
        switch (message.Type)
        {
            case RoomObjectRoomFloorHoleUpdateMessage.ADD_HOLE:
                _planeParser!.AddFloorHole(message.Id, message.X, message.Y, message.Width, message.Height, message.Invert);
                _floorHoleDirty = true;
                break;
            case RoomObjectRoomFloorHoleUpdateMessage.REMOVE_HOLE:
                _planeParser!.RemoveFloorHole(message.Id);
                _floorHoleDirty = true;
                break;
        }
    }

    private void UpdateColors(RoomObjectRoomColorUpdateMessage message, IRoomObjectModelController model)
    {
        int color = (int)message.Color;
        int light = message.Light;
        model.SetNumber("room_colorize_bg_only", message.BgOnly ? 1 : 0);

        int targetColor;
        int targetLight;
        if (message.BgOnly)
        {
            targetColor = color;
            targetLight = light;
        }
        else
        {
            targetColor = 0xFFFFFF;
            targetLight = 255;
        }

        _colorTransitioner?.StartTransition(targetColor, targetLight, System.Environment.TickCount);
    }
}
