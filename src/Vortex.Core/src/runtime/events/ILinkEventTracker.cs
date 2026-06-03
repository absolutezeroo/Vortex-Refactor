// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/ILinkEventTracker.as

namespace Vortex.Core.Runtime.Events;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/ILinkEventTracker.as
public interface ILinkEventTracker
{
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/ILinkEventTracker.as::get linkPattern
    string linkPattern { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/ILinkEventTracker.as::linkReceived
    void LinkReceived(string param1);
}
