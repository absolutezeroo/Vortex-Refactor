// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ICaptchaListener.as

namespace Vortex.Habbo.Communication.Login;

/// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ICaptchaListener.as
public interface ICaptchaListener
{
    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ICaptchaListener.as::handleCaptchaError
    void HandleCaptchaError();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ICaptchaListener.as::handleCaptchaResult
    void HandleCaptchaResult(string token);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ICaptchaListener.as::getProperty
    string GetProperty(string key, Dictionary<string, string>? parameters = null);
}
