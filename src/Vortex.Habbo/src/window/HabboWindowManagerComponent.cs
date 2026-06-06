// @see habbo/window/HabboWindowManagerComponent.as
// TODO(window-port): Complete runtime wiring (dependencies, renderer/theme, profiler, floorplan/help viewers).

using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Localization;
using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Graphics;
using Vortex.Core.Window.Utils;
using Vortex.Habbo.Window.Utils;
using Vortex.IID;

namespace Vortex.Habbo.Window;

/// @see habbo/window/HabboWindowManagerComponent.as
public class HabboWindowManagerComponent : Component, IHabboWindowManager, IInputEventTracker, IWidgetFactory, IUpdateReceiver
{
    private const uint NUMBER_OF_CONTEXT_LAYERS = 4;
    private const uint DEFAULT_CONTEXT_LAYER_INDEX = 1;
    private IWindowContext? _activeWindowContext;
    private readonly object? _assets;
    private object? _avatarRenderer;
    private object? _bcfloorPlanEditor;
    private object? _communication;
    private object? _elementPointerHandler;
    private object? _habbletLinkHandler;
    private object? _habboPagesViewer;
    private HintManager? _hintManager;
    private bool _initialized;
    private ICoreLocalizationManager? _localization;
    private ResourceManager? _resourceManager;
    private object? _roomEngine;
    private object? _sessionDataManager;
    private object? _themeManager;

    private IWindowContext?[]? _windowContextArray;
    private Viewport? _stageViewport;

    /// @see habbo/window/HabboWindowManagerComponent.as::HabboWindowManagerComponent
    public HabboWindowManagerComponent(IContext param1, uint param2 = 0, object? param3 = null) : base(param1, param2, param3)
    {
        // @see HabboWindowManagerComponent.as — AS3 stores assets from constructor param3 (inherited via Component.assets).
        // Wire _assets from base.assets so IHabboWindowManager.Assets() and ResourceManager can access the asset library.
        _assets = assets;
        RegisterInterface(new IIDCoreWindowManager(), this);
        RegisterInterface(new IIDHabboWindowManager(), this);
    }
    IWidget? IWidgetFactory.CreateWidget(string param1, IDesktopWindow param2)
    {
        return CreateWidget(param1, param2);
    }

    // Typed interface surface
    object? IHabboWindowManager.ResourceManager()
    {
        return ResourceManager();
    }

    object? IHabboWindowManager.Assets()
    {
        return _assets;
    }

    object? IHabboWindowManager.HabboPagesStyleSheet()
    {
        return HabboPagesStyleSheet();
    }

    IWindow? IHabboWindowManager.CreateWindow
    (
        string param1,
        string param2,
        uint param3,
        uint param4,
        uint param5,
        Rect2? param6,
        Action<WindowEvent, IWindow>? param7,
        uint param8,
        uint param9,
        string param10
    )
    {
        return CreateWindowCore(
            param1, param2, param3, param4, param5, param6 ?? new Rect2(0, 0, 0, 0), param7, param8, param9,
            param10
        );
    }

    void IHabboWindowManager.RemoveWindow(string param1, uint param2)
    {
        RemoveWindowCore(param1, param2);
    }

    IWindow? IHabboWindowManager.GetWindowByName(string param1, uint param2)
    {
        return GetWindowByNameCore(param1, param2);
    }

    IWindow? IHabboWindowManager.GetActiveWindow(uint param1)
    {
        return GetActiveWindowCore(param1);
    }

    void IHabboWindowManager.ToggleFullScreen()
    {
        ToggleFullScreen();
    }

    IWindowContext? IHabboWindowManager.GetWindowContext(uint param1)
    {
        return GetWindowContextCore(param1);
    }

    IClass3348? IHabboWindowManager.Alert(string param1, string param2, uint param3, Action? param4)
    {
        XElement? xml = GetAlertXml("habbo_alert_xml");

        return xml != null
            ? new AlertDialog(
                this, xml, param1, param2, param3, (d, _) =>
                {
                    param4?.Invoke();
                    d.Dispose();
                }, false
            )
            : null;
    }

    IClass3348? IHabboWindowManager.AlertWithModal(string param1, string param2, uint param3, Action? param4)
    {
        XElement? xml = GetAlertXml("habbo_alert_xml");

        return xml != null
            ? new AlertDialog(
                this, xml, param1, param2, param3, (d, _) =>
                {
                    param4?.Invoke();
                    d.Dispose();
                }, true
            )
            : null;
    }

    IAlertDialogWithLink? IHabboWindowManager.AlertWithLink(string param1, string param2, string param3, string param4, uint param5,
        Action? param6)
    {
        XElement? xml = GetAlertXml("habbo_alert_xml");

        return xml != null
            ? new AlertDialogWithLink(
                this, xml, param1, param2, param3, param4, param5, (d, _) =>
                {
                    param6?.Invoke();
                    d.Dispose();
                }
            )
            : null;
    }

