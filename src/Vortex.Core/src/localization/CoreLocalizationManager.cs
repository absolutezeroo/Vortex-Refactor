using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;

using Godot;

using Vortex.Core.Runtime;

using NetHttpClient = System.Net.Http.HttpClient;

namespace Vortex.Core.Localization;

/// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as
public class CoreLocalizationManager : Component, ICoreLocalizationManager
{
    private const int INTERPOLATION_DEPTH_LIMIT = 3;
    private static readonly NetHttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10),
    };
    private Dictionary<string, bool>? _acceptEmptyMap;
    private string _activeEnvironmentId = string.Empty;
    private string _activeLocalizationDefinitionKey = string.Empty;
    private Dictionary<string, LocalizationDefinition>? _definitions;
    private IGameDataResources? _gameDataResources;

    private Dictionary<string, Localization>? _localizations;
    private List<string>? _missingKeys;

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::CoreLocalizationManager
    public CoreLocalizationManager(IContext? param1 = null, uint param2 = 0, object? param3 = null)
        : base(param1, param2, param3)
    {
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::dispose
    public override void Dispose()
    {
        _localizations = null;
        _definitions = null;
        _acceptEmptyMap = null;
        _missingKeys = null;
        _gameDataResources = null;

        base.Dispose();
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::registerLocalizationDefinition
    public void RegisterLocalizationDefinition(string param1, string param2, string param3, string param4)
    {
        if (_definitions == null || _definitions.ContainsKey(param1))
        {
            return;
        }

        _definitions[param1] = new LocalizationDefinition(param4, param2, param3);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::activateLocalizationDefinition
    public bool ActivateLocalizationDefinition(string param1)
    {
        if (_definitions == null || !_definitions.TryGetValue(param1, out LocalizationDefinition? definition))
        {
            return false;
        }

        _activeLocalizationDefinitionKey = param1;
        LoadLocalizationFromURL(definition.url, definition.languageCode);
        return true;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::getGameDataResources
    public IGameDataResources? GetGameDataResources()
    {
        return _gameDataResources;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::loadLocalizationFromURL
    public void LoadLocalizationFromURL(string param1, string param2, bool param3 = false)
    {
        if (string.IsNullOrEmpty(param1))
        {
            Logger.Warn("[CoreLocalizationManager] Localization hashes URL was null or empty!");
            events.DispatchEvent("LOCALIZATION_EVENT_LOCALIZATION_FAILED");
            return;
        }

        string? responseData = null;

        try
        {
            if (param1.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                param1.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                using HttpResponseMessage response = _httpClient.GetAsync(param1).GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                {
                    events.DispatchEvent("LOCALIZATION_EVENT_LOCALIZATION_FAILED");
                    return;
                }

                responseData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            else
            {
                string resolved = ResolvePath(param1);
                if (File.Exists(resolved))
                {
                    responseData = File.ReadAllText(resolved);
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error("[CoreLocalizationManager] Failed loading hashes: " + e.Message, e);
            events.DispatchEvent("LOCALIZATION_EVENT_LOCALIZATION_FAILED");
            return;
        }

        if (string.IsNullOrEmpty(responseData))
        {
            events.DispatchEvent("LOCALIZATION_EVENT_LOCALIZATION_FAILED");
            return;
        }

        try
        {
            GameDataResources? gameData = GameDataResources.Parse(responseData);

            if (gameData == null || !gameData.IsValid())
            {
                events.DispatchEvent("LOCALIZATION_EVENT_LOCALIZATION_FAILED");
                return;
            }

            _gameDataResources = gameData;
            string assetName = "localization_" + param2.ToLowerInvariant() + "_" + gameData.GetExternalTextsHash();

            if (FindAssetByName(assetName) != null && string.Equals(param2, _activeEnvironmentId, StringComparison.Ordinal))
            {
                events.DispatchEvent("LOCALIZATION_EVENT_LOCALIZATION_LOADED");
                return;
            }

            _activeEnvironmentId = param2;
            if (_acceptEmptyMap != null)
            {
                _acceptEmptyMap[assetName] = param3;
            }

            string textsUrl = gameData.GetExternalTextsUrl() + "/" + gameData.GetExternalTextsHash();
            Logger.Info("[CoreLocalizationManager] load localization for url: " + textsUrl);

            LoadLocalizationTexts(textsUrl, assetName, param3);
        }
        catch (Exception e)
        {
            Logger.Error("[CoreLocalizationManager] Failed parsing hashes: " + e.Message, e);
            events.DispatchEvent("LOCALIZATION_EVENT_LOCALIZATION_FAILED");
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::hasLocalization
    public bool HasLocalization(string param1)
    {
        return _localizations != null && _localizations.ContainsKey(param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::getLocalization
    public virtual string GetLocalization(string param1, string param2 = "")
    {
        if (_localizations != null && _localizations.TryGetValue(param1, out Localization? localization))
        {
            return localization.value;
        }

        _missingKeys?.Add(param1);
        return param2;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::updateLocalization
    public void UpdateLocalization(string param1, string param2)
    {
        if (_localizations == null)
        {
            return;
        }

        if (_localizations.TryGetValue(param1, out Localization? existing))
        {
            existing.SetValue(param2);
        }
        else
        {
            _localizations[param1] = new Localization(this, param1, param2);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::registerParameter
    public string RegisterParameter(string param1, string param2, string param3, string param4 = "%")
    {
        if (_localizations == null)
        {
            return string.Empty;
        }

        if (!_localizations.TryGetValue(param1, out Localization? localization))
        {
            localization = new Localization(this, param1, param1);
            _localizations[param1] = localization;
        }

        localization.RegisterParameter(param2, param3, param4);
        return localization.value;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::getKeys
    public string[] GetKeys()
    {
        if (_localizations == null)
        {
            return [];
        }

        string[] keys = new string[_localizations.Count];
        _localizations.Keys.CopyTo(keys, 0);
        return keys;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::interpolate
    public override string? Interpolate(string param1)
    {
        if (_localizations == null)
        {
            return base.Interpolate(param1);
        }

        string result = param1;

        for (int depth = 0;
             depth < INTERPOLATION_DEPTH_LIMIT;
             depth++)
        {
            Match match = Regex.Match(result, @"\$\{([^}]*)\}");

            if (!match.Success)
            {
                return result;
            }

            bool changed = false;
            MatchCollection matches = Regex.Matches(result, @"\$\{([^}]*)\}");

            foreach (Match m in matches)
            {
                string locKey = m.Groups[1].Value;

                if (!_localizations.TryGetValue(locKey, out Localization? localization))
                {
                    continue;
                }

                result = result.Replace("${" + locKey + "}", localization.value, StringComparison.Ordinal);
                changed = true;
            }

            if (!changed)
            {
                break;
            }
        }

        return base.Interpolate(result);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::validateLocalizationData
    private static bool ValidateLocalizationData(string? param1, bool param2)
    {
        if (param1 == null)
        {
            return false;
        }

        if (param1.Length == 0 && !param2)
        {
            return false;
        }

        return !param1.Contains("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::initComponent
    protected override void InitComponent()
    {
        _localizations = new Dictionary<string, Localization>(StringComparer.Ordinal);
        _definitions = new Dictionary<string, LocalizationDefinition>(StringComparer.Ordinal);
        _acceptEmptyMap = new Dictionary<string, bool>(StringComparer.Ordinal);
        _missingKeys = [];
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::registerListener
    public bool RegisterListener(string param1, ILocalizable param2)
    {
        if (_localizations == null)
        {
            return false;
        }

        if (!_localizations.TryGetValue(param1, out Localization? localization))
        {
            _missingKeys?.Add(param1);
            localization = new Localization(this, param1, param1);
            _localizations[param1] = localization;
        }

        localization.RegisterListener(param2);
        return true;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::removeListener
    public bool RemoveListener(string param1, ILocalizable param2)
    {
        if (_localizations != null && _localizations.TryGetValue(param1, out Localization? localization))
        {
            localization.RemoveListener(param2);
        }

        return true;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::updateAllListeners
    private void UpdateAllListeners()
    {
        if (_localizations == null)
        {
            return;
        }

        foreach (Localization localization in _localizations.Values)
        {
            localization.UpdateListeners();
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::getActiveEnvironmentId
    public virtual string GetActiveEnvironmentId()
    {
        return _activeEnvironmentId;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::onLocalizationReady
    private void LoadLocalizationTexts(string param1, string param2, bool param3)
    {
        string? textsData = null;

        try
        {
            if (param1.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                param1.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                using HttpResponseMessage response = _httpClient.GetAsync(param1).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    textsData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            }
            else
            {
                string resolved = ResolvePath(param1);
                if (File.Exists(resolved))
                {
                    textsData = File.ReadAllText(resolved);
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error("[CoreLocalizationManager] Failed loading localization texts: " + e.Message, e);
        }

        if (!ValidateLocalizationData(textsData, param3))
        {
            events.DispatchEvent("LOCALIZATION_EVENT_LOCALIZATION_FAILED");
            return;
        }

        ParseLocalizationData(textsData!);
        events.DispatchEvent("LOCALIZATION_EVENT_LOCALIZATION_LOADED");
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/localization/CoreLocalizationManager.as::parseLocalizationData
    protected Dictionary<string, string>? ParseLocalizationData(string? param1)
    {
        if (param1 == null)
        {
            return null;
        }

        Dictionary<string, string> parsed = new(StringComparer.Ordinal);
        string[] lines = Regex.Split(param1, @"\n\r+|\n+|\r+");

        foreach (string rawLine in lines)
        {
            if (rawLine.Length == 0 || rawLine[0] == '#')
            {
                continue;
            }

            int separatorIndex = rawLine.IndexOf('=');

            if (separatorIndex <= 0)
            {
                continue;
            }

            string key = rawLine[..separatorIndex].Trim();

            if (key.Length == 0)
            {
                continue;
            }

            if (separatorIndex >= rawLine.Length - 1)
            {
                continue;
            }

            string value = rawLine[(separatorIndex + 1)..].Trim();
            value = value.Replace("\\n", "\n", StringComparison.Ordinal);

            if (value.Length <= 0)
            {
                continue;
            }

            UpdateLocalization(key, value);
            parsed[key] = value;
        }

        UpdateAllListeners();
        return parsed;
    }

    private static string ResolvePath(string path)
    {
        if (path.StartsWith("res://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("user://", StringComparison.OrdinalIgnoreCase))
        {
            return ProjectSettings.GlobalizePath(path);
        }

        if (path.StartsWith("file://", StringComparison.OrdinalIgnoreCase) && Uri.TryCreate(path, UriKind.Absolute, out Uri? fileUri))
        {
            return fileUri.LocalPath;
        }

        if (Path.IsPathRooted(path))
        {
            return path;
        }

        string normalizedPath = path.Replace('/', Path.DirectorySeparatorChar);

        return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), normalizedPath));
    }
}
