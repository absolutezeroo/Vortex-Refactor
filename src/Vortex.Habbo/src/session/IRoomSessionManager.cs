// @see com.sulake.habbo.session.IRoomSessionManager

using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.IRoomSessionManager
public interface IRoomSessionManager : IUnknown
{
    /// @see IRoomSessionManager.as::get events
    EventDispatcherWrapper? events { get; }

    /// @see IRoomSessionManager.as::get sessionStarting
    bool sessionStarting { get; }

    /// @see IRoomSessionManager.as::gotoRoom
    bool GotoRoom(int roomId, string password = "", string doorbell = "");

    /// @see IRoomSessionManager.as::gotoRoomNetwork
    bool GotoRoomNetwork(int roomId, int networkId);

    /// @see IRoomSessionManager.as::startSession
    bool StartSession(IRoomSession session);

    /// @see IRoomSessionManager.as::getSession
    IRoomSession? GetSession(int roomId);

    /// @see IRoomSessionManager.as::startGameSession
    void StartGameSession();

    /// @see IRoomSessionManager.as::disposeGameSession
    void DisposeGameSession();

    /// @see IRoomSessionManager.as::disposeSession
    void DisposeSession(int roomId, bool sendDisconnect = true);
}
