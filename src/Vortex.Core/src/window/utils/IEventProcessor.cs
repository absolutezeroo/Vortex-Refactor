// @see core/window/utils/IEventProcessor.as

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/IEventProcessor.as
public interface IEventProcessor
{
    /// @see core/window/utils/IEventProcessor.as::process
    void Process(EventProcessorState state, IEventQueue queue);
}
