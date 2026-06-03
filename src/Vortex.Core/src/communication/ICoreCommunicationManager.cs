using Vortex.Core.Communication.Connection;
using Vortex.Core.Runtime;

namespace Vortex.Core.Communication;

/// @see WIN63-202407091256-704579380-Source-main/core/communication/ICoreCommunicationManager.as
public interface ICoreCommunicationManager : IUnknown
{
    /// @see WIN63-202407091256-704579380-Source-main/core/communication/ICoreCommunicationManager.as::createConnection
    IConnection CreateConnection(IConnectionStateListener? param1 = null);
}
