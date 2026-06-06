// @see com.sulake.habbo.toolbar.extensions.CitizenshipVipQuestsPromoExtension

using Vortex.Core.Assets;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Toolbar.Extensions;

/// @see com.sulake.habbo.toolbar.extensions.CitizenshipVipQuestsPromoExtension
public class CitizenshipVipQuestsPromoExtension
{
    private HabboToolbar? _toolbar;
    private IClass3437? _window;
    private bool _disposed;
    private bool _expanded = true;
    private int _expandedHeight = 216;
    private string _vipQuestsCampaignName = string.Empty;

    // TODO(as3-port): class_256 (PerkAllowancesMessageEvent) — not ported yet

    /// @see CitizenshipVipQuestsPromoExtension.as::CitizenshipVipQuestsPromoExtension
    public CitizenshipVipQuestsPromoExtension(HabboToolbar toolbar)
    {
        _toolbar = toolbar;
        _vipQuestsCampaignName = toolbar.GetProperty("citizenship.vip.tutorial.quest.campaign.name");

        // TODO(as3-port): register class_256 (PerkAllowancesMessageEvent) on the connection
    }

    /// @see CitizenshipVipQuestsPromoExtension.as::dispose
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // TODO(as3-port): remove class_256 message event when ported
        DestroyWindow();
        _toolbar = null;
        _disposed = true;
    }

    private IClass3437? CreateWindow()
    {
        if (_toolbar?.WindowManager == null)
        {
            return null;
        }

        XmlAsset? xmlAsset = (_toolbar.assets as IAssetLibrary)?.GetAssetByName("vip_quests_promo_xml") as XmlAsset;
        System.Xml.Linq.XElement? layoutXml = xmlAsset?.Content as System.Xml.Linq.XElement
            ?? HabboAssetResolver.LoadXmlAsset("vip_quests_promo_xml");

        if (layoutXml == null)
        {
            return null;
        }

        IClass3437? win = _toolbar.WindowManager.BuildFromXml(layoutXml, 1) as IClass3437;

        if (win == null)
        {
            return null;
        }

        IRegionWindow? questsBtn = win.FindChildByName("quests_button") as IRegionWindow;
        questsBtn?.AddEventListener(WindowMouseEvent.CLICK, OnQuestsClicked);

        IRegionWindow? minimizeRegion = win.FindChildByName("minimize_region") as IRegionWindow;
        minimizeRegion?.AddEventListener(WindowMouseEvent.CLICK, OnMinMax);

        IRegionWindow? maximizeRegion = win.FindChildByName("maximize_region") as IRegionWindow;
        maximizeRegion?.AddEventListener(WindowMouseEvent.CLICK, OnMinMax);

        _expandedHeight = (int)win.height;

        return win;
    }

    private void DestroyWindow()
    {
        _toolbar?.extensionView?.DetachExtension(ToolbarDisplayExtensionIds.VIP_QUESTS);

        if (_window != null)
        {
            _window.Destroy();
            _window = null;
        }
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

    /// @see CitizenshipVipQuestsPromoExtension.as::onCitizenshipQuestPromoEnabled — called when server enables promo
    public void OnCitizenshipQuestPromoEnabled()
    {
        if (_window == null)
        {
            _window = CreateWindow();
        }

        AssignState();
        _toolbar?.extensionView?.DetachExtension(ToolbarDisplayExtensionIds.CLUB_PROMO);

        if (_window != null)
        {
            _toolbar?.extensionView?.AttachExtension(ToolbarDisplayExtensionIds.VIP_QUESTS, _window, 10);
        }
    }

    private void OnQuestsClicked(WindowEvent ev, IWindow window)
    {
        // TODO(as3-port): send class_748(_vipQuestsCampaignName) — quest activation composer not ported
        DestroyWindow();
    }

    private void OnMinMax(WindowEvent ev, IWindow window)
    {
        _expanded = !_expanded;
        AssignState();
    }
}
