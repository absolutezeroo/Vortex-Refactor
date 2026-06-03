using System.Globalization;
using System.Xml.Linq;

using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Data;

/// <summary>
/// Multi-size, multi-direction visualization configuration parsed from XML layout elements.
/// </summary>
/// @see com.sulake.habbo.room.object.visualization.data.SizeData
public class SizeData
{
    public const int LAYER_LIMIT = 1000;
    public const int DEFAULT_DIRECTION = 0;

    private readonly int _directionCount = 360;
    private DirectionData? _defaultDirection;
    private Dictionary<string, DirectionData>? _directions;
    private Dictionary<string, ColorData>? _colors;
    private DirectionData? _cachedDirection;
    private int _cachedDirectionId = -1;

    public SizeData(int layerCount, int directionCount)
    {
        if (layerCount < 0)
        {
            layerCount = 0;
        }

        if (layerCount > LAYER_LIMIT)
        {
            layerCount = LAYER_LIMIT;
        }

        LayerCount = layerCount;

        if (directionCount < 1)
        {
            directionCount = 1;
        }

        if (directionCount > 360)
        {
            directionCount = 360;
        }

        _directionCount = directionCount;
        _defaultDirection = new DirectionData(layerCount);
        _directions = new Dictionary<string, DirectionData>();
        _colors = new Dictionary<string, ColorData>();
    }

    public int LayerCount { get; }

    public virtual void Dispose()
    {
        _defaultDirection?.Dispose();
        _defaultDirection = null;

        if (_directions != null)
        {
            foreach (DirectionData dir in _directions.Values)
            {
                dir.Dispose();
            }

            _directions.Clear();
            _directions = null;
        }

        _cachedDirection = null;

        if (_colors == null)
        {
            return;
        }

        foreach (ColorData col in _colors.Values)
        {
            col.Dispose();
        }

        _colors.Clear();
        _colors = null;
    }

    public bool DefineLayers(XElement? xml)
    {
        if (xml == null)
        {
            return false;
        }

        IEnumerable<XElement> layers = xml.Elements("layer");

        return DefineDirection(_defaultDirection!, layers);
    }

    public bool DefineDirections(XElement? xml)
    {
        if (xml == null)
        {
            return false;
        }

        foreach (XElement dirElement in xml.Elements("direction"))
        {
            if (!XMLValidator.CheckRequiredAttributes(dirElement, ["id"]))
            {
                return false;
            }

            int id = int.Parse(dirElement.Attribute("id")!.Value, CultureInfo.InvariantCulture);
            IEnumerable<XElement> layerElements = dirElement.Elements("layer");
            string key = id.ToString(CultureInfo.InvariantCulture);

            if (_directions!.ContainsKey(key))
            {
                return false;
            }

            DirectionData dirData = new(LayerCount);

            dirData.CopyValues(_defaultDirection);
            DefineDirection(dirData, layerElements);

            _directions[key] = dirData;
            _cachedDirectionId = -1;
            _cachedDirection = null;
        }
        return true;
    }

