// @see core/window/components/BubbleController.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/BubbleController.as
public class BubbleController : FrameController
{
    /// @see BubbleController.as — pointer element tag constants
    public const string TAG_POINTER_UP = "_POINTER_UP";
    public const string TAG_POINTER_DOWN = "_POINTER_DOWN";
    public const string TAG_POINTER_LEFT = "_POINTER_LEFT";
    public const string TAG_POINTER_RIGHT = "_POINTER_RIGHT";

    private string _direction = "down";
    private int _pointerOffset;

    /// @see BubbleController.as::BubbleController (default)
    public BubbleController() : base() { }

    /// @see BubbleController.as::BubbleController (name + rect)
    public BubbleController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see BubbleController.as::BubbleController (full AS3 11-param signature)
    public BubbleController
    (
        string param1,
        uint param2,
        uint param3,
        uint param4,
        IWindowContext param5,
        Rect2 param6,
        IWindow? param7,
        Action<WindowEvent, IWindow>? param8 = null,
        IList<object>? param9 = null,
        IList<string>? param10 = null,
        uint param11 = 0, string param12 = ""
    ) : base(param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12)
    {
    }

    /// @see BubbleController.as::get direction
    public string Direction
    {
        get => _direction;
        set
        {
            // @see BubbleController.as — hide previous pointer, show new one
            SetPointerVisibility(_direction, false);

            _direction = value;

            SetPointerVisibility(_direction, true);
        }
    }

    /// @see BubbleController.as::get pointerOffset
    public int PointerOffset
    {
        get => _pointerOffset;
        set
        {
            _pointerOffset = value;
            ApplyPointerOffset();
        }
    }

    /// @see BubbleController.as::set properties
    public override void ApplyProperties(PropertyStruct[] properties)
    {
        foreach (PropertyStruct prop in properties)
        {
            switch (prop.key)
            {
                case "direction":
                    if (prop.value is string dir)
                    {
                        Direction = dir;
                    }
                    break;
                case "pointer_offset":
                    if (prop.value is int po)
                    {
                        PointerOffset = po;
                    }
                    break;
            }
        }

        base.ApplyProperties(properties);
    }

    /// @see BubbleController.as::update
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        if (param2.type != WindowEvent.WE_RESIZED)
        {
            return base.Update(param1, param2);
        }

        // @see BubbleController.as — re-apply pointer offset on resize
        if (_pointerOffset != 0)
        {
            ApplyPointerOffset();
        }

        return true;

    }

    /// @see BubbleController.as — helper to set pointer element visibility by direction
    private void SetPointerVisibility(string direction, bool visible)
    {
        string tag = GetPointerTag(direction);
        IWindow? pointer = FindChildByTag(tag);

        if (pointer != null)
        {
            pointer.visible = visible;
        }
    }

    /// @see BubbleController.as — apply offset to pointer element
    private void ApplyPointerOffset()
    {
        string tag = GetPointerTag(_direction);
        IWindow? pointer = FindChildByTag(tag);

        if (pointer == null)
        {
            return;
        }

        switch (_direction)
        {
            case "up":
            case "down":
                pointer.x = (width / 2f) + _pointerOffset;
                break;
            case "left":
            case "right":
                pointer.y = (height / 2f) + _pointerOffset;
                break;
        }
    }

    private static string GetPointerTag(string direction)
    {
        return direction switch
        {
            "up" => TAG_POINTER_UP,
            "down" => TAG_POINTER_DOWN,
            "left" => TAG_POINTER_LEFT,
            "right" => TAG_POINTER_RIGHT,
            _ => TAG_POINTER_DOWN,
        };
    }
}
