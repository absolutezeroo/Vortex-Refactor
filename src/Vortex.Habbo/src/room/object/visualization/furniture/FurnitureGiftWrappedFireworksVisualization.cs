using System;

using Vortex.Room.Object;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureGiftWrappedFireworksVisualization
public class FurnitureGiftWrappedFireworksVisualization : FurnitureFireworksVisualization
{
    private const int PRESENT_DEFAULT_STATE = 0;
    private const int MAX_PACKET_TYPE_VALUE = 9;
    private const int MAX_RIBBON_TYPE_VALUE = 11;

    private int _packetType;
    private int _ribbonType;
    private int _animState;

    public override void Update(IRoomGeometry geometry, int time, bool full, bool skip)
    {
        UpdateTypes();
        base.Update(geometry, time, full, skip);
    }

    protected override int GetFrameNumber(int scale, int layer)
    {
        if (_animState == PRESENT_DEFAULT_STATE)
        {
            if (layer <= 1)
            {
                return _packetType;
            }

            if (layer == 2)
            {
                return _ribbonType;
            }
        }

        return base.GetFrameNumber(scale, layer);
    }

    protected override string? GetSpriteAssetName(int scale, int spriteIndex)
    {
        int size = GetSize(scale);

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
        return Type + "_" + size + "_" + layerCode + "_" + Direction + "_" + frame;
    }

    protected override void SetAnimation(int animationId)
    {
        _animState = animationId;
        base.SetAnimation(animationId);
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

        int packetType = extrasValue / 1000;
        int ribbonType = extrasValue % 1000;

        _packetType = packetType > MAX_PACKET_TYPE_VALUE ? 0 : packetType;
        _ribbonType = ribbonType > MAX_RIBBON_TYPE_VALUE ? 0 : ribbonType;
    }
}
