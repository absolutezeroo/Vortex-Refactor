using Godot;

namespace Vortex.Habbo.Room.Object.Visualization.Room;

/// @see com.sulake.habbo.room.object.visualization.room.PlaneDrawingData
public class PlaneDrawingData
{
    private readonly bool _isBottomAligned;

    public PlaneDrawingData(PlaneDrawingData? source = null, uint color = 0, bool isBottomAligned = false)
    {
        if (source != null)
        {
            MaskAssetNames = source.MaskAssetNames;
            MaskAssetLocations = source.MaskAssetLocations;
            MaskAssetFlipHs = source.MaskAssetFlipHs;
            MaskAssetFlipVs = source.MaskAssetFlipVs;
        }
        else
        {
            MaskAssetNames = [];
            MaskAssetLocations = [];
            MaskAssetFlipHs = [];
            MaskAssetFlipVs = [];
        }

        Color = color;
        _isBottomAligned = isBottomAligned;
    }

    public uint Color { get; }

    public double Z { get; set; }

    public List<Vector2>? CornerPoints { get; set; }

    public List<string> MaskAssetNames { get; }

    public List<Vector2I> MaskAssetLocations { get; }

    public List<bool> MaskAssetFlipHs { get; }

    public List<bool> MaskAssetFlipVs { get; }

    public List<List<string>> AssetNameColumns { get; } = [];

    public bool IsBottomAligned()
    {
        return _isBottomAligned;
    }

    public void AddMask(string name, Vector2I location, bool flipH, bool flipV)
    {
        MaskAssetNames.Add(name);
        MaskAssetLocations.Add(location);
        MaskAssetFlipHs.Add(flipH);
        MaskAssetFlipVs.Add(flipV);
    }

    public void AddAssetColumn(List<string> column)
    {
        AssetNameColumns.Add(column);
    }
}
