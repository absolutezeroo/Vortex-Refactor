namespace Vortex.Habbo.Room.Object.Logic;

using System;

using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Messages;
using Vortex.Habbo.Room.Object.Data;
using Vortex.Room.Events;
using Vortex.Room.Messages;
using Vortex.Room.Object;
using Vortex.Room.Utils;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureRoomBrandingLogic
public class FurnitureRoomBrandingLogic : FurnitureLogic
{
    private const string STUFF_DATA_KEY_STATE = "state";
    private const string IMAGEURL = "imageUrl";
    private const string CLICKURL = "clickUrl";
    private const string OFFSETX = "offsetX";
    private const string OFFSETY = "offsetY";
    private const string OFFSETZ = "offsetZ";

    private bool _selectionDisabled;
    protected bool _hasClickUrl;

    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectRoomAdEvent.ROOM_AD_LOAD_IMAGE,
            RoomObjectRoomAdEvent.ROOM_AD_FURNI_CLICK,
            RoomObjectRoomAdEvent.ROOM_AD_FURNI_DOUBLE_CLICK,
            RoomObjectRoomAdEvent.ROOM_AD_TOOLTIP_SHOW,
            RoomObjectRoomAdEvent.ROOM_AD_TOOLTIP_HIDE,
        ];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is RoomObjectDataUpdateMessage dataMsg && Object != null)
        {
            SetupImageFromFurnitureData(dataMsg.Data);
        }
    }

    public override void MouseEvent(RoomSpriteMouseEvent mouseEvent, IRoomGeometry geometry)
    {
        if (mouseEvent.Type is "mouseMove" or "doubleClick")
        {
            return;
        }

        base.MouseEvent(mouseEvent, geometry);
    }

    protected override string? GetAdClickUrl(IRoomObjectModelController model)
    {
        return model.GetString("furniture_branding_url");
    }

    protected override void HandleAdClick(int objectId, string objectType, string clickUrl)
    {
        if (_hasClickUrl && clickUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            // Open web page — adapted from HabboWebTools.openWebPage
            base.HandleAdClick(objectId, objectType, clickUrl);
        }
        else
        {
            DispatchEvent(new RoomObjectRoomAdEvent(
                RoomObjectRoomAdEvent.ROOM_AD_FURNI_CLICK, Object, "", clickUrl));
        }
    }

    private void SetupImageFromFurnitureData(IStuffData? data)
    {
        if (Object == null || data is not MapStuffData mapData)
        {
            return;
        }

        string? imageUrl = mapData.GetValue(IMAGEURL);

        if (imageUrl != null)
        {
            imageUrl = ForceImageUrlToUseHttps(imageUrl);
            Object.ModelController.SetString("furniture_branding_image_url", imageUrl);
        }

        string? clickUrl = mapData.GetValue(CLICKURL);

        if (clickUrl != null)
        {
            Object.ModelController.SetString("furniture_branding_url", clickUrl);
        }

        UpdateOffset(mapData);

        if (imageUrl is { Length: > 0 })
        {
            DispatchEvent(new RoomObjectRoomAdEvent(
                RoomObjectRoomAdEvent.ROOM_AD_LOAD_IMAGE, Object, imageUrl, clickUrl ?? ""));
        }

        // Set infostand extra param
        string? state = mapData.GetValue(STUFF_DATA_KEY_STATE);
        Object.ModelController.SetString("RWEIEP_INFOSTAND_EXTRA_PARAM",
            $"branding_image={imageUrl ?? ""}&branding_url={clickUrl ?? ""}&state={state ?? ""}");

        _selectionDisabled = true;
    }

    private void UpdateOffset(MapStuffData mapData)
    {
        if (Object == null)
        {
            return;
        }

        string? offsetX = mapData.GetValue(OFFSETX);

        if (offsetX != null && int.TryParse(offsetX, out int ox))
        {
            Object.ModelController.SetNumber("furniture_branding_offset_x", ox);
        }

        string? offsetY = mapData.GetValue(OFFSETY);

        if (offsetY != null && int.TryParse(offsetY, out int oy))
        {
            Object.ModelController.SetNumber("furniture_branding_offset_y", oy);
        }

        string? offsetZ = mapData.GetValue(OFFSETZ);

        if (offsetZ != null && int.TryParse(offsetZ, out int oz))
        {
            Object.ModelController.SetNumber("furniture_branding_offset_z", oz);
        }
    }

    private static string ForceImageUrlToUseHttps(string url)
    {
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            return "https://" + url[7..];
        }

        return url;
    }
}
