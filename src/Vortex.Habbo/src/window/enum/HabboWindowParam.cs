// @see WIN63-202407091256-704579380-Source-main/habbo/window/enum/HabboWindowParam.as

namespace Vortex.Habbo.Window.Enum;

/// @see WIN63-202407091256-704579380-Source-main/habbo/window/enum/HabboWindowParam.as
public static class HabboWindowParam
{
    public const uint NULL = 0;
    public const uint INPUT_EVENT_PROCESSOR = 1;
    public const uint ROUTE_INPUT_EVENTS_TO_PARENT = 3;
    public const uint INPUT_EVENT_PROCESSOR_AND_ROUTE = 5;
    public const uint INTERNAL_EVENT_HANDLING = 9;
    public const uint USE_PARENT_GRAPHIC_CONTEXT = 16;
    public const uint BOUND_TO_PARENT_RECT = 32;

    // Horizontal relative scale flags
    public const uint RELATIVE_HORIZONTAL_SCALE_FIXED = 0;
    public const uint RELATIVE_HORIZONTAL_SCALE_MOVE = 64;
    public const uint RELATIVE_HORIZONTAL_SCALE_STRECH = 128;
    public const uint RELATIVE_HORIZONTAL_SCALE_CENTER = 192;
    public const uint RELATIVE_HORIZONTAL_SCALE_MASK = 192;

    // Vertical relative scale flags
    public const uint RELATIVE_VERTICAL_SCALE_FIXED = 0;
    public const uint RELATIVE_VERTICAL_SCALE_MOVE = 1024;
    public const uint RELATIVE_VERTICAL_SCALE_STRECH = 2048;
    public const uint RELATIVE_VERTICAL_SCALE_CENTER = 3072;
    public const uint RELATIVE_VERTICAL_SCALE_MASK = 3072;

    // Combined scale flags
    public const uint RELATIVE_SCALE_FIXED = 0;
    public const uint RELATIVE_SCALE_MOVE = 1088;
    public const uint RELATIVE_SCALE_STRECH = 2176;
    public const uint RELATIVE_SCALE_CENTER = 3264;

    // Horizontal resize alignment
    public const uint ON_RESIZE_ALIGN_LEFT = 0;
    public const uint ON_RESIZE_ALIGN_RIGHT = 262144;
    public const uint ON_RESIZE_ALIGN_CENTER = 786432;

    // Vertical resize alignment
    public const uint ON_RESIZE_ALIGN_TOP = 0;
    public const uint ON_RESIZE_ALIGN_BOTTOM = 1048576;
    public const uint ON_RESIZE_ALIGN_VERTICAL_CENTER = 3145728;

    // Resize to accommodate
    public const uint RESIZE_WIDTH_TO_ACCOMMODATE_CHILDREN = 131072;
    public const uint RESIZE_TO_ACCOMMODATE_CHILDREN = 147456;

    // Mouse dragging
    public const uint MOUSE_DRAGGING_TARGET = 32768;
    public const uint MOUSE_DRAGGING_TRIGGER = 257;
    public const uint DRAGGABLE_WITH_MOUSE = 33025;

    // Mouse scaling
    public const uint HORIZONTAL_MOUSE_SCALING_TRIGGER = 4096;
    public const uint VERTICAL_MOUSE_SCALING_TRIGGER = 8192;
    public const uint MOUSE_SCALING_TRIGGER = 12288;
    public const uint MOUSE_SCALING_TRIGGER_MASK = 12288;
    public const uint MOUSE_SCALING_TARGET = 65536;
    public const uint SCALABLE_WITH_MOUSE = 77824;

    // Composite presets
    public const uint PARENT_WINDOW = 1;
    public const uint CHILD_WINDOW = 33;
    public const uint EMBEDDED_CONTROLLER = 51;

    public const uint FORCE_CLIPPING = 1073741824;
}
