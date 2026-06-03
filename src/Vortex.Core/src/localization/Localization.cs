using System;
using System.Text.RegularExpressions;

namespace Vortex.Core.Localization;

/// @see WIN63-202407091256-704579380-Source-main/core/localization/Localization.as
public class Localization
{
    private readonly ICoreLocalizationManager _manager;
    private readonly string _key;
    private Dictionary<string, (string delimiter, string replacement)>? _parameters;
    private List<ILocalizable>? _listeners;

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/Localization.as::Localization
    public Localization(ICoreLocalizationManager param1, string param2, string? param3 = null)
    {
        _manager = param1;
        _key = param2;
        raw = param3;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/Localization.as::get value
    public string value => FillParameterValues();

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/Localization.as::get raw
    public string? raw { get; private set; }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/Localization.as::setValue
    public void SetValue(string param1)
    {
        raw = param1;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/Localization.as::registerParameter
    public void RegisterParameter(string param1, string param2, string param3 = "%")
    {
        _parameters ??= new Dictionary<string, (string, string)>(StringComparer.Ordinal);
        _parameters[param1] = (param3, param2);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/Localization.as::registerListener
    public void RegisterListener(ILocalizable param1)
    {
        _listeners ??= [];

        if (!_listeners.Contains(param1))
        {
            _listeners.Add(param1);
        }

        param1.SetLocalization(_manager.Interpolate(value));
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/Localization.as::removeListener
    public void RemoveListener(ILocalizable param1)
    {
        if (_listeners == null)
        {
            return;
        }

        int index = _listeners.IndexOf(param1);

        if (index >= 0)
        {
            _listeners.RemoveAt(index);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/Localization.as::updateListeners
    public void UpdateListeners()
    {
        string? interpolated = _manager.Interpolate(value);

        if (_listeners == null)
        {
            return;
        }

        foreach (ILocalizable listener in _listeners)
        {
            listener.SetLocalization(interpolated);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/Localization.as::fillParameterValues
    private string FillParameterValues()
    {
        string? result = raw;

        if (result == null)
        {
            return string.Empty;
        }

        if (_parameters != null)
        {
            foreach ((string paramName, (string delimiter, string replacement)) in _parameters)
            {
                // @see AS3 — simple token replacement first: %paramName% → replacement
                string token = delimiter + paramName + delimiter;
                result = Regex.Replace(result, Regex.Escape(token), replacement, RegexOptions.IgnoreCase);

                // @see AS3 — plural forms: %{paramName|singular|dual|plural}%
                if (result.IndexOf(delimiter + "{" + paramName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    int pluralBranch;

                    if (int.TryParse(replacement, out int intVal))
                    {
                        pluralBranch = intVal switch
                        {
                            0 => 1,
                            1 => 2,
                            _ => 3,
                        };
                    }
                    else
                    {
                        pluralBranch = 3;
                    }

                    Regex pluralPattern = new(
                        Regex.Escape(delimiter) + @"\{" + Regex.Escape(paramName) + @"\|([^|]*)\|([^|]*)\|([^}]*)\}",
                        RegexOptions.IgnoreCase
                    );
                    result = pluralPattern.Replace(result, "$" + pluralBranch);

                    // @see AS3 — double delimiter (%%) left from first pass → raw numeric value
                    Regex doubleDelimiter = new(Regex.Escape(delimiter) + Regex.Escape(delimiter), RegexOptions.IgnoreCase);
                    result = doubleDelimiter.Replace(result, replacement);
                }
            }
        }

        // @see AS3 — nested key substitution: %%%subkey%%% → manager.getLocalization(_key + "." + subkey, subkey)
        Regex nestedPattern = new(@"%%%([A-Za-z0-9_]+)%%%");
        MatchCollection nestedMatches = nestedPattern.Matches(result);

        for (int i = nestedMatches.Count - 1; i >= 0; i--)
        {
            Match match = nestedMatches[i];
            string subKey = match.Groups[1].Value;
            string fullKey = _key + "." + subKey;
            string resolved = _manager.GetLocalization(fullKey, subKey);
            result = result.Remove(match.Index, match.Length).Insert(match.Index, resolved);
        }

        return result;
    }
}
