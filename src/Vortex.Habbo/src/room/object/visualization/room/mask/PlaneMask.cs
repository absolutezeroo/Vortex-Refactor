using Vortex.Room.Object.Visualization.Utils;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Mask;

/// @see com.sulake.habbo.room.object.visualization.room.mask.PlaneMask
public class PlaneMask
{
    private Dictionary<string, PlaneMaskVisualization>? _visualizations = [];
    private List<int>? _sizes = [];
    private Dictionary<int, string>? _assetNames = [];
    private PlaneMaskVisualization? _cachedVisualization;
    private int _cachedSize = -1;

    public void Dispose()
    {
        if (_visualizations != null)
        {
            foreach (PlaneMaskVisualization viz in _visualizations.Values)
            {
                viz.Dispose();
            }

            _visualizations = null;
        }

        _cachedVisualization = null;
        _sizes = null;
        _assetNames = null;
    }

    public PlaneMaskVisualization? CreateMaskVisualization(int size)
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

        PlaneMaskVisualization viz = new();
        _visualizations[key] = viz;
        _sizes.Add(size);
        _sizes.Sort();

        return viz;
    }

    public IGraphicAsset? GetGraphicAsset(double scale, IVector3d normal)
    {
        PlaneMaskVisualization? viz = GetMaskVisualization((int)scale);

        if (viz == null)
        {
            return null;
        }

        return viz.GetAsset(normal);
    }

    public string? GetAssetName(int size)
    {
        if (_assetNames == null)
        {
            return null;
        }

        _assetNames.TryGetValue(size, out string? name);
        return name;
    }

    public void SetAssetName(int size, string? name)
    {
        if (_assetNames != null && name != null)
        {
            _assetNames[size] = name;
        }
    }

    protected PlaneMaskVisualization? GetMaskVisualization(int size)
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
