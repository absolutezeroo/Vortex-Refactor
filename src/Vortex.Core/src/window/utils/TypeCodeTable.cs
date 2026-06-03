// @see core/window/utils/class_3652.as

namespace Vortex.Core.Window.Utils;

/// <summary>
/// Static window component type table mapping type names to numeric IDs.
/// </summary>
/// @see core/window/utils/class_3652.as
public static class TypeCodeTable
{
    /// @see core/window/utils/class_3652.as::fillTables
    public static void FillTables(Dictionary<string, uint> nameToId, Dictionary<uint, string>? idToName = null)
    {
        nameToId["background"] = 2;
        nameToId["bitmap"] = 21;
        nameToId["border"] = 30;
        nameToId["boxsizer"] = 17;
        nameToId["border_notify"] = 33;
        nameToId["bubble"] = 45;
        nameToId["bubble_pointer_up"] = 46;
        nameToId["bubble_pointer_right"] = 47;
        nameToId["bubble_pointer_down"] = 48;
        nameToId["bubble_pointer_left"] = 49;
        nameToId["button"] = 60;
        nameToId["button_thick"] = 61;
        nameToId["button_icon"] = 62;
        nameToId["button_group_left"] = 67;
        nameToId["button_group_center"] = 68;
        nameToId["button_group_right"] = 69;
        nameToId["checkbox"] = 70;
        nameToId["closebutton"] = 72;
        nameToId["container"] = 4;
        nameToId["container_button"] = 41;
        nameToId["display_object_wrapper"] = 20;
        nameToId["dropmenu"] = 102;
        nameToId["dropmenu_item"] = 103;
        nameToId["droplist"] = 105;
        nameToId["droplist_item"] = 106;
        nameToId["formatted_text"] = 15;
        nameToId["frame"] = 35;
        nameToId["frame_notify"] = 38;
        nameToId["header"] = 6;
        nameToId["html"] = 11;
        nameToId["icon"] = 1;
        nameToId["iconbutton"] = 79;
        nameToId["itemgrid"] = 52;
        nameToId["itemgrid_horizontal"] = 54;
        nameToId["itemgrid_vertical"] = 53;
        nameToId["itemlist"] = 50;
        nameToId["itemlist_horizontal"] = 51;
        nameToId["itemlist_vertical"] = 50;
        nameToId["label"] = 12;
        nameToId["maximizebox"] = 74;
        nameToId["menu"] = 100;
        nameToId["menu_item"] = 101;
        nameToId["submenu"] = 104;
        nameToId["minimizebox"] = 73;
        nameToId["notify"] = 9;
        nameToId["null"] = 0;
        nameToId["password"] = 78;
        nameToId["radiobutton"] = 71;
        nameToId["region"] = 5;
        nameToId["restorebox"] = 75;
        nameToId["scaler"] = 120;
        nameToId["scaler_horizontal"] = 122;
        nameToId["scaler_vertical"] = 121;
        nameToId["scrollbar_horizontal"] = 130;
        nameToId["scrollbar_vertical"] = 131;
        nameToId["scrollbar_slider_button_up"] = 139;
        nameToId["scrollbar_slider_button_down"] = 137;
        nameToId["scrollbar_slider_button_left"] = 138;
        nameToId["scrollbar_slider_button_right"] = 136;
        nameToId["scrollbar_slider_bar_horizontal"] = 132;
        nameToId["scrollbar_slider_bar_vertical"] = 133;
        nameToId["scrollbar_slider_track_horizontal"] = 134;
        nameToId["scrollbar_slider_track_vertical"] = 135;
        nameToId["scrollable_itemlist"] = 55;
        nameToId["scrollable_itemlist_vertical"] = 56;
        nameToId["scrollable_itemgrid_vertical"] = 140;
        nameToId["scrollable_itemlist_horizontal"] = 57;
        nameToId["selector"] = 42;
        nameToId["selector_list"] = 43;
        nameToId["static_bitmap"] = 23;
        nameToId["tab_button"] = 93;
        nameToId["tab_container_button"] = 94;
        nameToId["tab_context"] = 91;
        nameToId["tab_content"] = 90;
        nameToId["tab_selector"] = 92;
        nameToId["text"] = 10;
        nameToId["input"] = 77;
        nameToId["link"] = 14;
        nameToId["toolbar"] = 7;
        nameToId["tooltip"] = 8;
        nameToId["widget"] = 16;

        if (idToName == null)
        {
            return;
        }

        foreach (KeyValuePair<string, uint> kvp in nameToId)
        {
            idToName[kvp.Value] = kvp.Key;
        }
    }
}
