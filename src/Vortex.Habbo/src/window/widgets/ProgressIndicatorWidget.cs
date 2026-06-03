// @see habbo/window/widgets/ProgressIndicatorWidget.as

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Iterators;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/ProgressIndicatorWidget.as
public class ProgressIndicatorWidget : IClass3528, IWidget
{
    private const uint MAXIMUM_SIZE = 1000;

    private IWidgetWindow? _widgetWindow;
    private HabboWindowManagerComponent? _windowManager;
    private IItemListWindow? _list;
    private uint _position;
    private readonly string _style = "flat";
    private string _mode = "position";

    /// @see habbo/window/widgets/ProgressIndicatorWidget.as::ProgressIndicatorWidget
    public ProgressIndicatorWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;

        // @see ProgressIndicatorWidget.as — build root from progress_indicator_xml asset
        object? xmlAsset = windowManager.FindAssetByName("progress_indicator_xml");

        if (xmlAsset is Core.Assets.IAsset { Content: System.Xml.Linq.XElement xml })
        {
            _list = ((IWindowFactory)windowManager).BuildFromXml(xml) as IItemListWindow;
        }

        if (_list == null)
        {
            return;
        }

        _widgetWindow.SetParamFlag(147456);
        _widgetWindow.RootWindow(_list);
    }

    /// @see habbo/window/widgets/ProgressIndicatorWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/ProgressIndicatorWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/ProgressIndicatorWidget.as::size
    public uint Size
    {
        get => (uint)(_list?.NumListItems ?? 0);
        set
        {
            uint uVal = value > MAXIMUM_SIZE ? MAXIMUM_SIZE : value;

            if (uVal < 1)
            {
                uVal = 1;
            }

            uint currentSize = (uint)(_list?.NumListItems ?? 0);

            if (uVal == currentSize)
            {
                return;
            }

            // @see ProgressIndicatorWidget.as — remove excess items
            while (uVal < currentSize)
            {
                _list!.RemoveListItemAt((int)(currentSize - 1));
                currentSize--;
            }

            // @see ProgressIndicatorWidget.as — clone first item to add more
            while (uVal > currentSize)
            {
                IWindow? firstItem = _list!.GetListItemAt(0);

                if (firstItem != null)
                {
                    _list.AddListItem(firstItem); // clone semantics handled by list
                }

                currentSize++;
            }

            Refresh();
        }
    }

    /// @see habbo/window/widgets/ProgressIndicatorWidget.as::position
    public uint Position
    {
        get => _position;
        set
        {
            _position = value;
            Refresh();
        }
    }

    /// @see habbo/window/widgets/ProgressIndicatorWidget.as::mode
    public string Mode
    {
        get => _mode;
        set
        {
            _mode = value;
            Refresh();
        }
    }

    /// @see habbo/window/widgets/ProgressIndicatorWidget.as::dispose
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

        if (_widgetWindow != null)
        {
            _widgetWindow.RootWindow(null);
            _widgetWindow = null;
        }

        _windowManager = null;
        disposed = true;
    }

    /// @see habbo/window/widgets/ProgressIndicatorWidget.as::refresh
    private void Refresh()
    {
        if (disposed || _list == null)
        {
            return;
        }

        int numItems = _list.NumListItems;

        for (int i = 0;
             i < numItems;
             i++)
        {
            if (_list.GetListItemAt(i) is not IStaticBitmapWrapperWindow bitmap)
            {
                continue;
            }

            bool isOn = _mode switch
            {
                "position" => i + 1 == _position,
                "progress" => i < _position,
                _ => false,
            };

            bitmap.AssetUri = "progress_disk_" + _style + (isOn ? "_on" : "_off");
        }
    }
}
