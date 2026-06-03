// @see habbo/window/utils/AlertDialogCaption.as

using Vortex.Core.Window.Utils;

namespace Vortex.Habbo.Window.Utils;

/// <summary>
/// Simple data holder for alert dialog button caption, tooltip, and visibility.
/// </summary>
/// @see habbo/window/utils/AlertDialogCaption.as
public class AlertDialogCaption : IClass3562
{
    /// @see AlertDialogCaption.as::AlertDialogCaption
    public AlertDialogCaption(string? text, string? toolTip, bool visible)
    {
        Text = text;
        ToolTip = toolTip;
        Visible = visible;
    }

    public string? Text { get; set; }

    public string? ToolTip { get; set; }

    public bool Visible { get; set; }
}
