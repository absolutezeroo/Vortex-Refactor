// @see habbo/window/HintTarget.as

using Vortex.Core.Window;

namespace Vortex.Habbo.Window;

/// <summary>
/// Data class holding a registered hint target: the window, its key, and display style.
/// </summary>
/// @see habbo/window/HintTarget.as
public class HintTarget
{
    /// @see HintTarget.as::HintTarget
    public HintTarget(IWindow? window, string? key, int style)
    {
        Window = window;
        Key = key;
        Style = style;
    }

    /// @see HintTarget.as::get/set window
    public IWindow? Window { get; set; }

    /// @see HintTarget.as::get/set key
    public string? Key { get; set; }

    /// @see HintTarget.as::get/set style
    public int Style { get; set; }
}
