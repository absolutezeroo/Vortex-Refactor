// @see habbo/window/utils/ConfirmDialog.as

using System;
using System.Xml.Linq;

using Vortex.Core.Window;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Utils;

namespace Vortex.Habbo.Window.Utils;

/// <summary>
/// Confirm dialog variant. Unlike AlertDialog, does NOT auto-dispose on button clicks;
/// always delegates to callback for both OK and Cancel.
/// </summary>
/// @see habbo/window/utils/ConfirmDialog.as
public class ConfirmDialog : AlertDialog, IClass3441
{
    /// @see ConfirmDialog.as::ConfirmDialog
    public ConfirmDialog
    (
        IHabboWindowManager windowManager, XElement xml,
        string title, string summary, uint flags,
        Action<AlertDialog, WindowEvent>? callback, bool modal
    )
        : base(windowManager, xml, title, summary, flags, callback, modal)
    {
    }

    /// @see ConfirmDialog.as::dialogEventProc
    protected override void DialogEventProc(WindowEvent evt, IWindow window)
    {
        if (evt.type != WindowMouseEvent.CLICK)
        {
            return;
        }

        switch (window.name)
        {
            case BUTTON_OK:
                if (CallbackAction != null)
                {
                    WindowEvent okEvt = WindowEvent.Allocate(WindowEvent.WE_OK, null, null);
                    CallbackAction(this, okEvt);
                }
                break;
            case BUTTON_CANCEL:
            case BUTTON_CLOSE:
                if (CallbackAction != null)
                {
                    WindowEvent cancelEvt = WindowEvent.Allocate(WindowEvent.WE_CANCEL, null, null);
                    CallbackAction(this, cancelEvt);
                }
                break;
        }
    }
}
