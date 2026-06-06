using Vortex.Room.Object;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureGiftWrappedVisualization
public class FurnitureGiftWrappedVisualization : FurnitureVisualization
{
    private int _packetType;
    private int _ribbonType;

    public override void Update(IRoomGeometry geometry, int time, bool full, bool skip)
    {
        UpdateTypes();
        base.Update(geometry, time, full, skip);
    }

    protected override int GetFrameNumber(int scale, int layer)
    {
        if (layer <= 1)
        {
            return _packetType;
        }

        return _ribbonType;
    }

    protected override string? GetSpriteAssetName(int scale, int spriteIndex)
    {
        int size = GetSize(scale);
        string type = Type;

        string layerCode;
        if (spriteIndex < SpriteCount - 1)
        {
            layerCode = ((char)('a' + spriteIndex)).ToString();
        }
        else
        {
            layerCode = "sd";
        }

        int frame = GetFrameNumber(scale, spriteIndex);
        return type + "_" + size + "_" + layerCode + "_" + Direction + "_" + frame;
    }

    private void UpdateTypes()
    {
        IRoomObject? obj = Object;

        if (obj == null)
        {
            return;
        }

        IRoomObjectModel model = obj.Model;

        string? extras = model.GetString("furniture_extras");
        int extrasValue = 0;

        if (!string.IsNullOrEmpty(extras))
        {
            int.TryParse(extras, out extrasValue);
        }

        _packetType = extrasValue / 1000;
        _ribbonType = extrasValue % 1000;
    }
}
