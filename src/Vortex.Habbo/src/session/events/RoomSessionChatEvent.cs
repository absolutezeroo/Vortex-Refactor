// @see com.sulake.habbo.session.events.RoomSessionChatEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionChatEvent
public class RoomSessionChatEvent : RoomSessionEvent
{
    public const string ROOM_SESSION_CHAT_EVENT = "RSCE_CHAT_EVENT";
    public const string ROOM_SESSION_FLOODCONTROL_EVENT = "RSCE_FLOOD_EVENT";

    public const int CHAT_TYPE_SPEAK = 0;
    public const int CHAT_TYPE_WHISPER = 1;
    public const int CHAT_TYPE_SHOUT = 2;
    public const int CHAT_TYPE_RESPECT = 3;
    public const int CHAT_TYPE_PETRESPECT = 4;
    public const int CHAT_TYPE_HAND_ITEM_RECEIVED = 5;
    public const int CHAT_TYPE_PETTREAT = 6;
    public const int CHAT_TYPE_PETREVIVE = 7;
    public const int CHAT_TYPE_PET_REBREED_FERTILIZE = 8;
    public const int CHAT_TYPE_PET_SPEED_FERTILIZE = 9;
    public const int CHAT_TYPE_MUTE_REMAINING = 10;

    /// @see RoomSessionChatEvent.as::RoomSessionChatEvent
    public RoomSessionChatEvent(string type, IRoomSession session, int userId, string text,
        int chatType = 0, int style = 0,
        List<(string Url, string Text, bool TrustedUrl)>? links = null,
        int extraParam = -1)
        : base(type, session, false)
    {
        this.userId = userId;
        this.text = text;
        this.chatType = chatType;
        this.style = style;
        this.links = links;
        this.extraParam = extraParam;
    }

    /// @see RoomSessionChatEvent.as::get userId
    public int userId { get; }

    /// @see RoomSessionChatEvent.as::get text
    public string text { get; }

    /// @see RoomSessionChatEvent.as::get chatType
    public int chatType { get; }

    /// @see RoomSessionChatEvent.as::get style
    public int style { get; }

    /// @see RoomSessionChatEvent.as::get links
    public List<(string Url, string Text, bool TrustedUrl)>? links { get; }

    /// @see RoomSessionChatEvent.as::get extraParam
    public int extraParam { get; }
}
