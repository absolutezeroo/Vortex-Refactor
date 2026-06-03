// @see core/window/enum/class_3459.as
// @see PRODUCTION: core/window/enum/WindowParam.as

namespace Vortex.Core.Window.Enum;

/// <summary>
/// Window parameter bit flags controlling behavior, scaling, mouse interaction, and alignment.
/// </summary>
/// @see core/window/enum/class_3459.as
public static class Class3459
{
    public const uint WINDOW_PARAM_NULL = 0;
    public const uint WINDOW_PARAM_INPUT_EVENT_PROCESSOR = 1;        // 1 << 0
    public const uint WINDOW_PARAM_ROUTE_INPUT_EVENTS_TO_PARENT = 3; // INPUT_EVENT_PROCESSOR | (1 << 1)
    public const uint WINDOW_PARAM_OBSERVE_PARENT_INPUT_EVENTS = 5;  // INPUT_EVENT_PROCESSOR | (1 << 2)
    public const uint WINDOW_PARAM_INTERNAL_EVENT_HANDLING = 9;      // INPUT_EVENT_PROCESSOR | (1 << 3)
    public const uint WINDOW_PARAM_USE_PARENT_GRAPHIC_CONTEXT = 16;  // 1 << 4
    public const uint WINDOW_PARAM_BOUND_TO_PARENT_RECT = 32;        // 1 << 5

    // Horizontal relative scaling
    public const uint WINDOW_PARAM_RELATIVE_HORIZONTAL_SCALE_FIXED = 0;
    public const uint WINDOW_PARAM_RELATIVE_HORIZONTAL_SCALE_MOVE = 64;     // 1 << 6
    public const uint WINDOW_PARAM_RELATIVE_HORIZONTAL_SCALE_STRETCH = 128; // 1 << 7
    public const uint WINDOW_PARAM_RELATIVE_HORIZONTAL_SCALE_CENTER = 192;  // MOVE | STRETCH
    public const uint WINDOW_PARAM_RELATIVE_HORIZONTAL_SCALE_MASK = 192;

    // Vertical relative scaling
    public const uint WINDOW_PARAM_RELATIVE_VERTICAL_SCALE_FIXED = 0;
    public const uint WINDOW_PARAM_RELATIVE_VERTICAL_SCALE_MOVE = 1024;    // 1 << 10
    public const uint WINDOW_PARAM_RELATIVE_VERTICAL_SCALE_STRETCH = 2048; // 1 << 11
    public const uint WINDOW_PARAM_RELATIVE_VERTICAL_SCALE_CENTER = 3072;  // MOVE | STRETCH
    public const uint WINDOW_PARAM_RELATIVE_VERTICAL_SCALE_MASK = 3072;

    // Combined relative scaling
    public const uint WINDOW_PARAM_RELATIVE_SCALE_FIXED = 0;
    public const uint WINDOW_PARAM_RELATIVE_SCALE_MOVE = 1088;    // H_MOVE | V_MOVE
    public const uint WINDOW_PARAM_RELATIVE_SCALE_STRETCH = 2176; // H_STRETCH | V_STRETCH
    public const uint WINDOW_PARAM_RELATIVE_SCALE_CENTER = 3264;  // H_CENTER | V_CENTER

    // Accommodation
    public const uint WINDOW_PARAM_EXPAND_TO_ACCOMMODATE_CHILDREN = 131072; // 1 << 17
    public const uint WINDOW_PARAM_RESIZE_TO_ACCOMMODATE_CHILDREN = 147456; // (1 << 14) | EXPAND

    // Mouse dragging
    public const uint WINDOW_PARAM_MOUSE_DRAGGING_TARGET = 32768; // 1 << 15
    public const uint WINDOW_PARAM_MOUSE_DRAGGING_TRIGGER = 257;  // (1 << 8) | INPUT_EVENT_PROCESSOR
    public const uint WINDOW_PARAM_DRAGGABLE_WITH_MOUSE = 33025;  // TARGET | TRIGGER

