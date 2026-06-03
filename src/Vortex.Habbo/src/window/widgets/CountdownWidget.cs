// @see habbo/window/widgets/CountdownWidget.as

using System;
using System.Diagnostics;

using Vortex.Core.Runtime;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Iterators;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/CountdownWidget.as
public class CountdownWidget : IClass3570, IWidget, IUpdateReceiver
{
    private static readonly string[] UNIT_NAMES =
    {
        "weeks",
        "days",
        "hours",
        "minutes",
        "seconds",
    };
    private static readonly int[] UNIT_DIVISORS =
    {
        604800,
        86400,
        3600,
        60,
        1,
    };
    private static readonly int[] UNIT_MAX_VALUES =
    {
        100,
        7,
        24,
        60,
        60,
    };
    private static readonly uint[] COLOR_STYLES_VALUES =
    {
        0,
        16777215,
    };
    private static readonly uint[] COLOR_STYLES_ETCHING_VALUES =
    {
        3003121663,
        0,
    };

    private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    private IWidgetWindow? _widgetWindow;
    private HabboWindowManagerComponent? _windowManager;
    private IItemListWindow? _list;
    private IWindowContainer? _counterTemplate;
    private ITextWindow? _separatorTemplate;
    private bool _running;
    private int _startSeconds;
    private long _startTime;
    private int _colorStyle;
    private int _displayedTime = -1;

    /// @see habbo/window/widgets/CountdownWidget.as::CountdownWidget
    public CountdownWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;
        _startTime = _stopwatch.ElapsedMilliseconds;

        // @see CountdownWidget.as — build root from clock_base_xml asset
        object? xmlAsset = windowManager.FindAssetByName("clock_base_xml");
        if (xmlAsset is Core.Assets.IAsset { Content: System.Xml.Linq.XElement xml })
        {
            _list = ((IWindowFactory)windowManager).BuildFromXml(xml) as IItemListWindow;
        }

        if (_list == null)
        {
            return;
        }

        _counterTemplate = (_list as IWindow)?.GetChildByName("counter") as IWindowContainer;
        _separatorTemplate = (_list as IWindow)?.GetChildByName("separator") as ITextWindow;

