// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/habbo/ui/DesktopLayoutManager.as

using System;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Habbo.Window;

namespace Vortex.Habbo.UI;

/// @see com.sulake.habbo.ui.DesktopLayoutManager
public sealed class DesktopLayoutManager : IDisposable
{
    private const string ROOM_VIEW = "room_view";
    private const string ROOM_NEW_CHAT = "room_new_chat";
    private const string ROOM_WIDGET = "room_widget";
    private const int BOTTOM_MARGIN = 47;

    private IWindowContainer? _layoutContainer;

    /// @see DesktopLayoutManager.as::dispose
    public void Dispose()
    {
        _layoutContainer?.Destroy();
        _layoutContainer = null;
    }

    /// @see DesktopLayoutManager.as::setLayout
    public void SetLayout(XElement? xml, IHabboWindowManager? windowManager, object? config)
    {
        _ = config;

        if (xml == null || windowManager == null)
        {
            throw new InvalidOperationException("Unable to set room desktop layout.");
        }

        _layoutContainer = ((IWindowFactory)windowManager).BuildFromXml(xml, 0) as IWindowContainer;

        if (_layoutContainer == null)
        {
            throw new InvalidOperationException("Failed to build layout from XML.");
        }

        IWindow? desktop = _layoutContainer.parent;

        if (desktop != null)
        {
            _layoutContainer.width = desktop.width;
            _layoutContainer.height = desktop.height;
        }

        if (_layoutContainer.FindChildByTag("room_widget_infostand") is IWindow infostand)
        {
            infostand.y -= BOTTOM_MARGIN;
        }

        for (int i = 0; i < _layoutContainer.numChildren; i++)
        {
            IWindow? child = _layoutContainer.GetChildAt(i);

            if (child?.TestParamFlag(0x100000) == true)
            {
                child.AddEventListener("WE_CHILD_RESIZED", TrimContainer);
            }
        }
    }

    /// @see DesktopLayoutManager.as::trimContainer
    private static void TrimContainer(Core.Window.Events.WindowEvent eventObj, IWindow window)
    {
        IWindowContainer? container = window as IWindowContainer;

        if (container == null || container.numChildren != 1)
        {
            return;
        }

        IWindow? child = container.GetChildAt(0);

        if (child == null)
        {
            return;
        }

        container.width = child.width;
        container.height = child.height;
    }

    /// @see DesktopLayoutManager.as::addWidgetWindow
    public bool AddWidgetWindow(string widgetType, IWindow? window)
    {
        if (window == null)
        {
            return false;
        }

        IWindowContainer? container = GetWidgetContainer(widgetType, window);

        if (container == null)
        {
            return false;
        }

        if (widgetType == "RWE_CHAT_INPUT_WIDGET")
        {
            container.AddChild(window);
            return true;
        }

        window.x = 0;
        window.y = 0;
        container.AddChild(window);
        container.width = window.width;
        container.height = window.height;

        return true;
    }

    /// @see DesktopLayoutManager.as::removeWidgetWindow
    public void RemoveWidgetWindow(string widgetType, IWindow? window)
    {
        if (window == null)
        {
            return;
        }

        GetWidgetContainer(widgetType, window)?.RemoveChild(window);
    }

    /// @see DesktopLayoutManager.as::addRoomView
    public bool AddRoomView(IWindow? window)
    {
        if (window == null)
        {
            return false;
        }

        if (_layoutContainer?.GetChildByTag(ROOM_VIEW) is not IWindowContainer container)
        {
            return false;
        }

        container.AddChild(window);
        return true;
    }

    /// @see DesktopLayoutManager.as::get roomViewRect
    public Rect2? RoomViewRect
    {
        get
        {
            if (_layoutContainer?.FindChildByTag(ROOM_VIEW) is not IWindowContainer roomView)
            {
                return null;
            }

            Rect2 rect = roomView.rectangle;
            rect.Position += _layoutContainer.position;

            return rect;
        }
    }

    /// @see DesktopLayoutManager.as::getRoomView
    public IWindow? GetRoomView()
    {
        if (_layoutContainer?.FindChildByTag(ROOM_VIEW) is IWindowContainer { numChildren: > 0 } roomView)
        {
            return roomView.GetChildAt(0);
        }

        return null;
    }

    /// @see DesktopLayoutManager.as::getChatContainer
    public IDisplayObjectWrapper? GetChatContainer()
    {
        return _layoutContainer?.FindChildByTag(ROOM_NEW_CHAT) as IDisplayObjectWrapper;
    }

    /// @see DesktopLayoutManager.as::getWidgetContainer
    private IWindowContainer? GetWidgetContainer(string widgetType, IWindow window)
    {
        if (_layoutContainer == null)
        {
            return null;
        }

        if (widgetType is "RWE_HIGH_SCORE_DISPLAY" or "RWE_WORD_QUIZZ")
        {
            return _layoutContainer.GetChildByName("background_widgets") as IWindowContainer;
        }

        if (widgetType == "RWE_CHAT_INPUT_WIDGET")
        {
            return window.parent as IWindowContainer;
        }

        string? widgetTag = null;

        foreach (string tag in window.tags)
        {
            if (tag.StartsWith(ROOM_WIDGET, StringComparison.Ordinal))
            {
                widgetTag = tag;
                break;
            }
        }

        return widgetTag == null
            ? null
            : _layoutContainer.GetChildByTag(widgetTag) as IWindowContainer;
    }
}
