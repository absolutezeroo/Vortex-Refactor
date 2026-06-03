// @see WIN63-202407091256-704579380-Source-main/core/runtime/IUnknown.as

using System;

namespace Vortex.Core.Runtime;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/IUnknown.as
public interface IUnknown : IDisposable
{
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IUnknown.as::queueInterface
    IUnknown? QueueInterface(IID param1, Action<IID, IUnknown?>? param2 = null);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IUnknown.as::release
    uint Release(IID param1);
}
