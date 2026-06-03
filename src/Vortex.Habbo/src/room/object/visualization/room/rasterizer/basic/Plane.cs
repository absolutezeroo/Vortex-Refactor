using System;

using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.basic.Plane
public class Plane
{
    private Dictionary<string, PlaneVisualization>? _visualizations = [];
    private List<int>? _sizes = [];
    private PlaneVisualization? _cachedVisualization;
    private int _cachedSize = -1;

    public virtual bool IsStatic(int scale)
    {
        return true;
    }

    public virtual void Dispose()
    {
        if (_visualizations != null)
        {
            foreach (PlaneVisualization viz in _visualizations.Values)
            {
                viz.Dispose();
            }

            _visualizations = null;
        }

        _cachedVisualization = null;
        _sizes = null;
    }

    public void ClearCache()
    {
        if (_visualizations == null)
        {
            return;
        }

        foreach (PlaneVisualization viz in _visualizations.Values)
        {
            viz.ClearCache();
        }
    }

    public PlaneVisualization? CreatePlaneVisualization(int size, int layerCount, IRoomGeometry? geometry)
    {
        if (_visualizations == null || _sizes == null)
        {
            return null;
        }

        string key = size.ToString();

        if (_visualizations.ContainsKey(key))
        {
            return null;
        }

        PlaneVisualization viz = new(size, layerCount, geometry);
        _visualizations[key] = viz;
        _sizes.Add(size);
        _sizes.Sort();

        return viz;
    }

    public List<object?>? GetLayers()
    {
        return GetPlaneVisualization(_cachedSize)?.GetLayers();
    }

    protected PlaneVisualization? GetPlaneVisualization(int size)
    {
        if (size == _cachedSize)
        {
            return _cachedVisualization;
        }

        if (_visualizations == null || _sizes == null)
        {
            return null;
        }

        int sizeIndex = GetSizeIndex(size);

        if (sizeIndex < _sizes.Count)
        {
            string key = _sizes[sizeIndex].ToString();
            _visualizations.TryGetValue(key, out _cachedVisualization);
        }
        else
        {
            _cachedVisualization = null;
        }

        _cachedSize = size;
        return _cachedVisualization;
    }

    private int GetSizeIndex(int size)
    {
        if (_sizes == null || _sizes.Count == 0)
        {
            return 0;
        }

        int bestIndex = 0;

        for (int i = 1; i < _sizes.Count; i++)
        {
            if (_sizes[i] > size)
            {
                if (_sizes[i] - size < size - _sizes[i - 1])
                {
                    bestIndex = i;
                }

                break;
            }

            bestIndex = i;
        }

        return bestIndex;
    }
}
