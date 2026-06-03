// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/IUIContext.as

using Godot;

namespace Vortex.OnBoardingHcUi;

/// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/IUIContext.as
public interface IUIContext
{
    /// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/IUIContext.as::stage
    Viewport? Stage { get; }

    /// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/IUIContext.as::debugText
    Label? DebugText { get; }
}
