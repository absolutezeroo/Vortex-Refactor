using System.Globalization;
using System.Text;
using System.Xml.Linq;

using Vortex.Habbo.Room.Object.Visualization.Data;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureVisualizationData
public class FurnitureVisualizationData : IRoomObjectVisualizationData
{
    public static readonly string[] LAYER_NAMES =
    [
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
        "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
    ];

    private Dictionary<string, SizeData>? _sizeDataMap = new();
    private List<int> _sortedSizes = new();
    private SizeData? _cachedSizeData;
    private int _cachedSizeKey = -1;
    private int _cachedGetSizeInput = -1;
    private int _cachedGetSizeResult = -1;
    private string _type = "";

    public virtual void Dispose()
    {
        if (_sizeDataMap != null)
        {
            foreach (SizeData sizeData in _sizeDataMap.Values)
            {
                sizeData.Dispose();
            }
            _sizeDataMap.Clear();
            _sizeDataMap = null;
        }
        _cachedSizeData = null;
        _sortedSizes = null!;
    }

    public bool Initialize(XElement xml)
    {
        Reset();

        string? type = xml.Attribute("type")?.Value;
        if (string.IsNullOrEmpty(type))
        {
            return false;
        }

        _type = type;

        if (!DefineVisualizations(xml))
        {
            Reset();
            return false;
        }

        return true;
    }

    public new string GetType()
    {
        return _type;
    }

    public int GetSize(int scale)
    {
        if (scale == _cachedGetSizeInput)
        {
            return _cachedGetSizeResult;
        }

        int index = GetSizeIndex(scale);
        int result = -1;

        if (index < _sortedSizes.Count)
        {
            result = _sortedSizes[index];
        }

        _cachedGetSizeInput = scale;
        _cachedGetSizeResult = result;
        return result;
    }

    public int GetLayerCount(int scale)
    {
        SizeData? sizeData = GetSizeData(scale);
        return sizeData?.LayerCount ?? 0;
    }

    public int GetDirectionValue(int scale, int angle)
    {
        SizeData? sizeData = GetSizeData(scale);
        return sizeData?.GetDirectionValue(angle) ?? 0;
    }

    public string GetTag(int scale, int direction, int layer)
    {
        SizeData? sizeData = GetSizeData(scale);
        return sizeData?.GetTag(direction, layer) ?? "";
    }

    public int GetInk(int scale, int direction, int layer)
    {
        SizeData? sizeData = GetSizeData(scale);
        return sizeData?.GetInk(direction, layer) ?? 0;
    }

    public int GetAlpha(int scale, int direction, int layer)
    {
        SizeData? sizeData = GetSizeData(scale);
        return sizeData?.GetAlpha(direction, layer) ?? 255;
    }

    public uint GetColor(int scale, int layer, int colorId)
    {
        SizeData? sizeData = GetSizeData(scale);
        return sizeData?.GetColor(layer, colorId) ?? 0xFFFFFF;
    }

    public bool GetIgnoreMouse(int scale, int direction, int layer)
    {
        SizeData? sizeData = GetSizeData(scale);
        return sizeData?.GetIgnoreMouse(direction, layer) ?? false;
    }

    public int GetXOffset(int scale, int direction, int layer)
    {
        SizeData? sizeData = GetSizeData(scale);
        return sizeData?.GetXOffset(direction, layer) ?? 0;
    }

    public int GetYOffset(int scale, int direction, int layer)
    {
        SizeData? sizeData = GetSizeData(scale);
        return sizeData?.GetYOffset(direction, layer) ?? 0;
    }

    public double GetZOffset(int scale, int direction, int layer)
    {
        SizeData? sizeData = GetSizeData(scale);
        return sizeData?.GetZOffset(direction, layer) ?? 0;
    }

    protected void Reset()
    {
        _type = "";

        if (_sizeDataMap != null)
        {
            foreach (SizeData sizeData in _sizeDataMap.Values)
            {
                sizeData.Dispose();
            }
            _sizeDataMap.Clear();
        }

        _sortedSizes = new List<int>();
        _cachedSizeData = null;
        _cachedSizeKey = -1;
    }

    protected virtual bool DefineVisualizations(XElement xml)
    {
        IEnumerable<XElement> visualizations = xml.Elements("graphics").Elements("visualization");
        bool found = false;

        foreach (XElement vizElement in visualizations)
        {
            found = true;

            if (!XMLValidator.CheckRequiredAttributes(vizElement, ["size", "layerCount", "angle"]))
            {
                return false;
            }

            int size = int.Parse(vizElement.Attribute("size")!.Value, CultureInfo.InvariantCulture);
            int layerCount = int.Parse(vizElement.Attribute("layerCount")!.Value, CultureInfo.InvariantCulture);
            int angle = int.Parse(vizElement.Attribute("angle")!.Value, CultureInfo.InvariantCulture);

            if (size < 1)
            {
                size = 1;
            }

            string sizeKey = size.ToString(CultureInfo.InvariantCulture);
            if (_sizeDataMap!.ContainsKey(sizeKey))
            {
                return false;
            }

            SizeData? sizeData = CreateSizeData(size, layerCount, angle);
            if (sizeData == null)
            {
                return false;
            }

            foreach (XElement childElement in vizElement.Elements())
            {
                if (!ProcessVisualizationElement(sizeData, childElement))
                {
                    sizeData.Dispose();
                    return false;
                }
            }

            _sizeDataMap[sizeKey] = sizeData;
            _sortedSizes.Add(size);
            _sortedSizes.Sort();
        }

        return found;
    }

    protected virtual SizeData CreateSizeData(int size, int layerCount, int angle)
    {
        return new SizeData(layerCount, angle);
    }

    protected virtual bool ProcessVisualizationElement(SizeData sizeData, XElement element)
    {
        switch (element.Name.LocalName)
        {
            case "layers":
                if (!sizeData.DefineLayers(element))
                {
                    return false;
                }
                break;
            case "directions":
                if (!sizeData.DefineDirections(element))
                {
                    return false;
                }
                break;
            case "colors":
                if (!sizeData.DefineColors(element))
                {
                    return false;
                }
                break;
        }
        return true;
    }

    protected SizeData? GetSizeData(int scale)
    {
        if (scale == _cachedSizeKey)
        {
            return _cachedSizeData;
        }

        int index = GetSizeIndex(scale);

        if (index < _sortedSizes.Count)
        {
            string key = _sortedSizes[index].ToString(CultureInfo.InvariantCulture);
            _sizeDataMap!.TryGetValue(key, out _cachedSizeData);
        }
        else
        {
            _cachedSizeData = null;
        }

        _cachedSizeKey = scale;
        return _cachedSizeData;
    }

    private int GetSizeIndex(int scale)
    {
        int result = 0;

        if (scale > 0)
        {
            for (int i = 1; i < _sortedSizes.Count; i++)
            {
                if (_sortedSizes[i] > scale)
                {
                    if ((double)_sortedSizes[i] / scale < (double)scale / _sortedSizes[i - 1])
                    {
                        result = i;
                    }
                    break;
                }
                result = i;
            }
        }

        return result;
    }
}
