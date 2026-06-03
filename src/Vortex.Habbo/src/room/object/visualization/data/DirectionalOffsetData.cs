namespace Vortex.Habbo.Room.Object.Visualization.Data;

/// <summary>
/// Per-direction frame offsets for animation sequences.
/// </summary>
/// @see com.sulake.habbo.room.object.visualization.data.DirectionalOffsetData (class_3534)
public class DirectionalOffsetData
{
    private readonly Dictionary<int, int> _offsetX = new();
    private readonly Dictionary<int, int> _offsetY = new();

    public int GetOffsetX(int direction, int defaultX)
    {
        return _offsetX.GetValueOrDefault(direction, defaultX);
    }

    public int GetOffsetY(int direction, int defaultY)
    {
        return _offsetY.GetValueOrDefault(direction, defaultY);
    }

    public void SetOffset(int direction, int offsetX, int offsetY)
    {
        _offsetX[direction] = offsetX;
        _offsetY[direction] = offsetY;
    }
}