    // Mouse scaling
    public const uint WINDOW_PARAM_MOUSE_SCALING_TARGET = 65536;            // 1 << 16
    public const uint WINDOW_PARAM_HORIZONTAL_MOUSE_SCALING_TRIGGER = 4096; // 1 << 12
    public const uint WINDOW_PARAM_VERTICAL_MOUSE_SCALING_TRIGGER = 8192;   // 1 << 13
    public const uint WINDOW_PARAM_MOUSE_SCALING_TRIGGER = 12288;           // H | V
    public const uint WINDOW_PARAM_MOUSE_SCALING_TRIGGER_MASK = 12288;
    public const uint WINDOW_PARAM_SCALABLE_WITH_MOUSE = 77824; // TARGET | TRIGGER

    // Accommodate alignment
    public const uint WINDOW_PARAM_ON_ACCOMMODATE_ALIGN_LEFT = 0;
    public const uint WINDOW_PARAM_ON_ACCOMMODATE_ALIGN_RIGHT = 262144;  // 1 << 18
    public const uint WINDOW_PARAM_ON_ACCOMMODATE_ALIGN_CENTER = 786432; // (1 << 19) | RIGHT
    public const uint WINDOW_PARAM_ON_ACCOMMODATE_ALIGN_TOP = 0;
    public const uint WINDOW_PARAM_ON_ACCOMMODATE_ALIGN_BOTTOM = 1048576;  // 1 << 20
    public const uint WINDOW_PARAM_ON_ACCOMMODATE_ALIGN_MIDDLE = 3145728;  // (1 << 21) | BOTTOM
    public const uint WINDOW_PARAM_ON_ACCOMMODATE_ALIGN_CONTENT = 3932160; // CENTER | MIDDLE
    public const uint WINDOW_PARAM_ON_ACCOMMODATE_ALIGN_MASK = 3932160;

    // Resize alignment (aliases to accommodate alignment)
    public const uint WINDOW_PARAM_ON_RESIZE_ALIGN_LEFT = 0;
    public const uint WINDOW_PARAM_ON_RESIZE_ALIGN_CENTER = 786432;
    public const uint WINDOW_PARAM_ON_RESIZE_ALIGN_RIGHT = 262144;
    public const uint WINDOW_PARAM_ON_RESIZE_ALIGN_TOP = 0;
    public const uint WINDOW_PARAM_ON_RESIZE_ALIGN_MIDDLE = 3145728;
    public const uint WINDOW_PARAM_ON_RESIZE_ALIGN_BOTTOM = 1048576;
    public const uint WINDOW_PARAM_ON_RESIZE_ALIGN_HORIZONTAL = 786432;
    public const uint WINDOW_PARAM_ON_RESIZE_ALIGN_VERTICAL = 3145728;

    // Resize reflection
    public const uint WINDOW_PARAM_REFLECT_HORIZONTAL_RESIZE_TO_PARENT = 4194304; // 1 << 22
    public const uint WINDOW_PARAM_REFLECT_VERTICAL_RESIZE_TO_PARENT = 8388608;   // 1 << 23
    public const uint WINDOW_PARAM_REFLECT_RESIZE_TO_PARENT = 12582912;           // H | V

    // Composite param presets
    public const uint WINDOW_PARAM_PARENT_WINDOW = 1;        // INPUT_EVENT_PROCESSOR | H_FIXED | V_FIXED
    public const uint WINDOW_PARAM_CHILD_WINDOW = 33;        // INPUT_EVENT_PROCESSOR | BOUND_TO_PARENT
    public const uint WINDOW_PARAM_EMBEDDED_CONTROLLER = 51; // INPUT_EVENT_PROCESSOR | BOUND | ROUTE | USE_PARENT_GC

    public const uint WINDOW_PARAM_FORCE_CLIPPING = 1073741824;  // 1 << 30
    public const uint WINDOW_PARAM_INHERIT_CAPTION = 2147483648; // 1 << 31
}
