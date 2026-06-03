namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomEngineToWidgetEvent
public class RoomEngineToWidgetEvent(string type,
    int roomId,
    int objectId,
    int category,
    string? widget = null)
    : RoomEngineObjectEvent(type, roomId, objectId, category)
{
    public const string REQUEST_OPEN_WIDGET = "RETWE_OPEN_WIDGET";
    public const string REQUEST_CLOSE_WIDGET = "RETWE_CLOSE_WIDGET";
    public const string REQUEST_OPEN_FURNI_CONTEXT_MENU = "RETWE_OPEN_FURNI_CONTEXT_MENU";
    public const string REQUEST_CLOSE_FURNI_CONTEXT_MENU = "RETWE_CLOSE_FURNI_CONTEXT_MENU";
    public const string REQUEST_PLACEHOLDER = "RETWE_REQUEST_PLACEHOLDER";
    public const string REQUEST_CREDITFURNI = "RETWE_REQUEST_CREDITFURNI";
    public const string REQUEST_STICKIE = "RETWE_REQUEST_STICKIE";
    public const string REQUEST_PRESENT = "RETWE_REQUEST_PRESENT";
    public const string REQUEST_TROPHY = "RETWE_REQUEST_TROPHY";
    public const string REQUEST_TEASER = "RETWE_REQUEST_TEASER";
    public const string REQUEST_ECOTRONBOX = "RETWE_REQUEST_ECOTRONBOX";
    public const string REQUEST_DIMMER = "RETWE_REQUEST_DIMMER";
    public const string REMOVE_DIMMER = "RETWE_REMOVE_DIMMER";
    public const string REQUEST_CLOTHING_CHANGE = "RETWE_REQUEST_CLOTHING_CHANGE";
    public const string REQUEST_PLAYLIST_EDITOR = "RETWE_REQUEST_PLAYLIST_EDITOR";
    public const string REQUEST_MANNEQUIN = "RETWE_REQUEST_MANNEQUIN";
    public const string REQUEST_MONSTERPLANT_SEED_PLANT_CONFIRMATION_DIALOG = "ROWRE_REQUEST_MONSTERPLANT_SEED_PLANT_CONFIRMATION_DIALOG";
    public const string REQUEST_PURCHASABLE_CLOTHING_CONFIRMATION_DIALOG = "ROWRE_REQUEST_PURCHASABLE_CLOTHING_CONFIRMATION_DIALOG";
    public const string REQUEST_BACKGROUND_COLOR = "RETWE_REQUEST_BACKGROUND_COLOR";
    public const string REQUEST_AREA_HIDE = "RETWE_REQUEST_AREA_HIDE";
    public const string REQUEST_MYSTERYBOX_OPEN_DIALOG = "RETWE_REQUEST_MYSTERYBOX_OPEN_DIALOG";
    public const string REQUEST_EFFECTBOX_OPEN_DIALOG = "RETWE_REQUEST_EFFECTBOX_OPEN_DIALOG";
    public const string REQUEST_MYSTERYTROPHY_OPEN_DIALOG = "RETWE_REQUEST_MYSTERYTROPHY_OPEN_DIALOG";
    public const string REQUEST_ACHIEVEMENT_RESOLUTION_ENGRAVING = "RETWE_REQUEST_ACHIEVEMENT_RESOLUTION_ENGRAVING";
    public const string REQUEST_ACHIEVEMENT_RESOLUTION_FAILED = "RETWE_REQUEST_ACHIEVEMENT_RESOLUTION_FAILED";
    public const string REQUEST_FRIEND_FURNITURE_CONFIRM = "RETWE_REQUEST_FRIEND_FURNITURE_CONFIRM";
    public const string REQUEST_FRIEND_FURNITURE_ENGRAVING = "RETWE_REQUEST_FRIEND_FURNITURE_ENGRAVING";
    public const string REQUEST_BADGE_DISPLAY_ENGRAVING = "RETWE_REQUEST_BADGE_DISPLAY_ENGRAVING";
    public const string REQUEST_HIGH_SCORE_DISPLAY = "RETWE_REQUEST_HIGH_SCORE_DISPLAY";
    public const string REQUEST_HIDE_HIGH_SCORE_DISPLAY = "RETWE_REQUEST_HIDE_HIGH_SCORE_DISPLAY";
    public const string REQUEST_INTERNAL_LINK = "RETWE_REQUEST_INTERNAL_LINK";
    public const string REQUEST_ROOM_LINK = "RETWE_REQUEST_ROOM_LINK";

    /// <summary>
    /// AS3: var_837 serves both widget and contextMenu getters (same backing field).
    /// </summary>
    public string? Widget { get; } = widget;

    public string? ContextMenu => Widget;
}
