// @see core/window/utils/class_3716.as

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/class_3716.as
public static class ParamCodeTable
{
    /// @see class_3716.as::fillTables
    public static void FillTables(Dictionary<string, uint> nameToCode, Dictionary<uint, string>? codeToName = null)
    {
        nameToCode["null"] = 0;
        nameToCode["parent_window"] = 1;
        nameToCode["input_event_processor"] = 1;
        nameToCode["observe_parent_input_events"] = 5;
        nameToCode["route_input_events_to_parent"] = 3;
        nameToCode["internal_event_handling"] = 9;
        nameToCode["use_parent_graphic_context"] = 16;
        nameToCode["bound_to_parent_rect"] = 32;
        nameToCode["child_window"] = 33;
        nameToCode["embedded_controller"] = 51;
        nameToCode["relative_horizontal_scale_fixed"] = 0;
        nameToCode["relative_horizontal_scale_move"] = 64;
        nameToCode["relative_horizontal_scale_strech"] = 128;
        nameToCode["relative_horizontal_scale_center"] = 192;
        nameToCode["relative_vertical_scale_fixed"] = 0;
        nameToCode["relative_vertical_scale_move"] = 1024;
        nameToCode["relative_vertical_scale_strech"] = 2048;
        nameToCode["relative_vertical_scale_center"] = 3072;
        nameToCode["relative_scale_fixed"] = 0;
        nameToCode["relative_scale_move"] = 1088;
        nameToCode["relative_scale_strech"] = 2176;
        nameToCode["relative_scale_center"] = 3264;
        nameToCode["horizontal_mouse_scaling_trigger"] = 4096;
        nameToCode["vertical_mouse_scaling_trigger"] = 8192;
        nameToCode["mouse_scaling_trigger"] = 12288;
        nameToCode["mouse_dragging_target"] = 32768;
        nameToCode["mouse_dragging_trigger"] = 257;
        nameToCode["draggable_with_mouse"] = 33025;
        nameToCode["mouse_scaling_target"] = 65536;
        nameToCode["expand_to_accommodate_children"] = 131072;
        nameToCode["resize_to_accommodate_children"] = 147456;
        nameToCode["scalable_with_mouse"] = 77824;
        nameToCode["on_resize_align_left"] = 0;
        nameToCode["on_resize_align_right"] = 262144;
        nameToCode["on_resize_align_center"] = 786432;
        nameToCode["on_resize_align_top"] = 0;
        nameToCode["on_resize_align_bottom"] = 1048576;
        nameToCode["on_resize_align_middle"] = 3145728;
        nameToCode["on_accommodate_align_left"] = 0;
        nameToCode["on_accommodate_align_right"] = 262144;
        nameToCode["on_accommodate_align_center"] = 786432;
        nameToCode["on_accommodate_align_top"] = 0;
        nameToCode["on_accommodate_align_bottom"] = 1048576;
        nameToCode["on_accommodate_align_middle"] = 3145728;
        nameToCode["reflect_horizontal_resize_to_parent"] = 4194304;
        nameToCode["reflect_vertical_resize_to_parent"] = 8388608;
        nameToCode["reflect_resize_to_parent"] = 12582912;
        nameToCode["force_clipping"] = 0x40000000;
        nameToCode["inherit_caption"] = 0x80000000;

        if (codeToName == null)
        {
            return;
        }

        foreach (KeyValuePair<string, uint> kvp in nameToCode)
        {
            codeToName.TryAdd(kvp.Value, kvp.Key);
        }
    }
}
