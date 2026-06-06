using Vortex.Core.Window;

namespace Vortex.Habbo.Toolbar;

/// @see com.sulake.habbo.toolbar.IExtensionView
public interface IExtensionView
{
    /// @see IExtensionView.as::get/set visible
    bool visible { get; set; }

    /// @see IExtensionView.as::get screenHeight
    uint screenHeight { get; }

    /// @see IExtensionView.as::attachExtension
    void AttachExtension(string id, IWindow window, int slot = -1, string[]? beforeIds = null);

    /// @see IExtensionView.as::detachExtension
    void DetachExtension(string id);

    /// @see IExtensionView.as::hasExtension
    bool HasExtension(string id);

    /// @see IExtensionView.as::get/set extraMargin
    int extraMargin { get; set; }

    /// @see IExtensionView.as::refreshItemWindow
    void RefreshItemWindow();
}
