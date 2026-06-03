// @see habbo/window/utils/IModalDialog.as

using Vortex.Core.Runtime;

namespace Vortex.Habbo.Window.Utils;

/// @see habbo/window/utils/IModalDialog.as
public interface IModalDialog : IDisposable
{
    /// @see habbo/window/utils/IModalDialog.as::get rootWindow
    object? RootWindow();

    /// @see habbo/window/utils/IModalDialog.as::get background
    object? Background();
}