    void IHabboWindowManager.RegisterLocalizationParameter(string param1, string param2, string param3, string param4)
    {
        _localization?.RegisterParameter(param1, param2, param3, param4);
    }

    void IHabboWindowManager.AddMouseEventTracker(IInputEventTracker param1)
    {
        AddMouseEventTracker(param1);
    }

    void IHabboWindowManager.RemoveMouseEventTracker(IInputEventTracker param1)
    {
        RemoveMouseEventTracker(param1);
    }

    IWindowContainer? IHabboWindowManager.CreateUnseenItemCounter()
    {
        return new WindowContainerModel("unseen_item_counter", new Rect2(0, 0, 0, 0));
    }

    IModalDialog? IHabboWindowManager.BuildModalDialogFromXml(XElement param1)
    {
        return new ModalDialog(this, param1);
    }

    void IHabboWindowManager.SimpleAlert
    (
        string param1,
        string param2,
        string param3,
        string? param4,
        string? param5,
        Dictionary<string, object?>? param6,
        string? param7,
        Action? param8,
        Action? param9
    )
    {
        _ = new SimpleAlertDialog(this, param1, param2, param3, param4, param5, param6, param7, param8, param9);
    }

    void IHabboWindowManager.RegisterHintWindow(string param1, IWindow param2, int param3)
    {
        _hintManager?.RegisterWindow(param1, param2, param3);
    }

    void IHabboWindowManager.UnregisterHintWindow(string param1)
    {
        _hintManager?.UnregisterWindow(param1);
    }

    void IHabboWindowManager.ShowHint(string param1, Rect2? param2)
    {
        _hintManager?.ShowHint(param1, param2);
    }

    void IHabboWindowManager.HideHint()
    {
        _hintManager?.HideHint();
    }

    void IHabboWindowManager.DisplayFloorPlanEditor()
    {
        DisplayFloorPlanEditor();
    }

    void IHabboWindowManager.OpenHelpPage(string param1)
    {
        TryInvoke(_habboPagesViewer, "openPage", param1);
    }

    void IHabboWindowManager.HideMatchingHint(string param1)
    {
        _hintManager?.HideMatchingHint(param1);
    }

    IWindow? IWindowFactory.Create
    (
        string param1,
        uint param2,
        uint param3,
        uint param4,
        Rect2 param5,
        Action<WindowEvent, IWindow>? param6,
        string param7,
        uint param8,
        IList<string>? param9,
        IWindow? param10,
        IList<object>? param11,
        string param12
    )
    {
        return CreateCore(param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12);
    }

    void IWindowFactory.Destroy(IWindow param1)
    {
        param1.Destroy();
    }

    IWindow? IWindowFactory.BuildFromXml(XElement param1, uint param2, Dictionary<string, object?>? param3)
    {
        return BuildFromXmlCore(param1, param2, param3);
    }

    string IWindowFactory.WindowToXmlString(IWindow param1)
    {
        return _activeWindowContext?.GetWindowParser().WindowToXmlString(param1) ?? string.Empty;
    }

    IWindow? IWindowFactory.FindWindowByName(string param1)
    {
        return FindWindowByNameCore(param1);
    }

    IWindow? IWindowFactory.FindWindowByTag(string param1)
    {
        return FindWindowByTagCore(param1);
    }

    uint IWindowFactory.GroupWindowsWithTag(string param1, IList<IWindow> param2, int param3)
    {
        return GroupWindowsWithTagCore(param1, param2, param3);
    }

