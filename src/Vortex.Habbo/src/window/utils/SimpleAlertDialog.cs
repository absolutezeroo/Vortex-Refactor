// @see habbo/window/utils/SimpleAlertDialog.as

using System;
using System.Xml.Linq;

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Utils;

namespace Vortex.Habbo.Window.Utils;

/// <summary>
/// Lightweight alert dialog with title, subtitle, message, illustration, and optional link.
/// </summary>
/// @see habbo/window/utils/SimpleAlertDialog.as
public class SimpleAlertDialog : IDisposable
{
    private const int WINDOW_MARGIN = 10;

    private IModalDialog? _modalDialog;
    private readonly string? _linkUrl;
    private IWindowContainer? _window;
    private IItemListWindow? _mainList;
    private IItemListWindow? _topList;
    private IItemListWindow? _bottomList;
    private IWindow? _messageWindow;
    private IWindow? _subtitleWindow;
    private IWindow? _linkWindow;
    private IWindow? _illustrationWindow;
    private Action? _linkCallback;
    private Action? _closeCallback;
    private HabboWindowManagerComponent? _windowManager;

    /// @see SimpleAlertDialog.as::SimpleAlertDialog
    public SimpleAlertDialog
    (
        HabboWindowManagerComponent windowManager,
        string title, string? subtitle, string message,
        string? linkCaption, string? linkUrl,
        Dictionary<string, object?>? parameters,
        string? illustrationAsset,
        Action? linkCallback, Action? closeCallback
    )
    {
        _linkCallback = linkCallback;
        _closeCallback = closeCallback;
        _windowManager = windowManager;

        // @see SimpleAlertDialog.as — load XML from asset library and build modal dialog
        object? xmlAsset = windowManager.FindAssetByName("simple_alert_xml");

        XElement? xml = null;

        if (xmlAsset is Core.Assets.IAsset { Content: XElement xmlContent })
        {
            xml = xmlContent;
        }

        if (xml != null)
        {
            _modalDialog = ((IHabboWindowManager)windowManager).BuildModalDialogFromXml(xml);
            _window = _modalDialog?.RootWindow() as IWindowContainer;
        }

        if (_window == null)
        {
            return;
        }

        _mainList = _window.FindChildByName("list") as IItemListWindow;
        _topList = _window.FindChildByName("list_top") as IItemListWindow;
        _bottomList = _window.FindChildByName("list_bottom") as IItemListWindow;
        _messageWindow = _window.FindChildByName("message");
        _subtitleWindow = _window.FindChildByName("subtitle");
        _linkWindow = _window.FindChildByName("link");
        _illustrationWindow = _window.FindChildByName("illustration");

        // @see SimpleAlertDialog.as — remove close button
        _window.FindChildByName("header_button_close")?.Destroy();

        _window.procedure = WindowProcedure;
        _window.caption = title;

        if (_messageWindow != null)
        {
            _messageWindow.caption = message;
        }

        // @see SimpleAlertDialog.as — register localization parameters for ${key} strings
        if (parameters != null)
        {
            foreach (string? text in new[]
                     {
                         title,
                         subtitle,
                         message,
                         linkCaption,
                     })
            {
                if (text is not { Length: > 2 } || !text.StartsWith("${", StringComparison.Ordinal) || text.IndexOf('}') <= 0)
                {
                    continue;
                }

                string key = text[2..text.IndexOf('}')];

                foreach (KeyValuePair<string, object?> kvp in parameters)
                {
                    ((IHabboWindowManager)windowManager).RegisterLocalizationParameter(key, kvp.Key, kvp.Value?.ToString() ?? "");
                }
            }
        }

        // @see SimpleAlertDialog.as — subtitle: set or dispose
        if (!string.IsNullOrEmpty(subtitle))
        {
            if (_subtitleWindow != null)
            {
                _subtitleWindow.caption = subtitle;
            }
        }
        else
        {
            _subtitleWindow?.Destroy();
            _subtitleWindow = null;
        }

        // @see SimpleAlertDialog.as — link: set caption + listener, or dispose
        if (!string.IsNullOrEmpty(linkCaption) && (!string.IsNullOrEmpty(linkUrl) || linkCallback != null))
        {
            if (_linkWindow != null)
            {
                _linkWindow.caption = linkCaption;
            }

            _linkUrl = linkUrl;
        }
        else
        {
            _linkWindow?.Destroy();
            _linkWindow = null;
        }

        // @see SimpleAlertDialog.as — illustration: set asset URI, or dispose
        if (!string.IsNullOrEmpty(illustrationAsset))
        {
            if (_illustrationWindow is IStaticBitmapWrapperWindow bmpWindow)
            {
                bmpWindow.AssetUri = illustrationAsset;
            }
        }
        else
        {
            _illustrationWindow?.Destroy();
            _illustrationWindow = null;
        }

        ResizeWindow();
    }

    /// @see SimpleAlertDialog.as::get disposed
    public bool Disposed { get; private set; }

    /// @see SimpleAlertDialog.as::dispose
    public void Dispose()
    {
        if (!Disposed)
        {
            Close();
            _windowManager = null;
            Disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    /// @see SimpleAlertDialog.as::close
    private void Close()
    {
        _closeCallback?.Invoke();

        if (_modalDialog == null)
        {
            return;
        }

        _linkWindow = null;
        _illustrationWindow = null;
        _window = null;
        _mainList = null;
        _topList = null;
        _bottomList = null;
        _messageWindow = null;
        _subtitleWindow = null;
        _linkCallback = null;
        _closeCallback = null;
        _modalDialog.Dispose();
        _modalDialog = null;
    }

    /// @see SimpleAlertDialog.as::windowProcedure
    private void WindowProcedure(WindowEvent evt, IWindow window)
    {
        if (evt.type == WindowMouseEvent.CLICK && window.name == "close_button")
        {
            Dispose();
        }
    }

    /// @see SimpleAlertDialog.as::onSimpleAlertClick
    private void OnSimpleAlertClick(WindowMouseEvent evt)
    {
        if (!string.IsNullOrEmpty(_linkUrl))
        {
            if (_linkUrl!.StartsWith("event:", StringComparison.Ordinal))
            {
                _windowManager?.context?.CreateLinkEvent(_linkUrl[6..]);

                Dispose();
            }
            else
            {
                HabboWebTools.OpenWebPage(_linkUrl, "habboMain");
            }
        }
        else if (_linkCallback != null)
        {
            _linkCallback();
            Dispose();
        }
    }

    /// @see SimpleAlertDialog.as::onIllustrationResized
    private void OnIllustrationResized(WindowEvent evt)
    {
        if (_topList is IWindow topWin && _illustrationWindow != null)
        {
            topWin.x = _illustrationWindow.width + 10;

            if (_bottomList is IWindow bottomWin)
            {
                bottomWin.width = topWin.x + topWin.width;
            }

            if (_window != null)
            {
                _window.width = topWin.x + topWin.width + (2 * WINDOW_MARGIN);
            }
        }
        ResizeWindow();
    }

    /// @see SimpleAlertDialog.as::resizeWindow
    private void ResizeWindow()
    {
        _topList?.ArrangeListItems();
        _bottomList?.ArrangeListItems();
        _mainList?.ArrangeListItems();

        if (_mainList == null || _window == null)
        {
            return;
        }

        _window.height = (_mainList is IWindow mainWin ? mainWin.height : 0) + 40;
        _window.Center();
    }
}
