// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/window/events/WindowEvent.as

using System;

namespace Vortex.Core.Window.Events;

/// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/window/events/WindowEvent.as
public class WindowEvent
{
    public const string UNKNOWN = "";
    public const string WE_DESTROY = "WE_DESTROY";
    public const string WE_DESTROYED = "WE_DESTROYED";
    public const string WE_OPEN = "WE_OPEN";
    public const string WE_OPENED = "WE_OPENED";
    public const string WE_CLOSE = "WE_CLOSE";
    public const string WE_CLOSED = "WE_CLOSED";
    public const string WE_FOCUS = "WE_FOCUS";
    public const string WE_FOCUSED = "WE_FOCUSED";
    public const string WE_UNFOCUS = "WE_UNFOCUS";
    public const string WE_UNFOCUSED = "WE_UNFOCUSED";
    public const string WE_ACTIVATE = "WE_ACTIVATE";
    public const string WE_ACTIVATED = "WE_ACTIVATED";
    public const string WE_DEACTIVATE = "WE_DEACTIVATE";
    public const string WE_DEACTIVATED = "WE_DEACTIVATED";
    public const string WE_SELECT = "WE_SELECT";
    public const string WE_SELECTED = "WE_SELECTED";
    public const string WE_UNSELECT = "WE_UNSELECT";
    public const string WE_UNSELECTED = "WE_UNSELECTED";
    public const string WE_LOCK = "WE_LOCK";
    public const string WE_LOCKED = "WE_LOCKED";
    public const string WE_UNLOCK = "WE_UNLOCK";
    public const string WE_UNLOCKED = "WE_UNLOCKED";
    public const string WE_ENABLE = "WE_ENABLE";
    public const string WE_ENABLED = "WE_ENABLED";
    public const string WE_DISABLE = "WE_DISABLE";
    public const string WE_DISABLED = "WE_DISABLED";
    public const string WE_RELOCATE = "WE_RELOCATE";
    public const string WE_RELOCATED = "WE_RELOCATED";
    public const string WE_RESIZE = "WE_RESIZE";
    public const string WE_RESIZED = "WE_RESIZED";
    public const string WE_MINIMIZE = "WE_MINIMIZE";
    public const string WE_MINIMIZED = "WE_MINIMIZED";
    public const string WE_MAXIMIZE = "WE_MAXIMIZE";
    public const string WE_MAXIMIZED = "WE_MAXIMIZED";
    public const string WE_RESTORE = "WE_RESTORE";
    public const string WE_RESTORED = "WE_RESTORED";
    public const string WE_CHILD_ADDED = "WE_CHILD_ADDED";
    public const string WE_CHILD_REMOVED = "WE_CHILD_REMOVED";
    public const string WE_CHILD_RELOCATED = "WE_CHILD_RELOCATED";
    public const string WE_CHILD_RESIZED = "WE_CHILD_RESIZED";
    public const string WE_CHILD_ACTIVATED = "WE_CHILD_ACTIVATED";
    public const string WE_CHILD_VISIBILITY = "WE_CHILD_VISIBILITY";
    public const string WE_PARENT_ADDED = "WE_PARENT_ADDED";
    public const string WE_PARENT_REMOVED = "WE_PARENT_REMOVED";
    public const string WE_PARENT_RELOCATED = "WE_PARENT_RELOCATED";
    public const string WE_PARENT_RESIZED = "WE_PARENT_RESIZED";
    public const string WE_PARENT_ACTIVATED = "WE_PARENT_ACTIVATED";
    public const string WE_OK = "WE_OK";
    public const string WE_CANCEL = "WE_CANCEL";
    public const string WE_CHANGE = "WE_CHANGE";
    public const string WE_SCROLL = "WE_SCROLL";
    public const string WE_EXPANDED = "WE_EXPANDED";
    public const string WE_COLLAPSE = "WE_COLLAPSE";

    // Legacy aliases for existing references
    public const string WINDOW_EVENT_CHANGE = WE_CHANGE;
    public const string WINDOW_EVENT_RESTORE = WE_RESTORE;
    public const string WINDOW_EVENT_FOCUS = WE_FOCUS;
    public const string WINDOW_EVENT_UNFOCUS = WE_UNFOCUS;
    public const string WINDOW_EVENT_LOCK = WE_LOCK;
    public const string WINDOW_EVENT_UNLOCK = WE_UNLOCK;

    private bool _prevented;

    /// @see WindowEvent.as::WindowEvent
    public WindowEvent(string param1, IWindow? param2, IWindow? param3, bool param4 = false)
    {
        type = param1;
        window = param2;
        related = param3;
        cancelable = param4;
    }

    public string type { get; protected set; }
    public IWindow? window { get; protected set; }
    public IWindow? related { get; protected set; }
    public bool cancelable { get; protected set; }

    /// @see WindowEvent.as::allocate
    public static WindowEvent Allocate(string param1, IWindow? param2, IWindow? param3, bool param4 = false)
    {
        return new WindowEvent(param1, param2, param3, param4);
    }

    /// @see WindowEvent.as::clone
    public virtual WindowEvent Clone()
    {
        return new WindowEvent(type, window, related, cancelable);
    }

    /// @see WindowEvent.as::preventDefault — delegates to preventWindowOperation in AS3
    public void PreventDefault()
    {
        PreventWindowOperation();
    }

    /// @see WindowEvent.as::preventWindowOperation
    public void PreventWindowOperation()
    {
        if (!cancelable)
        {
            throw new InvalidOperationException("Attempted to prevent window operation that is not cancelable.");
        }

        _prevented = true;
    }

    /// @see WindowEvent.as::isDefaultPrevented
    public bool IsDefaultPrevented()
    {
        return _prevented;
    }

    /// @see WindowEvent.as::isWindowOperationPrevented — same flag as isDefaultPrevented in AS3
    public bool IsWindowOperationPrevented()
    {
        return _prevented;
    }

    /// @see WindowEvent.as::stopPropagation
    public void StopPropagation()
    {
        _prevented = true;
    }

    /// @see WindowEvent.as::stopImmediatePropagation
    public void StopImmediatePropagation()
    {
        _prevented = true;
    }

    public override string ToString()
    {
        return $"WindowEvent {{ type: {type}, cancelable: {cancelable}, window: {window?.name} }}";
    }
}
