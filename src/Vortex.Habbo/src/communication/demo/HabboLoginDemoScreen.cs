// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as

using System;

using Vortex.Core.Runtime.Events;
using Vortex.Habbo.Communication.Login;

namespace Vortex.Habbo.Communication.Demo;

/// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as
public sealed class HabboLoginDemoScreen : ILoginViewer, Core.Runtime.IDisposable
{
    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::INIT_LOGIN
    public const string INIT_LOGIN = "INIT_LOGIN";

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::AVATAR_SELECTED
    public const string AVATAR_SELECTED = "AVATAR_SELECTED";

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::ENVIRONMENT_SELECTED
    public const string ENVIRONMENT_SELECTED = "ENVIRONMENT_SELECTED";

    private readonly Func<string, string> _propertyResolver;

    private IReadOnlyList<AvatarData>? _avatars;

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::HabboLoginDemoScreen
    public HabboLoginDemoScreen(Func<string, string> propertyResolver)
    {
        _propertyResolver = propertyResolver;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::name
    public string LoginName { get; set; } = string.Empty;

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::password
    public string Password { get; set; } = string.Empty;

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::get avatarId
    public int AvatarId { get; private set; }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::get selectedAccount
    public AvatarData? SelectedAccount { get; private set; }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::get selectedEnvironment
    public string SelectedEnvironment { get; private set; } = string.Empty;

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::get useWebApi
    public bool UseWebApi { get; } = true;

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::get useExistingSession
    public static bool UseExistingSession => false;

    public bool disposed { get; private set; }

    public EventDispatcherWrapper Events { get; } = new();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        Events.Dispose();
        _avatars = null;
        SelectedAccount = null;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::closeLoginWindow
    public static void CloseLoginWindow()
    {
        Logger.Info("[HabboLoginDemoScreen] Login window closed.");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::showLoginScreen
    public void ShowLoginScreen() { }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::showRegistrationError
    public void ShowRegistrationError(object? data)
    {
        ShowErrorMessage("Registration error");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::showInvalidLoginError
    public void ShowInvalidLoginError(object? data)
    {
        ShowErrorMessage("Invalid login");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::nameCheckResponse
    public void NameCheckResponse(object? data, bool checkOnly) { }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::showCaptchaError
    public void ShowCaptchaError()
    {
        ShowErrorMessage("Captcha required, please add your IP to Housekeeping property to avoid this.");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::showAccountError
    public void ShowAccountError(object? data)
    {
        ShowErrorMessage("Error with account during login");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::showLoadingScreen
    public void ShowLoadingScreen()
    {
        Dispose();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::saveLooksError
    public void SaveLooksError(object? data)
    {
        ShowErrorMessage("Save looks error");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::showTOS
    public void ShowTOS()
    {
        ShowErrorMessage("Web-api wants to show Terms of Service");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::environmentReady
    public void EnvironmentReady()
    {
        Logger.Info("[HabboLoginDemoScreen] Environment ready for (" + SelectedEnvironment + ").");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::populateCharacterList
    public void PopulateCharacterList(IReadOnlyList<AvatarData> avatars)
    {
        _avatars = avatars;

        Logger.Info("[HabboLoginDemoScreen] Populated " + avatars.Count + " avatar(s).");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::showSelectAvatar
    public void ShowSelectAvatar(object? data) { }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::showPromoHabbos
    public void ShowPromoHabbos(string? xml) { }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::showSelectRoom
    public void ShowSelectRoom() { }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::createCaptchaView
    public ICaptchaView CreateCaptchaView()
    {
        // Godot/C# adaptation: AS3 returns undefined — return a no-op view.
        return new NullCaptchaView();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::captchaReady
    public void CaptchaReady() { }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::getProperty
    public string GetProperty(string key, Dictionary<string, string>? parameters = null)
    {
        return _propertyResolver(key);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::showDisconnectedWithText
    public void ShowDisconnectedWithText(int reason)
    {
        ShowErrorMessage("Hotel is closed");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::showDisconnected
    public void ShowDisconnected(int reason, string text)
    {
        ShowErrorMessage("Disconnected reason: " + text + " (" + reason + ")");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::showError
    public static void ShowError(int errorCode, int messageId, string text)
    {
        Logger.Warn("[HabboLoginDemoScreen] Received error: " + errorCode + " regarding message: " + messageId);
    }

    /// Selects an avatar by index from the populated avatar list.
    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::listEventHandler
    public void SelectAvatarAtIndex(int index)
    {
        if (_avatars == null || index < 0 || index >= _avatars.Count)
        {
            return;
        }

        if (UseWebApi)
        {
            SelectedAccount = _avatars[index];
        }
        else
        {
            AvatarId = _avatars[index].Id;
        }

        Events.DispatchEvent(AVATAR_SELECTED);
    }

    /// Updates the selected environment and dispatches the environment selected event.
    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::onEnvironmentSelected
    public void SetSelectedEnvironment(string environment)
    {
        SelectedEnvironment = environment;
        Events.DispatchEvent(ENVIRONMENT_SELECTED);
    }

    /// Triggers a login attempt and dispatches the init login event.
    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::windowEventProcessor
    public void SubmitLogin()
    {
        Events.DispatchEvent(INIT_LOGIN);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboLoginDemoScreen.as::showErrorMessage
    private static void ShowErrorMessage(string message)
    {
        Logger.Warn("[HabboLoginDemoScreen] Error: " + message);
    }

    /// Godot/C# adaptation: AS3 createCaptchaView returns undefined — provide a no-op implementation.
    private sealed class NullCaptchaView : ICaptchaView
    {
        public void Dispose() { }
    }
}
