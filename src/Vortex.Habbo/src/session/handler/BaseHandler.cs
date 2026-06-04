// @see com.sulake.habbo.session.handler.BaseHandler

using Vortex.Core.Communication.Connection;

namespace Vortex.Habbo.Session.Handler;

/// @see com.sulake.habbo.session.handler.BaseHandler
public class BaseHandler
{
    private IConnection? _connection;
    private IRoomHandlerListener? _listener;

    /// @see BaseHandler.as::BaseHandler
    public BaseHandler(IConnection? connection, IRoomHandlerListener listener)
    {
        _connection = connection;
        _listener = listener;
    }

    /// @see BaseHandler.as::dispose
    public virtual void Dispose()
    {
        _connection = null;
        _listener = null;
        disposed = true;
    }

    /// @see BaseHandler.as::get disposed
    public bool disposed { get; private set; } = false;

    /// @see BaseHandler.as::get connection
    public IConnection? connection => _connection;

    /// @see BaseHandler.as::get listener
    public IRoomHandlerListener? listener => _listener;

    /// @see BaseHandler.as::_SafeStr_586 (current tracked room id, updated by RoomSessionManager.UpdateHandlers)
    public int currentRoomId { get; set; }
}
