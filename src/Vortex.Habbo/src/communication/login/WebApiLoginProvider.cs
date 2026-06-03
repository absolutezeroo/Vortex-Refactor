// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/WebApiLoginProvider.as

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Nodes;

using Vortex.Habbo.Utils;

using NetHttpClient = System.Net.Http.HttpClient;

namespace Vortex.Habbo.Communication.Login;

/// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/WebApiLoginProvider.as
public sealed class WebApiLoginProvider : ILoginProvider, ICaptchaListener
{
    public const string ERROR_TYPE_IO_ERROR = "ioError";
    public const string ERROR_CODE_MAINTENANCE = "maintenance";

    private const bool AUTO_RECONNECT = false;
    private const int POCKET_MODE_LOGIN_AND_REGISTER = 1;
    private const int MODE_READY = 2;

    private readonly ILoginViewer _viewer;
    private readonly NetHttpClient _httpClient;

    private IHabboCommunicationManager? _communication;
    private object? _pendingLoginError;
    private bool _autoLogin;
    private int _mode = POCKET_MODE_LOGIN_AND_REGISTER;
    private bool _initializing;
    private string _userName = string.Empty;
    private string _password = string.Empty;
    private string _selectedAvatarUniqueId = string.Empty;
    private string _ssoToken = string.Empty;
    private string _baseUrl = string.Empty;
    private bool _recoveringAvatarSelection;
    private ICaptchaView? _captchaView;

    public event Action<SsoTokenAvailableEvent>? SsoTokenAvailable;

