// @see com.sulake.habbo.toolbar.extensions.purse.PurseClubArea

using Vortex.Core.Assets;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Habbo.Window;

namespace Vortex.Habbo.Toolbar.Extensions.Purse;

/// @see com.sulake.habbo.toolbar.extensions.purse.PurseClubArea
public class PurseClubArea : CurrencyIndicatorBase
{
    private static readonly uint BG_COLOR_LIGHT = 4286084205u;
    private static readonly uint BG_COLOR_DARK = 4283781966u;
    private static readonly int ICON_STYLE_CLUB = 13;
    private static readonly int ICON_STYLE_VIP = 14;
    private static readonly string[] ICON_ANIMATION = ["toolbar_hc_icon_0", "toolbar_hc_icon_1", "toolbar_hc_icon_2", "toolbar_hc_icon_1", "toolbar_hc_icon_0"];

    private int _previousDays = -1;
    private HabboToolbar? _toolbar;

    /// @see PurseClubArea.as::PurseClubArea — uses parent window (purse container), not its own XML
    public PurseClubArea(HabboToolbar toolbar, IWindowContainer parentWindow)
        : base(toolbar.WindowManager!, toolbar.assets as IAssetLibrary)
    {
        _toolbar = toolbar;
        _window = parentWindow;

        bgColorLight = BG_COLOR_LIGHT;
        bgColorDark = BG_COLOR_DARK;
        textElementName = "days";
        iconAnimationSequence = ICON_ANIMATION;
        iconAnimationDelay = 50;

        // TODO(as3-port): ILocalization.getLocalization("purse.clubdays.zero.amount.text") — use fallback
        amountZeroText = "Get";

        // @see PurseClubArea.as — initial state render
        OnClubChanged();
    }

    /// @see PurseClubArea.as::setAmount
    protected override void SetAmount(int amount, int minutes = -1)
    {
        if (_window == null)
        {
            return;
        }

        if (amount < 1)
        {
            _window.FindChildByName("days")!.visible = false;
            IWindow? join = _window.FindChildByName("join");
            if (join != null)
            {
                join.visible = true;
            }
            textElementName = "join";
            SetText(amountZeroText ?? "Get");
        }
        else
        {
            IWindow? days = _window.FindChildByName("days");
            if (days != null)
            {
                days.visible = true;
            }
            IWindow? join = _window.FindChildByName("join");
            if (join != null)
            {
                join.visible = false;
            }
            textElementName = "days";

            // TODO(as3-port): FriendlyTime.GetShortFriendlyTime — not ported; show raw days
            if (minutes != -1 && minutes < 1440)
            {
                SetText($"{minutes / 60}h");
            }
            else
            {
                SetText(amount.ToString());
            }
        }
    }

    private void SetClubIcon(int style)
    {
        if (_window?.FindChildByName("club_icon") is IIconWindow iconWin)
        {
            iconWin.style = (uint)style;
            iconWin.Invalidate();
        }
    }

    /// @see PurseClubArea.as::dispose
    public override void Dispose()
    {
        _toolbar = null;
        // do NOT destroy _window — it's owned by PurseAreaExtension
        _window = null;
    }

    /// @see PurseClubArea.as::onClubChanged
    public void OnClubChanged()
    {
        // TODO(as3-port): IHabboInventory.clubPeriods / clubDays / clubMinutesUntilExpiration / clubLevel — not ported yet
        // Render neutral "no club" state until inventory is available.
        SetClubIcon(ICON_STYLE_VIP);
        SetText(amountZeroText ?? "Get");
    }
}
