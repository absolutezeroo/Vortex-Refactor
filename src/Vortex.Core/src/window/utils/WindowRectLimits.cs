// @see core/window/utils/WindowRectLimits.as

namespace Vortex.Core.Window.Utils;

/// <summary>
/// Size constraint system for windows. Enforces min/max width and height
/// limits, applying them immediately when set and on demand via Limit().
/// </summary>
/// @see core/window/utils/WindowRectLimits.as
public class WindowRectLimits : IRectLimiter
{
    private int _minWidth = int.MinValue;
    private int _maxWidth = int.MaxValue;
    private int _minHeight = int.MinValue;
    private int _maxHeight = int.MaxValue;
    private readonly IWindow? _window;

    /// @see WindowRectLimits.as::WindowRectLimits
    public WindowRectLimits() { }

    public WindowRectLimits(IWindow window)
    {
        _window = window;
    }

    /// @see WindowRectLimits.as::get/set minWidth
    public int MinWidth
    {
        get => _minWidth;
        set
        {
            _minWidth = value;
            if (_minWidth > int.MinValue && _window is { disposed: false } && _window.width < _minWidth)
            {
                _window.width = _minWidth;
            }
        }
    }

    /// @see WindowRectLimits.as::get/set maxWidth
    public int MaxWidth
    {
        get => _maxWidth;
        set
        {
            _maxWidth = value;
            if (_maxWidth < int.MaxValue && _window is { disposed: false } && _window.width > _maxWidth)
            {
                _window.width = _maxWidth;
            }
        }
    }

    /// @see WindowRectLimits.as::get/set minHeight
    public int MinHeight
    {
        get => _minHeight;
        set
        {
            _minHeight = value;
            if (_minHeight > int.MinValue && _window is { disposed: false } && _window.height < _minHeight)
            {
                _window.height = _minHeight;
            }
        }
    }

    /// @see WindowRectLimits.as::get/set maxHeight
    public int MaxHeight
    {
        get => _maxHeight;
        set
        {
            _maxHeight = value;
            if (_maxHeight < int.MaxValue && _window is { disposed: false } && _window.height > _maxHeight)
            {
                _window.height = _maxHeight;
            }
        }
    }

    /// @see WindowRectLimits.as::get isEmpty
    public bool IsEmpty => _minWidth == int.MinValue && _maxWidth == int.MaxValue &&
                           _minHeight == int.MinValue && _maxHeight == int.MaxValue;

    /// @see WindowRectLimits.as::setEmpty
    public void SetEmpty()
    {
        _minWidth = int.MinValue;
        _maxWidth = int.MaxValue;
        _minHeight = int.MinValue;
        _maxHeight = int.MaxValue;
    }

    /// @see WindowRectLimits.as::limit
    public void Limit()
    {
        if (IsEmpty || _window == null)
        {
            return;
        }

        if (_window.width < _minWidth)
        {
            _window.width = _minWidth;
        }
        else if (_window.width > _maxWidth)
        {
            _window.width = _maxWidth;
        }

        if (_window.height < _minHeight)
        {
            _window.height = _minHeight;
        }
        else if (_window.height > _maxHeight)
        {
            _window.height = _maxHeight;
        }
    }

    /// @see WindowRectLimits.as::assign
    public void Assign(int minW, int maxW, int minH, int maxH)
    {
        _minWidth = minW;
        _maxWidth = maxW;
        _minHeight = minH;
        _maxHeight = maxH;
        Limit();
    }

    /// @see WindowRectLimits.as::clone
    public WindowRectLimits Clone(IWindow newWindow)
    {
        WindowRectLimits copy = new(newWindow)
        {
            _minWidth = _minWidth,
            _maxWidth = _maxWidth,
            _minHeight = _minHeight,
            _maxHeight = _maxHeight,
        };
        return copy;
    }
}