    public WebApiLoginProvider(ILoginViewer viewer)
    {
        _viewer = viewer;

        HttpClientHandler handler = new()
        {
            UseCookies = true,
            CookieContainer = new CookieContainer(),
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        };

        _httpClient = new NetHttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(12),
        };
    }

    public void Init(IHabboCommunicationManager? communication)
    {
        if (_initializing)
        {
            return;
        }

        _initializing = true;
        try
        {
            _communication = communication;

            _baseUrl = GetProperty("web.api");
            if (string.IsNullOrWhiteSpace(_baseUrl))
            {
                _baseUrl = GetProperty("url.prefix").Replace("http:", "https:", StringComparison.OrdinalIgnoreCase);
            }

            if (string.IsNullOrWhiteSpace(_baseUrl))
            {
                _viewer.ShowLoginScreen();
                _viewer.EnvironmentReady();
                return;
            }

            if (TrySend("/api/public/info/hello", HttpMethod.Get, null, out _, out string errorType, out _, false))
            {
                if (_autoLogin)
                {
                    RequestSsoToken();
                }
                else
                {
                    _viewer.EnvironmentReady();
                }
            }
            else
            {
                HandleApiError("/api/public/info/hello", errorType, null, false);
            }
        }
        finally
        {
            _initializing = false;
        }
    }

    public void LoginWithCredentials(string userName, string password, int mode = 0)
    {
        _userName = userName;
        _password = password;
        _ = mode;

        Dictionary<string, string> payload = new(StringComparer.Ordinal)
        {
            ["email"] = userName,
            ["password"] = password,
        };

        if (TrySend("/api/public/authentication/login", HttpMethod.Post, payload, out JsonNode? response, out string errorType, out _,
                true))
        {
            CommunicationUtils.WriteSOLProperty(CommunicationUtils.SOL_PROPERTY_LOGIN_METHOD, CommunicationUtils.LOGIN_METHOD_HABBO);
            CommunicationUtils.WriteSOLProperty(CommunicationUtils.SOL_PROPERTY_LOGIN_NAME, userName);
            CommunicationUtils.StorePassword(password);
            FetchAvatars();
        }
        else
        {
            HandleApiError("/api/public/authentication/login", errorType, response, true);
        }
    }

    public void LoginWithCredentialsWeb(string uniqueId)
    {
        SelectAvatarUniqueid(uniqueId);
    }

    public void SelectAvatar(int id)
    {
        SelectAvatarUniqueid(id.ToString());
    }

    public void SelectAvatarUniqueid(string uniqueId)
    {
        if (string.IsNullOrWhiteSpace(uniqueId))
        {
            _viewer.ShowInvalidLoginError(
                new JsonObject
                {
                    ["error"] = "pocket.auth.no_avatars",
                }
            );
            return;
        }

        _selectedAvatarUniqueId = uniqueId;

        Dictionary<string, string> payload = new(StringComparer.Ordinal)
        {
            ["uniqueId"] = uniqueId,
        };

        if (TrySend("/api/user/avatars/select", HttpMethod.Post, payload, out JsonNode? response, out string errorType, out _, false))
        {
            CommunicationUtils.WriteSOLProperty(CommunicationUtils.SOL_PROPERTY_CHARACTER_UNIQUE_ID, uniqueId);
            RequestSsoToken();
        }
        else
        {
            HandleApiError("/api/user/avatars/select", errorType, response, false);
        }
    }

    public void CloseCaptcha()
    {
        RemoveCaptchaView();
    }

    public void HandleCaptchaError()
    {
        RemoveCaptchaView();
        _viewer.ShowCaptchaError();
    }

    public void HandleCaptchaResult(string token)
    {
        RemoveCaptchaView();
        _viewer.CaptchaReady();

        if (_pendingLoginError != null)
        {
            _viewer.ShowInvalidLoginError(_pendingLoginError);
            _pendingLoginError = null;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            _viewer.ShowCaptchaError();
        }
    }

    public string GetProperty(string key, Dictionary<string, string>? parameters = null)
    {
        return _viewer.GetProperty(key, parameters);
    }

    private void FetchAvatars()
    {
        if (TrySend("/api/user/avatars", HttpMethod.Get, null, out JsonNode? response, out string errorType, out _, false))
        {
            List<AvatarData> avatars = ParseAvatars(response);

            switch (avatars.Count)
            {
                case 0:
                    _viewer.ShowInvalidLoginError(
                        new JsonObject
                        {
                            ["error"] = "pocket.auth.no_avatars",
                        }
                    );
                    return;
                case 1:
                    CommunicationUtils.WriteSOLProperty(CommunicationUtils.SOL_PROPERTY_CHARACTER_UNIQUE_ID, avatars[0].UniqueId);
                    SelectAvatarUniqueid(avatars[0].UniqueId);
                    return;
            }

            if (_autoLogin)
            {
                string? uniqueId = CommunicationUtils.ReadSOLString(CommunicationUtils.SOL_PROPERTY_CHARACTER_UNIQUE_ID);

                if (!string.IsNullOrWhiteSpace(uniqueId) && UserExists(avatars, uniqueId))
                {
                    SelectAvatarUniqueid(uniqueId);

                    return;
                }
            }

            _viewer.PopulateCharacterList(avatars);
        }
        else
        {
            HandleApiError("/api/user/avatars", errorType, response, false);
        }
    }

    private void RequestSsoToken()
    {
        if (TrySend("/api/ssotoken", HttpMethod.Get, null, out JsonNode? response, out string errorType, out _, false))
        {
            string token = response?["ssoToken"]?.GetValue<string>() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(token))
            {
                HandleApiError("/api/ssotoken", ERROR_TYPE_IO_ERROR, response, false);

                return;
            }

            _ssoToken = token;
            _mode = MODE_READY;
            SsoTokenAvailable?.Invoke(new SsoTokenAvailableEvent(SsoTokenAvailableEvent.SSO_TOKEN_AVAILABLE, _ssoToken));
        }
        else
        {
            HandleApiError("/api/ssotoken", errorType, response, false);
        }
    }

    private bool TrySend
    (
        string endpoint,
        HttpMethod method,
        IReadOnlyDictionary<string, string>? payload,
        out JsonNode? responseNode,
        out string errorType,
        out int statusCode,
        bool allowCaptcha
    )
    {
        responseNode = null;
        errorType = ERROR_TYPE_IO_ERROR;
        statusCode = 0;

        if (string.IsNullOrWhiteSpace(_baseUrl))
        {
            return false;
        }

        try
        {
            Uri requestUri = BuildUri(endpoint);
            using HttpRequestMessage request = new(method, requestUri);

            if (payload != null)
            {
                request.Content = new FormUrlEncodedContent(payload);
            }

            using HttpResponseMessage response = _httpClient.Send(request);
            statusCode = (int)response.StatusCode;

            string body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            responseNode = ParseJson(body);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            errorType = response.ReasonPhrase ?? response.StatusCode.ToString();

            if (allowCaptcha && responseNode is JsonObject errorObject && errorObject["captcha"]?.GetValue<bool>() == true)
            {
                ShowCaptchaView();
            }

            return false;
        }
        catch (Exception exception)
        {
            responseNode = new JsonObject
            {
                ["error"] = ERROR_TYPE_IO_ERROR,
                ["message"] = exception.Message,
            };
            errorType = ERROR_TYPE_IO_ERROR;

            return false;
        }
    }

    private static JsonNode? ParseJson(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            return JsonNode.Parse(body);
        }
        catch
        {
            return new JsonObject
            {
                ["message"] = body,
            };
        }
    }

    private Uri BuildUri(string endpoint)
    {
        string root = _baseUrl.TrimEnd('/', '\\');
        string relative = endpoint.StartsWith("/", StringComparison.Ordinal) ? endpoint : "/" + endpoint;

        return new Uri(root + relative, UriKind.Absolute);
    }

    private void ShowCaptchaView()
    {
        _captchaView ??= _viewer.CreateCaptchaView();

        if (_captchaView == null)
        {
            _viewer.ShowCaptchaError();
        }
    }

    private void RemoveCaptchaView()
    {
        if (_captchaView == null)
        {
            return;
        }

        _captchaView.Dispose();
        _captchaView = null;
    }

    private static bool UserExists(IReadOnlyList<AvatarData> avatars, string uniqueId)
    {
        return avatars.Any(avatar => avatar.UniqueId == uniqueId);
    }

    private static List<AvatarData> ParseAvatars(JsonNode? response)
    {
        List<AvatarData> avatars = [];

        if (response is not JsonArray array)
        {
            return avatars;
        }

        foreach (JsonNode? node in array)
        {
            if (node is JsonObject avatarObject)
            {
                avatars.Add(new AvatarData(avatarObject));
            }
        }

        return avatars;
    }

    private void HandleApiError(string endpoint, string errorType, JsonNode? payload, bool captcha)
    {
        if (captcha && payload is JsonObject captchaObject && captchaObject["captcha"]?.GetValue<bool>() == true)
        {
            _pendingLoginError = payload;

            ShowCaptchaView();

            return;
        }

        switch (endpoint)
        {
            case "/api/public/info/hello":
                _viewer.ShowLoginScreen();
                break;
            case "/api/public/registration/new":
                _viewer.ShowRegistrationError(payload);
                break;
            case "/api/ssotoken":
                _viewer.ShowInvalidLoginError(payload);
                break;
            case "/api/user/avatars":
                _viewer.ShowInvalidLoginError(payload);
                break;
            case "/api/newuser/name/check":
            case "/api/newuser/name/select":
                _viewer.NameCheckResponse(payload, endpoint == "/api/newuser/name/check");
                break;
            case "/api/public/authentication/login":
            case "/api/public/authentication/facebook":
            case "/api/force/tos-accept":
                _viewer.ShowInvalidLoginError(payload);
                break;
            case "/api/user/avatars/select":
                _viewer.ShowAccountError(payload);
                _viewer.ShowLoadingScreen();
                if (!_recoveringAvatarSelection)
                {
                    _recoveringAvatarSelection = true;
                    try
                    {
                        FetchAvatars();
                    }
                    finally
                    {
                        _recoveringAvatarSelection = false;
                    }
                }
                break;
            case "/api/user/look/save":
                _viewer.SaveLooksError(payload);
                break;
            default:
                _viewer.ShowInvalidLoginError(payload);
                break;
        }

        if (!string.Equals(errorType, ERROR_TYPE_IO_ERROR, StringComparison.Ordinal))
        {
            _autoLogin = AUTO_RECONNECT;
        }
    }
}
