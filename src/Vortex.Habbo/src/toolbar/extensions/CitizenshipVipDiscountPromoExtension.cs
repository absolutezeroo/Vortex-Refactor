// @see com.sulake.habbo.toolbar.extensions.CitizenshipVipDiscountPromoExtension

using Vortex.Core.Assets;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Toolbar.Extensions;

/// @see com.sulake.habbo.toolbar.extensions.CitizenshipVipDiscountPromoExtension
public class CitizenshipVipDiscountPromoExtension
{
    private HabboToolbar? _toolbar;
    private IClass3437? _window;
    private bool _expanded = true;
    private int _expandedHeight = 216;

    // TODO(as3-port): IHabboInventory.citizenshipVipIsExpiring / clubLevel / clubMinutesUntilExpiration — not ported yet

    /// @see CitizenshipVipDiscountPromoExtension.as::CitizenshipVipDiscountPromoExtension
    public CitizenshipVipDiscountPromoExtension(HabboToolbar toolbar)
    {
        _toolbar = toolbar;
    }

    /// @see CitizenshipVipDiscountPromoExtension.as::dispose
    public void Dispose()
    {
        if (_toolbar == null)
        {
            return;
        }

        _toolbar.extensionView?.DetachExtension(ToolbarDisplayExtensionIds.CLUB_PROMO);
        DestroyWindow();
        _toolbar = null;
    }

    /// @see CitizenshipVipDiscountPromoExtension.as::onClubChanged
    public void OnClubChanged()
    {
        // TODO(as3-port): IHabboInventory.citizenshipVipIsExpiring, clubLevel, clubMinutesUntilExpiration — not ported
        // When available: if (citizenshipVipIsExpiring && _window == null && IsExtensionEnabled()) { create/attach }
        // else { detach/destroy }
    }

    private IClass3437? CreateWindow()
    {
        if (_toolbar?.WindowManager == null)
        {
            return null;
        }

        XmlAsset? xmlAsset = (_toolbar.assets as IAssetLibrary)?.GetAssetByName("vip_discount_promotion_v2_xml") as XmlAsset;
        System.Xml.Linq.XElement? layoutXml = xmlAsset?.Content as System.Xml.Linq.XElement
            ?? HabboAssetResolver.LoadXmlAsset("vip_discount_promotion_v2_xml");

        if (layoutXml == null)
        {
            return null;
        }

        IClass3437? win = _toolbar.WindowManager.BuildFromXml(layoutXml, 1) as IClass3437;

        if (win == null)
        {
            return null;
        }

        IRegionWindow? extendBtn = win.FindChildByName("extend_button") as IRegionWindow;
        extendBtn?.AddEventListener(WindowMouseEvent.CLICK, OnExtendClicked);

        IRegionWindow? minimizeRegion = win.FindChildByName("minimize_region") as IRegionWindow;
        minimizeRegion?.AddEventListener(WindowMouseEvent.CLICK, OnMinMax);

        IRegionWindow? maximizeRegion = win.FindChildByName("maximize_region") as IRegionWindow;
        maximizeRegion?.AddEventListener(WindowMouseEvent.CLICK, OnMinMax);

        _expandedHeight = (int)win.height;

        return win;
    }

    private void DestroyWindow()
    {
        if (_window == null)
        {
            return;
        }

        _window.Destroy();
        _window = null;
    }

    private void AssignState()
    {
        if (_window == null)
        {
            return;
        }

        IWindow? content = _window.FindChildByName("content_itemlist");
        IWindow? img = _window.FindChildByName("promo_img");

        if (content != null)
        {
            content.visible = _expanded;
        }

        if (img != null)
        {
            img.visible = _expanded;
        }

        _window.height = _expanded ? _expandedHeight : 33;
    }

    private bool IsExtensionEnabled()
    {
        // TODO(as3-port): IHabboInventory.clubLevel == 2 && toolbar.getBoolean("club.membership.extend.vip.promotion.enabled")
        return false;
    }

    private void OnExtendClicked(WindowEvent ev, IWindow window)
    {
        // TODO(as3-port): IHabboInventory.clubLevel check + send EventLogMessageComposer + class_930 — not ported
    }

    private void OnMinMax(WindowEvent ev, IWindow window)
    {
        _expanded = !_expanded;
        AssignState();
    }
}
