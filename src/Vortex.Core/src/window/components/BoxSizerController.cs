// @see core/window/components/BoxSizerController.as

using System;
using System.Text.RegularExpressions;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/BoxSizerController.as
public partial class BoxSizerController : ContainerController, IClass3651
{
    private int _spacing = 5;
    private int _horizontalPadding = 8;
    private int _verticalPadding = 8;
    private bool _vertical;
    private bool _autoRearrange = true;
    private bool _arranging;

    /// @see BoxSizerController.as::BoxSizerController (default)
    public BoxSizerController() : base() { }

    /// @see BoxSizerController.as::BoxSizerController (name + rect)
    public BoxSizerController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see BoxSizerController.as::BoxSizerController (full AS3 11-param signature)
    public BoxSizerController
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

    /// @see BoxSizerController.as::setSpacing
    public void SetSpacing(int value)
    {
        _spacing = value;
        ArrangeChildren();
    }

    /// @see BoxSizerController.as::setHorizontalPadding
    public void SetHorizontalPadding(int value)
    {
        _horizontalPadding = value;
        ArrangeChildren();
    }

    /// @see BoxSizerController.as::setVerticalPadding
    public void SetVerticalPadding(int value)
    {
        _verticalPadding = value;
        ArrangeChildren();
    }

    /// @see BoxSizerController.as::setVertical
    public void SetVertical(bool value)
    {
        _vertical = value;
        ArrangeChildren();
    }

    /// @see BoxSizerController.as::setAutoRearrange
    public void SetAutoRearrange(bool value)
    {
        _autoRearrange = value;
        if (value)
        {
            ArrangeChildren();
        }
    }

    /// @see BoxSizerController.as::getAutoRearrange
    public bool GetAutoRearrange()
    {
        return _autoRearrange;
    }

    /// @see BoxSizerController.as::set properties
    public override void ApplyProperties(PropertyStruct[] properties)
    {
        foreach (PropertyStruct prop in properties)
        {
            switch (prop.key)
            {
                case "padding_horizontal":
                    if (prop.value != null)
                    {
                        _horizontalPadding = Convert.ToInt32(prop.value);
                    }
                    break;
                case "padding_vertical":
                    if (prop.value != null)
                    {
                        _verticalPadding = Convert.ToInt32(prop.value);
                    }
                    break;
                case "spacing":
                    if (prop.value != null)
                    {
                        _spacing = Convert.ToInt32(prop.value);
                    }
                    break;
                case "vertical":
                    if (prop.value is bool boolVal)
                    {
                        _vertical = boolVal;
                    }
                    break;
            }
        }

        base.ApplyProperties(properties);
        ArrangeChildren();
    }

    /// @see BoxSizerController.as::update
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        // @see BoxSizerController.as — arrangeChildren() is called for these events,
        // but the method ALWAYS falls through to super.update() (no early return in AS3)
        switch (param2.type)
        {
            case WindowEvent.WE_CHILD_RELOCATED:
            case WindowEvent.WE_CHILD_REMOVED:
            case WindowEvent.WE_CHILD_ADDED:
            case WindowEvent.WE_CHILD_RESIZED:
            case WindowEvent.WE_RESIZED:
            case WindowEvent.WE_CHILD_VISIBILITY:
                ArrangeChildren();
                break;
        }

        return base.Update(param1, param2);
    }

    /// @see BoxSizerController.as::arrangeChildren
    public void ArrangeChildren()
    {
        if (!_autoRearrange || _arranging)
        {
            return;
        }

        _arranging = true;

        int relativeSum = GetRelativeValuesSum();
        int relativeSpace = CalculateSpaceForRelatives();

        float position = _vertical ? _verticalPadding : _horizontalPadding;

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
                child.x = _horizontalPadding;
                child.y = position;

                int relValue = GetRelativeValue(child);

                if (relValue > 0 && relativeSum > 0)
                {
                    child.height = (float)relativeSpace * relValue / relativeSum;
                }

                position += child.height + _spacing;
            }
            else
            {
                child.x = position;
                child.y = _verticalPadding;

                int relValue = GetRelativeValue(child);

                if (relValue > 0 && relativeSum > 0)
                {
                    child.width = (float)relativeSpace * relValue / relativeSum;
                }

                position += child.width + _spacing;
            }
        }

        _arranging = false;
    }

    /// @see BoxSizerController.as::getRelativeValue
    /// Parses tags for "relative(N)" pattern
    public static int GetRelativeValue(IWindow param1)
    {
        foreach (string tag in param1.tags)
        {
            Match match = RelativeTagRegex().Match(tag);

            if (match.Success && int.TryParse(match.Groups[1].Value, out int value))
            {
                return value;
            }
        }

        return 0;
    }

    /// @see BoxSizerController.as::getRelativeValuesSum
    public int GetRelativeValuesSum()
    {
        int sum = 0;

        for (int i = 0;
             i < numChildren;
             i++)
        {
            IWindow? child = GetChildAt(i);

            if (child is { visible: true })
            {
                sum += GetRelativeValue(child);
            }
        }

        return sum;
    }

    /// @see BoxSizerController.as::calculateSpaceForRelatives
    public int CalculateSpaceForRelatives()
    {
        float totalSize = _vertical ? height : width;
        float padding = _vertical ? _verticalPadding * 2 : _horizontalPadding * 2;
        float fixedSize = 0;
        int visibleCount = 0;

        for (int i = 0;
             i < numChildren;
             i++)
        {
            IWindow? child = GetChildAt(i);

            if (child is not { visible: true })
            {
                continue;
            }

            visibleCount++;

            if (GetRelativeValue(child) <= 0)
            {
                fixedSize += _vertical ? child.height : child.width;
            }
        }

        float spacing = visibleCount > 1 ? (visibleCount - 1) * _spacing : 0;

        return (int)(totalSize - padding - fixedSize - spacing);
    }

    [GeneratedRegex(@"relative\((\d+)\)")]
    private static partial Regex RelativeTagRegex();

    /// @see class_3651.as — IClass3651 explicit interface implementations
    void IClass3651.SetAutoRearrange(bool value)
    {
        SetAutoRearrange(value);
    }

    bool IClass3651.GetAutoRearrange()
    {
        return GetAutoRearrange();
    }

    void IClass3651.SetHorizontalPadding(int value)
    {
        SetHorizontalPadding(value);
    }

    void IClass3651.SetVerticalPadding(int value)
    {
        SetVerticalPadding(value);
    }

    void IClass3651.SetSpacing(int value)
    {
        SetSpacing(value);
    }

    void IClass3651.SetVertical(bool value)
    {
        SetVertical(value);
    }
}
