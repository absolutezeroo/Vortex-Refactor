// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginProvider.as

using System;

namespace Vortex.Habbo.Communication.Login;

/// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginProvider.as
public interface ILoginProvider
{
    event Action<SsoTokenAvailableEvent>? SsoTokenAvailable;

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginProvider.as::closeCaptcha
    void CloseCaptcha();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginProvider.as::init
    void Init(IHabboCommunicationManager? communication);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginProvider.as::loginWithCredentials
    void LoginWithCredentials(string userName, string password, int mode = 0);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginProvider.as::loginWithCredentialsWeb
    void LoginWithCredentialsWeb(string uniqueId);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginProvider.as::selectAvatar
    void SelectAvatar(int id);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/login/ILoginProvider.as::selectAvatarUniqueid
    void SelectAvatarUniqueid(string uniqueId);
}
