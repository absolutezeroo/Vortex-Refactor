using System.Linq;

namespace Vortex.Core.Utils;

/// @see WIN63-202407091256-704579380-Source-main/core/utils/ErrorReportStorage.as
public static class ErrorReportStorage
{
    private static readonly List<KeyValuePair<string, string>> _debugData = [];
    private static readonly Dictionary<string, string> _parameters = new();

    /// @see WIN63-202407091256-704579380-Source-main/core/utils/ErrorReportStorage.as::getDebugData
    public static string GetDebugData()
    {
        string result = string.Join(" ** ", _debugData.Select(kv => kv.Value));

        if (result.Length > 400)
        {
            result = result[^400..];
        }

        return result;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/utils/ErrorReportStorage.as::addDebugData
    public static void AddDebugData(string key, string value)
    {
        _debugData.RemoveAll(kv => kv.Key == key);
        _debugData.Add(new KeyValuePair<string, string>(key, value));

        Logger.Debug($"ErrorReportStorage: {key} = {value}");
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/utils/ErrorReportStorage.as::setParameter
    public static void SetParameter(string key, string value)
    {
        _parameters[key] = value;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/utils/ErrorReportStorage.as::getParameter
    public static string? GetParameter(string key)
    {
        return _parameters.GetValueOrDefault(key);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/utils/ErrorReportStorage.as::getParameterNames
    public static string[] GetParameterNames()
    {
        return [.. _parameters.Keys];
    }
}
