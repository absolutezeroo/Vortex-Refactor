// @see com.sulake.habbo.toolbar.extensions.VideoOfferExtension

using Vortex.Core.Assets;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Toolbar.Extensions;

/// @see com.sulake.habbo.toolbar.extensions.VideoOfferExtension
public class VideoOfferExtension
{
    private static readonly uint LINK_COLOR_NORMAL = 0xFFFFFF;
    private static readonly uint LINK_COLOR_HIGHLIGHT = 0xBAFFB9;
    private static readonly uint CLOSE_COLOR_NORMAL = 0x666666;
    private static readonly uint CLOSE_COLOR_OVER = 0xCCCCCC;

    private HabboToolbar? _toolbar;
    private IWindowContainer? _window;
    private IRegionWindow? _textRegion;
    private IIconWindow? _closeIcon;
    private bool _dismissed;

    // TODO(as3-port): IHabboCatalog.videoOffers / IVideoOfferLauncher — not ported yet

    /// @see VideoOfferExtension.as::VideoOfferExtension
    public VideoOfferExtension(HabboToolbar toolbar)
    {
        _toolbar = toolbar;
    }

    /// @see VideoOfferExtension.as::get window
    public IWindow? window => _window;

    /// @see VideoOfferExtension.as::offersAvailable
    public void OffersAvailable(int count)
    {
        if (_toolbar == null)
        {
            return;
        }

        if (count <= 0 || _dismissed)
        {
            if (_window != null)
            {
                DestroyWindow();
            }

            return;
        }

        if (_window == null)
        {
            _window = CreateWindow();
        }
    }

    /// @see VideoOfferExtension.as::dispose
    public void Dispose()
    {
        if (_toolbar == null)
        {
            return;
        }

        _toolbar.extensionView?.DetachExtension("video_offer");
        DestroyWindow();
        _toolbar = null;
    }

    private IWindowContainer? CreateWindow()
    {
        if (_toolbar?.WindowManager == null)
        {
            return null;
        }

        XmlAsset? xmlAsset = (_toolbar.assets as IAssetLibrary)?.GetAssetByName("video_offer_promotion_xml") as XmlAsset;
        System.Xml.Linq.XElement? layoutXml = xmlAsset?.Content as System.Xml.Linq.XElement
            ?? HabboAssetResolver.LoadXmlAsset("video_offer_promotion_xml");

        if (layoutXml == null)
        {
            return null;
        }

        IWindowContainer? win = _toolbar.WindowManager.BuildFromXml(layoutXml, 1) as IWindowContainer;

        if (win == null)
        {
            return null;
        }

        // TODO(as3-port): set promo_text via ILocalization — use fallback text
        IWindow? promoText = win.FindChildByName("promo_text");
        IWindow? promoTextShadow = win.FindChildByName("promo_text_shadow");
        string text = "Watch a video and earn a credit!";
        if (promoText != null)
        {
            promoText.caption = text;
        }

        if (promoTextShadow != null)
        {
            promoTextShadow.caption = text;
        }

        // TODO(as3-port): set promo_icon bitmap from assets

        _textRegion = win.FindChildByName("text_region") as IRegionWindow;
        if (_textRegion != null)
        {
            _textRegion.AddEventListener(WindowMouseEvent.CLICK, OnTextRegionClicked);
            _textRegion.AddEventListener(WindowMouseEvent.OVER, OnTextRegionOver);
            _textRegion.AddEventListener(WindowMouseEvent.OUT, OnTextRegionOut);
        }

        _closeIcon = win.FindChildByName("promo_close_icon") as IIconWindow;
        if (_closeIcon != null)
        {
            _closeIcon.AddEventListener(WindowMouseEvent.CLICK, OnCloseClicked);
            _closeIcon.AddEventListener(WindowMouseEvent.OVER, OnCloseOver);
            _closeIcon.AddEventListener(WindowMouseEvent.OUT, OnCloseOut);
        }

        _toolbar.extensionView?.AttachExtension("video_offer", win, 10);

        return win;
    }

    private void DestroyWindow()
    {
        if (_window == null)
        {
            return;
        }

        if (_textRegion != null)
        {
            _textRegion.RemoveEventListener(WindowMouseEvent.CLICK, OnTextRegionClicked);
            _textRegion.RemoveEventListener(WindowMouseEvent.OVER, OnTextRegionOver);
            _textRegion.RemoveEventListener(WindowMouseEvent.OUT, OnTextRegionOut);
            _textRegion = null;
        }

        if (_closeIcon != null)
        {
            _closeIcon.RemoveEventListener(WindowMouseEvent.CLICK, OnCloseClicked);
            _closeIcon.RemoveEventListener(WindowMouseEvent.OVER, OnCloseOver);
            _closeIcon.RemoveEventListener(WindowMouseEvent.OUT, OnCloseOut);
            _closeIcon = null;
        }

        _window.Destroy();
        _window = null;
    }

    private void OnCloseClicked(WindowEvent ev, IWindow window)
    {
        _dismissed = true;
        DestroyWindow();
        // TODO(as3-port): send EventLogMessageComposer("SuperSaverAds","client_action","supersaverads.video.promo.close_clicked")
    }

    private void OnCloseOver(WindowEvent ev, IWindow window)
    {
        if (_closeIcon != null)
        {
            _closeIcon.color = CLOSE_COLOR_OVER;
        }
    }

    private void OnCloseOut(WindowEvent ev, IWindow window)
    {
        if (_closeIcon != null)
        {
            _closeIcon.color = CLOSE_COLOR_NORMAL;
        }
    }

    private void OnTextRegionClicked(WindowEvent ev, IWindow window)
    {
        // TODO(as3-port): IHabboCatalog.videoOffers.launch(VideoOfferTypeEnum.CREDIT) — not ported
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
