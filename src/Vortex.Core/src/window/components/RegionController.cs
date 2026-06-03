// @see core/window/components/RegionController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/RegionController.as
public class RegionController : ContainerController, IRegionWindow, IInteractiveWindow
{
    private readonly Dictionary<uint, uint> _cursorMap = new();

    /// @see RegionController.as::RegionController (default)
    public RegionController() : base() { }

    /// @see RegionController.as::RegionController (name + rect)
    public RegionController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see RegionController.as::RegionController (full AS3 11-param signature)
    /// @see RegionController.as — param4 |= 1
    public RegionController
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
    ) : base(param1, param2, param3, param4 | 1, param5, param6, param7, param8, param9, param10, param11, param12)
    {
    }

    /// @see RegionController.as::get toolTipCaption
    private string? _toolTipCaption = "";

    /// @see RegionController.as::get toolTipDelay
    private uint _toolTipDelay = 500;

    /// @see RegionController.as::get toolTipIsDynamic
    private bool _toolTipIsDynamic;

    /// @see RegionController.as::get interactiveCursorDisabled
    private bool _cursorDisabled;

    /// @see RegionController.as::update — calls super.update() FIRST, then processes interactive events
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        bool result = base.Update(param1, param2);

        if (param1 == this)
        {
            InteractiveController.ProcessInteractiveWindowEvents(this, param2);
        }

        return result;
    }

    /// @see RegionController.as::showToolTip
    public void ShowToolTip() { }

    /// @see RegionController.as::hideToolTip
    public void HideToolTip() { }

    /// @see RegionController.as::setMouseCursorForState
    public uint SetMouseCursorForState(uint stateFlag, uint cursorId)
    {
        uint previous = _cursorMap.TryGetValue(stateFlag, out uint old) ? old : 0;

        // @see RegionController.as — remove entry when cursorId is 0 or uint.MaxValue
        if (cursorId is 0 or uint.MaxValue)
        {
            _cursorMap.Remove(stateFlag);
        }
        else
        {
            _cursorMap[stateFlag] = cursorId;
        }

        return previous;
    }

    /// @see RegionController.as::getMouseCursorByState
    public uint GetMouseCursorByState(uint stateFlag)
    {
        // @see RegionController.as — if disabled (flag 32), return cursor 1
        if (TestStateFlag(STATE_DISABLED))
        {
            return 1;
        }

        return _cursorMap.GetValueOrDefault(stateFlag, 0u);
    }

    // IInteractiveWindow property implementations

    /// @see RegionController.as::get/set toolTipCaption
    string? IInteractiveWindow.ToolTipCaption
    {
        get => _toolTipCaption;
        set => _toolTipCaption = value;
    }

    /// @see RegionController.as::get/set toolTipDelay
    uint IInteractiveWindow.ToolTipDelay
    {
        get => _toolTipDelay;
        set => _toolTipDelay = value;
    }

    /// @see RegionController.as::get/set toolTipIsDynamic
    bool IInteractiveWindow.ToolTipIsDynamic
    {
        get => _toolTipIsDynamic;
        set => _toolTipIsDynamic = value;
    }

    /// @see RegionController.as::get/set interactiveCursorDisabled
    bool IInteractiveWindow.InteractiveCursorDisabled
    {
        get => _cursorDisabled;
        set => _cursorDisabled = value;
    }
}
