// @see habbo/window/utils/AlertDialogWithLink.as

using System;
using System.Xml.Linq;

using Vortex.Core.Window;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Utils;

namespace Vortex.Habbo.Window.Utils;

/// <summary>
/// Alert dialog with an additional clickable link button.
/// </summary>
/// @see habbo/window/utils/AlertDialogWithLink.as
public class AlertDialogWithLink : AlertDialog, IAlertDialogWithLink
{
    private string _linkTitle = "";

    /// @see AlertDialogWithLink.as::AlertDialogWithLink
    public AlertDialogWithLink
    (
        IHabboWindowManager windowManager, XElement xml,
        string title, string summary, string linkTitle, string linkUrl,
        uint flags, Action<AlertDialog, WindowEvent>? callback
    )
        : base(windowManager, xml, title, summary, flags, callback, false)
    {
        LinkTitle = linkTitle;
        LinkUrl = linkUrl;
    }

    /// @see AlertDialogWithLink.as::get/set linkTitle
    public string LinkTitle
    {
        get => _linkTitle;
        set
        {
            _linkTitle = value;
            IWindow? link = _window?.FindChildByTag("LINK");

            if (link != null)
            {
                link.caption = value;
            }
        }
    }

    /// @see AlertDialogWithLink.as::get/set linkUrl
    public string LinkUrl { get; set; } = "";

    // IAlertDialogWithLink — public properties directly satisfy the interface
    string? IAlertDialogWithLink.LinkTitle
    {
        get => LinkTitle;
        set => LinkTitle = value ?? "";
    }

    string? IAlertDialogWithLink.LinkUrl
    {
        get => LinkUrl;
        set => LinkUrl = value ?? "";
    }

    /// @see AlertDialogWithLink.as::dialogEventProc
    protected override void DialogEventProc(WindowEvent evt, IWindow window)
    {
        if (evt.type == WindowMouseEvent.CLICK && window.name == "_alert_button_link")
        {
            Habbo.Utils.HabboWebTools.OpenWebPage(LinkUrl, "_empty");

            return;
        }

        base.DialogEventProc(evt, window);
    }
}
