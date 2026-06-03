// @see com.sulake.habbo.session.IRoomSession

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.IRoomSession
public interface IRoomSession
{
    int roomId { get; }
    string password { get; }
    bool isSpectator { get; }
}
