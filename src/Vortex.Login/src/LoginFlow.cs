// @see WIN63-202111081545-75921380-Source-main/src/login/LoginFlow.as

using System;
using System.Linq;
using System.Text.Json.Nodes;

using Godot;

using Vortex.Core;
using Vortex.Core.Runtime;
using Vortex.IID;
using Vortex.Core.Utils;
using Vortex.Habbo.Communication;
using Vortex.Habbo.Communication.Login;
using Vortex.Habbo.Configuration;
using Vortex.Habbo.Localization;
using Vortex.Habbo.Utils;
using Vortex.OnBoardingHcUi;

using HcButton = Vortex.OnBoardingHcUi.Button;
using Timer = Godot.Timer;

namespace Vortex.Login;

/// @see WIN63-202111081545-75921380-Source-main/src/login/LoginFlow.as
public partial class LoginFlow : Control, ILoginContext, ILoginViewer
{
    public const string LOGIN_FLOW_FINISHED_EVENT = "LOGIN_FLOW_FINISHED_EVENT";
    private const string ERROR_TYPE_IO_ERROR = "ioError";
    private const int LOGO_AREA_HEIGHT = 50;
    private const int MAIN_AREA_MARGIN = 5;
    public const int SCREEN_ENVIRONMENT = 1;
    public const int SCREEN_LOGIN = 2;
    public const int SCREEN_AVATARS = 3;
    public const int SCREEN_SSO_TOKEN = 4;

    private readonly Dictionary<string, object?> _properties;
    private AvatarView? _avatarView;

    private Background? _background;
    private ColouredButton? _closeButton;
    private IHabboCommunicationManager? _communication;
    private HabboConfigurationManager? _configuration;
    private bool _disposed;

    private EnvironmentView? _environmentView;

    private Control? _errorBalloon;
    private Timer? _errorTimer;
    private CoreComponentContext? _fakeContext;

    private bool _initialized;
    private TextureRect? _leftImage;
    private HabboLocalizationManager? _localization;

    private ILoginProvider? _loginProvider;
    private LoginView? _loginView;
    private Control? _logoArea;
    private Control? _mainArea;
    private TextureRect? _rightImage;
    private SsoTokenView? _ssoTokenView;
    private Control? _viewHost;

    public LoginFlow() : this(new Dictionary<string, object?>(StringComparer.Ordinal)) { }

    public LoginFlow(Dictionary<string, object?> properties)
    {
        Name = nameof(LoginFlow);
        _properties = Clone(properties);
        CreateFakeContext(_properties);

        MouseFilter = MouseFilterEnum.Stop;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }
    public string SsoToken { get; private set; } = string.Empty;

    public Viewport? Stage => GetViewport();
    public Label? DebugText => null;

    public void InitLoginWithSsoToken(string environmentId, string token)
    {
        UpdateEnvironment(environmentId, false);
        SsoToken = token;
        LoginFlowFinished?.Invoke();
    }

    public void InitLogin(string userName, string password)
    {
        _loginProvider?.LoginWithCredentials(userName, password);
    }

