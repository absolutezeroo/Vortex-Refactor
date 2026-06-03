// @see WIN63-202111081545-75921380-Source-main/src/login/ILoginContext.as

using Vortex.Habbo.Communication.Login;
using Vortex.OnBoardingHcUi;

namespace Vortex.Login;

/// @see WIN63-202111081545-75921380-Source-main/src/login/ILoginContext.as
public interface ILoginContext : IUIContext
{
    /// @see WIN63-202111081545-75921380-Source-main/src/login/ILoginContext.as::initLogin
    void InitLogin(string userName, string password);

    /// @see WIN63-202111081545-75921380-Source-main/src/login/ILoginContext.as::initLoginWithSsoToken
    void InitLoginWithSsoToken(string environmentId, string token);

    /// @see WIN63-202111081545-75921380-Source-main/src/login/ILoginContext.as::loginWithAvatar
    void LoginWithAvatar(AvatarData avatar);

    /// @see WIN63-202111081545-75921380-Source-main/src/login/ILoginContext.as::showScreen
    void ShowScreen(int screenId);
}
