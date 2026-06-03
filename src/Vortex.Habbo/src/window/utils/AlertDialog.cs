// @see habbo/window/utils/AlertDialog.as

using System;
using System.Xml.Linq;

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Utils;

namespace Vortex.Habbo.Window.Utils;

/// <summary>
/// Base modal alert dialog. Manages title, summary, buttons, and callback dispatch.
/// </summary>
/// @see habbo/window/utils/AlertDialog.as
public class AlertDialog : IClass3348
{
    protected const string LIST_BUTTONS = "_alert_button_list";
    protected const string BUTTON_OK = "_alert_button_ok";
    protected const string BUTTON_CANCEL = "_alert_button_cancel";
    protected const string BUTTON_CUSTOM = "_alert_button_custom";
    protected const string BUTTON_CLOSE = "header_button_close";
    protected const string TEXT_SUMMARY = "_alert_text_summary";

    private static uint _instanceCount;

    protected string _title = "";
    protected string _summary = "";
    protected bool _disposed;
    protected Action<AlertDialog, WindowEvent>? _callback;
    protected IWindowContainer? _window;
    protected IModalDialog? _modalDialog;

    /// @see AlertDialog.as::AlertDialog
    public AlertDialog
    (
        IHabboWindowManager windowManager, XElement xml, string title, string summary,
        uint flags, Action<AlertDialog, WindowEvent>? callback, bool modal
    )
    {
        _instanceCount++;

        if (modal)
        {
            _modalDialog = windowManager.BuildModalDialogFromXml(xml);
            _window = _modalDialog?.RootWindow() as IWindowContainer;
        }
        else
        {
            _window = windowManager.BuildFromXml(xml, 2) as IWindowContainer;
        }

        if (flags == 0)
        {
            flags = 16 | 1 | 2;
        }

        // @see AlertDialog.as — remove buttons not in flags
        if (_window?.FindChildByName(LIST_BUTTONS) is IItemListWindow buttonList)
        {
            if ((flags & 16) == 0)
            {
                buttonList.GetListItemByName(BUTTON_OK)?.Destroy();
            }
            if ((flags & 32) == 0)
            {
                buttonList.GetListItemByName(BUTTON_CANCEL)?.Destroy();
            }
            if ((flags & 64) == 0)
            {
                buttonList.GetListItemByName(BUTTON_CUSTOM)?.Destroy();
            }
        }

        if (_window != null)
        {
            _window.procedure = DialogEventProc;
            _window.Center();
        }

        Title = title;
        Summary = summary;
        _callback = callback;
    }

    /// @see AlertDialog.as::get/set title
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            if (_window != null)
            {
                _window.caption = _title;
            }
        }
    }

    /// @see AlertDialog.as::get/set summary
    public string Summary
    {
        get => _summary;
        set
        {
            _summary = value;

            if (_window?.FindChildByTag(TEXT_SUMMARY) is ITextWindow textWindow)
            {
                textWindow.Text = value;
            }
        }
    }

    /// @see AlertDialog.as::get/set titleBarColor
    public uint TitleBarColor
    {
        get => _window?.color ?? 0;
        set
        {
            if (_window != null)
            {
                _window.color = value;
            }
        }
    }

    /// @see AlertDialog.as::get/set callback
    public Action<AlertDialog, WindowEvent>? CallbackAction
    {
        get => _callback;
        set => _callback = value;
    }

    /// @see AlertDialog.as::get disposed
    public bool Disposed => _disposed;

    /// @see AlertDialog.as::dispose
    public virtual void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_modalDialog is { disposed: false })
        {
            _modalDialog.Dispose();
            _modalDialog = null;
            _window = null;
        }

        if (_window is { disposed: false })
        {
            _window.Destroy();
            _window = null;
        }

        _callback = null;
        _disposed = true;
    }

    /// @see AlertDialog.as::dialogEventProc
    protected virtual void DialogEventProc(WindowEvent evt, IWindow window)
    {
        if (evt.type != WindowMouseEvent.CLICK)
        {
            return;
        }

        switch (window.name)
        {
            case BUTTON_OK:
                if (_callback != null)
                {
                    WindowEvent okEvt = WindowEvent.Allocate(WindowEvent.WE_OK, null, null);
                    _callback(this, okEvt);
                }
                else
                {
                    Dispose();
                }
                break;
            case BUTTON_CLOSE:
            case BUTTON_CANCEL:
                if (_callback != null)
                {
                    WindowEvent cancelEvt = WindowEvent.Allocate(WindowEvent.WE_CANCEL, null, null);
                    _callback(this, cancelEvt);
                }
                else
                {
                    Dispose();
                }
                break;
        }
    }

    // IClass3348 typed property implementations
    string? IClass3348.Title
    {
        get => _title;
        set => Title = value ?? "";
    }

    string? IClass3348.Summary
    {
        get => _summary;
        set => Summary = value ?? "";
    }

    object? IClass3348.Callback
    {
        get => _callback;
        set => _callback = value as Action<AlertDialog, WindowEvent>;
    }

    uint IClass3348.TitleBarColor
    {
        get => TitleBarColor;
        set => TitleBarColor = value;
    }

    IClass3562? IClass3348.GetButtonCaption(int buttonFlag)
    {
        return GetButtonCaption(buttonFlag);
    }

    void IClass3348.SetButtonCaption(int buttonFlag, IClass3562? caption)
    {
        SetButtonCaption(buttonFlag, caption as AlertDialogCaption);
    }

    /// @see AlertDialog.as::getButtonCaption
    public AlertDialogCaption? GetButtonCaption(int buttonFlag)
    {
        if (_disposed || _window == null)
        {
            return null;
        }

        string? buttonName = buttonFlag switch
        {
            16 => BUTTON_OK,
            32 => BUTTON_CANCEL,
            64 => BUTTON_CUSTOM,
            _ => null,
        };

        if (buttonName == null)
        {
            return null;
        }

        IWindow? button = _window.FindChildByName(buttonName);

        if (button == null)
        {
            return null;
        }

        return new AlertDialogCaption(
            button.caption,
            (button as IInteractiveWindow)?.ToolTipCaption,
            button.visible
        );
    }

    /// @see AlertDialog.as::setButtonCaption
    public void SetButtonCaption(int buttonFlag, AlertDialogCaption? caption)
    {
        if (_disposed || _window == null || caption == null)
        {
            return;
        }

        string? buttonName = buttonFlag switch
        {
            16 => BUTTON_OK,
            32 => BUTTON_CANCEL,
            64 => BUTTON_CUSTOM,
            _ => null,
        };

        if (buttonName == null)
        {
            return;
        }

        IWindow? button = _window.FindChildByName(buttonName);

        if (button != null)
        {
            button.caption = caption.Text ?? "";
        }
    }
}