    void IInputEventTracker.EventReceived(WindowEvent param1, IWindow? param2)
    {
        _ = param1;
        _ = param2;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::get roomEngine
    public virtual object? RoomEngine()
    {
        return _roomEngine;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::get dependencies
    protected override IList<ComponentDependency> dependencies =>
        base.dependencies.Concat(
                [
                    new ComponentDependency(new IIDSessionDataManager(), param1 => _sessionDataManager = param1, false),
                    new ComponentDependency(new IIDHabboLocalizationManager(),
                        param1 => _localization = param1 as ICoreLocalizationManager),
                    new ComponentDependency(
                        new IIDHabboConfigurationManager(), _ =>
                        {
                        }, false,
                        [new DependencyEventListener("complete", OnConfigurationCompleteHandler)]
                    ),
                    new ComponentDependency(new IIDAvatarRenderManager(), param1 => _avatarRenderer = param1, false),
                    new ComponentDependency(new IIDHabboCommunicationManager(), param1 => _communication = param1, false),
                    new ComponentDependency(new IIDRoomEngine(), param1 => _roomEngine = param1, false),
                ]
            )
            .ToList();

    /// @see habbo/window/HabboWindowManagerComponent.as::get avatarRenderer
    public virtual object? AvatarRenderer()
    {
        return _avatarRenderer;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::get resourceManager
    public virtual object? ResourceManager()
    {
        return _resourceManager;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::get localization
    public virtual object? Localization()
    {
        return _localization;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::get communication
    public virtual object? Communication()
    {
        return _communication;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::get sessionDataManager
    public virtual object? SessionDataManager()
    {
        return _sessionDataManager;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::get habboPagesStyleSheet
    public virtual object? HabboPagesStyleSheet()
    {
        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::onConfigurationComplete
    private void OnConfigurationCompleteHandler(object? param1)
    {
        if (_communication == null)
        {
            return;
        }

        _bcfloorPlanEditor ??= new object();
        _elementPointerHandler ??= new object();
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::initComponent
    protected override void InitComponent()
    {
        EnsureInitialized();
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::toggleFullScreen
    public virtual object? ToggleFullScreen(params object?[] args)
    {
        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::receiveProfilerInterface
    public virtual object? ReceiveProfilerInterface(params object?[] args)
    {
        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::dispose
    public override void Dispose()
    {
        DisposeLinkHandlers();
        DisposeEditors();
        DisposeWindowContexts();
        DisposeRenderer();
        DisposeManagers();
        base.Dispose();
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::disposeLinkHandlers
    public virtual object? DisposeLinkHandlers(params object?[] args)
    {
        TryInvoke(_habbletLinkHandler, "dispose");

        _habbletLinkHandler = null;

        TryInvoke(_elementPointerHandler, "dispose");

        _elementPointerHandler = null;

        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::disposeEditors
    public virtual object? DisposeEditors(params object?[] args)
    {
        TryInvoke(_bcfloorPlanEditor, "dispose");

        _bcfloorPlanEditor = null;

        TryInvoke(_habboPagesViewer, "dispose");

        _habboPagesViewer = null;

        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::disposeWindowContexts
    public virtual object? DisposeWindowContexts(params object?[] args)
    {
        if (_stageViewport != null)
        {
            _stageViewport.SizeChanged -= OnViewportSizeChanged;
            _stageViewport = null;
        }

        if (_windowContextArray != null)
        {
            foreach (IWindowContext? context in _windowContextArray)
            {
                if (context is WindowContext windowContext)
                {
                    windowContext.Dispose();
                }
            }
        }

        _windowContextArray = null;
        _activeWindowContext = null;

        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::disposeRenderer
    public virtual object? DisposeRenderer(params object?[] args)
    {
        _windowRenderer?.Dispose();
        _windowRenderer = null;
        _skinContainer = null;

        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::disposeManagers
    public virtual object? DisposeManagers(params object?[] args)
    {
        _resourceManager?.Dispose();
        _resourceManager = null;
        _hintManager?.Dispose();
        _hintManager = null;

        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::create — typed overload
    public IWindow? Create
    (
        string name,
        string caption,
        uint type,
        uint style,
        uint param,
        Rect2 bounds,
        Action<WindowEvent, IWindow>? procedure = null,
        IWindow? parent = null,
        uint id = 0,
        IList<object>? properties = null,
        string dynamicStyle = "",
        IList<string>? tags = null
    )
    {
        EnsureInitialized();

        return _activeWindowContext?.Create(name, caption, type, style, param, bounds, procedure, parent, id, properties, dynamicStyle,
            tags);
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::create
    public virtual object? Create(params object?[] args)
    {
        if (args.Length < 5 || args[0] == null || args[4] is not Rect2 rect)
        {
            return null;
        }

        return CreateCore(
            Convert.ToString(args[0]) ?? string.Empty,
            ToUInt(args[1]),
            ToUInt(args[2]),
            ToUInt(args[3]),
            rect,
            args.Length > 5 ? args[5] as Action<WindowEvent, IWindow> : null,
            args.Length > 6 && args[6] != null ? Convert.ToString(args[6]) ?? string.Empty : string.Empty,
            args.Length > 7 ? ToUInt(args[7]) : 0,
            args.Length > 8 ? args[8] as IList<string> : null,
            args.Length > 9 ? args[9] as IWindow : null,
            args.Length > 10 ? args[10] as IList<object> : null,
            args.Length > 11 && args[11] != null ? Convert.ToString(args[11]) ?? string.Empty : string.Empty
        );
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::destroy
    public virtual object? Destroy(params object?[] args)
    {
        if (args.Length > 0 && args[0] is IWindow window)
        {
            window.Destroy();
        }

        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::buildFromXML
    public virtual object? BuildFromXml(params object?[] args)
    {
        if (args.Length == 0 || args[0] == null)
        {
            return null;
        }

        XElement? xml = args[0] switch
        {
            XElement element => element,
            XDocument { Root: not null } doc => doc.Root,
            string xmlString => XElement.Parse(xmlString),
            _ => null,
        };

        return xml == null
            ? null
            : BuildFromXmlCore(xml, args.Length > 1 ? ToUInt(args[1]) : 1,
                args.Length > 2 ? args[2] as Dictionary<string, object?> : null);
    }
    /// @see habbo/window/HabboWindowManagerComponent.as::windowToXMLString
    public virtual object? WindowToXmlString(params object?[] args)
    {
        if (args.Length == 0 || args[0] is not IWindow window)
        {
            return string.Empty;
        }

        EnsureInitialized();

        return _activeWindowContext?.GetWindowParser().WindowToXmlString(window) ?? string.Empty;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::getLayoutByTypeAndStyle
    public virtual object? GetLayoutByTypeAndStyle(params object?[] args)
    {
        if (args.Length < 2)
        {
            return null;
        }

        EnsureInitialized();

        return _skinContainer?.GetWindowLayoutByTypeAndStyle(ToUInt(args[0]), ToUInt(args[1]));
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::getDefaultsByTypeAndStyle
    public virtual object? GetDefaultsByTypeAndStyle(params object?[] args)
    {
        if (args.Length < 2)
        {
            return null;
        }

        EnsureInitialized();

        return _skinContainer?.GetDefaultAttributesByTypeAndStyle(ToUInt(args[0]), ToUInt(args[1]));
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::getRendererByTypeAndStyle
    public virtual object? GetRendererByTypeAndStyle(params object?[] args)
    {
        if (args.Length < 2)
        {
            return null;
        }

        EnsureInitialized();

        return _skinContainer?.GetSkinRendererByTypeAndStyle(ToUInt(args[0]), ToUInt(args[1]));
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::createWindow
    public virtual object? CreateWindow(params object?[] args)
    {
        return CreateWindowCore(
            args.Length > 0 && args[0] != null ? Convert.ToString(args[0]) ?? string.Empty : string.Empty,
            args.Length > 1 && args[1] != null ? Convert.ToString(args[1]) ?? string.Empty : string.Empty,
            args.Length > 2 ? ToUInt(args[2]) : 0,
            args.Length > 3 ? ToUInt(args[3]) : 0,
            args.Length > 4 ? ToUInt(args[4]) : 0,
            args.Length > 5 && args[5] is Rect2 rect ? rect : new Rect2(0, 0, 0, 0),
            args.Length > 6 ? args[6] as Action<WindowEvent, IWindow> : null,
            args.Length > 7 ? ToUInt(args[7]) : 0,
            args.Length > 8 ? ToUInt(args[8]) : 1,
            args.Length > 9 && args[9] != null ? Convert.ToString(args[9]) ?? string.Empty : string.Empty
        );
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::removeWindow
    public virtual object? RemoveWindow(params object?[] args)
    {
        RemoveWindowCore(
            args.Length > 0 && args[0] != null ? Convert.ToString(args[0]) ?? string.Empty : string.Empty,
            args.Length > 1 ? ToUInt(args[1]) : 1
        );
        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::getWindowByName
    public virtual object? GetWindowByName(params object?[] args)
    {
        return GetWindowByNameCore(
            args.Length > 0 && args[0] != null ? Convert.ToString(args[0]) ?? string.Empty : string.Empty,
            args.Length > 1 ? ToUInt(args[1]) : 1
        );
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::getActiveWindow
    public virtual object? GetActiveWindow(params object?[] args)
    {
        return GetActiveWindowCore(args.Length > 0 ? ToUInt(args[0]) : 1);
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::getWindowContext
    public virtual object? GetWindowContext(params object?[] args)
    {
        return GetWindowContextCore(args.Length > 0 ? ToUInt(args[0]) : 1);
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::getDesktop
    public virtual object? GetDesktop(params object?[] args)
    {
        return GetWindowContextCore(args.Length > 0 ? ToUInt(args[0]) : 1)?.GetDesktopWindow();
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::notify
    public virtual object? Notify(params object?[] args)
    {
        string title = args.Length > 0 && args[0] != null ? Convert.ToString(args[0]) ?? string.Empty : string.Empty;
        string summary = args.Length > 1 && args[1] != null ? Convert.ToString(args[1]) ?? string.Empty : string.Empty;
        Action? cb = args.Length > 2 ? args[2] as Action : null;
        XElement? xml = GetAlertXml("habbo_alert_xml");
        return xml != null
            ? new AlertDialog(
                this, xml, title, summary, 0, (d, _) =>
                {
                    cb?.Invoke();
                    d.Dispose();
                }, false
            )
            : null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::alert
    public virtual object? Alert(params object?[] args)
    {
        string title = args.Length > 0 && args[0] != null ? Convert.ToString(args[0]) ?? string.Empty : string.Empty;
        string summary = args.Length > 1 && args[1] != null ? Convert.ToString(args[1]) ?? string.Empty : string.Empty;
        uint flags = args.Length > 2 ? ToUInt(args[2]) : 0u;
        Action? cb = args.Length > 3 ? args[3] as Action : null;
        XElement? xml = GetAlertXml("habbo_alert_xml");
        return xml != null
            ? new AlertDialog(
                this, xml, title, summary, flags, (d, _) =>
                {
                    cb?.Invoke();
                    d.Dispose();
                }, false
            )
            : null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::alertWithModal
    public virtual object? AlertWithModal(params object?[] args)
    {
        string title = args.Length > 0 && args[0] != null ? Convert.ToString(args[0]) ?? string.Empty : string.Empty;
        string summary = args.Length > 1 && args[1] != null ? Convert.ToString(args[1]) ?? string.Empty : string.Empty;
        uint flags = args.Length > 2 ? ToUInt(args[2]) : 0u;
        Action? cb = args.Length > 3 ? args[3] as Action : null;
        XElement? xml = GetAlertXml("habbo_alert_xml");
        return xml != null
            ? new AlertDialog(
                this, xml, title, summary, flags, (d, _) =>
                {
                    cb?.Invoke();
                    d.Dispose();
                }, true
            )
            : null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::alertWithLink
    public virtual object? AlertWithLink(params object?[] args)
    {
        string title = args.Length > 0 && args[0] != null ? Convert.ToString(args[0]) ?? string.Empty : string.Empty;
        string summary = args.Length > 1 && args[1] != null ? Convert.ToString(args[1]) ?? string.Empty : string.Empty;
        string linkTitle = args.Length > 2 ? Convert.ToString(args[2]) ?? string.Empty : string.Empty;
        string linkUrl = args.Length > 3 ? Convert.ToString(args[3]) ?? string.Empty : string.Empty;
        uint flags = args.Length > 4 ? ToUInt(args[4]) : 0u;
        Action? cb = args.Length > 5 ? args[5] as Action : null;
        XElement? xml = GetAlertXml("habbo_alert_xml");
        return xml != null
            ? new AlertDialogWithLink(
                this, xml, title, summary, linkTitle, linkUrl, flags, (d, _) =>
                {
                    cb?.Invoke();
                    d.Dispose();
                }
            )
            : null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::confirm
    public virtual object? Confirm(params object?[] args)
    {
        return Alert(args);
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::confirmWithModal
    public virtual object? ConfirmWithModal(params object?[] args)
    {
        return AlertWithModal(args);
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::registerLocalizationParameter
    public virtual object? RegisterLocalizationParameter(params object?[] args)
    {
        if (args.Length >= 3)
        {
            _localization?.RegisterParameter(
                Convert.ToString(args[0]) ?? "",
                Convert.ToString(args[1]) ?? "",
                Convert.ToString(args[2]) ?? "",
                args.Length > 3 ? Convert.ToString(args[3]) ?? "%" : "%"
            );
        }
        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::update — IUpdateReceiver
    void IUpdateReceiver.Update(uint param1)
    {
        Update(param1);
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::update
    public virtual object? Update(params object?[] args)
    {
        EnsureInitialized();
        uint delta = args.Length > 0 ? ToUInt(args[0]) : 0;

        if (_windowContextArray == null)
        {
            return null;
        }

        foreach (IWindowContext? context in _windowContextArray)
        {
            if (context is not WindowContext wc)
            {
                continue;
            }

            wc.Update(delta);
            wc.Render(delta);
        }

        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::purge
    public override void Purge() { }

    /// @see habbo/window/HabboWindowManagerComponent.as::addMouseEventTracker
    public virtual object? AddMouseEventTracker(params object?[] args)
    {
        if (args.Length <= 0 || args[0] is not IInputEventTracker tracker || _windowContextArray == null)
        {
            return null;
        }

        foreach (IWindowContext? context in _windowContextArray)
        {
            context?.AddMouseEventTracker(tracker);
        }

        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::removeMouseEventTracker
    public virtual object? RemoveMouseEventTracker(params object?[] args)
    {
        if (args.Length <= 0 || args[0] is not IInputEventTracker tracker || _windowContextArray == null)
        {
            return null;
        }

        foreach (IWindowContext? context in _windowContextArray)
        {
            context?.RemoveMouseEventTracker(tracker);
        }

        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::eventReceived
    public virtual object? EventReceived(params object?[] args)
    {
        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::performTestCases
    public virtual object? PerformTestCases(params object?[] args)
    {
        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::findWindowByName
    public virtual object? FindWindowByName(params object?[] args)
    {
        return FindWindowByNameCore(args.Length > 0 && args[0] != null ? Convert.ToString(args[0]) ?? string.Empty : string.Empty);
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::findWindowByTag
    public virtual object? FindWindowByTag(params object?[] args)
    {
        return FindWindowByTagCore(args.Length > 0 && args[0] != null ? Convert.ToString(args[0]) ?? string.Empty : string.Empty);
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::groupWindowsWithTag
    public virtual object? GroupWindowsWithTag(params object?[] args)
    {
        return args.Length > 1 && args[1] is IList<IWindow> windows
            ? GroupWindowsWithTagCore(
                args[0] != null ? Convert.ToString(args[0]) ?? string.Empty : string.Empty, windows,
                args.Length > 2 && args[2] != null ? Convert.ToInt32(args[2]) : 0
            )
            : 0u;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::getThemeManager
    public virtual object? GetThemeManager(params object?[] args)
    {
        return _themeManager;
    }

    /// Explicit implementation of IWindowFactory.GetThemeManager()
    Core.Window.Theme.IThemeManager? IWindowFactory.GetThemeManager()
    {
        return _themeManager as Core.Window.Theme.IThemeManager;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::createUnseenItemCounter
    public virtual object? CreateUnseenItemCounter(params object?[] args)
    {
        return new WindowContainerModel("unseen_item_counter", new Rect2(0, 0, 0, 0));
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::createWidget
    public virtual object? CreateWidget(params object?[] args)
    {
        return args.Length > 1 && args[0] != null && args[1] is IDesktopWindow desktop
            ? CreateWidget(Convert.ToString(args[0]) ?? string.Empty, desktop)
            : null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::buildModalDialogFromXML
    public virtual object? BuildModalDialogFromXml(params object?[] args)
    {
        return args.Length > 0 && args[0] != null
            ? new ModalDialog(this, args[0])
            : null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::simpleAlert
    public virtual object? SimpleAlert(params object?[] args)
    {
        return new SimpleAlertDialog(
            this,
            args.Length > 0 && args[0] != null ? Convert.ToString(args[0]) ?? string.Empty : string.Empty,
            args.Length > 1 ? Convert.ToString(args[1]) : null,
            args.Length > 2 && args[2] != null ? Convert.ToString(args[2]) ?? string.Empty : string.Empty,
            args.Length > 3 ? Convert.ToString(args[3]) : null,
            args.Length > 4 ? Convert.ToString(args[4]) : null,
            args.Length > 5 ? args[5] as Dictionary<string, object?> : null,
            args.Length > 6 ? Convert.ToString(args[6]) : null,
            args.Length > 7 ? args[7] as Action : null,
            args.Length > 8 ? args[8] as Action : null
        );
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::registerHintWindow
    public virtual object? RegisterHintWindow(params object?[] args)
    {
        if (args is
            [
                string key, IWindow window, int style, ..,
            ])
        {
            _hintManager?.RegisterWindow(key, window, style);
        }
        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::unregisterHintWindow
    public virtual object? UnregisterHintWindow(params object?[] args)
    {
        if (args is
            [
                string key, ..,
            ])
        {
            _hintManager?.UnregisterWindow(key);
        }
        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::showHint
    public virtual object? ShowHint(params object?[] args)
    {
        if (args is
            [
                string key, ..,
            ])
        {
            _hintManager?.ShowHint(key, args.Length > 1 ? args[1] as Rect2? : null);
        }
        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::hideHint
    public virtual object? HideHint(params object?[] args)
    {
        _hintManager?.HideHint();
        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::hideMatchingHint
    public virtual object? HideMatchingHint(params object?[] args)
    {
        if (args is
            [
                string key, ..,
            ])
        {
            _hintManager?.HideMatchingHint(key);
        }
        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::displayFloorPlanEditor
    public virtual object? DisplayFloorPlanEditor(params object?[] args)
    {
        _bcfloorPlanEditor ??= new object();

        return null;
    }

    /// @see habbo/window/HabboWindowManagerComponent.as::openHelpPage
    public virtual object? OpenHelpPage(params object?[] args)
    {
        TryInvoke(_habboPagesViewer, "openPage", args);

        return null;
    }

    public static IWidget? CreateWidget(string param1, IDesktopWindow param2)
    {
        _ = param2;

        throw new Exception("Unknown widget type " + param1 + "! You might need to update Glaze to be able to work on this layout.");
    }

    private ISkinContainer? _skinContainer;
    private IClass3354? _windowRenderer;

    private void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        _resourceManager ??= new ResourceManager(this);
        _hintManager ??= new HintManager(this);

        // Godot adaptation: create SkinContainer, load skins, then create ThemeManager + WindowRenderer.
        if (_skinContainer == null)
        {
            _skinContainer = new SkinContainer();

            // @see HabboWindowManagerComponent.as::initComponent
            // AS3: var asset:IAsset = findAssetByName("habbo_element_description_xml");
            //      class_3503.parse(asset.content as XML, assets, _skinContainer);
            // Godot adaptation: bootstrap passes null as assets; fall back to HabboFileSystemAssetLibrary
            // (same adapter used by WindowSystemCreation). Element-description read from local storage first;
            // if not preloaded, fetched from the library (covers the un-wired runtime case).
            IAssetLibrary assetLib = (_assets as IAssetLibrary) ?? new HabboFileSystemAssetLibrary();

            IAsset? elementDescAsset = (FindAssetByName("habbo_element_description_xml") as IAsset)
                ?? assetLib.GetAssetByName("habbo_element_description_xml");

            if (elementDescAsset is null)
                throw new InvalidOperationException(
                    "Required asset 'habbo_element_description_xml' is missing.");

            XElement elementDescXml = (elementDescAsset.Content as XElement)
                ?? throw new InvalidOperationException(
                    "Asset 'habbo_element_description_xml' has no XML content.");

            SkinParserUtil.Parse(elementDescXml, assetLib, (SkinContainer)_skinContainer);

            // @see HabboWindowManagerComponent.as::initComponent — this.removeAsset(asset); asset.dispose()
            RemoveAsset("habbo_element_description_xml");
            elementDescAsset.Dispose();
        }

        // @see HabboWindowManagerComponent.as — init text style registry and parse CSS
        TextStyleManager.Init();

        // @see HabboWindowManagerComponent.as — AS3 loads CSS from asset library too
        string? cssText = null;
        if (_assets is Core.Assets.IAssetLibrary cssLib)
        {
            IAsset? cssAsset = cssLib.GetAssetByName("text_styles_css");
            if (cssAsset?.Content is string cssContent)
            {
                cssText = cssContent;
            }
        }
        // Godot adaptation: fallback to filesystem
        cssText ??= HabboAssetResolver.LoadTextAsset("text_styles_css");

        if (cssText != null)
        {
            List<TextStyle> parsedStyles = TextStyleManager.ParseCss(cssText);

            TextStyleManager.SetStyles(parsedStyles, true);

            GD.Print($"[HabboWindowManagerComponent] Loaded {TextStyleManager.StyleCount} text styles.");
        }

        _themeManager ??= new Theme.ThemeManager(_skinContainer);
        _windowRenderer ??= new WindowRenderer(_skinContainer);

        // Godot adaptation: resolve the Godot display node from the component context.
        Node? displayNode = context.displayObjectContainer as Node;

        // Godot adaptation: init text-to-image renderer using scene tree
        if (displayNode != null)
        {
            TextImageRenderer.Initialize(displayNode);
        }

        _windowContextArray = new IWindowContext?[NUMBER_OF_CONTEXT_LAYERS];

        Rect2 bounds = GetInitialContextBounds(displayNode);

        for (int i = 0;
             i < NUMBER_OF_CONTEXT_LAYERS;
             i++)
        {
            // Godot adaptation: create a root Node2D per layer, attached to the scene tree.
            Node2D? layerRoot = null;

            if (displayNode != null)
            {
                layerRoot = new Node2D
                {
                    Name = $"WindowLayer_{i}",
                };
                displayNode.AddChild(layerRoot);
            }

            _windowContextArray[i] = new WindowContext(
                $"layer_{i}", _windowRenderer, this, this,
                _resourceManager, _localization, this,
                layerRoot, bounds, null
            );
        }

        _activeWindowContext = _windowContextArray[DEFAULT_CONTEXT_LAYER_INDEX];

        // @see HabboWindowManagerComponent.as::initComponent — register for frame updates
        context.RegisterUpdateReceiver(this, 0);

        // @see WindowContext.as::stageResizedHandler — connect to Godot viewport resize signal
        _stageViewport = displayNode?.GetViewport();
        if (_stageViewport != null)
        {
            _stageViewport.SizeChanged += OnViewportSizeChanged;
            OnViewportSizeChanged();
        }

        _initialized = true;
    }

    /// @see WindowContext.as::stageResizedHandler
    private void OnViewportSizeChanged()
    {
        if (_windowContextArray == null)
        {
            return;
        }

        foreach (IWindowContext? ctx in _windowContextArray)
        {
            if (ctx is WindowContext windowContext)
            {
                windowContext.StageResizedHandler(null);
            }
        }
    }

    private static Rect2 GetInitialContextBounds(Node? displayNode)
    {
        Vector2 size = displayNode?.GetViewport()?.GetVisibleRect().Size ?? Vector2.Zero;

        if (size.X < 10 || size.Y < 10)
        {
            size = new Vector2(800, 600);
        }

        return new Rect2(0, 0, size.X, size.Y);
    }

    private IWindow? CreateCore
    (
        string name,
        uint type,
        uint style,
        uint param,
        Rect2 bounds,
        Action<WindowEvent, IWindow>? procedure,
        string caption,
        uint id,
        IList<string>? tags,
        IWindow? parent,
        IList<object>? properties,
        string dynamicStyle
    )
    {
        EnsureInitialized();

        return _activeWindowContext?.Create(name, caption, type, style, param, bounds, procedure, parent, id, properties, dynamicStyle,
            tags);
    }

    private IWindow? CreateWindowCore
    (
        string name,
        string caption,
        uint type,
        uint style,
        uint param,
        Rect2 bounds,
        Action<WindowEvent, IWindow>? procedure,
        uint id,
        uint layer,
        string dynamicStyle
    )
    {
        EnsureInitialized();

        return GetWindowContextCore(layer)?.Create(name, caption, type, style, param, bounds, procedure, null, id, null, dynamicStyle);
    }

    private IWindowContext? GetWindowContextCore(uint layer)
    {
        EnsureInitialized();

        return _windowContextArray != null && layer < _windowContextArray.Length ? _windowContextArray[layer] : null;
    }

    private IWindow? BuildFromXmlCore(XElement xml, uint layer, Dictionary<string, object?>? map)
    {
        return GetWindowContextCore(layer)?.GetWindowParser().ParseAndConstruct(xml, null, map);
    }

    private void RemoveWindowCore(string name, uint layer)
    {
        if (GetWindowContextCore(layer)?.GetDesktopWindow() is IWindow desktopWindow)
        {
            desktopWindow.GetChildByName(name)?.Destroy();
        }
    }

    private IWindow? GetWindowByNameCore(string name, uint layer)
    {
        return GetWindowContextCore(layer)?.GetDesktopWindow() is IWindow desktopWindow
            ? desktopWindow.GetChildByName(name)
            : null;
    }

    private IWindow? GetActiveWindowCore(uint layer)
    {
        if (GetWindowContextCore(layer)?.GetDesktopWindow() is not IWindow desktopWindow)
        {
            return null;
        }

        int idx = desktopWindow.numChildren - 1;

        return idx >= 0 ? desktopWindow.GetChildAt(idx) : null;
    }

    private IWindow? FindWindowByNameCore(string name)
    {
        EnsureInitialized();

        return _windowContextArray?.Select(context => context?.FindWindowByName(name)).OfType<IWindow>().FirstOrDefault();
    }

    private IWindow? FindWindowByTagCore(string tag)
    {
        EnsureInitialized();

        return _windowContextArray?.Select(context => context?.FindWindowByTag(tag)).OfType<IWindow>().FirstOrDefault();
    }

    private uint GroupWindowsWithTagCore(string tag, IList<IWindow> windows, int layer)
    {
        EnsureInitialized();

        return _windowContextArray == null
            ? (uint)0
            : _windowContextArray.OfType<IWindowContext>()
                                 .Aggregate<IWindowContext?, uint>(0,
                                     (current, context) => current + context.GroupChildrenWithTag(tag, windows, layer));
    }

    private static uint ToUInt(object? value)
    {
        if (value == null)
        {
            return 0;
        }

        return value switch
        {
            uint u => u,
            int i and >= 0 => (uint)i,
            long l and >= 0 => (uint)l,
            _ => Convert.ToUInt32(value),
        };
    }

    private static object? TryInvoke(object? target, string methodName, params object?[] args)
    {
        if (target == null)
        {
            return null;
        }

        foreach (MethodInfo method in target.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (!string.Equals(method.Name, methodName, StringComparison.Ordinal))
            {
                continue;
            }

            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length == args.Length)
            {
                return method.Invoke(target, args);
            }

            if (parameters.Length == 1 && parameters[0].GetCustomAttribute<ParamArrayAttribute>() != null)
            {
                return method.Invoke(
                    target, [
                        args,
                    ]
                );
            }
        }

        return null;
    }


    private sealed class WindowContainerModel(string param1, Rect2 param2) : WindowModel(param1, param2), IWindowContainer
    {
        public IWindow? GetChildUnderPoint(Vector2 param1)
        {
            for (int i = _children.Count - 1;
                 i >= 0;
                 i--)
            {
                IWindow child = _children[i];

                if (!child.visible)
                {
                    continue;
                }

                if (param1.X >= child.x && param1.X <= child.x + child.width &&
                    param1.Y >= child.y && param1.Y <= child.y + child.height)
                {
                    return child;
                }
            }

            return null;
        }

        public void GroupChildrenUnderPoint(Vector2 param1, IList<IWindow> param2)
        {
            for (int i = _children.Count - 1;
                 i >= 0;
                 i--)
            {
                IWindow child = _children[i];

                if (!child.visible)
                {
                    continue;
                }

                if (param1.X >= child.x && param1.X <= child.x + child.width &&
                    param1.Y >= child.y && param1.Y <= child.y + child.height)
                {
                    param2.Add(child);
                }
            }
        }
    }

    /// <summary>
    /// Looks up an alert XML asset by name from the asset library.
    /// </summary>
    private XElement? GetAlertXml(string assetName)
    {
        object? asset = FindAssetByName(assetName);
        if (asset is Core.Assets.IAsset { Content: XElement xmlContent })
        {
            return xmlContent;
        }

        if (_assets is Core.Assets.IAssetLibrary assetLib)
        {
            IAsset? libAsset = assetLib.GetAssetByName(assetName);
            if (libAsset?.Content is XElement libXml)
            {
                return libXml;
            }
        }

        return HabboAssetResolver.LoadXmlAsset(assetName);
    }

    /// @see HabboWindowManagerComponent.as::get windowContext (typed helper for ModalDialog)
    internal IWindowContext? GetWindowContext(uint layer)
    {
        return GetWindowContextCore(layer);
    }
}
