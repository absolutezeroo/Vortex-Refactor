// @see com.sulake.habbo.toolbar.extensions.purse.indicators.SeasonalCurrencyIndicator

using Vortex.Core.Assets;
using Vortex.Core.Window.Components;
using Vortex.Habbo.Window;

namespace Vortex.Habbo.Toolbar.Extensions.Purse.Indicators;

/// @see com.sulake.habbo.toolbar.extensions.purse.indicators.SeasonalCurrencyIndicator
public class SeasonalCurrencyIndicator : CurrencyIndicatorBase
{
    private static readonly uint BG_COLOR_LIGHT = 4286084205u;
    private static readonly uint BG_COLOR_DARK = 4283781966u;

    private HabboToolbar? _toolbar;
    private int _lastBalance = -1;

    // TODO(as3-port): IHabboCatalog — not ported yet

    /// @see SeasonalCurrencyIndicator.as::SeasonalCurrencyIndicator
    public SeasonalCurrencyIndicator(HabboToolbar toolbar, IHabboWindowManager windowManager, IAssetLibrary? assets)
        : base(windowManager, assets)
    {
        _toolbar = toolbar;

        bgColorLight = BG_COLOR_LIGHT;
        bgColorDark = BG_COLOR_DARK;
        textElementName = "amount";

        // TODO(as3-port): ILocalization.getLocalization("purse.snowflakes.zero.amount.text") — use fallback
        amountZeroText = "Info";

        CreateWindow("purse_indicator_seasonal_xml", null);
        SetAmount(0);

        // TODO(as3-port): IHabboCatalog.events — register catalog_purse_activity_point_balance when catalog is ported

        // TODO(as3-port): attach to extensionView at slot ExtensionFixedSlotsEnum.SLOT_SEASONAL_CURRENCY (5)
        // toolbar.extensionView.AttachExtension(ToolbarDisplayExtensionIds.const_890, window!, ExtensionFixedSlotsEnum.SLOT_SEASONAL_CURRENCY);

        // TODO(as3-port): initializeCurrencyLayouts — needs IHabboCatalog, ColorConverter, class_3473 icon styles
    }

    /// @see SeasonalCurrencyIndicator.as::registerUpdateEvents
    public override void RegisterUpdateEvents(object? eventDispatcher)
    {
        // TODO(as3-port): add catalog_purse_activity_point_balance listener when IHabboCatalog is ported
    }

    /// @see SeasonalCurrencyIndicator.as::onBalance
    public void OnBalance(int activityPointType, int balance)
    {
        int displayedType = _toolbar?.GetInteger("seasonalcurrencyindicator.currency", 1) ?? 1;

        if (activityPointType == displayedType)
        {
            SetAmount(balance);
            if (_lastBalance != -1)
            {
                AnimateChange(_lastBalance, balance);
            }
            _lastBalance = balance;
        }
    }

    /// @see SeasonalCurrencyIndicator.as::setAmount
    protected override void SetAmount(int amount, int minutes = -1)
    {
        string text = amount.ToString();

        if (amount == 0)
        {
            text = amountZeroText ?? "Info";
            SetTextUnderline(true);
        }
        else
        {
            SetTextUnderline(false);
        }

        SetText(text);
    }

    /// @see SeasonalCurrencyIndicator.as::dispose
    public override void Dispose()
    {
        _toolbar = null;
        base.Dispose();
    }
}
