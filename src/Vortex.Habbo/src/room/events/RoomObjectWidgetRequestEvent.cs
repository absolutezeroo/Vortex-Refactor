namespace Vortex.Habbo.Room.Events;

using Vortex.Room.Events;
using Vortex.Room.Object;

/// @see com.sulake.habbo.room.events.RoomObjectWidgetRequestEvent
public class RoomObjectWidgetRequestEvent(string type, IRoomObject? obj, string? widget = null) : RoomObjectEvent(type, obj)
{
    public const string OPEN_WIDGET = "ROWRE_OPEN_WIDGET";
    public const string CLOSE_WIDGET = "ROWRE_CLOSE_WIDGET";
    public const string OPEN_FURNI_CONTEXT_MENU = "ROWRE_OPEN_FURNI_CONTEXT_MENU";
    public const string CLOSE_FURNI_CONTEXT_MENU = "ROWRE_CLOSE_FURNI_CONTEXT_MENU";
    public const string PLACEHOLDER = "ROWRE_PLACEHOLDER";
    public const string CREDITFURNI = "ROWRE__CREDITFURNI";
    public const string STICKIE = "ROWRE__STICKIE";
    public const string PRESENT = "ROWRE_PRESENT";
    public const string TROPHY = "ROWRE_TROPHY";
    public const string TEASER = "ROWRE_TEASER";
    public const string ECOTRONBOX = "ROWRE_ECOTRONBOX";
    public const string DIMMER = "ROWRE_DIMMER";
    public const string WIDGET_REMOVE_DIMMER = "ROWRE_WIDGET_REMOVE_DIMMER";
    public const string CLOTHING_CHANGE = "ROWRE_CLOTHING_CHANGE";
    public const string JUKEBOX_PLAYLIST_EDITOR = "ROWRE_JUKEBOX_PLAYLIST_EDITOR";
    public const string MANNEQUIN = "ROWRE_MANNEQUIN";
    public const string PET_PRODUCT_MENU = "ROWRE_PET_PRODUCT_MENU";
    public const string GUILD_FURNI_CONTEXT_MENU = "ROWRE_GUILD_FURNI_CONTEXT_MENU";
    public const string MONSTERPLANT_SEED_PLANT_CONFIRMATION_DIALOG = "ROWRE_MONSTERPLANT_SEED_PLANT_CONFIRMATION_DIALOG";
    public const string PURCHASABLE_CLOTHING_CONFIRMATION_DIALOG = "ROWRE_PURCHASABLE_CLOTHING_CONFIRMATION_DIALOG";
    public const string BACKGROUND_COLOR = "ROWRE_BACKGROUND_COLOR";
    public const string MYSTERYBOX_OPEN_DIALOG = "ROWRE_MYSTERYBOX_OPEN_DIALOG";
    public const string EFFECTBOX_OPEN_DIALOG = "ROWRE_EFFECTBOX_OPEN_DIALOG";
    public const string MYSTERYTROPHY_OPEN_DIALOG = "ROWRE_MYSTERYTROPHY_OPEN_DIALOG";
    public const string ACHIEVEMENT_RESOLUTION_OPEN = "ROWRE_ACHIEVEMENT_RESOLUTION_OPEN";
    public const string ACHIEVEMENT_RESOLUTION_ENGRAVING = "ROWRE_ACHIEVEMENT_RESOLUTION_ENGRAVING";
    public const string ACHIEVEMENT_RESOLUTION_FAILED = "ROWRE_ACHIEVEMENT_RESOLUTION_FAILED";
    public const string FRIEND_FURNITURE_CONFIRM = "ROWRE_FRIEND_FURNITURE_CONFIRM";
    public const string FRIEND_FURNITURE_ENGRAVING = "ROWRE_FRIEND_FURNITURE_ENGRAVING";
    public const string BADGE_DISPLAY_ENGRAVING = "ROWRE_BADGE_DISPLAY_ENGRAVING";
    public const string HIGH_SCORE_DISPLAY = "ROWRE_HIGH_SCORE_DISPLAY";
    public const string HIDE_HIGH_SCORE_DISPLAY = "ROWRE_HIDE_HIGH_SCORE_DISPLAY";
    public const string INTERNAL_LINK = "ROWRE_INTERNAL_LINK";
    public const string ROOM_LINK = "ROWRE_ROOM_LINK";

    public string? Widget => widget;

    public string? ContextMenu => widget;
}
