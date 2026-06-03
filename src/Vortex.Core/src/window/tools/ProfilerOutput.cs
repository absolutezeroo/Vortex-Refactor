// @see core/window/tools/ProfilerOutput.as

using System;

using Vortex.Core.Window.Components;
using Vortex.Core.Window.Graphics;

namespace Vortex.Core.Window.Tools;

/// <summary>
/// Profiler UI overlay. Displays component profiler task metrics in a window.
/// Partial implementation — full UI wiring depends on profiler agent system (IProfiler).
/// </summary>
/// @see core/window/tools/ProfilerOutput.as
public class ProfilerOutput : IClass3394, IDisposable
{
    private object? _profilerComponent;
    private readonly object? _core;
    private IWindowContainer? _window;
    private readonly List<object?> _windowItemArray = new();
    private ICoreWindowManager? _windowManager;
    private IClass3354? _windowRenderer;
    private readonly bool _memoryTracking;
    private Dictionary<IWindow, object>? _windowToTaskMap;

    /// @see ProfilerOutput.as::ProfilerOutput
    public ProfilerOutput(object? context, ICoreWindowManager? windowManager, IClass3354? windowRenderer)
    {
        _core = context;
        _profilerComponent = null;
        _windowManager = windowManager;
        _windowRenderer = windowRenderer;
        _windowToTaskMap = new Dictionary<IWindow, object>();
    }

    /// @see ProfilerOutput.as::padAlign
    private static string PadAlign(string text, int width, char pad = ' ', bool rightAlign = false)
    {
        int diff = width - text.Length;
        if (diff <= 0)
        {
            return text[..width];
        }

        string padding = new(pad, diff);
        return rightAlign ? padding + text : text + padding;
    }

    /// @see ProfilerOutput.as::get caption
    public static string CaptionText => "Component Profiler";

    /// @see ProfilerOutput.as::get disposed
    public bool Disposed { get; private set; }

    /// @see ProfilerOutput.as::get visible
    public bool IsVisible => _window is { visible: true };

    /// @see ProfilerOutput.as::set visible
    public static void SetVisible(bool value)
    {
        // TODO(profiler): Wire profiler UI loading when IProfiler is ported.
        // AS3 loads profiler_dialog_xml, builds window, sets procedure, activates profiler mode.
    }

    /// @see ProfilerOutput.as::get profiler
    public object? Profiler()
    {
        return _profilerComponent;
    }

    /// @see ProfilerOutput.as::set profiler
    public void Profiler(object? value)
    {
        // TODO(profiler): Wire profiler stop event listener when IProfiler is ported.
        _profilerComponent = value;
    }

    /// @see ProfilerOutput.as::dispose
    public void Dispose()
    {
        if (!Disposed)
        {
            if (_window != null)
            {
                (_window as IDisposable)?.Dispose();
                _window = null;
            }

            _profilerComponent = null;
            _windowManager = null;
            _windowRenderer = null;
            _windowToTaskMap?.Clear();
            _windowToTaskMap = null;
            Disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    /// @see ProfilerOutput.as::profilerWindowEventProc
    public static void ProfilerWindowEventProc(object? evt, IWindow? window)
    {
        // TODO(profiler): Wire window event handling (close, gc button, memory toggle).
    }

    /// @see ProfilerOutput.as::refresh
    public static void Refresh(object? evt)
    {
        // TODO(profiler): Wire profiler agent iteration and list refresh.
    }

    /// @see ProfilerOutput.as::recursiveRedraw
    private static uint RecursiveRedraw(object? task, IItemListWindow? list, uint index, uint depth)
    {
        // TODO(profiler): Wire task metrics display and sub-task recursion.
        return index;
    }

    /// @see ProfilerOutput.as::refreshBitmapImage
    private static void RefreshBitmapImage(IBitmapWrapperWindow? canvas, object? task)
    {
        // TODO(profiler): Wire bitmap performance graph (scrolling pixel column).
    }

    /// @see ProfilerOutput.as::createListItem
    private static IWindowContainer? CreateListItem(IItemListWindow? list)
    {
        // TODO(profiler): Wire profiler_task_xml loading and list item creation.
        return null;
    }

    /// @see ProfilerOutput.as::onCheckMouseClick
    private static void OnCheckMouseClick(object? evt)
    {
        // TODO(profiler): Wire task pause toggle on check click.
    }

    string? IClass3394.Caption => CaptionText;

    bool IClass3394.Visible
    {
        get => IsVisible;
        set => SetVisible(value);
    }
}
