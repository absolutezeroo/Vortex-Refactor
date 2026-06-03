using Vortex.Room.Object.Visualization;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureStickieVisualization
public class FurnitureStickieVisualization : FurnitureVisualization
{
    private FurnitureVisualizationData? _furnitureData;

    public override bool Initialize(IRoomObjectVisualizationData data)
    {
        _furnitureData = data as FurnitureVisualizationData;
        return base.Initialize(data);
    }

    protected override uint GetSpriteColor(int scale, int layer, int colorId)
    {
        if (_furnitureData == null)
        {
            return 0xFFFFFF;
        }

        return _furnitureData.GetColor(scale, layer, colorId);
    }
}
