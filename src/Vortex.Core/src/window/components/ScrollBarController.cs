// @see core/window/components/ScrollBarController.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/ScrollBarController.as
public class ScrollBarController : InteractiveController, IScrollbarWindow
{
    private const string SCROLL_BUTTON_INCREMENT = "increment";
    private const string SCROLL_BUTTON_DECREMENT = "decrement";
    private const string SCROLL_SLIDER_TRACK = "slider_track";
    private const string SCROLL_SLIDER_BAR = "slider_bar";

    protected float _offset;
    protected float _wheelIncrement = 0.1f;
    protected IScrollableWindow? _scrollableTarget;
    private bool _settingScroll;

    /// @see ScrollBarController.as::ScrollBarController (default)
    public ScrollBarController() : base() { }

    /// @see ScrollBarController.as::ScrollBarController (name + rect)
    public ScrollBarController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see ScrollBarController.as::ScrollBarController (full AS3 11-param signature)
    public ScrollBarController
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
        // @see ScrollBarController.as — determine orientation from type
        Horizontal = param2 == 130;

        // @see ScrollBarController.as — wire internal button event procs
        for (int i = 0;
             i < numChildren;
             i++)
        {
            IWindow? child = GetChildAt(i);

            if (child is { tags: not null } && child.tags.Contains("_INTERNAL"))
            {
                child.procedure = ScrollButtonEventProc;
            }
        }

