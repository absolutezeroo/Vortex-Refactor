// @see habbo/window/utils/ModalDialog.as

using System;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Window;
using Vortex.Core.Window.Components;

namespace Vortex.Habbo.Window.Utils;

/// <summary>
/// Window overlay manager for modal dialogs. Creates a semi-transparent background
/// behind the dialog content, disabling interaction with lower layers.
/// </summary>
/// @see habbo/window/utils/ModalDialog.as
public class ModalDialog : IModalDialog, IDisposable
{
    private const int MODAL_DIALOG_LAYER = 3;

    private static HabboWindowManagerComponent? _windowManager;
    private static IWindowContainer? _container;
    private static readonly int _refreshCountdown;

    /// @see ModalDialog.as::ModalDialog
    public ModalDialog(HabboWindowManagerComponent windowManager, object? xml)
    {
        InitialiseStaticMembers(windowManager);

        IWindowContext? modalContext = _windowManager!.GetWindowContext((uint)MODAL_DIALOG_LAYER);
        if (modalContext != null)
        {
            // @see ModalDialog.as — create background bitmap wrapper (type 21, param 1)
            Background = modalContext.Create("", "", 21, 0, 1, new Rect2(0, 0, 1, 1), null, _container as IWindow, 0);

            RootWindow = xml switch
            {
                // @see ModalDialog.as — build the dialog content from XML
                XElement xmlElement => ((IWindowFactory)_windowManager).BuildFromXml(xmlElement, (uint)MODAL_DIALOG_LAYER) as IWindow,
                string xmlString => ((IWindowFactory)_windowManager).BuildFromXml(XElement.Parse(xmlString), (uint)MODAL_DIALOG_LAYER) as
                    IWindow,
                _ => RootWindow,
            };

            if (RootWindow != null && _container != null)
            {
                _container.AddChild(RootWindow);
                RootWindow.Center();
                _container.visible = true;
            }
        }

        Refresh();
    }

    /// @see ModalDialog.as::initialiseStaticMembers
    private static void InitialiseStaticMembers(HabboWindowManagerComponent windowManager)
    {
        if (_windowManager != null)
        {
            return;
        }

        _windowManager = windowManager;

        // @see ModalDialog.as — create container (type 4, style 0, param 0) on modal layer
        IWindowContext? modalContext = _windowManager.GetWindowContext((uint)MODAL_DIALOG_LAYER);

        _container = modalContext?.Create("", "", 4, 0, 0, new Rect2(0, 0, 1, 1), null, null, 0) as IWindowContainer;
    }

    /// @see ModalDialog.as::refresh
    private static void Refresh()
    {
        if (_container == null || _windowManager == null)
        {
            return;
        }

        bool isEmpty = _container.numChildren == 0;

        // @see ModalDialog.as — toggle visibility of lower layers
        for (uint i = 0;
             i < MODAL_DIALOG_LAYER;
             i++)
        {
            IWindowContext? ctx = _windowManager.GetWindowContext(i);
            IDesktopWindow? desktop = ctx?.GetDesktopWindow();

            if (desktop != null)
            {
                desktop.visible = isEmpty;
            }
        }

        if (isEmpty)
        {
            return;
        }

        // @see ModalDialog.as — resize container to stage size, center dialog children
        object? viewport = _windowManager.context?.displayObjectContainer;
        float stageWidth = viewport is Control ctrl ? ctrl.Size.X : 800;
        float stageHeight = viewport is Control ctrl2 ? ctrl2.Size.Y : 600;
        Rect2 stageRect = new(0, 0, Math.Max(1, stageWidth), Math.Max(1, stageHeight));

        _container.SetRectangle(0, 0, stageRect.Size.X, stageRect.Size.Y);

        for (int j = 0;
             j < _container.numChildren;
             j++)
        {
            IWindow? child = _container.GetChildAt(j);

            if (child == null)
            {
                continue;
            }

            if (j % 2 == 0)
            {
                // Background windows: stretch to fill
                child.SetRectangle(0, 0, stageRect.Size.X, stageRect.Size.Y);
            }
            else
            {
                // Dialog windows: center
                child.Center();
            }

            // @see ModalDialog.as — only last two children visible (background + dialog)
            child.visible = j >= _container.numChildren - 2;
        }
    }

    /// @see ModalDialog.as::get rootWindow
    public IWindow? RootWindow { get; private set; }

    /// @see ModalDialog.as::get background
    public IWindow? Background { get; private set; }

    /// @see ModalDialog.as::get disposed
    public bool Disposed { get; private set; }

    // Vortex.Core.Runtime.IDisposable interface
    bool Core.Runtime.IDisposable.disposed => Disposed;

    /// @see ModalDialog.as::dispose
    public void Dispose()
    {
        if (Disposed)
        {
            return;
        }

        if (Background is { disposed: false })
        {
            Background.Destroy();
            Background = null;
        }

        if (RootWindow is { disposed: false })
        {
            RootWindow.Destroy();
            RootWindow = null;
        }

        Refresh();

        if (_container is { numChildren: 0 })
        {
            _container.visible = false;
        }

        Disposed = true;
        GC.SuppressFinalize(this);
    }

    // IModalDialog scaffold interface
    object? IModalDialog.RootWindow()
    {
        return RootWindow;
    }

    object? IModalDialog.Background()
    {
        return Background;
    }
}
