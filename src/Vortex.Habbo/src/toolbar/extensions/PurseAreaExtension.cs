// @see com.sulake.habbo.toolbar.extensions.PurseAreaExtension

using Vortex.Core.Assets;
using Vortex.Core.Window;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Toolbar.Extensions.Purse;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Toolbar.Extensions;

/// @see com.sulake.habbo.toolbar.extensions.PurseAreaExtension
public class PurseAreaExtension
{
    private HabboToolbar? _toolbar;
    private IWindowContainer? _window;
    private PurseClubArea? _clubArea;

    // TODO(as3-port): IHabboCatalog — not ported yet; PurseAreaExtension depends on catalog for credit/point values

    /// @see PurseAreaExtension.as::PurseAreaExtension
    public PurseAreaExtension(HabboToolbar toolbar)
    {
        _toolbar = toolbar;

        XmlAsset? xmlAsset = (toolbar.assets as IAssetLibrary)?.GetAssetByName("purse_xml") as XmlAsset;
        System.Xml.Linq.XElement? layoutXml = xmlAsset?.Content as System.Xml.Linq.XElement
            ?? HabboAssetResolver.LoadXmlAsset("purse_xml");

        if (layoutXml == null || toolbar.WindowManager == null)
        {
            return;
        }

        _window = toolbar.WindowManager.BuildFromXml(layoutXml) as IWindowContainer;

        if (_window == null)
        {
            return;
        }

        _window.procedure = WindowProcedure;
        _clubArea = new PurseClubArea(toolbar, _window);

        // TODO(as3-port): catalog events (credit/point balance) — not ported
        // TODO(as3-port): updateCreditAndPointValues() — needs IHabboCatalog.getPurse()

        // @see PurseAreaExtension.as — attach to extension view at slot 0
        toolbar.extensionView?.AttachExtension(ToolbarDisplayExtensionIds.const_1144, _window, ExtensionFixedSlotsEnum.SLOT_PURSE);

        // @see PurseAreaExtension.as — register hint for credit_count
        IWindow? creditCount = _window.FindChildByName("credit_count");
        if (creditCount != null)
        {
            toolbar.WindowManager.RegisterHintWindow("credit_count", creditCount);
        }
    }

    /// @see PurseAreaExtension.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        _clubArea?.Dispose();
        _clubArea = null;
        _toolbar = null;
    }

    /// @see PurseAreaExtension.as::get disposed
    public bool disposed => _toolbar == null;

    /// @see PurseAreaExtension.as::getClubArea
    public PurseClubArea? GetClubArea() => _clubArea;

    private void WindowProcedure(WindowEvent ev, IWindow window)
    {
        if (ev.type != WindowMouseEvent.CLICK)
        {
            return;
        }

        _toolbar?.WindowManager?.HideMatchingHint(window.name ?? string.Empty);

        switch (window.name)
        {
            case "vault_button":
                // TODO(as3-port): IHabboCatalog.openVault() — not ported
                break;
            case "hc_join_button":
                // TODO(as3-port): IHabboCatalog.openClubCenter() — not ported
                break;
            case "help_button":
                _toolbar?.ToggleWindowVisibility("HELP");
                break;
            case "settings_button":
                // TODO(as3-port): toggleSettingVisibility() — not ported
                break;
            case "credit_count_button":
                // TODO(as3-port): IHabboCatalog.openCreditsHabblet() — not ported
                break;
            case "ducket_count_button":
                // TODO(as3-port): IHabboCatalog.openCatalogPage("ducket_info") — not ported
                break;
            case "diamond_count_button":
                // TODO(as3-port): IHabboCatalog.openCatalogPage("loyalty_info") — not ported
                break;
            case "logout_button":
                // TODO(as3-port): toolbar.reboot() — not ported
                break;
        }
    }
}
