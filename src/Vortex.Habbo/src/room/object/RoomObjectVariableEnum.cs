namespace Vortex.Habbo.Room.Object;

/// <summary>
/// Room object model variable key constants for all object types.
/// </summary>
/// @see com.sulake.habbo.room.object.RoomObjectVariableEnum
public static class RoomObjectVariableEnum
{
    // General
    public const string OBJECT_ROOM_ID = "object_room_id";
    public const string OBJECT_ACCURATE_Z_VALUE = "object_accurate_z_value";
    public const string IMAGE_QUERY_SCALE = "image_query_scale";

    // Avatar
    public const string AVATAR_FIGURE = "figure";
    public const string RACE = "race";
    public const string AVATAR_GENDER = "gender";
    public const string AVATAR_OWN_USER = "own_user";
    public const string AVATAR_TALK = "figure_talk";
    public const string FIGURE_DANCE = "figure_dance";
    public const string FIGURE_SLEEP = "figure_sleep";
    public const string FIGURE_BLINK = "figure_blink";
    public const string FIGURE_EFFECT = "figure_effect";
    public const string FIGURE_CARRY_OBJECT = "figure_carry_object";
    public const string FIGURE_USE_OBJECT = "figure_use_object";
    public const string AVATAR_GESTURE = "figure_gesture";
    public const string AVATAR_POSTURE = "figure_posture";
    public const string AVATAR_POSTURE_PARAMETER = "figure_posture_parameter";
    public const string STD = "std";
    public const string AVATAR_SIGN = "figure_sign";
    public const string FIGURE_FLAT_CONTROL = "figure_flat_control";
    public const string AVATAR_IS_TYPING = "figure_is_typing";
    public const string FIGURE_EXPRESSION = "figure_expression";
    public const string AVATAR_EXPERIENCE_TIMESTAMP = "figure_experience_timestamp";
    public const string FIGURE_GAINED_EXPERIENCE = "figure_gained_experience";
    public const string FIGURE_NUMBER_VALUE = "figure_number_value";
    public const string AVATAR_MOUSE_HIGHLIGHT = "figure_highlight";
    public const string FIGURE_HIGHLIGHT_ENABLE = "figure_highlight_enable";
    public const string AVATAR_WIRED_VARIABLE_HOLDER_HIGHLIGHT = "figure_highlight_variable_holder";
    public const string FIGURE_CAN_STAND_UP = "figure_can_stand_up";
    public const string FIGURE_VERTICAL_OFFSET = "figure_vertical_offset";
    public const string AVATAR_IS_PLAYING_GAME = "figure_is_playing_game";
    public const string FIGURE_IS_MUTED = "figure_is_muted";
    public const string AVATAR_GUIDE_STATUS = "figure_guide_status";
    public const string HEAD_DIRECTION = "head_direction";

    // Pet
    public const string PET_PALETTE_INDEX = "pet_palette_index";
    public const string PET_COLOR = "pet_color";
    public const string PET_HEAD_ONLY = "pet_head_only";
    public const string PET_CUSTOM_LAYER_IDS = "pet_custom_layer_ids";
    public const string PET_CUSTOM_PART_IDS = "pet_custom_part_ids";
    public const string PET_CUSTOM_PALETTE_IDS = "pet_custom_palette_ids";
    public const string PET_IS_RIDING = "pet_is_riding";
    public const string PET_TYPE = "pet_type";
    public const string PET_ALLOWED_DIRECTIONS = "pet_allowed_directions";

    // Furniture — general
    public const string FURNITURE_REAL_ROOM_OBJECT = "furniture_real_room_object";
    public const string FURNITURE_COLOR = "furniture_color";
    public const string FURNITURE_TYPE_ID = "furniture_type_id";
    public const string FURNITURE_SIZE_X = "furniture_size_x";
    public const string FURNITURE_SIZE_Y = "furniture_size_y";
    public const string FURNITURE_SIZE_Z = "furniture_size_z";
    public const string FURNITURE_CENTER_X = "furniture_center_x";
    public const string FURNITURE_CENTER_Y = "furniture_center_y";
    public const string FURNITURE_CENTER_Z = "furniture_center_z";
    public const string FURNITURE_LIFT_AMOUNT = "furniture_lift_amount";
    public const string FURNITURE_ALLOWED_DIRECTIONS = "furniture_allowed_directions";
    public const string FURNITURE_DATA = "furniture_data";
    public const string FURNITURE_DATA_FORMAT = "furniture_data_format";
    public const string FURNITURE_EXTRAS = "furniture_extras";
    public const string FURNITURE_ITEMDATA = "furniture_itemdata";
    public const string FURNITURE_ALPHA_MULTIPLIER = "furniture_alpha_multiplier";
    public const string FURNITURE_EXPIRY_TIME = "furniture_expiry_time";
    public const string FURNITURE_EXPIRY_TIMESTAMP = "furniture_expirty_timestamp";
    public const string FURNITURE_STATE_UPDATE_TIME = "furniture_state_update_time";
    public const string FURNITURE_AUTOMATIC_STATE_INDEX = "furniture_automatic_state_index";
    public const string FURNITURE_UNIQUE_SERIAL_NUMBER = "furniture_unique_serial_number";
    public const string FURNITURE_UNIQUE_EDITION_SIZE = "furniture_unique_edition_size";
    public const string FURNITURE_CUSTOM_VARIABLES = "furniture_custom_variables";
    public const string FURNITURE_IS_VARIABLE_HEIGHT = "furniture_is_variable_height";
    public const string FURNITURE_ALWAYS_STACKABLE = "furniture_always_stackable";

