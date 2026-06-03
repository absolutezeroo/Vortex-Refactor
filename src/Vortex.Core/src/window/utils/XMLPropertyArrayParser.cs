// @see core/window/utils/XMLPropertyArrayParser.as
// @see core/window/utils/class_3540.as (parseVariableList, parseVariableToken, parseValueType)

using System;
using System.Globalization;
using System.Xml.Linq;

using Godot;

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/XMLPropertyArrayParser.as
public static class XmlPropertyArrayParser
{
    /// @see core/window/utils/XMLPropertyArrayParser.as::parse
    /// Converts the IList of Dictionary (from WindowParser.ParseProperties) into PropertyStruct[].
    public static PropertyStruct[] Parse(IList<object> properties)
    {
        List<PropertyStruct> result = new(properties.Count);

        foreach (object item in properties)
        {
            if (item is not IDictionary<string, object?> dict)
            {
                continue;
            }

            string? key = GetString(dict, "key") ?? GetString(dict, "name");
            string? rawValue = GetString(dict, "value");
            string type = GetString(dict, "type") ?? PropertyStruct.STRING;

            if (key == null)
            {
                continue;
            }

            object? typedValue;

            // @see class_3540.as::parseVariableToken — Map/Array types without a value attribute
            // have their data as nested child elements (stored in "_element" by ParseProperties).
            if (type is PropertyStruct.MAP or PropertyStruct.ARRAY && rawValue == null
                                                                   && dict.TryGetValue("_element", out object? elemObj) &&
                                                                   elemObj is XElement element)
            {
                typedValue = ParseValueType(element, type);
            }
            else
            {
                typedValue = ConvertValue(rawValue, type);
            }

            result.Add(new PropertyStruct(key, typedValue, type, true));
        }

        return result.ToArray();
    }

    /// @see class_3540.as::parseValueType — parse nested Map/Array XML into typed objects
    private static object? ParseValueType(XElement param1, string type)
    {
        switch (type)
        {
            case PropertyStruct.MAP:
                {
                    // @see class_3540.as — Map: recursively parse children into Dictionary
                    Dictionary<string, object?> map = new(StringComparer.Ordinal);

                    foreach (XElement child in param1.Elements())
                    {
                        string childKey = (string?)child.Attribute("key") ?? child.Name.LocalName;
                        string childType = (string?)child.Attribute("type") ?? PropertyStruct.STRING;
                        string? childValue = (string?)child.Attribute("value");

                        if (childValue != null)
                        {
                            map[childKey] = ConvertValue(childValue, childType);
                        }
                        else
                        {
                            // Nested Map/Array
                            string? nestedType = (string?)child.Attribute("type");

                            if (nestedType is PropertyStruct.MAP or PropertyStruct.ARRAY)
                            {
                                map[childKey] = ParseValueType(child, nestedType);
                            }
                            else
                            {
                                map[childKey] = child.Value;
                            }
                        }
                    }

                    return map;
                }
            case PropertyStruct.ARRAY:
                {
                    // @see class_3540.as — Array: parse children into list, extract values
                    List<object?> list = new();

                    foreach (XElement child in param1.Elements())
                    {
                        string childType = (string?)child.Attribute("type") ?? PropertyStruct.STRING;
                        string? childValue = (string?)child.Attribute("value");

                        if (childValue != null)
                        {
                            list.Add(ConvertValue(childValue, childType));
                        }
                        else if (childType is PropertyStruct.MAP or PropertyStruct.ARRAY)
                        {
                            list.Add(ParseValueType(child, childType));
                        }
                        else
                        {
                            list.Add(child.Value);
                        }
                    }

                    return list;
                }
            default:
                return param1.Value;
        }
    }

    /// @see XMLPropertyArrayParser.as — convert string value to typed object based on type tag
    private static object? ConvertValue(string? rawValue, string type)
    {
        if (rawValue == null)
        {
            return null;
        }

        return type switch
        {
            PropertyStruct.HEX => ParseHex(rawValue),
            PropertyStruct.UINT => uint.TryParse(rawValue, out uint u) ? u : 0u,
            PropertyStruct.INT => int.TryParse(rawValue, out int i) ? i : 0,
            PropertyStruct.NUMBER => float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float f) ? f : 0f,
            PropertyStruct.BOOLEAN => rawValue.Equals("true", StringComparison.OrdinalIgnoreCase),
            PropertyStruct.POINT => ParsePoint(rawValue),
            PropertyStruct.RECTANGLE => ParseRectangle(rawValue),
            _ => rawValue,
        };
    }

    /// @see XMLPropertyArrayParser.as — parse hex string (with or without 0x prefix)
    private static uint ParseHex(string raw)
    {
        string trimmed = raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? raw[2..]
            : raw;

        return uint.TryParse(trimmed, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint val) ? val : 0u;
    }

    /// @see XMLPropertyArrayParser.as — parse "x,y" into Vector2
    private static Vector2 ParsePoint(string raw)
    {
        string[] parts = raw.Split(',');

        if (parts.Length >= 2 &&
            float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float px) &&
            float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float py))
        {
            return new Vector2(px, py);
        }

        return Vector2.Zero;
    }

    /// @see XMLPropertyArrayParser.as — parse "x,y,w,h" into Rect2
    private static Rect2 ParseRectangle(string raw)
    {
        string[] parts = raw.Split(',');

        if (parts.Length >= 4 &&
            float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float rx) &&
            float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float ry) &&
            float.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float rw) &&
            float.TryParse(parts[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float rh))
        {
            return new Rect2(rx, ry, rw, rh);
        }

        return new Rect2();
    }

    private static string? GetString(IDictionary<string, object?> dict, string key)
    {
        return dict.TryGetValue(key, out object? val) ? val?.ToString() : null;
    }
}
