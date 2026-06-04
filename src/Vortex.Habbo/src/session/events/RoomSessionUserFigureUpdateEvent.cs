// @see com.sulake.habbo.session.events.RoomSessionUserFigureUpdateEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionUserFigureUpdateEvent
public class RoomSessionUserFigureUpdateEvent : RoomSessionEvent
{
    public const string USER_FIGURE = "RSUBE_FIGURE";

    /// @see RoomSessionUserFigureUpdateEvent.as::RoomSessionUserFigureUpdateEvent
    public RoomSessionUserFigureUpdateEvent(IRoomSession session, int userId, string figure,
        string gender, string customInfo, int achievementScore)
        : base(USER_FIGURE, session)
    {
        this.userId = userId;
        this.figure = figure;
        this.gender = gender;
        this.customInfo = customInfo;
        this.achievementScore = achievementScore;
    }

    /// @see RoomSessionUserFigureUpdateEvent.as::get userId
    public int userId { get; }

    /// @see RoomSessionUserFigureUpdateEvent.as::get figure
    public string figure { get; }

    /// @see RoomSessionUserFigureUpdateEvent.as::get gender
    public string gender { get; }

    /// @see RoomSessionUserFigureUpdateEvent.as::get customInfo
    public string customInfo { get; }

    /// @see RoomSessionUserFigureUpdateEvent.as::get achievementScore
    public int achievementScore { get; }
}
