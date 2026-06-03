// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentInterfaceQueue.as

using System;

namespace Vortex.Core.Runtime;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentInterfaceQueue.as
internal class ComponentInterfaceQueue : IDisposable
{
    private List<Action<IID, IUnknown?>>? _receivers;

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentInterfaceQueue.as::ComponentInterfaceQueue
    public ComponentInterfaceQueue(IID param1)
    {
        identifier = param1;
        _receivers = [];
        disposed = false;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentInterfaceQueue.as::get identifier
    public IID? identifier { get; private set; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentInterfaceQueue.as::get disposed
    public bool disposed { get; private set; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentInterfaceQueue.as::get receivers
    public IList<Action<IID, IUnknown?>> receivers => _receivers ?? (IList<Action<IID, IUnknown?>>)Array.Empty<Action<IID, IUnknown?>>();

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentInterfaceQueue.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        identifier = null;
        _receivers?.Clear();
        _receivers = null;
    }
}
