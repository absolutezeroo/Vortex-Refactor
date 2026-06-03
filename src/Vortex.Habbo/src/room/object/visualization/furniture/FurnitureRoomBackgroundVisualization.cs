using Godot;

using Vortex.Habbo.Room.Object.Visualization.Data;
using Vortex.Room.Object;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureRoomBackgroundVisualization
public class FurnitureRoomBackgroundVisualization : FurnitureRoomBrandingVisualization
{
    private Dictionary<int, DirectionalOffsetData>? _directionalOffsets;

    public override void Dispose()
    {
        base.Dispose();
        _directionalOffsets = null;
    }

    protected override string? GetAdClickUrl(IRoomObjectModel model)
    {
        return null;
    }

    protected override void ImageReady(Image? image, string url)
    {
        base.ImageReady(image, url);

        if (image == null)
        {
            return;
        }

        _directionalOffsets = new Dictionary<int, DirectionalOffsetData>();
        int imageW = image.GetWidth();
        int imageH = image.GetHeight();

        AddDirectionalOffsets(64, imageH, imageW);
        AddDirectionalOffsets(32, imageH / 2, imageW / 2);
    }

    protected override int GetSpriteXOffset(int scale, int direction, int layer)
    {
        if (_directionalOffsets == null)
        {
            return base.GetSpriteXOffset(scale, direction, layer) + GetScaledOffset(_brandingOffsetX, scale);
        }

        int size = GetSize(scale);

        if (_directionalOffsets.TryGetValue(size, out DirectionalOffsetData? data))
        {
            return data.GetOffsetX(direction, 0) + GetScaledOffset(_brandingOffsetX, scale);
        }

        return base.GetSpriteXOffset(scale, direction, layer) + GetScaledOffset(_brandingOffsetX, scale);
    }

    protected override int GetSpriteYOffset(int scale, int direction, int layer)
    {
        if (_directionalOffsets == null)
        {
            return base.GetSpriteYOffset(scale, direction, layer) + GetScaledOffset(_brandingOffsetY, scale);
        }

        int size = GetSize(scale);

        if (_directionalOffsets.TryGetValue(size, out DirectionalOffsetData? data))
        {
            return data.GetOffsetY(direction, 0) + GetScaledOffset(_brandingOffsetY, scale);
        }

        return base.GetSpriteYOffset(scale, direction, layer) + GetScaledOffset(_brandingOffsetY, scale);
    }

    protected override double GetSpriteZOffset(int scale, int direction, int layer)
    {
        return base.GetSpriteZOffset(scale, direction, layer) + (_brandingOffsetZ * -1);
    }

    protected override bool GetSpriteMouseCapture(int scale, int direction, int layer)
    {
        return false;
    }

    private void AddDirectionalOffsets(int size, int height, int width)
    {
        DirectionalOffsetData data = new();

        data.SetOffset(1, 0, -height);
        data.SetOffset(3, 0, 0);
        data.SetOffset(5, -width, 0);
        data.SetOffset(7, -width, -height);
        data.SetOffset(4, -width / 2, -height / 2);

        _directionalOffsets![size] = data;
    }

    private static int GetScaledOffset(int offset, int scale)
    {
        return offset * scale / 64;
    }
}
