// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/SsoTokenAvailableEvent.as

namespace Vortex.Habbo.Communication.Login;

/// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/SsoTokenAvailableEvent.as
public sealed class SsoTokenAvailableEvent(string eventType, string ssoToken)
{
    public const string SSO_TOKEN_AVAILABLE = "SSO_TOKEN_AVAILABLE";

    public string EventType { get; } = eventType;

    public string SsoToken { get; } = ssoToken;
}
