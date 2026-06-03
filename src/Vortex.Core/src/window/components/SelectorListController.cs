// @see core/window/components/SelectorListController.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/SelectorListController.as
public class SelectorListController : SelectorController, ISelectorListWindow
{
    private int _spacing;
    private bool _vertical;
    private bool _updatingRegion;

    /// @see SelectorListController.as::SelectorListController (default)
    public SelectorListController() : base() { }

    /// @see SelectorListController.as::SelectorListController (name + rect)
    public SelectorListController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see SelectorListController.as::SelectorListController (full AS3 11-param signature)
    public SelectorListController
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
        // @see SelectorListController.as — disable reorder on select
        _reorderOnSelect = false;
    }

    /// @see SelectorListController.as::get spacing
    public int Spacing
    {
        get => _spacing;
        set
        {
            _spacing = value;
            UpdateSelectableRegion();
        }
    }

    /// @see SelectorListController.as::get vertical
    public bool IsVertical
    {
        get => _vertical;
        set
        {
            _vertical = value;
            UpdateSelectableRegion();
        }
    }

    /// @see SelectorListController.as::set properties
    public override void ApplyProperties(PropertyStruct[] properties)
    {
        foreach (PropertyStruct prop in properties)
        {
            switch (prop.key)
            {
                case "spacing":
                    if (prop.value is int s)
                    {
                        Spacing = s;
                    }
                    break;
                case "vertical":
                    if (prop.value is bool v)
                    {
                        IsVertical = v;
                    }
                    break;
            }
        }

        base.ApplyProperties(properties);
    }

    /// @see SelectorListController.as::update
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        switch (param2.type)
        {
            case WindowEvent.WE_CHILD_ADDED:
            case WindowEvent.WE_CHILD_RESIZED:
            case WindowEvent.WE_CHILD_RELOCATED:
                UpdateSelectableRegion();
                return true;
        }

        return base.Update(param1, param2);
    }

    /// @see SelectorListController.as::updateSelectableRegion
    public void UpdateSelectableRegion()
    {
        // @see SelectorListController.as — re-entrancy guard
        if (_updatingRegion)
        {
            return;
        }

        _updatingRegion = true;

        float position = 0;

        for (int i = 0;
             i < numChildren;
             i++)
        {
            IWindow? child = GetChildAt(i);

            if (child is not { visible: true })
            {
                continue;
            }

            if (_vertical)
            {
                child.x = 0;
                child.y = position;
                position += child.height + _spacing;
            }
            else
            {
                child.x = position;
                child.y = 0;
                position += child.width + _spacing;
            }
        }

        _updatingRegion = false;
    }
}
