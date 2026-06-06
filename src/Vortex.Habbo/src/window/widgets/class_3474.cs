// @see habbo/window/widgets/class_3474.as

using System;
using System.Collections.Frozen;

namespace Vortex.Habbo.Window.Widgets;

/// <summary>
/// Static widget type registry. Maps type name strings to widget classes.
/// Used by IWidgetFactory.CreateWidget() for dispatch.
/// </summary>
/// @see habbo/window/widgets/class_3474.as
public class Class3474
{
    /// @see class_3474.as::var_1728 — widget type name → Type mapping
    public static readonly FrozenDictionary<string, Type> WidgetTypeMap =
        new Dictionary<string, Type>(StringComparer.Ordinal)
        {
            ["avatar_image"] = typeof(AvatarImageWidget),
            ["badge_image"] = typeof(BadgeImageWidget),
            ["balloon"] = typeof(BalloonWidget),
            ["countdown"] = typeof(CountdownWidget),
            ["furniture_image"] = typeof(FurnitureImageWidget),
            ["hover_bitmap"] = typeof(HoverBitmapWidget),
            ["illumina_border"] = typeof(IlluminaBorderWidget),
            ["illumina_chat_bubble"] = typeof(IlluminaChatBubbleWidget),
            ["illumina_input"] = typeof(IlluminaInputWidget),
            ["progress_indicator"] = typeof(ProgressIndicatorWidget),
            ["limited_item_overlay_grid"] = typeof(LimitedItemGridOverlayWidget),
            ["limited_item_overlay_preview"] = typeof(LimitedItemPreviewOverlayWidget),
            ["limited_item_overlay_supply"] = typeof(LimitedItemSupplyLeftOverlayWidget),
            ["rarity_item_overlay_grid"] = typeof(RarityItemGridOverlayWidget),
            ["rarity_item_overlay_preview"] = typeof(RarityItemPreviewOverlayWidget),
            ["separator"] = typeof(SeparatorWidget),
            ["updating_timestamp"] = typeof(UpdatingTimeStampWidget),
            ["running_number"] = typeof(RunningNumberWidget),
            ["pet_image"] = typeof(PetImageWidget),
            ["room_previewer"] = typeof(RoomPreviewerWidget),
            ["pixel_limit"] = typeof(PixelLimitWidget),
            ["room_thumbnail"] = typeof(RoomThumbnailWidget),
            ["room_user_count"] = typeof(RoomUserCountWidget),
        }.ToFrozenDictionary(StringComparer.Ordinal);

    /// @see class_3474.as::WIDGET_TYPES — sorted array of all registered widget type keys
    public static readonly string[] WIDGET_TYPES;

    static Class3474()
    {
        List<string> keys = new(WidgetTypeMap.Keys);

        keys.Sort(StringComparer.Ordinal);

        WIDGET_TYPES = keys.ToArray();
    }
}
