// @see com.sulake.habbo.toolbar.BottomBackgroundBorder

using System.Xml.Linq;

using Vortex.Core.Assets;
using Vortex.Core.Window;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Window;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Toolbar;

/// @see com.sulake.habbo.toolbar.BottomBackgroundBorder
public class BottomBackgroundBorder
{
    private IWindowContainer? _window;
    private bool _disposed;

    /// @see BottomBackgroundBorder.as::BottomBackgroundBorder
    public BottomBackgroundBorder(HabboToolbar toolbar)
    {
        XmlAsset? xmlAsset = (toolbar.assets as IAssetLibrary)?.GetAssetByName("bottom_background_border_xml") as XmlAsset;
        XElement? layoutXml = xmlAsset?.Content as XElement
            ?? HabboAssetResolver.LoadXmlAsset("bottom_background_border_xml");

        if (layoutXml == null)
        {
            return;
        }

        _window = toolbar.WindowManager?.BuildFromXml(layoutXml) as IWindowContainer;

        if (_window == null)
        {
            return;
        }

        _window.procedure = OnWindowEvent;
        UpdatePosition();
    }

    /// @see BottomBackgroundBorder.as::dispose
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_window != null)
        {
            _window.Destroy();
            _window = null;
        }

        _disposed = true;
    }

    /// @see BottomBackgroundBorder.as::get disposed
    public bool disposed => _disposed;

    private void OnWindowEvent(WindowEvent ev, IWindow window)
    {
        if (ev.type == WindowEvent.WE_PARENT_RESIZED)
        {
            UpdatePosition();
        }
    }

    /// @see BottomBackgroundBorder.as::updatePosition
    private void UpdatePosition()
    {
        if (_window == null)
        {
            return;
        }

        // @see BottomBackgroundBorder.as::updatePosition — position relative to desktop bottom
        IWindow? desktop = _window.parent;

        if (desktop == null)
        {
            return;
        }

        _window.x = -10;
        _window.y = desktop.height - (_window.height - 3);
        _window.width = desktop.width + 20;
    }
}