    // Furniture — specific types
    public const string FURNITURE_CREDIT_VALUE = "furniture_credit_value";
    public const string FURNITURE_PLANETSYSTEM_DATA = "furniture_planetsystem_data";
    public const string FURNITURE_FIREWORKS_DATA = "furniture_fireworks_data";
    public const string FURNITURE_USES_PLANE_MASK = "furniture_uses_plane_mask";
    public const string FURNITURE_PLANE_MASK_TYPE = "furniture_plane_mask_type";
    public const string FURNITURE_AD_URL = "furniture_ad_url";
    public const string FURNITURE_BRANDING_IMAGE_STATUS = "furniture_branding_image_status";
    public const string FURNITURE_BRANDING_IMAGE_URL = "furniture_branding_image_url";
    public const string FURNITURE_BRANDING_URL = "furniture_branding_url";
    public const string FURNITURE_BRANDING_OFFSET_X = "furniture_branding_offset_x";
    public const string FURNITURE_BRANDING_OFFSET_Y = "furniture_branding_offset_y";
    public const string FURNITURE_BRANDING_OFFSET_Z = "furniture_branding_offset_z";
    public const string FURNITURE_SELECTION_DISABLE = "furniture_selection_disable";
    public const string FURNITURE_CLOTHING_GIRL = "furniture_clothing_girl";
    public const string FURNITURE_CLOTHING_BOY = "furniture_clothing_boy";
    public const string FURNITURE_MANNEQUIN_NAME = "furniture_mannequin_name";
    public const string FURNITURE_MANNEQUIN_GENDER = "furniture_mannequin_gender";
    public const string FURNITURE_MANNEQUIN_FIGURE = "furniture_mannequin_figure";
    public const string FURNITURE_IS_STICKIE = "furniture_is_stickie";
    public const string FURNITURE_USAGE_POLICY = "furniture_usage_policy";
    public const string FURNITURE_OWNER_ID = "furniture_owner_id";
    public const string FURNITURE_OWNER_NAME = "furniture_owner_name";
    public const string FURNITURE_GUILD_CUSTOMIZED_GUILD_ID = "furniture_guild_customized_guild_id";
    public const string FURNITURE_GUILD_CUSTOMIZED_COLOR_1 = "furniture_guild_customized_color_1";
    public const string FURNITURE_GUILD_CUSTOMIZED_COLOR_2 = "furniture_guild_customized_color_2";
    public const string FURNITURE_GUILD_CUSTOMIZED_BADGE_ASSET_NAME = "furniture_guild_customized_asset_name";
    public const string FURNITURE_PURCHASER_NAME = "furniture_purchaser_name";
    public const string FURNITURE_PURCHASER_FIGURE = "furniture_purchaser_figure";
    public const string FURNITURE_TRUSTED_SENDER = "furniture_trusted_sender";
    public const string FURNITURE_DISABLE_PICKING_ANIMATION = "furniture_disable_picking_animation";
    public const string FURNITURE_VOTE_COUNTER_COUNT = "furniture_vote_counter_count";
    public const string FURNITURE_VOTE_MAJORITY_RESULT = "furniture_vote_majority_result";
    public const string FURNITURE_SOUNDBLOCK_RELATIVE_ANIMATION_SPEED = "furniture_soundblock_relative_animation_speed";
    public const string FURNITURE_ROOM_BACKGROUND_COLOR_HUE = "furniture_room_background_color_hue";
    public const string FURNITURE_ROOM_BACKGROUND_COLOR_SATURATION = "furniture_room_background_color_saturation";
    public const string FURNITURE_ROOM_BACKGROUND_COLOR_LIGHTNESS = "furniture_room_background_color_lightness";
    public const string FURNITURE_AREA_HIDE_ROOT_X = "furniture_area_hide_root_x";
    public const string FURNITURE_AREA_HIDE_ROOT_Y = "furniture_area_hide_root_y";
    public const string FURNITURE_AREA_HIDE_WIDTH = "furniture_area_hide_width";
    public const string FURNITURE_AREA_HIDE_LENGTH = "furniture_area_hide_length";
    public const string FURNITURE_AREA_HIDE_INVISIBILITY = "furniture_area_hide_invisibility";
    public const string FURNITURE_AREA_HIDE_WALL_ITEMS = "furniture_area_hide_wallitems";
    public const string FURNITURE_AREA_HIDE_INVERT = "furniture_area_hide_invert";
    public const string FURNITURE_BADGE_ASSET_NAME = "furniture_badge_asset_name";
    public const string FURNITURE_BADGE_VISIBLE_IN_STATE = "furniture_badge_visible_in_state";
    public const string FURNITURE_BADGE_IMAGE_STATUS = "furniture_badge_image_status";
    public const string FURNITURE_FRIENDFURNI_ENGRAVING_TYPE = "furniture_friendfurni_engraving_type";
    public const string FURNITURE_HIGHSCORE_SCORE_TYPE = "furniture_highscore_score_type";
    public const string FURNITURE_HIGHSCORE_CLEAR_TYPE = "furniture_highscore_clear_type";
    public const string FURNITURE_HIGHSCORE_DATA_ENTRY_COUNT = "furniture_highscore_data_entry_count";
    public const string FURNITURE_HIGHSCORE_DATA_ENTRY_BASE_USERS = "furniture_highscore_data_entry_base_users_";
    public const string FURNITURE_HIGHSCORE_DATA_ENTRY_BASE_SCORE = "furniture_highscore_data_entry_base_score_";
    public const string FURNITURE_INTERNAL_LINK = "furniture_internal_link";

