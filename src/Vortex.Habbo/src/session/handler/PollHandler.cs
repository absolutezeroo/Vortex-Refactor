// @see com.sulake.habbo.session.handler.PollHandler

using Vortex.Core.Communication.Connection;
using Vortex.Habbo.Session.Events;

namespace Vortex.Habbo.Session.Handler;

/// @see com.sulake.habbo.session.handler.PollHandler
public class PollHandler : BaseHandler
{
    /// @see PollHandler.as::PollHandler
    public PollHandler(IConnection? connection, IRoomHandlerListener listener)
        : base(connection, listener)
    {
        if (connection == null)
            return;
        // TODO(as3-port): PollContentsEvent not yet ported
        // TODO(as3-port): PollOfferEvent not yet ported
        // TODO(as3-port): PollErrorEvent not yet ported
    }

    // TODO(as3-port): onPollOfferEvent — parser: PollOfferParser (id, headline, summary)
    //   → dispatch RoomSessionPollEvent("RSPE_POLL_OFFER", session, id) with summary set

    // TODO(as3-port): onPollErrorEvent — parser: _SafeStr_58
    //   → dispatch RoomSessionPollEvent("RSPE_POLL_ERROR", session, -1) headline="???" summary="???"

    // TODO(as3-port): onPollContentsEvent — parser: PollContentsParser (id, startMessage, endMessage, numQuestions, questionArray, npsPoll)
    //   → dispatch RoomSessionPollEvent("RSPE_POLL_CONTENT", session, id) with all fields set
}
