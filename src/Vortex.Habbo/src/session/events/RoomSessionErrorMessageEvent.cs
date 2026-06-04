// @see com.sulake.habbo.session.events.RoomSessionErrorMessageEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionErrorMessageEvent
public class RoomSessionErrorMessageEvent : RoomSessionEvent
{
    public const string KICKED_BY_OWNER = "RSEME_KICKED";
    public const string PETS_FORBIDDEN_IN_HOTEL = "RSEME_PETS_FORBIDDEN_IN_HOTEL";
    public const string PETS_FORBIDDEN_IN_FLAT = "RSEME_PETS_FORBIDDEN_IN_FLAT";
    public const string MAX_NUMBER_OF_PETS = "RSEME_MAX_PETS";
    public const string MAX_NUMBER_OF_OWN_PETS = "RSEME_MAX_NUMBER_OF_OWN_PETS";
    public const string NO_FREE_TILES_FOR_PET = "RSEME_NO_FREE_TILES_FOR_PET";
    public const string SELECTED_TILE_NOT_FREE_FOR_PET = "RSEME_SELECTED_TILE_NOT_FREE_FOR_PET";
    public const string BOTS_FORBIDDEN_IN_HOTEL = "RSEME_BOTS_FORBIDDEN_IN_HOTEL";
    public const string BOTS_FORBIDDEN_IN_FLAT = "RSEME_BOTS_FORBIDDEN_IN_FLAT";
    public const string BOT_LIMIT_REACHED = "RSEME_BOT_LIMIT_REACHED";
    public const string SELECTED_TILE_NOT_FREE_FOR_BOT = "RSEME_SELECTED_TILE_NOT_FREE_FOR_BOT";
    public const string BOT_NAME_NOT_ACCEPTED = "RSEME_BOT_NAME_NOT_ACCEPTED";

    /// @see RoomSessionErrorMessageEvent.as::RoomSessionErrorMessageEvent
    public RoomSessionErrorMessageEvent(string type, IRoomSession session, string? message = null)
        : base(type, session)
    {
        this.message = message;
    }

    /// @see RoomSessionErrorMessageEvent.as::get message
    public string? message { get; }
}
