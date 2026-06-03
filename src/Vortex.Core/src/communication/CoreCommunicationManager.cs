// @see WIN63-202407091256-704579380-Source-main/core/communication/CoreCommunicationManager.as

using Vortex.Core.Communication.Connection;
using Vortex.Core.Runtime;

namespace Vortex.Core.Communication;

/// @see WIN63-202407091256-704579380-Source-main/core/communication/CoreCommunicationManager.as
public partial class CoreCommunicationManager : Component, ICoreCommunicationManager, IUpdateReceiver
{
    private List<IConnection> _connections;

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/CoreCommunicationManager.as::CoreCommunicationManager
    public CoreCommunicationManager(IContext param1, uint param2 = 0, object? param3 = null)
        : base(param1, param2, param3)
    {
        _connections = [];

        RegisterUpdateReceiver(this, 0);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/CoreCommunicationManager.as::createConnection
    public IConnection CreateConnection(IConnectionStateListener? connectionParams = null)
    {
        SocketConnection connection = new(this, connectionParams);

        _connections.Add(connection);

        return connection;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/CoreCommunicationManager.as::update
    public void Update(uint deltaTime)
    {
        int i = 0;

        while (i < _connections.Count)
        {
            IConnection connection = _connections[i];

            connection.ProcessReceivedData();

            if (disposed)
            {
                return;
            }

            if (connection.disposed)
            {
                _connections.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/CoreCommunicationManager.as::dispose
    public override void Dispose()
    {
        if (disposed)
        {
            return;
        }

        RemoveUpdateReceiver(this);

        foreach (IConnection connection in _connections)
        {
            connection.Dispose();
        }

        _connections.Clear();
        _connections = [];

        base.Dispose();
    }
}
