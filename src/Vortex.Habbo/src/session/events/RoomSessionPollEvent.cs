// @see com.sulake.habbo.session.events.RoomSessionPollEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionPollEvent
public class RoomSessionPollEvent : RoomSessionEvent
{
    public const string OFFER = "RSPE_POLL_OFFER";
    public const string ERROR = "RSPE_POLL_ERROR";
    public const string CONTENT = "RSPE_POLL_CONTENT";

    /// @see RoomSessionPollEvent.as::RoomSessionPollEvent
    public RoomSessionPollEvent(string type, IRoomSession session, int id)
        : base(type, session)
    {
        this.id = id;
    }

    /// @see RoomSessionPollEvent.as::get id
    public int id { get; }

    /// @see RoomSessionPollEvent.as::get/set headline
    public string? headline { get; set; }

    /// @see RoomSessionPollEvent.as::get/set summary
    public string? summary { get; set; }

    /// @see RoomSessionPollEvent.as::get/set numQuestions
    public int numQuestions { get; set; } = 0;

    /// @see RoomSessionPollEvent.as::get/set startMessage
    public string startMessage { get; set; } = "";

    /// @see RoomSessionPollEvent.as::get/set endMessage
    public string endMessage { get; set; } = "";

    /// @see RoomSessionPollEvent.as::get/set questionArray
    public object[]? questionArray { get; set; }

    /// @see RoomSessionPollEvent.as::get/set npsPoll
    public bool npsPoll { get; set; } = false;
}
