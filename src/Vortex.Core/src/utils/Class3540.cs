// @see core/utils/class_3540.as

using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Vortex.Core.Utils;

/// <summary>
/// Parses XML variable lists into string dictionaries for skin variable substitution.
/// </summary>
/// @see core/utils/class_3540.as
public static class Class3540
{
    private const string HEX = "hex";
    private const string INT = "int";
    private const string UINT = "uint";
    private const string NUMBER = "Number";
    private const string FLOAT = "float";
    private const string BOOLEAN = "Boolean";
    private const string BOOL = "bool";
    private const string STRING = "String";

    /// @see class_3540.as::parseVariableList
    public static uint ParseVariableList(IEnumerable<XElement> variables, Dictionary<string, string> map)
    {
        uint count = 0;

        foreach (XElement variable in variables)
        {
            ParseVariableToken(variable, map);
            count++;
        }

        return count;
    }

    /// @see class_3540.as::parseVariableToken
    private static void ParseVariableToken(XElement variable, Dictionary<string, string> map)
    {
        string? key = (variable.Attribute("key")?.Value) ?? (variable.Element("key")?.Value);

        if (key == null)
        {
            return;
        }

        string type = variable.Attribute("type")?.Value ?? STRING;
        string? value = variable.Attribute("value")?.Value;

        if (value != null)
        {
            map[key] = CastStringToType(value, type);
        }
        else
        {
            XElement? valueEl = variable.Element("value");

            if (valueEl == null)
            {
                return;
            }

            XElement? firstChild = valueEl.Elements().FirstOrDefault();

            if (firstChild == null)
            {
                return;
            }

            _ = firstChild.Name.LocalName;
            map[key] = firstChild.Value;
        }
    }

    /// @see class_3540.as::castStringToType
    public static string CastStringToType(string value, string type)
    {
        // For skin parsing, all values are stored as strings in the variable map.
        // The AS3 version returns typed objects, but the C# callers only need
        // string representations for variable substitution in XML parsing.
        return value;
    }

    /// <summary>
    /// Safely parses a float from a possibly null/empty string, returning a default on failure.
    /// </summary>
    public static float SafeParseFloat(string? value, float defaultValue = 0f)
    {
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result)
            ? result
            : defaultValue;
    }

    /// <summary>
    /// Ensures a dictionary entry has an array of at least <paramref name="requiredLength"/>.
    /// Mirrors AS3 dynamic array auto-growth on index assignment.
    /// </summary>
    public static void EnsureArray<T>(Dictionary<uint, T?[]> dict, uint key, int requiredLength, int minLength)
    {
        int length = Math.Max(requiredLength, minLength);

        if (!dict.TryGetValue(key, out T?[]? array))
        {
            dict[key] = new T?[length];
        }
        else if (array.Length < requiredLength)
        {
            T?[] grown = new T?[requiredLength];

            Array.Copy(array, grown, array.Length);

            dict[key] = grown;
        }
    }
}