    // Session
    public const string SESSION_CURRENT_USER_ID = "session_current_user_id";
    public const string SESSION_URL_PREFIX = "session_url_prefix";

    // Room
    public const string ROOM_PLANE_XML = "room_plane_xml";
    public const string ROOM_PLANE_MASK_XML = "room_plane_mask_xml";
    public const string ROOM_FLOOR_TYPE = "room_floor_type";
    public const string ROOM_WALL_TYPE = "room_wall_type";
    public const string ROOM_LANDSCAPE_TYPE = "room_landscape_type";
    public const string ROOM_WALL_THICKNESS_MULTIPLIER = "room_wall_thickness";
    public const string ROOM_FLOOR_THICKNESS_MULTIPLIER = "room_floor_thickness";
    public const string ROOM_FLOOR_HOLE_UPDATE_TIME = "room_floor_hole_update_time";
    public const string ROOM_FLOOR_VISIBILITY = "room_floor_visibility";
    public const string ROOM_WALL_VISIBILITY = "room_wall_visibility";
    public const string ROOM_LANDSCAPE_VISIBILITY = "room_landscape_visibility";
    public const string ROOM_DOOR_X = "room_door_x";
    public const string ROOM_DOOR_Y = "room_door_y";
    public const string ROOM_DOOR_Z = "room_door_z";
    public const string ROOM_DOOR_DIR = "room_door_dir";
    public const string ROOM_BACKGROUND_COLOR = "room_background_color";
    public const string ROOM_COLORIZE_BG_ONLY = "room_colorize_bg_only";
    public const string ROOM_RANDOM_SEED = "room_random_seed";
    public const string ROOM_WORLD_TYPE = "room_world_type";
    public const string ROOM_AD_SPRITE_TAG = "billboard";
    public const string ROOM_AD_IMAGE_ASSET = "room_ad_image_asset";
    public const string ROOM_AD_CLICK_URL = "room_ad_click_url";
    public const string ROOM_AD_WAITING = "room_ad_waiting";
    public const string ROOM_AD_WARNING_IMAGE_LEFT = "room_ad_warning_image_left";
    public const string ROOM_AD_WARNING_IMAGE_RIGHT = "room_ad_warning_image_right";
    public const string ROOM_SELECTED_X = "room_selected_x";
    public const string ROOM_SELECTED_Y = "room_selected_y";
    public const string ROOM_SELECTED_Z = "room_selected_z";
    public const string ROOM_SELECTED_PLANE = "room_selected_plane";

    // Tile cursor
    public const string TILE_CURSOR_HEIGHT = "tile_cursor_height";
}
