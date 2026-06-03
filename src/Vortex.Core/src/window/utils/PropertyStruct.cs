// @see WIN63-202407091256-704579380-Source-main/core/window/utils/PropertyStruct.as

using System.Linq;

namespace Vortex.Core.Window.Utils;

/// @see WIN63-202407091256-704579380-Source-main/core/window/utils/PropertyStruct.as
public class PropertyStruct
{
    public const string HEX = "hex";
    public const string INT = "int";
    public const string UINT = "uint";
    public const string NUMBER = "Number";
    public const string BOOLEAN = "Boolean";
    public const string STRING = "String";
    public const string POINT = "Point";
    public const string RECTANGLE = "Rectangle";
    public const string ARRAY = "Array";
    public const string MAP = "Map";

    /// @see PropertyStruct.as::PropertyStruct
    public PropertyStruct(string param1, object? param2, string param3, bool param4 = false, string[]? param5 = null)
    {
        key = param1;
        value = param2;
        type = param3;
        valid = param4;
        range = param5;
    }

    /// @see PropertyStruct.as::get key
    public string key { get; }

    /// @see PropertyStruct.as::get value
    public object? value { get; }

    /// @see PropertyStruct.as::get type
    public string type { get; }

    /// @see PropertyStruct.as::get valid
    public bool valid { get; }

    /// @see PropertyStruct.as::get range
    public string[]? range { get; }

    /// @see PropertyStruct.as::withNameSpace
    public PropertyStruct WithNameSpace(string param1)
    {
        return new PropertyStruct(param1 + ":" + key, value, type, valid, range);
    }

    /// @see PropertyStruct.as::withoutNameSpace
    public PropertyStruct WithoutNameSpace()
    {
        int idx = key.LastIndexOf(':');
        string stripped = idx >= 0 ? key[(idx + 1)..] : key;

        return new PropertyStruct(stripped, value, type, valid, range);
    }

    /// @see PropertyStruct.as::withValue
    public PropertyStruct WithValue(object? param1)
    {
        bool changed = true;

        switch (type)
        {
            case UINT:
            case HEX:
                changed = !Equals(ToUInt(value), ToUInt(param1));
                break;
            case INT:
                changed = !Equals(ToInt(value), ToInt(param1));
                break;
            case NUMBER:
                changed = !Equals(ToDouble(value), ToDouble(param1));
                break;
            case BOOLEAN:
                changed = !Equals(ToBool(value), ToBool(param1));
                break;
            case STRING:
                changed = !string.Equals(value?.ToString(), param1?.ToString());
                break;
            case ARRAY:
                if (value is object[] oldArr && param1 is object[] newArr && oldArr.Length == newArr.Length)
                {
                    changed = newArr.Where((t, i) => !Equals(oldArr[i], t)).Any();
                }
                break;
        }

        if (changed)
        {
            return new PropertyStruct(key, param1, type, true, range);
        }

        return this;
    }

    /// @see PropertyStruct.as::toString
    public override string ToString()
    {
        return type switch
        {
            HEX => "0x" + ToUInt(value).ToString("x"),
            BOOLEAN => ToBool(value) ? "true" : "false",
            _ => value?.ToString() ?? string.Empty,
        };
    }

    /// @see PropertyStruct.as::toXMLString
    public string ToXmlString()
    {
        return type switch
        {
            HEX => $"<var key=\"{key}\" value=\"0x{ToUInt(value):x}\" type=\"{type}\" />",
            _ => $"<var key=\"{key}\" value=\"{value}\" type=\"{type}\" />",
        };
    }

    private static uint ToUInt(object? value)
    {
        return value is uint u ? u : 0;
    }

    private static int ToInt(object? value)
    {
        return value is int i ? i : 0;
    }

    private static double ToDouble(object? value)
    {
        return value is double d ? d : 0;
    }

    private static bool ToBool(object? value)
    {
        return value is true;
    }
}
