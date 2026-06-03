// @see WIN63-202111081545-75921380-Source-main/src/IHabboLoadingScreen.as

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/IHabboLoadingScreen.as
public interface IHabboLoadingScreen : IDisposable
{
    /// @see WIN63-202111081545-75921380-Source-main/src/IHabboLoadingScreen.as::updateLoadingBar
    void UpdateLoadingBar(double param1);
}
