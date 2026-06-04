// @see com.sulake.habbo.session.IRoomHandlerListener

using Vortex.Core.Runtime.Events;

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.IRoomHandlerListener
public interface IRoomHandlerListener
{
    /// @see IRoomHandlerListener.as::sessionUpdate
    void SessionUpdate(int roomId, string state);

    /// @see IRoomHandlerListener.as::sessionReinitialize
    void SessionReinitialize(int oldRoomId, int newRoomId);

    /// @see IRoomHandlerListener.as::getSession
    IRoomSession? GetSession(int roomId);

    /// @see IRoomHandlerListener.as::get events
    EventDispatcherWrapper events { get; }
}
