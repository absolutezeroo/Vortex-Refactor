// @see core/window/utils/TextMargins.as

using System;

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/TextMargins.as
public class TextMargins : IMargins, IDisposable
{
    private Action<IMargins>? _callback;

    /// @see TextMargins.as::TextMargins
    public TextMargins(int left, int top, int right, int bottom, Action<IMargins>? callback)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
        _callback = callback ?? NullCallback;
    }

    public int LeftValue => Left;

    public int TopValue => Top;

    public int RightValue => Right;

    public int BottomValue => Bottom;

    public bool Disposed { get; private set; }

    /// @see TextMargins.as::get isZeroes
    public bool IsZeroes => Left == 0 && Right == 0 && Top == 0 && Bottom == 0;

    /// @see TextMargins.as::get/set left
    public int Left { get; private set; }

    /// @see TextMargins.as::get/set right
    public int Right { get; private set; }

    /// @see TextMargins.as::get/set top
    public int Top { get; private set; }

    /// @see TextMargins.as::get/set bottom
    public int Bottom { get; private set; }

    /// @see IMargins — explicit interface setters with callback
    int IMargins.Left
    {
        get => Left;
        set
        {
            Left = value;
            _callback?.Invoke(this);
        }
    }

    int IMargins.Right
    {
        get => Right;
        set
        {
            Right = value;
            _callback?.Invoke(this);
        }
    }

    int IMargins.Top
    {
        get => Top;
        set
        {
            Top = value;
            _callback?.Invoke(this);
        }
    }

    int IMargins.Bottom
    {
        get => Bottom;
        set
        {
            Bottom = value;
            _callback?.Invoke(this);
        }
    }

    /// @see TextMargins.as::assign
    public void Assign(int left, int top, int right, int bottom, Action<IMargins>? callback)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
        _callback = callback ?? NullCallback;
    }

    /// @see TextMargins.as::clone
    public TextMargins Clone(Action<IMargins>? callback)
    {
        return new TextMargins(Left, Top, Right, Bottom, callback);
    }

    /// @see TextMargins.as::dispose
    public void Dispose()
    {
        _callback = null;
        Disposed = true;
    }

    /// @see TextMargins.as::nullCallback
    private static void NullCallback(IMargins margins) { }
}
