// @see core/window/utils/IInputProcessorRoot.as

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/IInputProcessorRoot.as
public interface IInputProcessorRoot
{
    /// @see core/window/utils/IInputProcessorRoot.as::process
    void Process(WindowMouseEvent evt);
}