        UpdateLiftSizeAndPosition();
    }


    /// @see ScrollBarController.as::get scrollH
    public float ScrollHValue
    {
        get => Horizontal ? _offset : 0;
        set
        {
            if (!Horizontal)
            {
                return;
            }

            SetScrollPosition(value, true);
            UpdateLiftSizeAndPosition();
        }
    }

    /// @see ScrollBarController.as::get scrollV
    public float ScrollVValue
    {
        get => !Horizontal ? _offset : 0;
        set
        {
            if (Horizontal)
            {
                return;
            }

            SetScrollPosition(value, true);
            UpdateLiftSizeAndPosition();
        }
    }

    /// @see ScrollBarController.as::get horizontal
    public bool Horizontal { get; }

    /// @see ScrollBarController.as::get vertical
    public bool Vertical => !Horizontal;

    /// @see ScrollBarController.as::get scrollable
    public IScrollableWindow? ScrollableTarget
    {
        get => _scrollableTarget;
        set
        {
            if (_scrollableTarget == value)
            {
                return;
            }

            // @see ScrollBarController.as — remove old listeners
            if (_scrollableTarget is IWindow oldTarget)
            {
                oldTarget.RemoveEventListener(WindowEvent.WE_RESIZED, OnScrollableResized);
                oldTarget.RemoveEventListener(WindowEvent.WE_SCROLL, OnScrollableScrolled);
            }

            _scrollableTarget = value;

            // @see ScrollBarController.as — add new listeners
            if (_scrollableTarget is IWindow newTarget)
            {
                newTarget.AddEventListener(WindowEvent.WE_RESIZED, OnScrollableResized);
                newTarget.AddEventListener(WindowEvent.WE_SCROLL, OnScrollableScrolled);
            }

            UpdateLiftSizeAndPosition();
        }
    }

    /// @see ScrollBarController.as::get track
    public IWindow? Track => FindChildByName(SCROLL_SLIDER_TRACK);

    /// @see ScrollBarController.as::get lift
    public IWindow? Lift
    {
        get
        {
            IWindow? track = Track;

            if (track is IWindowContainer container)
            {
                return container.FindChildByName(SCROLL_SLIDER_BAR);
            }

            return null;
        }
    }

    /// @see ScrollBarController.as::setScrollPosition
    public virtual bool SetScrollPosition(float value, bool propagate)
    {
        value = Math.Clamp(value, 0f, 1f);
        _offset = value;

        if (!propagate || _scrollableTarget == null)
        {
            return true;
        }

        // @see ScrollBarController.as — propagate to scrollable target
        if (Horizontal)
        {
            _scrollableTarget.ScrollH = value;
        }
        else
        {
            _scrollableTarget.ScrollV = value;
        }

        return true;
    }

    /// @see ScrollBarController.as::updateLiftSizeAndPosition
    public virtual void UpdateLiftSizeAndPosition()
    {
        if (_scrollableTarget == null)
        {
            ResolveScrollTarget();
        }

        IWindow? track = Track;
        IWindow? lift = Lift;

        if (track == null || lift == null)
        {
            return;
        }

        // @see ScrollBarController.as — calculate lift size ratio
        float visibleSize;
        float scrollableSize;

        if (Horizontal)
        {
            visibleSize = _scrollableTarget != null ? _scrollableTarget.VisibleRegion.Size.X : track.width;
            scrollableSize = _scrollableTarget != null ? _scrollableTarget.ScrollableRegion.Size.X : track.width;
        }
        else
        {
            visibleSize = _scrollableTarget != null ? _scrollableTarget.VisibleRegion.Size.Y : track.height;
            scrollableSize = _scrollableTarget != null ? _scrollableTarget.ScrollableRegion.Size.Y : track.height;
        }

        float ratio = scrollableSize > 0 ? Math.Min(visibleSize / scrollableSize, 1f) : 1f;

        if (Horizontal)
        {
            lift.width = ratio * track.width;
            lift.x = _offset * (track.width - lift.width);
        }
        else
        {
            lift.height = ratio * track.height;
            lift.y = _offset * (track.height - lift.height);
        }
    }

    /// @see ScrollBarController.as::resolveScrollTarget
    public virtual bool ResolveScrollTarget()
    {
        if (_scrollableTarget != null)
        {
            return true;
        }

        // @see ScrollBarController.as — walk parent chain / siblings to find IScrollableWindow
        if (parent is IScrollableWindow scrollableParent)
        {
            _scrollableTarget = scrollableParent;
            return true;
        }

        if (parent is IWindowContainer parentContainer)
        {
            for (int i = 0;
                 i < parentContainer.numChildren;
                 i++)
            {
                IWindow? sibling = parentContainer.GetChildAt(i);

                if (sibling != this && sibling is IScrollableWindow scrollableSibling)
                {
                    _scrollableTarget = scrollableSibling;
                    return true;
                }
            }
        }

        return false;
    }

    /// @see ScrollBarController.as::set properties
    public override void ApplyProperties(PropertyStruct[] properties)
    {
        foreach (PropertyStruct prop in properties)
        {
            if (prop is { key: "wheel_increment", value: not null })
            {
                _wheelIncrement = Convert.ToSingle(prop.value);
            }
        }

        base.ApplyProperties(properties);
    }

    /// @see ScrollBarController.as::update
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        if (param2.type == WindowEvent.WE_CHILD_RELOCATED && !_settingScroll)
        {
            // @see ScrollBarController.as — lift was repositioned by drag
            IWindow? lift = Lift;

            if (lift is ScrollBarLiftController liftCtrl)
            {
                float offset = Horizontal ? liftCtrl.ScrollbarOffsetX : liftCtrl.ScrollbarOffsetY;
                SetScrollPosition(offset, true);
            }
        }
        else if (param2.type == WindowEvent.WE_RESIZED)
        {
            UpdateLiftSizeAndPosition();
        }
        else if (param2 is WindowMouseEvent { type: WindowMouseEvent.WHEEL } mouseEvt)
        {
            // @see ScrollBarController.as — wheel scroll: delta > 0 = scroll up, delta < 0 = scroll down
            _settingScroll = true;

            if (mouseEvt.delta > 0)
            {
                SetScrollPosition(Math.Max(_offset - _wheelIncrement, 0f), true);
            }
            else if (mouseEvt.delta < 0)
            {
                SetScrollPosition(Math.Min(_offset + _wheelIncrement, 1f), true);
            }

            UpdateLiftSizeAndPosition();
            _settingScroll = false;

            return true;
        }

        return base.Update(param1, param2);
    }

    /// @see ScrollBarController.as — handle target resized
    private void OnScrollableResized(WindowEvent evt, IWindow sender)
    {
        UpdateLiftSizeAndPosition();
    }

    /// @see ScrollBarController.as — handle target scrolled
    private void OnScrollableScrolled(WindowEvent evt, IWindow sender)
    {
        if (_settingScroll)
        {
            return;
        }

        if (_scrollableTarget == null)
        {
            return;
        }

        _offset = Horizontal ? _scrollableTarget.ScrollH : _scrollableTarget.ScrollV;
        UpdateLiftSizeAndPosition();
    }

    /// @see ScrollBarController.as::scrollButtonEventProc
    private void ScrollButtonEventProc(WindowEvent param1, IWindow param2)
    {
        if (param1.type != WindowMouseEvent.DOWN)
        {
            return;
        }

        string childName = param2.name;

        if (childName == SCROLL_BUTTON_INCREMENT)
        {
            _settingScroll = true;

            if (Horizontal)
            {
                ScrollHValue = Math.Min(_offset + _wheelIncrement, 1f);
            }
            else
            {
                ScrollVValue = Math.Min(_offset + _wheelIncrement, 1f);
            }

            _settingScroll = false;
            UpdateLiftSizeAndPosition();
        }
        else if (childName == SCROLL_BUTTON_DECREMENT)
        {
            _settingScroll = true;

            if (Horizontal)
            {
                ScrollHValue = Math.Max(_offset - _wheelIncrement, 0f);
            }
            else
            {
                ScrollVValue = Math.Max(_offset - _wheelIncrement, 0f);
            }

            _settingScroll = false;
            UpdateLiftSizeAndPosition();
        }
    }

    /// @see ScrollBarController.as::dispose
    public override bool Destroy()
    {
        if (_disposed)
        {
            return false;
        }

        // @see ScrollBarController.as — remove listeners before nulling
        if (_scrollableTarget is IWindow oldTarget)
        {
            oldTarget.RemoveEventListener(WindowEvent.WE_RESIZED, OnScrollableResized);
            oldTarget.RemoveEventListener(WindowEvent.WE_SCROLL, OnScrollableScrolled);
        }

        _scrollableTarget = null;

        return base.Destroy();
    }

    float IScrollbarWindow.ScrollH { get => ScrollHValue; set => ScrollHValue = value; }

    float IScrollbarWindow.ScrollV { get => ScrollVValue; set => ScrollVValue = value; }

    IScrollableWindow? IScrollbarWindow.Scrollable { get => _scrollableTarget; set => ScrollableTarget = value; }

    bool IScrollbarWindow.Vertical => Vertical;

    bool IScrollbarWindow.Horizontal => Horizontal;
}