        // @see CountdownWidget.as — default 3 digits, register for updates, set size-to-content
        SetDigits(3);
        windowManager.RegisterUpdateReceiver(this, 10);
        _widgetWindow.SetParamFlag(147456);
        _widgetWindow.RootWindow(_list);
    }

    /// @see habbo/window/widgets/CountdownWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/CountdownWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/CountdownWidget.as::colorStyle
    public int ColorStyle
    {
        get => _colorStyle;
        set => SetColorStyle(value);
    }

    /// @see habbo/window/widgets/CountdownWidget.as::running
    public bool Running
    {
        get => _running;
        set
        {
            if (_running && !value)
            {
                _startSeconds = GetSeconds();
            }

            if (!_running && value)
            {
                _startTime = _stopwatch.ElapsedMilliseconds;
            }

            _running = value;
        }
    }

    /// @see habbo/window/widgets/CountdownWidget.as::digits
    public uint Digits
    {
        get => GetDigits();
        set => SetDigits(value);
    }

    /// @see habbo/window/widgets/CountdownWidget.as::seconds
    public int Seconds
    {
        get => GetSeconds();
        set
        {
            _startSeconds = value;
            _startTime = _stopwatch.ElapsedMilliseconds;
            UpdateTime();
        }
    }

    /// @see habbo/window/widgets/CountdownWidget.as::update
    public void Update(uint param1)
    {
        if (disposed || !_running)
        {
            return;
        }
        UpdateTime();
    }

    /// @see habbo/window/widgets/CountdownWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (_list is IWindow listWin)
        {
            listWin.Destroy();
            _list = null;
        }

        _counterTemplate = null;
        _separatorTemplate = null;

        if (_widgetWindow != null)
        {
            _widgetWindow.RootWindow(null);
            _widgetWindow = null;
        }

        _windowManager?.RemoveUpdateReceiver(this);
        _windowManager = null;
        disposed = true;
        _running = false;
    }

    /// @see CountdownWidget.as::get seconds — computes remaining time
    private int GetSeconds()
    {
        if (!_running)
        {
            return _startSeconds;
        }

        int elapsed = (int)((_stopwatch.ElapsedMilliseconds - _startTime) / 1000);

        return Math.Max(0, _startSeconds - elapsed);
    }

    /// @see CountdownWidget.as::get digits — derived from list item count
    private uint GetDigits()
    {
        if (_list == null)
        {
            return 0;
        }

        uint numItems = (uint)_list.NumListItems;

        return (numItems + 1) / 2;
    }

    /// @see CountdownWidget.as::set digits — rebuilds list with counter+separator clones
    private void SetDigits(uint value)
    {
        if (_list == null || _counterTemplate == null)
        {
            return;
        }

        value = Math.Clamp(value, 2, 4);

        if (value == GetDigits())
        {
            return;
        }

        _list.RemoveListItems();

        for (uint i = 0;
             i < value;
             i++)
        {
            if (i != 0 && _separatorTemplate is IWindow)
            {
                // TODO(window-port): Clone separator template and add to list
                // _list.AddListItem(sepWin.Clone());
            }

            // TODO(window-port): Clone counter template and add to list
            // _list.AddListItem(((IWindow)_counterTemplate).Clone());
        }

        UpdateTime(true);
    }

    /// @see CountdownWidget.as::set colorStyle — applies color to all unit labels
    private void SetColorStyle(int value)
    {
        _colorStyle = value;

        if (_list == null)
        {
            return;
        }

        int numItems = _list.NumListItems;

        for (int i = 0;
             i < numItems;
             i++)
        {
            if (_list.GetListItemAt(i) is not IWindow itemWin)
            {
                continue;
            }

            if (itemWin.GetChildByName("unit") is not ITextWindow unitText)
            {
                continue;
            }

            if (_colorStyle < 0 || _colorStyle >= COLOR_STYLES_VALUES.Length)
            {
                continue;
            }

            unitText.TextColor = COLOR_STYLES_VALUES[_colorStyle];
            unitText.EtchingColor = COLOR_STYLES_ETCHING_VALUES[_colorStyle];
        }
    }

    /// @see CountdownWidget.as — static helper: find starting unit index for display
    private static int GetMaxUnitIndex(int digits, int seconds)
    {
        for (int i = 0;
             i < UNIT_DIVISORS.Length - digits;
             i++)
        {
            if (seconds >= UNIT_DIVISORS[i])
            {
                return i;
            }
        }

        return UNIT_DIVISORS.Length - digits;
    }

    /// @see CountdownWidget.as::updateTime — renders digit values into list items
    private void UpdateTime(bool force = false)
    {
        int remaining = GetSeconds();
        if (remaining == _displayedTime && !force)
        {
            return;
        }

        if (_list == null)
        {
            return;
        }

        int digits = (int)GetDigits();
        int unitStart = GetMaxUnitIndex(digits, remaining);

        for (int i = 0;
             i < digits;
             i++)
        {
            int unitIndex = unitStart + i;

            if (unitIndex >= UNIT_DIVISORS.Length)
            {
                break;
            }

            // @see CountdownWidget.as — list items at even indices (i*2) are counter containers
            if (_list.GetListItemAt(i * 2) is not IWindow counterWin)
            {
                continue;
            }

            int unitValue = remaining / UNIT_DIVISORS[unitIndex] % UNIT_MAX_VALUES[unitIndex];

            IWindow? valueWin = counterWin.GetChildByName("value");

            if (valueWin != null)
            {
                valueWin.caption = (unitValue < 10 ? "0" : "") + unitValue;
            }

            IWindow? unitWin = counterWin.GetChildByName("unit");

            if (unitWin != null)
            {
                unitWin.caption = "${countdown_clock_unit_" + UNIT_NAMES[unitIndex] + "}";
            }
        }

        _displayedTime = remaining;
    }
}
