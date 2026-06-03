// @see core/window/components/DropBaseController.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/DropBaseController.as
public class DropBaseController : InteractiveController
{
    /// @see DropBaseController.as — child element name constants
    protected const string TEXT_FIELD_NAME = "_DROPLIST_TITLETEXT";
    protected const string ITEM_LIST_NAME = "_DROPLIST_ITEMLIST";
    protected const string REGION_NAME = "_DROPLIST_REGION";
    protected const float CAPTION_BLEND_CHANGE = 0.5f;

    protected readonly List<IWindow> _itemArray = [];

    protected int _selectionIndex = -1;
    protected bool _menuIsOpen;
    protected bool _expanded;

    private bool _openUpward;
    private bool _keepOpenOnDeactivate;

    /// @see DropBaseController.as::DropBaseController (default)
    public DropBaseController() : base() { }

    /// @see DropBaseController.as::DropBaseController (name + rect)
    public DropBaseController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see DropBaseController.as::DropBaseController (full AS3 11-param signature)
    /// @see DropBaseController.as — param4 |= 1
    public DropBaseController
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

    /// @see DropBaseController.as::get selection
    public int Selection
    {
        get => _selectionIndex;
        set
        {
            if (value < 0 || value >= _itemArray.Count)
            {
                return;
            }

            if (value == _selectionIndex)
            {
                return;
            }

            WindowEvent evt = new(WindowEvent.WE_SELECT, this, null, true);
            NotifyEventListeners(evt);

            if (evt.IsWindowOperationPrevented())
            {
                return;
            }

            _selectionIndex = value;

            WindowEvent selectedEvt = new(WindowEvent.WE_SELECTED, this, null, false);

            NotifyEventListeners(selectedEvt);
        }
    }

    /// @see DropBaseController.as::get numMenuItems
    public int NumMenuItems => _itemArray.Count;

    /// @see DropBaseController.as::set caption
    public override string caption
    {
        get => _caption;
        set
        {
            _caption = value;

            IWindow? titleLabel = GetTitleLabel();
            if (titleLabel != null)
            {
                titleLabel.caption = value;
            }
        }
    }

    /// @see DropBaseController.as::getTitleLabel
    public IWindow? GetTitleLabel()
    {
        return GetChildByName(TEXT_FIELD_NAME);
    }

    /// @see DropBaseController.as::getItemList
    public IWindow? GetItemList()
    {
        return GetChildByName(ITEM_LIST_NAME);
    }

    /// @see DropBaseController.as::getRegion
    public IWindow? GetRegion()
    {
        return GetChildByName(REGION_NAME);
    }

    /// @see DropBaseController.as::populate
    public virtual void Populate(IList<IWindow>? items)
    {
        _itemArray.Clear();

        if (items != null)
        {
            foreach (IWindow item in items)
            {
                _itemArray.Add(item);
            }
        }

        _selectionIndex = _itemArray.Count > 0 ? 0 : -1;
    }

    /// @see DropBaseController.as::open
    public virtual void Open()
    {
        WindowEvent evt = new(WindowEvent.WE_OPEN, this, null, true);
        NotifyEventListeners(evt);

        if (evt.IsWindowOperationPrevented())
        {
            return;
        }

        _menuIsOpen = true;

        WindowEvent openedEvt = new(WindowEvent.WE_OPENED, this, null, false);
        NotifyEventListeners(openedEvt);
    }

    /// @see DropBaseController.as::close
    public virtual void Close()
    {
        WindowEvent evt = new(WindowEvent.WE_CLOSE, this, null, true);
        NotifyEventListeners(evt);

        if (evt.IsWindowOperationPrevented())
        {
            return;
        }

        _menuIsOpen = false;

        WindowEvent closedEvt = new(WindowEvent.WE_CLOSED, this, null, false);
        NotifyEventListeners(closedEvt);
    }

    /// @see DropBaseController.as::openExpandedMenuView
    public virtual void OpenExpandedMenuView()
    {
        if (_expanded)
        {
            return;
        }

        _expanded = true;
        PopulateExpandedMenu();
        Open();
    }

    /// @see DropBaseController.as::closeExpandedMenuView
    public virtual void CloseExpandedMenuView()
    {
        if (!_expanded)
        {
            return;
        }

        _expanded = false;
        Close();
    }

    /// @see DropBaseController.as::populateExpandedMenu
    public virtual void PopulateExpandedMenu() { }

    /// @see DropBaseController.as::resolveSelection
    protected int ResolveSelection(IWindow? item)
    {
        if (item == null)
        {
            return -1;
        }

        return _itemArray.IndexOf(item);
    }

    /// @see DropBaseController.as::set properties
    public override void ApplyProperties(PropertyStruct[] properties)
    {
        foreach (PropertyStruct prop in properties)
        {
            switch (prop.key)
            {
                case "open_upward":
                    if (prop.value is bool ou)
                    {
                        _openUpward = ou;
                    }
                    break;
                case "keep_open_on_deactivate":
                    if (prop.value is bool ko)
                    {
                        _keepOpenOnDeactivate = ko;
                    }
                    break;
            }
        }

        base.ApplyProperties(properties);
    }

    /// @see DropBaseController.as::update
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        switch (param2.type)
        {
            case WindowEvent.WE_ENABLED:
                {
                    IWindow? titleLabel = GetTitleLabel();

                    if (titleLabel != null)
                    {
                        titleLabel.blend = base.blend;
                    }

                    return true;
                }
            case WindowEvent.WE_DISABLED:
                {
                    IWindow? titleLabel = GetTitleLabel();

                    if (titleLabel != null)
                    {
                        titleLabel.blend = base.blend * CAPTION_BLEND_CHANGE;
                    }

                    return true;
                }
        }

        return base.Update(param1, param2);
    }

    /// @see DropBaseController.as::dispose
    public override bool Destroy()
    {
        if (_disposed)
        {
            return false;
        }

        _itemArray.Clear();

        return base.Destroy();
    }
}
