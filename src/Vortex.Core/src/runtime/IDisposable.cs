// @see WIN63-202407091256-704579380-Source-main/core/runtime/IDisposable.as

namespace Vortex.Core.Runtime;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/IDisposable.as
public interface IDisposable
{
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IDisposable.as::dispose
    void Dispose();

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IDisposable.as::get disposed
    bool disposed { get; }
}
