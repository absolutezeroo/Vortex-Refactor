using Vortex.Room.Object;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureRoomBillboardVisualization
public class FurnitureRoomBillboardVisualization : FurnitureRoomBrandingVisualization
{
    protected override string? GetAdClickUrl(IRoomObjectModel model)
    {
        return model.GetString("furniture_branding_url");
    }

    protected override int GetSpriteXOffset(int scale, int direction, int layer)
    {
        return base.GetSpriteXOffset(scale, direction, layer) + _brandingOffsetX;
    }

    protected override int GetSpriteYOffset(int scale, int direction, int layer)
    {
        return base.GetSpriteYOffset(scale, direction, layer) + _brandingOffsetY;
    }

    protected override double GetSpriteZOffset(int scale, int direction, int layer)
    {
        return base.GetSpriteZOffset(scale, direction, layer) + (_brandingOffsetZ * -1);
    }
}
