// @see WIN63-202407091256-704579380-Source-main/core/window/IInputEventTracker.as

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window;

/// @see WIN63-202407091256-704579380-Source-main/core/window/IInputEventTracker.as
public interface IInputEventTracker
{
    /// @see WIN63-202407091256-704579380-Source-main/core/window/IInputEventTracker.as::eventReceived
    void EventReceived(WindowEvent param1, IWindow? param2);
}
