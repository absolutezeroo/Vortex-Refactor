// @see com.sulake.habbo.session.events.RoomSessionWordQuizEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionWordQuizEvent
public class RoomSessionWordQuizEvent : RoomSessionEvent
{
    public const string NEW_QUESTION = "RWPUW_NEW_QUESTION";
    public const string FINISHED = "RWPUW_QUESION_FINSIHED"; // intentional typo from source
    public const string QUESTION_ANSWERED = "RWPUW_QUESTION_ANSWERED";

    /// @see RoomSessionWordQuizEvent.as::RoomSessionWordQuizEvent
    public RoomSessionWordQuizEvent(string type, IRoomSession session, int id = -1)
        : base(type, session)
    {
        this.id = id;
    }

    /// @see RoomSessionWordQuizEvent.as::get id
    public int id { get; }

    /// @see RoomSessionWordQuizEvent.as::get/set pollType
    public string? pollType { get; set; }

    /// @see RoomSessionWordQuizEvent.as::get/set pollId
    public int pollId { get; set; } = -1;

    /// @see RoomSessionWordQuizEvent.as::get/set questionId
    public int questionId { get; set; } = -1;

    /// @see RoomSessionWordQuizEvent.as::get/set duration
    public int duration { get; set; } = -1;

    /// @see RoomSessionWordQuizEvent.as::get/set question (AS3 Dictionary → Dictionary<object,object>)
    public Dictionary<object, object>? question { get; set; }

    /// @see RoomSessionWordQuizEvent.as::get/set userId
    public int userId { get; set; } = -1;

    /// @see RoomSessionWordQuizEvent.as::get/set value
    public string? value { get; set; }

    /// @see RoomSessionWordQuizEvent.as::get/set answerCounts (AS3 Map → Dictionary<string,int>)
    public Dictionary<string, int>? answerCounts { get; set; }
}
