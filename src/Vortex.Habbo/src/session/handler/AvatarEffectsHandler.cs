// @see com.sulake.habbo.session.handler.AvatarEffectsHandler

using Vortex.Core.Communication.Connection;

namespace Vortex.Habbo.Session.Handler;

/// @see com.sulake.habbo.session.handler.AvatarEffectsHandler
public class AvatarEffectsHandler : BaseHandler
{
    /// @see AvatarEffectsHandler.as::AvatarEffectsHandler
    /// Note: AS3 never wires the AvatarEffectsMessageEvent — onAvatarEffects is unreachable dead code in source
    public AvatarEffectsHandler(IConnection? connection, IRoomHandlerListener listener)
        : base(connection, listener)
    {
    }
}
