// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as

using System;
using System.Net.Http;
using System.Threading.Tasks;

using Godot;

using Vortex.Core.Runtime;
using Vortex.Habbo.Utils;
using Vortex.Login;

using HttpClient = System.Net.Http.HttpClient;

namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as
public partial class HabboAir : Control
{
    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::CORE_RATIO
    public const double CORE_RATIO = 0.6;

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::ERROR_VARIABLE_IS_FATAL
    public const string ERROR_VARIABLE_IS_FATAL = "is_fatal";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::ERROR_VARIABLE_CLIENT_CRASH_TIME
    public const string ERROR_VARIABLE_CLIENT_CRASH_TIME = "crash_time";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::ERROR_VARIABLE_CONTEXT
    public const string ERROR_VARIABLE_CONTEXT = "error_ctx";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::ERROR_VARIABLE_FLASH_VERSION
    public const string ERROR_VARIABLE_FLASH_VERSION = "flash_version";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::ERROR_VARIABLE_AVERAGE_UPDATE_INTERVAL
    public const string ERROR_VARIABLE_AVERAGE_UPDATE_INTERVAL = "avg_update";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::ERROR_VARIABLE_DEBUG
    public const string ERROR_VARIABLE_DEBUG = "debug";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::ERROR_VARIABLE_DESCRIPTION
    public const string ERROR_VARIABLE_DESCRIPTION = "error_desc";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::ERROR_VARIABLE_CATEGORY
    public const string ERROR_VARIABLE_CATEGORY = "error_cat";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::ERROR_VARIABLE_DATA
    public const string ERROR_VARIABLE_DATA = "error_data";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::RECEPTION_LOG_STEP_FUNCTION
    private const string RECEPTION_LOG_STEP_FUNCTION = "NewUserReception.logStep";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::STEP_NUX_ENTERED
    private const string STEP_NUX_ENTERED = "NUX_ENTERED";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::STEP_RECEPTION_EXITED
    private const string STEP_RECEPTION_EXITED = "RECEPTION_EXITED";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::STEP_NUX_EXITED
    private const string STEP_NUX_EXITED = "NUX_EXITED";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::STEP_CLIENT_LOADED
    private const string STEP_CLIENT_LOADED = "CLIENT_LOADED";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::ERROR_CATEGORY_FINALIZE_PRELOADING
    public const int ERROR_CATEGORY_FINALIZE_PRELOADING = 9;

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::ERROR_CATEGORY_DOWNLOAD_FONT
    public const int ERROR_CATEGORY_DOWNLOAD_FONT = 11;

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::ERROR_UNCAUGHT_ERROR
    public const int ERROR_UNCAUGHT_ERROR = 40;

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::ARGUMENT_ENVIRONMENT
    private const string ARGUMENT_ENVIRONMENT = "server";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::ARGUMENT_SSO_TOKEN
    private const string ARGUMENT_SSO_TOKEN = "ticket";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::_SafeStr_236
    private static readonly int SafeStr236 = 7000000;

    protected static bool ProcessLogEnabled;
    private static string _safeStr237 = "https://www.habbo.com/api/log/crash";
    private static bool _safeStr238;
    private static readonly HttpClient HttpClient = new();
    private uint _cachedBytesLoaded;
    private bool _disposed;
    private int _httpStatus;
    private IHabboLoadingScreen? _loadingScreen;
    private LoginFlow? _loginFlow;
    private bool _preloadingFinalized;
    private double _simulatedPreloadProgress;
    private long _startTime;

