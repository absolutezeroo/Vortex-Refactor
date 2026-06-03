using Vortex.Room.Object;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureBuilderPlaceholderVisualization
public class FurnitureBuilderPlaceholderVisualization : FurnitureVisualization
{
    private int _sizeX = -1;
    private int _sizeY = -1;

    protected override bool UpdateModel(double scale)
    {
        bool result = base.UpdateModel(scale);

        IRoomObjectModel model = Object!.Model;
        int sizeX = (int)model.GetNumber("furniture_size_x");
        int sizeY = (int)model.GetNumber("furniture_size_y");

        if (sizeX != _sizeX || sizeY != _sizeY)
        {
            _sizeX = sizeX;
            _sizeY = sizeY;
            InstantiateSprites(scale);
        }

        return result;
    }

    protected override void UpdateLayerCount(int count)
    {
        _totalSpriteCount = count;

        if (_sizeX * _sizeY > 1)
        {
            _totalSpriteCount *= _sizeX * _sizeY;
        }
    }

    protected override int GetAdditionalSpriteCount()
    {
        return 0;
    }

    protected override string GetSpriteTag(int scale, int direction, int layer)
    {
        return base.GetSpriteTag(scale, direction, GetIndex(scale, layer));
    }

    protected override int GetSpriteAlpha(int scale, int direction, int layer)
    {
        return base.GetSpriteAlpha(scale, direction, GetIndex(scale, layer));
    }

    protected override uint GetSpriteColor(int scale, int layer, int colorId)
    {
        return base.GetSpriteColor(scale, GetIndex(scale, layer), colorId);
    }

    protected override string? GetSpriteAssetName(int scale, int spriteIndex)
    {
        return base.GetSpriteAssetName(scale, GetIndex(scale, spriteIndex));
    }

    protected override int GetSpriteXOffset(int scale, int direction, int layer)
    {
        int offset = base.GetSpriteXOffset(scale, direction, GetIndex(scale, layer));

        int layerCount = Data!.GetLayerCount(scale);
        int gridIndex = layer / layerCount;
        int gridY = gridIndex % _sizeY;
        int gridX = gridIndex / _sizeY;

        return offset + (gridY - gridX) * scale / 2;
    }

    protected override int GetSpriteYOffset(int scale, int direction, int layer)
    {
        int offset = base.GetSpriteYOffset(scale, direction, GetIndex(scale, layer));

        int layerCount = Data!.GetLayerCount(scale);
        int gridIndex = layer / layerCount;
        int gridY = gridIndex % _sizeY;
        int gridX = gridIndex / _sizeY;

        return offset + (gridY + gridX) * scale / 4;
    }

    protected override bool GetSpriteMouseCapture(int scale, int direction, int layer)
    {
        return base.GetSpriteMouseCapture(scale, direction, GetIndex(scale, layer));
    }

    protected override int GetSpriteInk(int scale, int direction, int layer)
    {
        return base.GetSpriteInk(scale, direction, GetIndex(scale, layer));
    }

    protected override double GetSpriteZOffset(int scale, int direction, int layer)
    {
        return base.GetSpriteZOffset(scale, direction, GetIndex(scale, layer));
    }

    private void InstantiateSprites(double scale)
    {
        int layerCount = Data!.GetLayerCount((int)scale);
        UpdateLayerCount(layerCount);
        CreateSprites(layerCount * _sizeX * _sizeY);
        UpdateSprites((int)scale, true, 0);
    }

    private int GetIndex(int scale, int spriteIndex)
    {
        if (Data == null)
        {
            return spriteIndex;
        }

        int layerCount = Data.GetLayerCount(scale);
        return layerCount > 0 ? spriteIndex % layerCount : spriteIndex;
    }
}
