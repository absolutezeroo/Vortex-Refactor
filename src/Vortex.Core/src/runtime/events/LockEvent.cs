// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/LockEvent.as

namespace Vortex.Core.Runtime.Events;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/LockEvent.as
public sealed class LockEvent
{
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/LockEvent.as::LockEvent
    public LockEvent(string param1, IUnknown param2)
    {
        type = param1;
        unknown = param2;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/LockEvent.as::type
    public string type { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/LockEvent.as::unknown
    public IUnknown unknown { get; }
}