    private bool safeStr_239;
    private uint safeStr_240;
    private bool safeStr_241;
    private bool safeStr_242;
    private bool safeStr_243;
    private bool safeStr_244 = true;
    private Dictionary<string, object?> safeStr_245 = new(StringComparer.Ordinal);
    private bool safeStr_246;

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::HabboAir
    public HabboAir()
    {
        Name = nameof(HabboAir);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::get progress
    public double progress => bytesTotal != 0 ? bytesLoaded / (double)bytesTotal : safeStr_241 ? 1 : 0;

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::get bytesLoaded
    public uint bytesLoaded
    {
        get
        {
            if (safeStr_239)
            {
                CalculateProgress();
            }

            return _cachedBytesLoaded;
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::get bytesTotal
    public uint bytesTotal
    {
        get
        {
            if (safeStr_239)
            {
                CalculateProgress();
            }

            return safeStr_240;
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::get ssoTokenAvailable
    private bool ssoTokenAvailable
    {
        get
        {
            string? token = Convert.ToString(GetPropertyValue("sso.token"));

            return !string.IsNullOrEmpty(token);
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::_Ready
    public override void _Ready()
    {
        _startTime = (long)Time.GetTicksMsec();
        safeStr_245 = new Dictionary<string, object?>(StringComparer.Ordinal);

        OnAddedToStage();
        ParseArguments([]);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::_Process
    public override void _Process(double delta)
    {
        if (_disposed || !safeStr_243 || !safeStr_242 || _preloadingFinalized)
        {
            return;
        }

        if (!safeStr_241)
        {
            _simulatedPreloadProgress = Math.Clamp(_simulatedPreloadProgress + (delta * 0.45), 0, 1);
            safeStr_240 = safeStr_240 == 0 ? (uint)SafeStr236 : safeStr_240;
            _cachedBytesLoaded = (uint)Math.Round(safeStr_240 * _simulatedPreloadProgress);
            safeStr_239 = true;

            OnPreLoadingProgress();

            if (_simulatedPreloadProgress >= 1)
            {
                OnPreLoadingCompleted();
            }
        }
        else
        {
            CheckPreLoadingStatus();
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::trackLoginStep
    public static void TrackLoginStep(string param1, string? param2 = null)
    {
        Logger.Info($"* HabboMain Login Step: {param1}");

        if (!ProcessLogEnabled)
        {
            return;
        }

        HabboWebTools.LogLoginStep(param1, param2);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::reportCrash
    public static void ReportCrash(string param1, int param2, bool param3, Exception? param4 = null, ICoreErrorLogger? param5 = null)
    {
        _ = param5;

        string stack = param4?.ToString() ?? string.Empty;

        ReportCrashStack(param1, param2, param3, stack, param5);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::reportCrashStack
    public static void ReportCrashStack(string param1, int param2, bool param3, string param4, ICoreErrorLogger? param5 = null)
    {
        _ = param5;

        Dictionary<string, string> payload = new(StringComparer.Ordinal)
        {
            [ERROR_VARIABLE_CLIENT_CRASH_TIME] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
            [ERROR_VARIABLE_IS_FATAL] = param3.ToString(),
            [ERROR_VARIABLE_CONTEXT] = string.Empty,
            [ERROR_VARIABLE_FLASH_VERSION] = $"{OS.GetName()} {Engine.GetVersionInfo()["string"]}",
            [ERROR_VARIABLE_AVERAGE_UPDATE_INTERVAL] = "0",
            [ERROR_VARIABLE_DESCRIPTION] = param1,
            [ERROR_VARIABLE_CATEGORY] = param2.ToString(),
            [ERROR_VARIABLE_DEBUG] = $"Memory usage: {Math.Round(GC.GetTotalMemory(false) / 1048576d)} MB",
        };

        if (!string.IsNullOrEmpty(param4))
        {
            payload[ERROR_VARIABLE_DATA] = param4;
        }

        if (param3 && !_safeStr238)
        {
            _safeStr238 = true;
        }

        _ = Task.Run(async () =>
            {
                try
                {
                    using FormUrlEncodedContent content = new(payload);
                    using HttpRequestMessage request = new(HttpMethod.Post, _safeStr237)
                    {
                        Content = content,
                    };
                    using HttpResponseMessage _ = await HttpClient.SendAsync(request).ConfigureAwait(false);
                }
                catch (Exception error)
                {
                    Logger.Error($"Error while sending error report: {error.Message}");
                }
            }
        );
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::onBrowserInvoke
    private static void OnBrowserInvoke(object? param1)
    {
        Logger.Debug($"Received Browser Invoke: {param1}");
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::onInvoke
    private static void OnInvoke(object? param1)
    {
        _ = param1;
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::parseArguments
    private void ParseArguments(IList<string> param1)
    {
        string environmentId = GetArgumentValue(ARGUMENT_ENVIRONMENT, param1)
                               ?? GetArgumentValue("environment.id", param1)
                               ?? "en";

        safeStr_245 = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["environment.id"] = environmentId,
            ["sso.token"] = GetArgumentValue("sso.ticket", param1) ?? GetArgumentValue(ARGUMENT_SSO_TOKEN, param1),
        };

        safeStr_243 = true;

        TryInit();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::onAddedToStage
    private void OnAddedToStage()
    {
        safeStr_242 = true;
        TryInit();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::tryInit
    private void TryInit()
    {
        if (!safeStr_243 || !safeStr_242)
        {
            return;
        }

        if (safeStr_245.TryGetValue("client.fatal.error.url", out object? fatalUrl) && fatalUrl != null)
        {
            _safeStr237 = Convert.ToString(fatalUrl) ?? _safeStr237;
        }
        else if (safeStr_245.TryGetValue("url.prefix", out object? urlPrefix) && urlPrefix != null)
        {
            _safeStr237 = $"{urlPrefix}/flash_client_error";
        }

        ProcessLogEnabled = string.Equals(Convert.ToString(GetPropertyValue("processlog.enabled")), "1", StringComparison.Ordinal);

        TrackLoginStep("client.init.start");
        CreateNewUserLobbyOrLoadingScreen();
        CheckPreLoadingStatus();
        safeStr_239 = true;
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::onLoginFLowFinished
    private void OnLoginFLowFinished()
    {
        if (_loginFlow != null)
        {
            safeStr_245["sso.token"] = _loginFlow.SsoToken;

            string? environmentId = CommunicationUtils.ReadSOLString(CommunicationUtils.SOL_PROPERTY_ENVIRONMENT);

            if (!string.IsNullOrWhiteSpace(environmentId))
            {
                safeStr_245["environment.id"] = environmentId;
            }

            _loginFlow.LoginFlowFinished -= OnLoginFLowFinished;
            _loginFlow.QueueFree();
            _loginFlow = null;
        }

        _loadingScreen = null;
        CreateLoadingScreen();
        CheckPreLoadingStatus();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::onPreLoadingStatus
    private void OnPreLoadingStatus(int param1)
    {
        _httpStatus = param1;
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::onPreLoadingProgress
    private void OnPreLoadingProgress()
    {
        CheckPreLoadingStatus();
        UpdateLoadingBarProgress();
        safeStr_239 = true;
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::onPreLoadingCompleted
    private void OnPreLoadingCompleted()
    {
        try
        {
            safeStr_241 = true;
            CheckPreLoadingStatus();
        }
        catch (Exception error)
        {
            TrackLoginStep("client.init.swf.error");
            ReportCrash(
                $"Failed to finalize main swf preloading: {error.Message} runtime: {(Time.GetTicksMsec() - (ulong)_startTime) / 1000d}s",
                ERROR_CATEGORY_FINALIZE_PRELOADING, true, error
            );
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::onPreLoadingFailed
    private void OnPreLoadingFailed(string param1)
    {
        TrackLoginStep("client.init.swf.error");
        ReportCrash(
            $"IO error in main swf preloading: {param1} / HTTP status: {_httpStatus} / Loaded: {_cachedBytesLoaded} of {safeStr_240} bytes. Runtime: {(Time.GetTicksMsec() - (ulong)_startTime) / 1000d}s",
            ERROR_CATEGORY_FINALIZE_PRELOADING, true
        );
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::checkPreLoadingStatus
    private void CheckPreLoadingStatus()
    {
        if (_loginFlow != null)
        {
            return;
        }

        if (safeStr_241 && progress >= 1)
        {
            FinalizePreloading();
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::calculateProgress
    private void CalculateProgress()
    {
        if (safeStr_240 == 0)
        {
            safeStr_240 = (uint)SafeStr236;
            if (!safeStr_246)
            {
                safeStr_246 = true;
                TrackLoginStep("client.gzip.environment");
            }
        }

        if (safeStr_240 < _cachedBytesLoaded || safeStr_241)
        {
            safeStr_240 = _cachedBytesLoaded;
        }

        safeStr_239 = false;
        if (!safeStr_241 && _cachedBytesLoaded == safeStr_240)
        {
            safeStr_239 = true;
            _cachedBytesLoaded = (uint)Math.Round(_cachedBytesLoaded * 0.99);
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::clone
    private static Dictionary<string, object?> Clone(Dictionary<string, object?> param1)
    {
        Dictionary<string, object?> result = new(StringComparer.Ordinal);
        foreach (KeyValuePair<string, object?> pair in param1)
        {
            if (pair.Value is Dictionary<string, object?> nested)
            {
                result[pair.Key] = Clone(nested);
            }
            else
            {
                result[pair.Key] = pair.Value;
            }
        }

        return result;
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::createNewUserLobbyOrLoadingScreen
    private void CreateNewUserLobbyOrLoadingScreen()
    {
        if (!ssoTokenAvailable && safeStr_244)
        {
            if (_loginFlow == null)
            {
                _loginFlow = new LoginFlow(Clone(safeStr_245));
                _loginFlow.LoginFlowFinished += OnLoginFLowFinished;
                AddChild(_loginFlow);
                _loginFlow.Init();
                UpdateLoadingBarProgress();
            }

            return;
        }

        CreateLoadingScreen();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::createLoadingScreen
    public void CreateLoadingScreen()
    {
        Rect2 viewportRect = GetViewportRect();
        int width = (int)Math.Max(1, viewportRect.Size.X);
        int height = (int)Math.Max(1, viewportRect.Size.Y);

        _loadingScreen = new HabboLoadingScreen(width, height, Clone(safeStr_245));

        UpdateLoadingBarProgress();

        if (_loadingScreen is Node node && node.GetParent() == null)
        {
            AddChild(node);
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::updateLoadingBarProgress
    private void UpdateLoadingBarProgress()
    {
        if (_loadingScreen == null)
        {
            return;
        }

        double localProgress = progress;
        double ratio = localProgress == 0
            ? bytesLoaded / (double)SafeStr236 * CORE_RATIO
            : localProgress * CORE_RATIO;

        _loadingScreen.UpdateLoadingBar(ratio);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::finalizePreloading
    private void FinalizePreloading()
    {
        if (_preloadingFinalized)
        {
            return;
        }

        TrackLoginStep("client.init.swf.loaded");

        _preloadingFinalized = true;

        HabboAirMain main = new(_loadingScreen, safeStr_245);

        main.TreeExited += OnMainRemoved;

        AddChild(main);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::onMainRemoved
    private void OnMainRemoved()
    {
        Dispose();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::dispose
    private new void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _loadingScreen = null;

        GetParent()?.RemoveChild(this);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::getPropertyValue
    private object? GetPropertyValue(string param1)
    {
        safeStr_245.TryGetValue(param1, out object? value);

        return value;
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::getArgumentValue
    private static string? GetArgumentValue(string param1, IList<string> param2)
    {
        foreach (string argument in param2)
        {
            if (TryParseArgument(argument, out string? key, out string? value) &&
                (string.Equals(key, param1, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(key, $"--{param1}", StringComparison.OrdinalIgnoreCase)))
            {
                return value;
            }
        }

        foreach (string? argument in OS.GetCmdlineArgs())
        {
            if (TryParseArgument(argument, out string? key, out string? value) &&
                (string.Equals(key, param1, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(key, $"--{param1}", StringComparison.OrdinalIgnoreCase)))
            {
                return value;
            }
        }

        return null;
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAir.as::tryParseArgument
    private static bool TryParseArgument(string param1, out string? param2, out string? param3)
    {
        int index = param1.IndexOf('=');

        if (index < 0)
        {
            param2 = null;
            param3 = null;
            return false;
        }

        param2 = param1[..index];
        param3 = param1[(index + 1)..];

        return true;
    }
}
