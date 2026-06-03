namespace Vortex.Habbo.Room.Object.Logic;

using System;

using Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureRoomBillboardLogic
public class FurnitureRoomBillboardLogic : FurnitureRoomBrandingLogic
{
    public FurnitureRoomBillboardLogic()
    {
        _hasClickUrl = true;
    }

    protected override void HandleAdClick(int objectId, string objectType, string clickUrl)
    {
        if (clickUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            // External link — adapted from HabboWebTools.openWebPage
            base.HandleAdClick(objectId, objectType, clickUrl);
        }
        else
        {
            DispatchEvent(new RoomObjectRoomAdEvent(
                RoomObjectRoomAdEvent.ROOM_AD_FURNI_CLICK, Object, "", clickUrl));
        }
    }
}