    public void LoginWithAvatar(AvatarData avatar)
    {
        _loginProvider?.LoginWithCredentialsWeb(avatar.UniqueId);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/login/LoginFlow.as::showScreen
    public void ShowScreen(int screenId)
    {
        HideViews();

        if (_viewHost == null)
        {
            return;
        }

        switch (screenId)
        {
            case SCREEN_ENVIRONMENT:
                _viewHost.AddChild(_environmentView!);
                _environmentView!.Init();
                break;
            case SCREEN_LOGIN:
                _viewHost.AddChild(_loginView!);
                _loginView!.Init();
                _loginProvider?.Init(_communication);
                break;
            case SCREEN_SSO_TOKEN:
                _viewHost.AddChild(_ssoTokenView!);
                _ssoTokenView!.Init();
                break;
            case SCREEN_AVATARS:
                _viewHost.AddChild(_avatarView!);
                _avatarView!.Init();
                _avatarView!.BaseUrl = GetProperty("web.api");
                LayoutMainElements();
                break;
        }

        LayoutMainElements();
    }

    public string GetProperty(string key, Dictionary<string, string>? parameters = null)
    {
        if (_configuration != null)
        {
            string configured = _configuration.GetProperty(key, parameters);

            if (!string.IsNullOrWhiteSpace(configured))
            {
                return configured;
            }
        }

        if (!_properties.TryGetValue(key, out object? value) || value == null)
        {
            return string.Empty;
        }

        string text = Convert.ToString(value) ?? string.Empty;

        if (text.Length == 0)
        {
            return string.Empty;
        }

        text = Interpolate(text);

        if (parameters != null)
        {
            text = parameters.Aggregate(text, (current, pair) => current.Replace($"%{pair.Key}%", pair.Value, StringComparison.Ordinal));
        }

        return text;
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/login/LoginFlow.as::showLoginScreen
    public void ShowLoginScreen() { }

    public void ShowRegistrationError(object? data)
    {
        ShowError(data);
    }

    public void ShowInvalidLoginError(object? data)
    {
        ShowError(data);
    }

    public void NameCheckResponse(object? data, bool checkOnly)
    {
        _ = data;
        _ = checkOnly;
    }

    public void ShowAccountError(object? data)
    {
        ShowError(data);
    }

    public void ShowLoadingScreen() { }

    public void SaveLooksError(object? data)
    {
        ShowError(data);
    }

    public void ShowTOS()
    {
        ShowErrorMessage("Need to show TOS");
    }

    public void EnvironmentReady()
    {
        _loginView?.Ready();
    }

    public void PopulateCharacterList(IReadOnlyList<AvatarData> avatars)
    {
        ShowScreen(SCREEN_AVATARS);

        _avatarView?.PopulateAvatars(avatars);
    }

    public void ShowSelectAvatar(object? data)
    {
        _ = data;
    }

    public void ShowPromoHabbos(string? xml)
    {
        _ = xml;
    }

    public void ShowSelectRoom() { }

    public void ShowCaptchaError()
    {
        ShowScreen(SCREEN_LOGIN);
        ShowErrorMessage("Error with captcha");
    }

    public ICaptchaView CreateCaptchaView()
    {
        if (_closeButton != null && _closeButton.GetParent() == null)
        {
            AddChild(_closeButton);
        }

        ICaptchaListener listener = _loginProvider as ICaptchaListener ?? new EmptyCaptchaListener(this);
        WebCaptchaView captchaView = new(listener)
        {
            Name = "WebCaptchaView",
        };
        AddChild(captchaView);

        LayoutMainElements();
        return captchaView;
    }

    public void CaptchaReady()
    {
        if (_closeButton?.GetParent() == this)
        {
            RemoveChild(_closeButton);
        }

        ShowScreen(SCREEN_LOGIN);
    }

    public event Action? LoginFlowFinished;

    public override void _Ready()
    {
        base._Ready();

        Init();
    }

    public override void _ExitTree()
    {
        _disposed = true;

        if (GetViewport() != null)
        {
            GetViewport().SizeChanged -= OnStageResize;
        }

        if (_loginProvider != null)
        {
            _loginProvider.SsoTokenAvailable -= OnSsoTokenAvailable;
        }

        NodeUtils.FreeIfOrphaned(_environmentView);
        NodeUtils.FreeIfOrphaned(_loginView);
        NodeUtils.FreeIfOrphaned(_avatarView);
        NodeUtils.FreeIfOrphaned(_ssoTokenView);
        NodeUtils.FreeIfOrphaned(_closeButton);
        _environmentView = null;
        _loginView = null;
        _avatarView = null;
        _ssoTokenView = null;
        _closeButton = null;

        _fakeContext?.Dispose();
        _fakeContext = null;
        _configuration = null;
        _communication = null;
        _localization = null;

        base._ExitTree();
    }

    public void Init()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        LocalizedSprite.LocalizationResolver = ResolveLocalization;

        BuildScene();
        CreateViews();
        LoadImages();

        ShowScreen(SCREEN_SSO_TOKEN);
        LayoutMainElements();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/login/LoginFlow.as::editorFinished
    public void EditorFinished()
    {
        LoginFlowFinished?.Invoke();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/login/LoginFlow.as::updateEnvironment
    public void UpdateEnvironment(string environmentId, bool previewOnly)
    {
        if (string.IsNullOrWhiteSpace(environmentId))
        {
            return;
        }

        if (previewOnly)
        {
            _localization?.LoadDefaultEmbedLocalizations(environmentId);
            return;
        }

        CommunicationUtils.WriteSOLProperty(CommunicationUtils.SOL_PROPERTY_ENVIRONMENT, environmentId);

        _configuration?.UpdateEnvironmentId(environmentId);

        _localization?.LoadDefaultEmbedLocalizations(
            _configuration?.GetProperty("environment.id") ?? environmentId
        );

        _environmentView?.UpdateEnvironment();

        _communication?.UpdateHostParameters();
    }

    private void BuildScene()
    {
        _background = new Background();

        AddChild(_background);

        // AS3: _SafeStr_4566 = new Loader(); visible = false; alpha = 0;
        _leftImage = new TextureRect
        {
            Name = "LandingLeft",
            Visible = false,
            Modulate = new Color(1, 1, 1, 0),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(_leftImage);

        // AS3: _SafeStr_4567 = new Loader(); visible = false; alpha = 0;
        _rightImage = new TextureRect
        {
            Name = "LandingRight",
            Visible = false,
            Modulate = new Color(1, 1, 1, 0),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(_rightImage);

        _logoArea = new Control
        {
            Name = "LogoArea",
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(_logoArea);

        if (ResourceLoader.Exists("res://assets/images/habbo_logo.png"))
        {
            TextureRect logo = new()
            {
                Texture = GD.Load<Texture2D>("res://assets/images/habbo_logo.png"),
                Position = new Vector2(40, 40),
            };
            _logoArea.AddChild(logo);
        }

        _mainArea = new Control
        {
            Name = "MainArea",
            Position = new Vector2(MAIN_AREA_MARGIN, LOGO_AREA_HEIGHT),
        };
        AddChild(_mainArea);

        _viewHost = new Control
        {
            Name = "ViewHost",
            Position = new Vector2(0, LOGO_AREA_HEIGHT),
        };
        _mainArea.AddChild(_viewHost);

        _closeButton = new ColouredButton("red", "X", new Rect2(0, 0, 0, 40), true, OnClose, 0xD8D8D8);

        if (GetViewport() != null)
        {
            GetViewport().SizeChanged += OnStageResize;
        }
    }

    private void CreateViews()
    {
        _environmentView = new EnvironmentView(this);
        _loginView = new LoginView(this);
        _avatarView = new AvatarView(this);
        _ssoTokenView = new SsoTokenView(this);

        _loginProvider = new WebApiLoginProvider(this);
        _loginProvider.SsoTokenAvailable += OnSsoTokenAvailable;
    }

    private void CreateFakeContext(Dictionary<string, object?> properties)
    {
        if (_fakeContext != null)
        {
            return;
        }

        try
        {
            // Register assemblies so manifest type resolution can find IID and Bootstrap types.
            ComponentContext.RegisterManifestAssembly(typeof(ComponentContext).Assembly);         // Vortex.Core
            ComponentContext.RegisterManifestAssembly(typeof(CoreCommunicationFrameworkLib).Assembly); // Vortex.Bootstrap
            ComponentContext.RegisterManifestAssembly(typeof(IIDCoreCommunicationManager).Assembly);   // Vortex.IID

            _fakeContext = new CoreComponentContext(
                null,
                new class_516(),
                CoreEnvironment.CORE_SETUP_FRAME_UPDATE_SIMPLE,
                Clone(properties)
            );

            // Use CoreCommunicationFrameworkLib (not the bootstrap directly) so the manifest
            // is processed and IIDCoreCommunicationManager is published to the context.
            _ = _fakeContext.PrepareComponent(typeof(CoreCommunicationFrameworkLib));
            _configuration = _fakeContext.PrepareComponent(typeof(HabboConfigurationManagerBootstrap)) as HabboConfigurationManager;
            _communication = _fakeContext.PrepareComponent(typeof(HabboCommunicationManagerBootstrap)) as IHabboCommunicationManager;
            _localization = _fakeContext.PrepareComponent(
                typeof(HabboLocalizationManagerBootstrap),
                HabboLocalizationManager.SKIP_EXTERNAL_LOCALIZATIONS
            ) as HabboLocalizationManager;

            if (_configuration == null)
            {
                return;
            }

            string configuredEnvironmentId = _configuration.GetProperty("environment.id");

            if (!string.IsNullOrWhiteSpace(configuredEnvironmentId))
            {
                _properties["environment.id"] = configuredEnvironmentId;
            }

            string environmentId = configuredEnvironmentId;

            if (string.IsNullOrWhiteSpace(environmentId))
            {
                environmentId = "en";
            }

            _localization?.LoadDefaultEmbedLocalizations(environmentId);
        }
        catch (Exception exception)
        {
            Logger.Warn("[LoginFlow] Failed to create fake context: " + exception.Message);
            _fakeContext?.Dispose();
            _fakeContext = null;
            _configuration = null;
            _communication = null;
            _localization = null;
        }
    }

    private void LoadImages()
    {
        if (_rightImage != null)
        {
            string rightUri = GetProperty("landing.view.background_right.uri");

            if (!string.IsNullOrWhiteSpace(rightUri))
            {
                ImageLoader.CreateLoader(_rightImage, rightUri, OnImageComplete);
            }
        }

        if (_leftImage == null)
        {
            return;
        }

        string leftUri = GetProperty("landing.view.background_left.uri");

        if (!string.IsNullOrWhiteSpace(leftUri))
        {
            ImageLoader.CreateLoader(_leftImage, leftUri, OnImageComplete);
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/login/LoginFlow.as::onImageComplete
    private void OnImageComplete(ImageLoaderEvent args)
    {
        args.Loader.Visible = true;

        // AS3: TweenUtils.alphaTweenVisible(loader, 0, 1.2)
        Tween? tween = CreateTween();
        tween.TweenProperty(args.Loader, "modulate:a", 1.0f, 1.2f).From(0.0f);

        LayoutMainElements();
    }

    private void OnSsoTokenAvailable(SsoTokenAvailableEvent args)
    {
        SsoToken = args.SsoToken;
        LoginFlowFinished?.Invoke();
    }

    private void OnStageResize()
    {
        if (_disposed)
        {
            return;
        }

        LayoutMainElements();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/login/LoginFlow.as::layoutMainElements
    private void LayoutMainElements()
    {
        if (_disposed)
        {
            return;
        }

        _background?.Resize();

        Vector2 stageSize = GetViewportRect().Size;

        if (_mainArea != null)
        {
            // AS3: _SafeStr_4564.width — Flash Sprite.width returns the bounding box of all
            // visible children. Godot Control.Size does not auto-compute from children.
            float desiredWidth = DisplayUtils.ComputeDescendantBounds(_mainArea).Size.X + 20;
            float x = stageSize.X > desiredWidth ? Mathf.Max(5, (stageSize.X - desiredWidth) / 2f) : 5;
            _mainArea.Position = new Vector2(x, LOGO_AREA_HEIGHT);
        }

        if (_closeButton != null)
        {
            _closeButton.Position = new Vector2(stageSize.X - _closeButton.Size.X - 30, 30);
        }

        if (_rightImage is { Texture: not null })
        {
            _rightImage.Position = new Vector2(Mathf.Max(400, stageSize.X - _rightImage.Size.X + 50), stageSize.Y - _rightImage.Size.Y + 50);
        }

        if (_leftImage is { Texture: not null })
        {
            _leftImage.Position = new Vector2(-50, stageSize.Y - _leftImage.Size.Y + 50);
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/login/LoginFlow.as::showErrorMessage
    private void ShowErrorMessage(string message)
    {
        if (_errorBalloon == null)
        {
            if (LoaderUI.CreateTextField(message, 12, 0xFFFFFF, true) is not Label textLabel)
            {
                return;
            }

            LoaderUI.AddEtching(textLabel, true);

            Vector2 textSize = textLabel.GetCombinedMinimumSize();
            Control balloon = LoaderUI.CreateBalloon(
                (int)textSize.X + 30,
                (int)textSize.Y + 17,
                -1, true, 11411485, "down"
            );

            _errorBalloon = new Control
            {
                Name = "ErrorBalloon",
            };
            _errorBalloon.AddChild(balloon);
            textLabel.Position = new Vector2(15, 14);
            _errorBalloon.AddChild(textLabel);
            _mainArea?.AddChild(_errorBalloon);
            _errorBalloon.Position = new Vector2(300, 300);

            // AS3: _errorBalloon.filters = [new GlowFilter(0, 0.24, 6, 6)];
            // No native GlowFilter in Godot; would require a custom shader.
        }

        if (_errorTimer == null)
        {
            _errorTimer = new Timer
            {
                Name = "ErrorTimer",
                OneShot = true,
                WaitTime = 3.0,
            };
            _errorTimer.Timeout += OnHideError;
            AddChild(_errorTimer);
        }

        _errorTimer.Stop();
        _errorTimer.Start();
        _errorBalloon.Visible = true;
    }

    private void OnHideError()
    {
        if (_errorBalloon != null)
        {
            _errorBalloon.Visible = false;
        }
    }

    private void HideViews()
    {
        if (_viewHost == null)
        {
            return;
        }

        while (_viewHost.GetChildCount() > 0)
        {
            _viewHost.RemoveChild(_viewHost.GetChild(0));
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/login/LoginFlow.as::showError
    private void ShowError(object? data)
    {
        string code = ExtractErrorCode(data);

        switch (code)
        {
            case "invalid-captcha":
                ShowCaptchaError();
                return;
            case "pocket.auth.facebook_not_connected":
                return;
        }

        string localizationKey = code switch
        {
            "login.user_banned" => "connection.login.error.banned.desc",
            "login.blocked" => "connection.login.error.blocked.desc",
            "unauthorized-staff-login" => "connection.login.error.unauthorized.staff",
            "pocket.auth.login_failed" => "connection.login.error.-3.desc",
            "pocket.auth.no_avatars" => "connection.login.missing_avatars",
            "pocket.auth.valid_email_required" => "connection.login.missing_credentials",
            "pocket.auth.password_required" => "connection.login.missing_credentials",
            "pocket.auth.facebook_disabled" => "connection.login.error.facebook_disabled.desc",
            "pocket.auth.access_token_required" => "connection.login.error.facebook_accesstoken.desc",
            ERROR_TYPE_IO_ERROR => "connection.login.error.-400.desc",
            "account_issue" => "generic.error",
            _ => "generic.error",
        };

        if (localizationKey.Length > 0)
        {
            ShowErrorMessage(ResolveLocalization(localizationKey));
        }
    }

    private static string ExtractErrorCode(object? data)
    {
        return data switch
        {
            JsonObject jsonObject when jsonObject["errors"] is JsonArray { Count: > 0 } errors => errors[0]?.GetValue<string>()
                ?? string.Empty,
            JsonObject jsonObject when jsonObject["error"] != null => jsonObject["error"]?.GetValue<string>() ?? string.Empty,
            JsonObject jsonObject when jsonObject["message"] != null => jsonObject["message"]?.GetValue<string>() ?? string.Empty,
            IDictionary<string, object?> dictionary when dictionary.TryGetValue("error", out object? error) => Convert.ToString(error)
                ?? string.Empty,
            IDictionary<string, object?> dictionary when dictionary.TryGetValue("message", out object? message) => Convert.ToString(message)
                ?? string.Empty,
            _ => Convert.ToString(data) ?? string.Empty,
        };
    }

    private void OnClose(HcButton _)
    {
        if (_closeButton?.GetParent() == this)
        {
            RemoveChild(_closeButton);
        }

        _loginProvider?.CloseCaptcha();

        ShowScreen(SCREEN_LOGIN);
    }

    private string ResolveLocalization(string key)
    {
        if (_localization != null)
        {
            string localized = _localization.GetLocalization(key);

            if (!string.IsNullOrWhiteSpace(localized) && !string.Equals(localized, key, StringComparison.Ordinal))
            {
                return localized;
            }
        }

        string value = GetProperty(key);

        return string.IsNullOrWhiteSpace(value) ? key : value;
    }

    private string Interpolate(string value)
    {
        string result = value;

        for (int i = 0;
             i < 3;
             i++)
        {
            int start = result.IndexOf("${", StringComparison.Ordinal);

            if (start < 0)
            {
                break;
            }

            int end = result.IndexOf('}', start + 2);

            if (end < 0)
            {
                break;
            }

            string token = result[(start + 2)..end];
            string replacement = GetProperty(token);

            if (string.IsNullOrWhiteSpace(replacement))
            {
                replacement = ResolveLocalization(token);
            }

            result = result[..start] + replacement + result[(end + 1)..];
        }

        return result;
    }

    private static Dictionary<string, object?> Clone(Dictionary<string, object?> source)
    {
        Dictionary<string, object?> clone = new(StringComparer.Ordinal);

        foreach (KeyValuePair<string, object?> pair in source)
        {
            if (pair.Value is Dictionary<string, object?> nested)
            {
                clone[pair.Key] = Clone(nested);
            }
            else
            {
                clone[pair.Key] = pair.Value;
            }
        }

        return clone;
    }

    private sealed class EmptyCaptchaListener(LoginFlow owner) : ICaptchaListener
    {
        public void HandleCaptchaError()
        {
            owner.ShowCaptchaError();
        }

        public void HandleCaptchaResult(string token)
        {
            _ = token;
            owner.CaptchaReady();
        }

        public string GetProperty(string key, Dictionary<string, string>? parameters = null)
        {
            return owner.GetProperty(key, parameters);
        }
    }
}
