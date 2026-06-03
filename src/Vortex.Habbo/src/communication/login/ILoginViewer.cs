// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as

namespace Vortex.Habbo.Communication.Login;

/// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as
public interface ILoginViewer
{
    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as::getProperty
    string GetProperty(string key, Dictionary<string, string>? parameters = null);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as::showLoginScreen
    void ShowLoginScreen();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as::showRegistrationError
    void ShowRegistrationError(object? data);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as::showInvalidLoginError
    void ShowInvalidLoginError(object? data);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as::nameCheckResponse
    void NameCheckResponse(object? data, bool checkOnly);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as::showAccountError
    void ShowAccountError(object? data);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as::showLoadingScreen
    void ShowLoadingScreen();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as::saveLooksError
    void SaveLooksError(object? data);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as::showTOS
    void ShowTOS();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as::environmentReady
    void EnvironmentReady();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as::populateCharacterList
    void PopulateCharacterList(IReadOnlyList<AvatarData> avatars);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as::showSelectAvatar
    void ShowSelectAvatar(object? data);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as::showPromoHabbos
    void ShowPromoHabbos(string? xml);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as::showSelectRoom
    void ShowSelectRoom();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as::showCaptchaError
    void ShowCaptchaError();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as::createCaptchaView
    ICaptchaView CreateCaptchaView();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginViewer.as::captchaReady
    void CaptchaReady();
}
