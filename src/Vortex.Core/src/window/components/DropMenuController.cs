// @see core/window/components/DropMenuController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/DropMenuController.as
public class DropMenuController : DropBaseController, IDropMenuWindow
{
    private const int DROP_MENU_ITEM_MAX_LENGTH = 200;

    private readonly List<string> _stringArray = [];

    /// @see DropMenuController.as::DropMenuController (default)
    public DropMenuController() : base() { }

    /// @see DropMenuController.as::DropMenuController (name + rect)
    public DropMenuController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see DropMenuController.as::DropMenuController (full AS3 11-param signature)
    public DropMenuController
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

    /// @see DropMenuController.as::get numMenuItems
    public new int NumMenuItems => _stringArray.Count;

    /// @see DropMenuController.as::populate — reset selection to -1, close menu
    public void Populate(string[] items)
    {
        _stringArray.Clear();
        _stringArray.AddRange(items);
        _selectionIndex = -1;
        CloseExpandedMenuView();
    }

    /// @see DropMenuController.as::populateWithVector — reset selection to -1, close menu
    public void PopulateWithVector(IList<string> items)
    {
        _stringArray.Clear();

        foreach (string item in items)
        {
            _stringArray.Add(item);
        }

        _selectionIndex = -1;
        CloseExpandedMenuView();
    }

    /// @see DropMenuController.as::enumerateSelection
    public List<string> EnumerateSelection()
    {
        return
        [
            .._stringArray,
        ];
    }

    /// @see DropMenuController.as::openMenu
    public void OpenMenu()
    {
        OpenExpandedMenuView();
    }

    /// @see DropMenuController.as::closeExpandedMenuView
    public override void CloseExpandedMenuView()
    {
        base.CloseExpandedMenuView();

        // @see DropMenuController.as — update title label with selected text
        IWindow? titleLabel = GetTitleLabel();

        if (titleLabel != null && _selectionIndex >= 0 && _selectionIndex < _stringArray.Count)
        {
            titleLabel.caption = _stringArray[_selectionIndex];
        }
        else if (titleLabel != null)
        {
            titleLabel.caption = caption;
        }
    }

    /// @see DropMenuController.as::dispose
    public override bool Destroy()
    {
        if (_disposed)
        {
            return false;
        }

        _stringArray.Clear();

        return base.Destroy();
    }
}
