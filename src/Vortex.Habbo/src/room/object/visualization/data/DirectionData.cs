namespace Vortex.Habbo.Room.Object.Visualization.Data;

/// <summary>
/// Per-direction layer configuration (tag, ink, alpha, offsets for each layer).
/// </summary>
/// @see com.sulake.habbo.room.object.visualization.data.DirectionData
public class DirectionData
{
    public const int USE_DEFAULT_DIRECTION = -1;

    private LayerData?[]? _layers;

    public DirectionData(int layerCount)
    {
        _layers = new LayerData[layerCount];

        for (int i = 0;
             i < layerCount;
             i++)
        {
            _layers[i] = new LayerData();
        }
    }

    public int LayerCount => _layers?.Length ?? 0;

    public void Dispose()
    {
        _layers = null;
    }

    public string GetTag(int layerIndex)
    {
        LayerData? layer = GetLayer(layerIndex);

        return layer?.Tag ?? "";
    }

    public void SetTag(int layerIndex, string tag)
    {
        LayerData? layer = GetLayer(layerIndex);

        if (layer != null)
        {
            layer.Tag = tag;
        }
    }

    public int GetInk(int layerIndex)
    {
        LayerData? layer = GetLayer(layerIndex);

        return layer?.Ink ?? 0;
    }

    public void SetInk(int layerIndex, int ink)
    {
        LayerData? layer = GetLayer(layerIndex);

        if (layer != null)
        {
            layer.Ink = ink;
        }
    }

    public int GetAlpha(int layerIndex)
    {
        LayerData? layer = GetLayer(layerIndex);

        return layer?.Alpha ?? 255;
    }

    public void SetAlpha(int layerIndex, int alpha)
    {
        LayerData? layer = GetLayer(layerIndex);

        if (layer != null)
        {
            layer.Alpha = alpha;
        }
    }

    public bool GetIgnoreMouse(int layerIndex)
    {
        LayerData? layer = GetLayer(layerIndex);

        return layer?.IgnoreMouse ?? false;
    }

    public void SetIgnoreMouse(int layerIndex, bool value)
    {
        LayerData? layer = GetLayer(layerIndex);

        if (layer != null)
        {
            layer.IgnoreMouse = value;
        }
    }

    public int GetXOffset(int layerIndex)
    {
        LayerData? layer = GetLayer(layerIndex);

        return layer?.XOffset ?? 0;
    }

    public void SetXOffset(int layerIndex, int offset)
    {
        LayerData? layer = GetLayer(layerIndex);

        if (layer != null)
        {
            layer.XOffset = offset;
        }
    }

    public int GetYOffset(int layerIndex)
    {
        LayerData? layer = GetLayer(layerIndex);

        return layer?.YOffset ?? 0;
    }

    public void SetYOffset(int layerIndex, int offset)
    {
        LayerData? layer = GetLayer(layerIndex);

        if (layer != null)
        {
            layer.YOffset = offset;
        }
    }

    public double GetZOffset(int layerIndex)
    {
        LayerData? layer = GetLayer(layerIndex);

        return layer?.ZOffset ?? 0;
    }

    public void SetZOffset(int layerIndex, double offset)
    {
        LayerData? layer = GetLayer(layerIndex);

        if (layer != null)
        {
            layer.ZOffset = offset;
        }
    }

    public void CopyValues(DirectionData? source)
    {
        if (source == null || LayerCount != source.LayerCount)
        {
            return;
        }

        for (int i = 0;
             i < LayerCount;
             i++)
        {
            LayerData? dst = GetLayer(i);
            LayerData? src = source.GetLayer(i);

            dst?.CopyValues(src);
        }
    }

    private LayerData? GetLayer(int index)
    {
        if (_layers == null || index < 0 || index >= _layers.Length)
        {
            return null;
        }

        return _layers[index];
    }
}
