// @see core/window/utils/MouseCursorControl.as

using Godot;

namespace Vortex.Core.Window.Utils;

/// <summary>
/// Static cursor management. Maps cursor type IDs to Godot DisplayServer cursor shapes.
/// Godot adaptation: uses DisplayServer.CursorSetShape() instead of Flash Mouse.cursor.
/// </summary>
/// @see core/window/utils/MouseCursorControl.as
public static class MouseCursorControl
{
    private static uint _type;
    private static bool _visible = true;
    private static bool _dirty = true;

    public static bool disposed { get; private set; }

    /// @see MouseCursorControl.as::get/set type
    public static uint type
    {
        get => _type;
        set
        {
            if (_type == value)
            {
                return;
            }

            _type = value;
            _dirty = true;
        }
    }

    /// @see MouseCursorControl.as::get/set visible
    public static bool visible
    {
        get => _visible;
        set
        {
            _visible = value;

            if (_visible)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            else
            {
                Input.MouseMode = Input.MouseModeEnum.Hidden;
            }
        }
    }

    /// @see MouseCursorControl.as::dispose
    public static void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
        }
    }

    /// @see MouseCursorControl.as::change
    public static void Change()
    {
        if (!_dirty)
        {
            return;
        }

        switch (_type)
        {
            case Enum.Class3549.DEFAULT:
            case Enum.Class3549.ARROW:
                DisplayServer.CursorSetShape(DisplayServer.CursorShape.Arrow);
                break;
            case Enum.Class3549.ARROW_LINK:
                DisplayServer.CursorSetShape(DisplayServer.CursorShape.PointingHand);
                break;
            case Enum.Class3549.ARROW_BUSY:
                DisplayServer.CursorSetShape(DisplayServer.CursorShape.Busy);
                break;
            case Enum.Class3549.ARROW_HELP:
                DisplayServer.CursorSetShape(DisplayServer.CursorShape.Help);
                break;
            case Enum.Class3549.DRAG:
            case Enum.Class3549.MOVE:
                DisplayServer.CursorSetShape(DisplayServer.CursorShape.Move);
                break;
            case Enum.Class3549.MOVE_VERTICAL:
                DisplayServer.CursorSetShape(DisplayServer.CursorShape.Vsize);
                break;
            case Enum.Class3549.MOVE_HORIZONTAL:
                DisplayServer.CursorSetShape(DisplayServer.CursorShape.Hsize);
                break;
            case Enum.Class3549.RESIZE_VERTICAL:
                DisplayServer.CursorSetShape(DisplayServer.CursorShape.Vsize);
                break;
            case Enum.Class3549.RESIZE_HORIZONTAL:
                DisplayServer.CursorSetShape(DisplayServer.CursorShape.Hsize);
                break;
            case Enum.Class3549.RESIZE_DIAGONAL:
                DisplayServer.CursorSetShape(DisplayServer.CursorShape.Fdiagsize);
                break;
            case Enum.Class3549.DENIED:
                DisplayServer.CursorSetShape(DisplayServer.CursorShape.Forbidden);
                break;
            case Enum.Class3549.BUSY:
                DisplayServer.CursorSetShape(DisplayServer.CursorShape.Busy);
                break;
            case Enum.Class3549.NONE:
                DisplayServer.CursorSetShape(DisplayServer.CursorShape.Arrow);
                Input.MouseMode = Input.MouseModeEnum.Hidden;
                break;
        }

        _dirty = false;
    }
}
