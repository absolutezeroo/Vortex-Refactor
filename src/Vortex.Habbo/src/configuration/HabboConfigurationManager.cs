using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;

using Godot;

using Vortex.Core;
using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;
using Vortex.Habbo.Configuration.Enum;
using Vortex.Habbo.Utils;
using Vortex.IID;

using NetHttpClient = System.Net.Http.HttpClient;

namespace Vortex.Habbo.Configuration;

/// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as
public class HabboConfigurationManager : Component, ICoreConfiguration, IHabboConfigurationManager
{
    private const int INTERPOLATION_DEPTH_LIMIT = 3;
    private const string REPLACE_CHAR = "%";
    private static readonly NetHttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10),
    };

    private static readonly string[] EnvironmentKeysToUpdate =
    [
        HabboProperty.CONNECTION_INFO_HOST,
        HabboProperty.CONNECTION_INFO_PORT,
        HabboProperty.URL_PREFIX,
        HabboProperty.SITE_URL,
        HabboProperty.const_61,
        HabboProperty.const_334,
        HabboProperty.const_379,
        HabboProperty.const_246,
        HabboProperty.const_81,
        HabboProperty.const_114,
        HabboProperty.const_122,
        HabboProperty.const_205,
    ];

    private readonly Dictionary<string, string> _configurationData = new(StringComparer.Ordinal);
    private readonly HashSet<string> _configurationKeys = new(StringComparer.Ordinal);
    private bool _configurationDownloadRequested;
    private string _environmentId = string.Empty;
    private bool _isConfigLoaded;
    private readonly bool _isConfigReadOnly;
    private object? _localization;
    private readonly bool _skipLocalizations;
    private bool _useHttps;

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::HabboConfigurationManager
    public HabboConfigurationManager(IContext context, uint flags = 0, object? assets = null) : base(context, flags, assets)
    {
        context.configuration = this;
        _isConfigReadOnly = (flags & HabboConfigurationFlags.SKIP_EXTERNAL_VARIABLES) > 0;
        _skipLocalizations = (flags & HabboConfigurationFlags.SKIP_LOCALIZATIONS) > 0;

        RegisterInterface(new IIDHabboConfigurationManager(), this);

        Lock();
        ResetAll();

        if (!PropertyExists(HabboProperty.const_378) && CommunicationUtils.PropertyExists(CommunicationUtils.SOL_PROPERTY_ENVIRONMENT))
        {
            string? storedEnvironment = CommunicationUtils.ReadSOLString(CommunicationUtils.SOL_PROPERTY_ENVIRONMENT);

            if (!string.IsNullOrWhiteSpace(storedEnvironment))
            {
                UpdateEnvironmentId(storedEnvironment);
            }
        }

        if (!_isConfigLoaded && !_isConfigReadOnly && !_configurationDownloadRequested)
        {
            InitConfigurationDownload();
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::dependencies
    protected override IList<ComponentDependency> dependencies =>
        base.dependencies.Concat(
                [
                    new ComponentDependency(
                        new IIDHabboLocalizationManager(), localization => _localization = localization, false,
                        [new DependencyEventListener("complete", OnLocalizationComplete)]
                    ),
                ]
            )
            .ToList();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::propertyExists
    public override bool PropertyExists(string key)
    {
        return _configurationData.ContainsKey(key);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::getProperty
    public override string GetProperty(string key, IDictionary<string, string>? parameters = null)
    {
        if (!_configurationData.TryGetValue(key, out string? value))
        {
            return string.Empty;
        }

        value = Interpolate(value);

        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.StartsWith("//", StringComparison.Ordinal))
        {
            value = (_useHttps ? "https:" : "http:") + value;
        }

        value = UpdateUrlProtocol(value);

        if (parameters != null)
        {
            value = FillParams(value, parameters);
        }

        return value;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::setProperty
    public override void SetProperty(string key, string value, bool persistent = false, bool log = false)
    {
        if (log && !_configurationData.ContainsKey(key))
        {
            context.Debug(key + "=" + value);
        }

        if (!_configurationKeys.Contains(key) || persistent)
        {
            _configurationData[key] = value;
        }

        if (persistent)
        {
            _configurationKeys.Add(key);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::getBoolean
    public override bool GetBoolean(string key)
    {
        return _configurationData.TryGetValue(key, out string? value) &&
               !string.IsNullOrWhiteSpace(value) &&
               (value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase));
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::getInteger
    public override int GetInteger(string key, int defaultValue)
    {
        return _configurationData.TryGetValue(key, out string? value) &&
               int.TryParse(
                   value, NumberStyles.Integer, CultureInfo.InvariantCulture,
                   out int parsed
               )
            ? parsed
            : defaultValue;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::updateUrlProtocol
    public override string UpdateUrlProtocol(string url)
    {
        return _useHttps
            ? url.Replace("http://", "https://", StringComparison.Ordinal).Replace(":8090/", ":8443/", StringComparison.Ordinal)
            : url;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::interpolate
    public override string? Interpolate(string? value)
    {
        if (value == null)
        {
            return null;
        }

        string interpolated = value;
        int depth = INTERPOLATION_DEPTH_LIMIT;

        while (depth-- > 0)
        {
            int cursor = 0;
            bool changed = false;
            string result = string.Empty;

            while (cursor < interpolated.Length)
            {
                int start = interpolated.IndexOf("${", cursor, StringComparison.Ordinal);
                if (start < 0)
                {
                    result += interpolated[cursor..];
                    break;
                }

                int end = interpolated.IndexOf('}', start + 2);
                if (end < 0)
                {
                    result += interpolated[cursor..];
                    break;
                }

                string propertyName = interpolated[(start + 2)..end];
                if (!PropertyExists(propertyName))
                {
                    return null;
                }

                result += interpolated[cursor..start];
                result += GetProperty(propertyName);
                cursor = end + 1;
                changed = true;
            }

            if (!changed || string.Equals(interpolated, result, StringComparison.Ordinal))
            {
                break;
            }

            interpolated = result;
        }

        return interpolated;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/IHabboConfigurationManager.as::isInitialized
    public bool IsInitialized()
    {
        return _isConfigLoaded;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::updateEnvironmentId
    public void UpdateEnvironmentId(string envId)
    {
        if (string.IsNullOrWhiteSpace(envId))
        {
            return;
        }

        if (!string.Equals(_environmentId, envId, StringComparison.Ordinal))
        {
            _environmentId = envId;
            SetProperty(HabboProperty.const_378, envId);
            UpdateEnvironmentVariables();
        }

        InitEmbeddedConfigurations();
        SetDefaults();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::resetAll
    public void ResetAll()
    {
        _isConfigLoaded = false;
        _configurationDownloadRequested = false;
        _configurationData.Clear();
        _configurationKeys.Clear();
        _environmentId = string.Empty;

        ParseDevelopmentVariables();
        ParseCommonVariables();
        ParseLocalizationVariables();
        SetProperty(HabboProperty.CLIENT_URL, "app:/");
        ParseArguments();

        if (string.IsNullOrWhiteSpace(_environmentId))
        {
            _environmentId = GetProperty(HabboProperty.const_378);
        }

        SetDefaults();
        UpdateEnvironmentVariables();

        if (!PropertyExists(HabboProperty.const_378))
        {
            InitEmbeddedConfigurations();
        }

        if (!_isConfigLoaded && _isConfigReadOnly)
        {
            _isConfigLoaded = true;

            if (locked)
            {
                Unlock();
            }

            events.DispatchEvent("complete");
        }
        else if (!_isConfigLoaded && _skipLocalizations)
        {
            InitConfigurationDownload();
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::initConfigurationDownload
    public void InitConfigurationDownload()
    {
        if (disposed || _isConfigLoaded || _configurationDownloadRequested)
        {
            return;
        }

        _configurationDownloadRequested = true;
        _isConfigLoaded = false;
        List<string> attemptedSources = new();

        foreach (string requestUri in GetExternalVariablesRequestCandidates())
        {
            if (!TryReadConfigurationFromRequest(requestUri, out string configurationData, out string errorMessage))
            {
                attemptedSources.Add(requestUri + " -> " + errorMessage);
                continue;
            }

            context.Debug("[HabboConfigurationManager] External variables loaded from " + requestUri);
            OnInitConfiguration(configurationData);
            return;
        }

        if (attemptedSources.Count == 0)
        {
            OnConfigurationError("No external variables URL configured.");
            return;
        }

        OnConfigurationError("All external variables sources failed: " + string.Join(" | ", attemptedSources));
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::onLocalizationComplete
    private void OnLocalizationComplete(object? param1)
    {
        _ = param1;
        InitConfigurationDownload();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::updateEnvironmentVariables
    private void UpdateEnvironmentVariables()
    {
        string environmentId = GetEffectiveEnvironmentId();

        if (string.IsNullOrWhiteSpace(environmentId))
        {
            return;
        }

        foreach (string key in EnvironmentKeysToUpdate)
        {
            string defaultValue = GetProperty(key);
            string envKey = key + "." + environmentId;

            SetProperty(key, PropertyExists(envKey) ? GetProperty(envKey) : defaultValue);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::parseConfiguration
    private void ParseConfiguration(string config)
    {
        string[] lines = config.Split(["\n\r", "\r\n", "\n", "\r"], StringSplitOptions.None);
        bool isReadOnly = false;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            int separatorIndex = line.IndexOf('=');

            if (separatorIndex <= 0 || separatorIndex >= line.Length - 1)
            {
                continue;
            }

            string key = line[..separatorIndex].Trim();
            string value = line[(separatorIndex + 1)..].Trim();

            if (key.Length == 0 || value.Length == 0)
            {
                continue;
            }

            if (string.Equals(key, "configuration.readonly", StringComparison.Ordinal) &&
                string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                isReadOnly = true;
            }

            if (string.IsNullOrWhiteSpace(_environmentId) &&
                string.Equals(key, HabboProperty.const_378, StringComparison.Ordinal))
            {
                _environmentId = value;
            }

            SetProperty(key, value, isReadOnly);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::initEmbeddedConfigurations
    private void InitEmbeddedConfigurations()
    {
        string? environment = CommunicationUtils.ReadSOLString(CommunicationUtils.SOL_PROPERTY_ENVIRONMENT);

        if (string.IsNullOrWhiteSpace(environment))
        {
            environment = _environmentId;
        }

        if (string.IsNullOrWhiteSpace(environment))
        {
            return;
        }

        context.Debug("[HabboConfigurationManager] Default Environment: " + environment);

        foreach (string key in _configurationData.Keys.ToArray())
        {
            string suffix = "." + environment;

            if (!key.EndsWith(suffix, StringComparison.Ordinal))
            {
                continue;
            }

            string baseKey = key[..^suffix.Length];

            SetProperty(baseKey, GetProperty(key));
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::fillParams
    private static string FillParams(string template, IDictionary<string, string> parameters)
    {
        string value = template;

        for (int i = 0;
             i < 10;
             i++)
        {
            int startIndex = value.IndexOf(REPLACE_CHAR, StringComparison.Ordinal);

            if (startIndex < 0)
            {
                break;
            }

            int endIndex = value.IndexOf(REPLACE_CHAR, startIndex + 1, StringComparison.Ordinal);

            if (endIndex < 0)
            {
                break;
            }

            string key = value[(startIndex + 1)..endIndex];

            parameters.TryGetValue(key, out string? replacement);

            value = value.Replace(REPLACE_CHAR + key + REPLACE_CHAR, replacement ?? string.Empty, StringComparison.Ordinal);
        }

        return value;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::onConfigurationError
    private void OnConfigurationError(string message)
    {
        events.DispatchEvent(HabboConfigurationEvent.CONFIGURATION_ERROR, message);

        CoreEnvironment.Error(
            "Could not load external variables. " + message + ". Client startup failed!",
            true,
            CoreEnvironment.ERROR_CATEGORY_DOWNLOAD_EXTERNAL_VARIABLES
        );
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::onInitConfiguration
    private void OnInitConfiguration(string? configData)
    {
        if (disposed)
        {
            return;
        }

        if (!string.IsNullOrEmpty(configData))
        {
            ParseConfiguration(configData);
        }
        else
        {
            CoreEnvironment.Error(
                "Could not load external variables, got empty data. Client startup failed!",
                false,
                CoreEnvironment.ERROR_CATEGORY_DOWNLOAD_EXTERNAL_VARIABLES
            );
        }

        if (!_isConfigLoaded)
        {
            ConfigurationsLoaded();
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::configurationsLoaded
    private void ConfigurationsLoaded()
    {
        events.DispatchEvent(HabboConfigurationEvent.CONFIGURATION_LOADED);
        ConfigurationsComplete();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::configurationsComplete
    private void ConfigurationsComplete()
    {
        if (disposed || _isConfigLoaded)
        {
            return;
        }

        _isConfigLoaded = true;

        if (locked)
        {
            Unlock();
        }

        events.DispatchEvent("complete");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::setDefaults
    private void SetDefaults()
    {
        context.Debug("Flashvars/host: " + GetProperty(HabboProperty.CONNECTION_INFO_HOST));
        context.Debug("Flashvars/port: " + GetProperty(HabboProperty.CONNECTION_INFO_PORT));

        SetProperty("client.fatal.error.url", "${url.prefix}/flash_client_error");
        SetProperty("game.center.error.url", "${url.prefix}/log/gameerror");

        string crossDomainPolicyFiles = GetProperty("flashclient.crossdomain.policy.files");
        if (!string.IsNullOrWhiteSpace(crossDomainPolicyFiles))
        {
            foreach (string policyFile in crossDomainPolicyFiles.Split(','))
            {
                string sanitized = policyFile.Trim();
                if (sanitized.Length > 0)
                {
                    context.Debug("Crossdomain policy file: " + sanitized);
                }
            }
        }

        _useHttps = GetBoolean("use.https");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::parseDevelopmentVariables
    private void ParseDevelopmentVariables()
    {
        ParseConfigurationAsset("development_configuration");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::parseCommonVariables
    private void ParseCommonVariables()
    {
        ParseConfigurationAsset("common_configuration");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::parseLocalizationVariables
    private void ParseLocalizationVariables()
    {
        ParseConfigurationAsset("localization_configuration");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::parseConfigurationAsset
    private void ParseConfigurationAsset(string assetName)
    {
        if (TryLoadConfigurationAsset(assetName, out string content))
        {
            ParseConfiguration(content);
        }
        else
        {
            context.Debug("Could not parse configuration " + assetName);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/configuration/HabboConfigurationManager.as::parseArguments
    private void ParseArguments()
    {
        if (context is not ICore core)
        {
            return;
        }

        foreach (KeyValuePair<string, object?> pair in core.arguments.ToArray())
        {
            string key = pair.Key.Replace("_", ".", StringComparison.Ordinal);
            string value = pair.Value switch
            {
                null => string.Empty,
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => pair.Value.ToString() ?? string.Empty,
            };

            SetProperty(key, value);

            if (string.IsNullOrEmpty(_environmentId) && string.Equals(key, HabboProperty.const_378, StringComparison.Ordinal))
            {
                _environmentId = value;
            }
        }

        core.ClearArguments();
    }

    private static bool TryLoadConfigurationAsset(string assetName, out string content)
    {
        string[] candidatePaths =
        [
            Path.Combine("data", "configuration", assetName + ".txt"),
            Path.Combine("data", "configuration", assetName),
            Path.Combine("data", assetName + ".txt"),
            Path.Combine("data", assetName),
        ];

        foreach (string candidate in candidatePaths)
        {
            string resolved = ResolvePath(candidate);
            if (!File.Exists(resolved))
            {
                continue;
            }

            content = File.ReadAllText(resolved);
            return true;
        }

        content = string.Empty;
        return false;
    }

    private static bool TryReadConfigurationFromRequest(string requestUri, out string content, out string errorMessage)
    {
        try
        {
            if (requestUri.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                requestUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                using HttpResponseMessage response = _httpClient.GetAsync(requestUri).GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                {
                    content = string.Empty;
                    errorMessage = "HTTP status " + (int)response.StatusCode;
                    return false;
                }

                content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                errorMessage = string.Empty;
                return true;
            }

            string filePath = ResolvePath(requestUri);
            if (!File.Exists(filePath))
            {
                content = string.Empty;
                errorMessage = "File not found: " + filePath;
                return false;
            }

            content = File.ReadAllText(filePath);
            errorMessage = string.Empty;
            return true;
        }
        catch (Exception e)
        {
            content = string.Empty;
            errorMessage = e.Message;
            return false;
        }
    }

    private IList<string> GetExternalVariablesRequestCandidates()
    {
        List<string> candidates = new();

        if (TryGetLocalizationValue("GetActiveEnvironmentId", out string localizationEnvironmentId) &&
            TryGetLocalizationValue("GetExternalVariablesHash", out string hash) &&
            TryGetLocalizationValue("GetExternalVariablesUrl", out string externalVariablesUrl))
        {
            AddCandidate(candidates, externalVariablesUrl.TrimEnd('/', '\\') + "/" + hash);
            AddCandidate(candidates, externalVariablesUrl.TrimEnd('/', '\\') + "/" + localizationEnvironmentId + "/" + hash);
        }

        string environmentId = GetEffectiveEnvironmentId();
        if (!string.IsNullOrWhiteSpace(environmentId))
        {
            AddCandidate(candidates, GetProperty(HabboProperty.EXTERNAL_VARIABLES + "." + environmentId));
            AddCandidate(candidates, Path.Combine("data", "configuration", "external_variables." + environmentId + ".txt"));
        }

        AddCandidate(candidates, GetProperty(HabboProperty.EXTERNAL_VARIABLES));
        AddCandidate(candidates, Path.Combine("data", "configuration", "external_variables.txt"));

        return candidates;
    }

    private string GetEffectiveEnvironmentId()
    {
        if (!string.IsNullOrWhiteSpace(_environmentId))
        {
            return _environmentId;
        }

        string configuredEnvironmentId = GetProperty(HabboProperty.const_378);
        if (!string.IsNullOrWhiteSpace(configuredEnvironmentId))
        {
            _environmentId = configuredEnvironmentId;
            return _environmentId;
        }

        string? persistedEnvironmentId = CommunicationUtils.ReadSOLString(CommunicationUtils.SOL_PROPERTY_ENVIRONMENT);
        if (!string.IsNullOrWhiteSpace(persistedEnvironmentId))
        {
            _environmentId = persistedEnvironmentId;
            return _environmentId;
        }

        _environmentId = "en";
        return _environmentId;
    }

    private static void AddCandidate(ICollection<string> candidates, string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return;
        }

        if (!candidates.Contains(candidate))
        {
            candidates.Add(candidate);
        }
    }

    private bool TryGetLocalizationValue(string methodName, out string value)
    {
        value = string.Empty;
        if (_localization == null)
        {
            return false;
        }

        MethodInfo? method = _localization.GetType().GetMethod(methodName);
        if (method == null || method.GetParameters().Length != 0)
        {
            return false;
        }

        object? result = method.Invoke(_localization, null);
        value = result as string ?? Convert.ToString(result, CultureInfo.InvariantCulture) ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
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
