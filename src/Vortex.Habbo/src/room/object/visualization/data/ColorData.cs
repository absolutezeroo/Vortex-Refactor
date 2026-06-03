namespace Vortex.Habbo.Room.Object.Visualization.Data;

/// <summary>
/// Color palette container for furniture color variations per layer.
/// </summary>
/// @see com.sulake.habbo.room.object.visualization.data.ColorData
public class ColorData
{
    public const uint DEFAULT_COLOR = 0xFFFFFF;

    private uint[]? _colors;

    public ColorData(int layerCount)
    {
        _colors = new uint[layerCount];

        for (int i = 0;
             i < layerCount;
             i++)
        {
            _colors[i] = DEFAULT_COLOR;
        }
    }

    public void Dispose()
    {
        _colors = null;
    }

    public void SetColor(uint color, int layerIndex)
    {
        if (_colors == null || layerIndex < 0 || layerIndex >= _colors.Length)
        {
            return;
        }

        _colors[layerIndex] = color;
    }

    public uint GetColor(int layerIndex)
    {
        if (_colors == null || layerIndex < 0 || layerIndex >= _colors.Length)
        {
            return DEFAULT_COLOR;
        }

        return _colors[layerIndex];
    }
}
