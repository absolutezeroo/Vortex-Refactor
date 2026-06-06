// @see com.sulake.habbo.toolbar.ExtensionView

using System;
using System.Linq;
using System.Xml.Linq;

using Vortex.Core.Assets;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Habbo.Toolbar.Events;
using Vortex.Habbo.Window;

namespace Vortex.Habbo.Toolbar;

/// @see com.sulake.habbo.toolbar.ExtensionView
public class ExtensionView : IExtensionView
{
    private static readonly int MARGIN = 3;
    private static readonly int PURSE_EXTENSION_OFFSET = -8;

    private HabboToolbar? _toolbar;
    private IItemListWindow? _itemList;
    private readonly Dictionary<string, IWindow> _items = new();
    private readonly List<IWindow> _orderedItems = new();
    private bool _disposed;
    private bool _landingView;
    private readonly IHabboWindowManager _windowManager;
    private int _extraMargin;

    /// @see ExtensionView.as::ExtensionView
    public ExtensionView(IHabboWindowManager windowManager, HabboToolbar toolbar)
    {
        _toolbar = toolbar;
        _windowManager = windowManager;

        XmlAsset? xmlAsset = (toolbar.assets as IAssetLibrary)?.GetAssetByName("extension_grid_xml") as XmlAsset;
        XElement? layoutXml = xmlAsset?.Content as XElement
                              ?? Window.Utils.HabboAssetResolver.LoadXmlAsset("extension_grid_xml");

        if (layoutXml != null)
        {
            _itemList = windowManager.BuildFromXml(layoutXml, 1) as IItemListWindow;
        }

        if (_itemList is IWindow listWindow)
        {
            // @see ExtensionView.as::ExtensionView — position at top-right of desktop
            if (listWindow.parent is IWindow desktop)
            {
                listWindow.x = desktop.width - listWindow.width - MARGIN - _extraMargin;
                listWindow.y = MARGIN;
            }

            listWindow.visible = true;
        }

        _items = new Dictionary<string, IWindow>();
    }

    /// @see ExtensionView.as::dispose
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (string key in _items.Keys.ToList())
        {
            DetachExtension(key);
        }

        if (_itemList is IWindow listWindow)
        {
            listWindow.Destroy();
        }

        _itemList = null;
        _orderedItems.Clear();
        _toolbar = null;
        _disposed = true;
    }

    /// @see IExtensionView.as::get visible
    public bool visible
    {
        get => _itemList is IWindow w && w.visible;
        set
        {
            if (_itemList is IWindow w)
            {
                w.visible = value;
            }
        }
    }

    /// @see IExtensionView.as::get screenHeight
    public uint screenHeight
    {
        get
        {
            if (_itemList is not IWindow w)
            {
                return 0;
            }

            return (uint)(w.height + w.y);
        }
    }

    /// @see IExtensionView.as::attachExtension
    public void AttachExtension(string id, IWindow window, int slot = -1, string[]? beforeIds = null)
    {
        if (_disposed)
        {
            return;
        }

        if (_items.ContainsKey(id))
        {
            return;
        }

        _items[id] = window;

        if (beforeIds != null)
        {
            slot = ResolveIndex(beforeIds);
        }

        if (slot == -1)
        {
            _orderedItems.Add(window);
        }
        else
        {
            int clamped = Math.Min(slot, _orderedItems.Count);
            _orderedItems.Insert(clamped, window);
        }

        if (_itemList != null && _toolbar != null)
        {
            _toolbar.CreateAndAttachDimmerWindow();
            RefreshItemWindow();
        }

        QueueResizeEvent();
    }

    /// @see IExtensionView.as::hasExtension
    public bool HasExtension(string id)
    {
        return _items.ContainsKey(id);
    }

    /// @see IExtensionView.as::detachExtension
    public void DetachExtension(string id)
    {
        if (_disposed)
        {
            return;
        }

        if (!_items.TryGetValue(id, out IWindow? win))
        {
            return;
        }

        _orderedItems.Remove(win);

        if (_itemList != null && _toolbar != null)
        {
            _toolbar.RemoveDimmer();
            RefreshItemWindow();
        }

        _items.Remove(id);
        QueueResizeEvent();
    }

    /// @see IExtensionView.as::refreshItemWindow
    public void RefreshItemWindow()
    {
        if (_itemList == null)
        {
            return;
        }

        _itemList.RemoveListItems();

        foreach (IWindow item in _orderedItems)
        {
            string key = GetKeyForWindow(item);

            switch (key)
            {
                case ToolbarDisplayExtensionIds.const_936:          // logout_tools
                case ToolbarDisplayExtensionIds.CREDITS_EXTENSION_ID:
                case ToolbarDisplayExtensionIds.const_791:          // purse_engagement_currency
                case ToolbarDisplayExtensionIds.const_1138:         // purse_habbo_club
                case ToolbarDisplayExtensionIds.const_890:          // purse_seasonal_currency
                case ToolbarDisplayExtensionIds.TALENT_PROMO_EXTENSION_ID:
                case ToolbarDisplayExtensionIds.CLUB_PROMO:
                case ToolbarDisplayExtensionIds.VIP_QUESTS:
                case ToolbarDisplayExtensionIds.VIDEO_OFFERS:
                case ToolbarDisplayExtensionIds.const_673:          // settings
                case ToolbarDisplayExtensionIds.PHONE_NUMBER:
                case ToolbarDisplayExtensionIds.VERIFICATION_CODE:
                case ToolbarDisplayExtensionIds.RETURN_GIFT:
                case ToolbarDisplayExtensionIds.TARGETED_OFFER:
                    _itemList.AddListItem(item);
                    break;

                case ToolbarDisplayExtensionIds.const_1144:         // purse
                    _itemList.AddListItem(item);
                    if (_itemList is IWindow lw)
                    {
                        // @see ExtensionView.as::refreshItemWindow — purse raises the list by 8px
                        lw.y = MARGIN + PURSE_EXTENSION_OFFSET;
                    }
                    break;

                default:
                    if (!_landingView)
                    {
                        _itemList.AddListItem(item);
                    }
                    break;
            }
        }

        _itemList.ArrangeListItems();
        if (_itemList is IWindow listWin)
        {
            listWin.Invalidate();
        }
    }

    /// @see IExtensionView.as::get/set extraMargin
    public int extraMargin
    {
        get => _extraMargin;
        set
        {
            _extraMargin = value;
            if (_itemList is IWindow w && w.parent != null)
            {
                w.x = w.parent.width - w.width - MARGIN - _extraMargin;
            }
        }
    }

    /// @see ExtensionView.as::get/set landingView
    public bool landingView
    {
        get => _landingView;
        set
        {
            _landingView = value;
            RefreshItemWindow();
        }
    }

    // --- Private helpers ---

    private string GetKeyForWindow(IWindow window)
    {
        foreach (KeyValuePair<string, IWindow> kv in _items)
        {
            if (kv.Value == window)
            {
                return kv.Key;
            }
        }

        return string.Empty;
    }

    private int ResolveIndex(string[] beforeIds)
    {
        for (int i = 0; i < _orderedItems.Count; i++)
        {
            string name = _orderedItems[i].name ?? string.Empty;
            if (Array.IndexOf(beforeIds, name) >= 0)
            {
                return i;
            }
        }

        return -1;
    }

    /// @see ExtensionView.as::queueResizeEvent — fires after 25ms delay in AS3; dispatch directly in C#
    private void QueueResizeEvent()
    {
        _toolbar?.events?.DispatchEvent(new ExtensionViewEvent(ExtensionViewEvent.EXTENSION_VIEW_RESIZED));
    }
}