    public bool DefineColors(XElement? xml)
    {
        if (xml == null)
        {
            return true;
        }

        foreach (XElement colorElement in xml.Elements("color"))
        {
            if (!XMLValidator.CheckRequiredAttributes(colorElement, ["id"]))
            {
                return false;
            }

            string colorId = colorElement.Attribute("id")!.Value;

            if (_colors!.ContainsKey(colorId))
            {
                return false;
            }

            ColorData colorData = new(LayerCount);

            foreach (XElement layerElement in colorElement.Elements("colorLayer"))
            {
                if (!XMLValidator.CheckRequiredAttributes(layerElement, ["id", "color"]))
                {
                    colorData.Dispose();
                    return false;
                }

                int layerIndex = int.Parse(layerElement.Attribute("id")!.Value, CultureInfo.InvariantCulture);
                uint color = uint.Parse(layerElement.Attribute("color")!.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

                colorData.SetColor(color, layerIndex);
            }

            _colors[colorId] = colorData;
        }

        return true;
    }

    public int GetDirectionValue(int angle)
    {
        int normalized = (((angle % 360) + 360 + (_directionCount / 2)) % 360) / _directionCount;

        if (_directions!.ContainsKey(normalized.ToString(CultureInfo.InvariantCulture)))
        {
            return normalized;
        }

        int rawAngle = ((angle % 360) + 360) % 360;
        int bestDist = -1;
        int bestKey = -1;

        foreach (KeyValuePair<string, DirectionData> kvp in _directions)
        {
            if (!int.TryParse(kvp.Key, out int dirKey))
            {
                continue;
            }

            int dirAngle = dirKey * _directionCount;
            int dist = (dirAngle - rawAngle + 360) % 360;

            if (dist > 180)
            {
                dist = 360 - dist;
            }

            if (dist >= bestDist && bestDist >= 0)
            {
                continue;
            }

            bestDist = dist;
            bestKey = dirKey;
        }

        return bestKey >= 0 ? bestKey : 0;
    }

    public string GetTag(int directionId, int layerIndex)
    {
        DirectionData? dir = GetDirectionData(directionId);

        return dir?.GetTag(layerIndex) ?? "";
    }

    public int GetInk(int directionId, int layerIndex)
    {
        DirectionData? dir = GetDirectionData(directionId);

        return dir?.GetInk(layerIndex) ?? 0;
    }

    public int GetAlpha(int directionId, int layerIndex)
    {
        DirectionData? dir = GetDirectionData(directionId);

        return dir?.GetAlpha(layerIndex) ?? 255;
    }

    public uint GetColor(int layerIndex, int colorId)
    {
        string key = colorId.ToString(CultureInfo.InvariantCulture);

        if (_colors != null && _colors.TryGetValue(key, out ColorData? colorData))
        {
            return colorData.GetColor(layerIndex);
        }

        return ColorData.DEFAULT_COLOR;
    }

    public bool GetIgnoreMouse(int directionId, int layerIndex)
    {
        DirectionData? dir = GetDirectionData(directionId);

        return dir?.GetIgnoreMouse(layerIndex) ?? false;
    }

    public int GetXOffset(int directionId, int layerIndex)
    {
        DirectionData? dir = GetDirectionData(directionId);

        return dir?.GetXOffset(layerIndex) ?? 0;
    }

    public int GetYOffset(int directionId, int layerIndex)
    {
        DirectionData? dir = GetDirectionData(directionId);

        return dir?.GetYOffset(layerIndex) ?? 0;
    }

    public double GetZOffset(int directionId, int layerIndex)
    {
        DirectionData? dir = GetDirectionData(directionId);

        return dir?.GetZOffset(layerIndex) ?? 0;
    }

    private bool DefineDirection(DirectionData dirData, System.Collections.Generic.IEnumerable<XElement> layers)
    {
        foreach (XElement layerElement in layers)
        {
            if (!XMLValidator.CheckRequiredAttributes(layerElement, ["id"]))
            {
                return false;
            }

            int layerIndex = int.Parse(layerElement.Attribute("id")!.Value, CultureInfo.InvariantCulture);

            if (layerIndex < 0 || layerIndex >= LayerCount)
            {
                return false;
            }

            XAttribute? tagAttr = layerElement.Attribute("tag");

            if (tagAttr is
                {
                    Value.Length: > 0,
                })
            {
                dirData.SetTag(layerIndex, tagAttr.Value);
            }

            XAttribute? inkAttr = layerElement.Attribute("ink");

            if (inkAttr != null)
            {
                dirData.SetInk(
                    layerIndex, inkAttr.Value switch
                    {
                        "ADD" => LayerData.INK_ADD,
                        "SUBTRACT" => LayerData.INK_SUBTRACT,
                        "DARKEN" => LayerData.INK_DARKEN,
                        "DIFFERENCE" => LayerData.INK_DIFFERENCE,
                        "MULTIPLY" => LayerData.INK_MULTIPLY,
                        "INVERT" => LayerData.INK_INVERT,
                        "SCREEN" => LayerData.INK_SCREEN,
                        _ => LayerData.DEFAULT_INK,
                    }
                );
            }

            XAttribute? alphaAttr = layerElement.Attribute("alpha");

            if (alphaAttr is
                {
                    Value.Length: > 0,
                })
            {
                dirData.SetAlpha(layerIndex, int.Parse(alphaAttr.Value, CultureInfo.InvariantCulture));
            }

            XAttribute? mouseAttr = layerElement.Attribute("ignoreMouse");

            if (mouseAttr is
                {
                    Value.Length: > 0,
                })
            {
                dirData.SetIgnoreMouse(layerIndex, int.Parse(mouseAttr.Value, CultureInfo.InvariantCulture) != 0);
            }

            XAttribute? xAttr = layerElement.Attribute("x");

            if (xAttr is
                {
                    Value.Length: > 0,
                })
            {
                dirData.SetXOffset(layerIndex, int.Parse(xAttr.Value, CultureInfo.InvariantCulture));
            }

            XAttribute? yAttr = layerElement.Attribute("y");

            if (yAttr is
                {
                    Value.Length: > 0,
                })
            {
                dirData.SetYOffset(layerIndex, int.Parse(yAttr.Value, CultureInfo.InvariantCulture));
            }

            XAttribute? zAttr = layerElement.Attribute("z");

            if (zAttr is
                not
                {
                    Value.Length: > 0,
                })
            {
                continue;
            }

            int zVal = int.Parse(zAttr.Value, CultureInfo.InvariantCulture);

            dirData.SetZOffset(layerIndex, zVal / -1000.0);
        }
        return true;
    }

    private DirectionData? GetDirectionData(int directionId)
    {
        if (directionId == _cachedDirectionId && _cachedDirection != null)
        {
            return _cachedDirection;
        }

        string key = directionId.ToString(CultureInfo.InvariantCulture);

        DirectionData? result = null;

        _directions?.TryGetValue(key, out result);

        result ??= _defaultDirection;
        _cachedDirectionId = directionId;
        _cachedDirection = result;

        return result;
    }
}
