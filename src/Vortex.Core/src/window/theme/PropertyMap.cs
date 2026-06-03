// @see WIN63-202407091256-704579380-Source-main/core/window/theme/PropertyMap.as

using System;

using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Theme;

/// @see WIN63-202407091256-704579380-Source-main/core/window/theme/PropertyMap.as
public class PropertyMap : IPropertyMap
{
    private readonly Dictionary<string, PropertyStruct> _properties = new(StringComparer.Ordinal);

    /// @see PropertyMap.as::PropertyMap
    public PropertyMap() { }

    /// @see PropertyMap.as::add
    private void Add(string param1, object? param2, string param3, string[]? param4 = null)
    {
        _properties[param1] = new PropertyStruct(param1, param2, param3, false, param4);
    }

    /// @see PropertyMap.as::addBoolean
    public void AddBoolean(string param1, bool param2)
    {
        Add(param1, param2, PropertyStruct.BOOLEAN);
    }

    /// @see PropertyMap.as::addInt
    public void AddInt(string param1, int param2)
    {
        Add(param1, param2, PropertyStruct.INT);
    }

    /// @see PropertyMap.as::addUint
    public void AddUint(string param1, uint param2)
    {
        Add(param1, param2, PropertyStruct.UINT);
    }

    /// @see PropertyMap.as::addHex
    public void AddHex(string param1, uint param2)
    {
        Add(param1, param2, PropertyStruct.HEX);
    }

    /// @see PropertyMap.as::addNumber
    public void AddNumber(string param1, double param2)
    {
        Add(param1, param2, PropertyStruct.NUMBER);
    }

    /// @see PropertyMap.as::addString
    public void AddString(string param1, string? param2)
    {
        Add(param1, param2, PropertyStruct.STRING);
    }

    /// @see PropertyMap.as::addEnumeration
    public void AddEnumeration(string param1, string param2, string[] param3)
    {
        Add(param1, param2, PropertyStruct.STRING, param3);
    }

    /// @see PropertyMap.as::addArray
    public void AddArray(string param1, object[] param2)
    {
        Add(param1, param2, PropertyStruct.ARRAY);
    }

    /// @see PropertyMap.as::method_20
    public PropertyStruct? GetValue(string param1)
    {
        return _properties.GetValueOrDefault(param1);
    }

    /// @see PropertyMap.as::clone
    public PropertyMap Clone()
    {
        PropertyMap clone = new();

        foreach (KeyValuePair<string, PropertyStruct> kvp in _properties)
        {
            clone._properties[kvp.Key] = kvp.Value;
        }

        return clone;
    }
}
