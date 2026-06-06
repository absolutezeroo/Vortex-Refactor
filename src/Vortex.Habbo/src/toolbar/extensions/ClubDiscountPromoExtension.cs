// @see com.sulake.habbo.toolbar.extensions.ClubDiscountPromoExtension

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Toolbar.Extensions;

/// @see com.sulake.habbo.toolbar.extensions.ClubDiscountPromoExtension
public class ClubDiscountPromoExtension
{
    private static readonly uint LINK_COLOR_NORMAL = 0xFFFFFF;
    private static readonly uint LINK_COLOR_HIGHLIGHT = 0xBAFFB9;
    private static readonly int ICON_STYLE_VIP = 14;

    private HabboToolbar? _toolbar;
    private IWindowContainer? _window;
    private bool _disposed;

    // TODO(as3-port): IBitmapWrapperWindow _flashingAnimation — animation effect not ported
    // TODO(as3-port): IHabboInventory.clubIsExpiring / clubLevel / clubMinutesUntilExpiration — not ported

    /// @see ClubDiscountPromoExtension.as::ClubDiscountPromoExtension
    public ClubDiscountPromoExtension(HabboToolbar toolbar)
    {
        _toolbar = toolbar;
    }

    /// @see ClubDiscountPromoExtension.as::dispose
    public void Dispose()
    {
        if (_disposed || _toolbar == null)
        {
            return;
        }

        _toolbar.extensionView?.DetachExtension(ToolbarDisplayExtensionIds.CLUB_PROMO);
        DestroyWindow();
        _toolbar = null;
        _disposed = true;
    }

    /// @see ClubDiscountPromoExtension.as::onClubChanged
    public void OnClubChanged()
    {
        // TODO(as3-port): IHabboInventory.clubIsExpiring, clubLevel, clubMinutesUntilExpiration — not ported
        // When available: if (clubIsExpiring && _window == null && IsExtensionEnabled()) { create/attach }
        // else { detach/destroy }
    }

    private IWindowContainer? CreateWindow()
    {
        if (_toolbar?.WindowManager == null)
        {
            return null;
        }

        var xmlAsset = (_toolbar.assets as Core.Assets.IAssetLibrary)?.GetAssetByName("club_discount_promotion_xml") as Core.Assets.XmlAsset;
        System.Xml.Linq.XElement? layoutXml = xmlAsset?.Content as System.Xml.Linq.XElement
            ?? HabboAssetResolver.LoadXmlAsset("club_discount_promotion_xml");

        if (layoutXml == null)
        {
            return null;
        }

        IWindowContainer? win = _toolbar.WindowManager.BuildFromXml(layoutXml, 1) as IWindowContainer;

        if (win == null)
        {
            return null;
        }

        // TODO(as3-port): set flashing_animation bitmap from assets, set up animation timers
        IRegionWindow? textRegion = win.FindChildByName("text_region") as IRegionWindow;
        textRegion?.AddEventListener(WindowMouseEvent.CLICK, OnTextRegionClicked);
        textRegion?.AddEventListener(WindowMouseEvent.OVER, OnTextRegionOver);
        textRegion?.AddEventListener(WindowMouseEvent.OUT, OnTextRegionOut);

        AssignState(win);

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

    private void AssignState(IWindowContainer win)
    {
        // TODO(as3-port): IHabboInventory.clubLevel — not ported; use neutral state
        SetText(win, "${discount.bar.vip.expiring}");
        SetClubIcon(win, ICON_STYLE_VIP);
        // TODO(as3-port): animate(true) — animation timers not ported
    }

    private bool IsExtensionEnabled()
    {
        // TODO(as3-port): IHabboInventory.clubLevel == 2 && toolbar.getBoolean("club.membership.extend.vip.promotion.enabled")
        return false;
    }

    private static void SetText(IWindowContainer win, string text)
    {
        IWindow? promoText = win.FindChildByName("promo_text");
        IWindow? promoTextShadow = win.FindChildByName("promo_text_shadow");
        if (promoText != null)
        {
            promoText.caption = text;
        }

        if (promoTextShadow != null)
        {
            promoTextShadow.caption = text;
        }
    }

    private static void SetClubIcon(IWindowContainer win, int style)
    {
        if (win.FindChildByName("club_icon") is IIconWindow icon)
        {
            icon.style = (uint)style;
            icon.Invalidate();
        }
    }

    private void OnTextRegionClicked(WindowEvent ev, IWindow window)
    {
        // TODO(as3-port): IHabboInventory.clubLevel check + send EventLogMessageComposer + class_930 — not ported
    }

    private void OnTextRegionOver(WindowEvent ev, IWindow window)
    {
        IWindow? text = _window?.FindChildByName("promo_text");
        if (text != null)
        {
            text.color = LINK_COLOR_HIGHLIGHT;
        }
    }

    private void OnTextRegionOut(WindowEvent ev, IWindow window)
    {
        IWindow? text = _window?.FindChildByName("promo_text");
        if (text != null)
        {
            text.color = LINK_COLOR_NORMAL;
        }
    }
}
