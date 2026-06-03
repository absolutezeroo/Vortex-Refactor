// @see core/window/services/class_3694.as

using System;

using Vortex.Core.Window.Components;

namespace Vortex.Core.Window.Services;

/// @see core/window/services/class_3694.as
public interface IFocusManagerService : IDisposable
{
    /// @see core/window/services/class_3694.as::registerFocusWindow
    void RegisterFocusWindow(IFocusWindow? args);

    /// @see core/window/services/class_3694.as::removeFocusWindow
    void RemoveFocusWindow(IFocusWindow? args);
}
