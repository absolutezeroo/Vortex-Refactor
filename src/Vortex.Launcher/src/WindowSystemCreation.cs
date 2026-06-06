using System;
using System.IO;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Graphics;
using Vortex.Core.Window.Utils;
using Vortex.Habbo.Localization;
using Vortex.Habbo.Window;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Test;

/// <summary>
/// Visual test harness for the Window System rendering pipeline.
/// Lists all XML layouts from data/layouts/ and loads them through the real
/// WindowParser → WindowRenderer → SkinContainer pipeline, same as in-game.
/// </summary>
public partial class WindowSystemCreation : Control
{
    private WindowContext? _context;
    private WindowRenderer? _renderer;
    private SkinContainer? _skinContainer;
    private HabboLocalizationManager? _localization;
    private FakeContext? _fakeContext;

    private SubViewport? _viewport;
    private Tree? _treeView;
    private ItemList? _layoutList;
    private RichTextLabel? _logOutput;
    private Label? _statsLabel;

    private readonly List<string> _layoutPaths = new();
    private int _hostCounter;

    private const int RENDER_WIDTH = 800;
    private const int RENDER_HEIGHT = 600;

    public override void _Ready()
    {
        BuildUi();
        InitRenderPipeline();
        LoadLayouts();
        Log($"Ready. {_layoutPaths.Count} layouts found. Render target: {RENDER_WIDTH}x{RENDER_HEIGHT}");
    }

    private bool _loggedRenderIdle;

    public override void _Process(double delta)
    {
        _context?.Update((uint)(delta * 1000));

        int queueSize = GetRenderQueueSize();

        if (queueSize > 0)
        {
            Log($"[render] Queue has {queueSize} item(s) — processing...");
            _loggedRenderIdle = false;
        }

        _context?.Render((uint)(delta * 1000));

        if (queueSize > 0)
        {
            Log($"[render] Done. GC count: {GraphicContext.NumGraphicContexts}, TrackedImage: {TrackedImage.NumInstances}");
        }
        else if (!_loggedRenderIdle)
        {
            Log("[render] Queue empty — idle.");
            _loggedRenderIdle = true;
        }

        UpdateStats();
    }

    private int GetRenderQueueSize()
    {
        if (_context == null)
        {
            return 0;
        }

        IClass3354? renderer = WindowContext.GetRenderer();

        if (renderer is WindowRenderer wr)
        {
            return wr.RenderQueueCount;
        }

        return 0;
    }

    public override void _ExitTree()
    {
        _context?.Dispose();
        _context = null;
        _renderer?.Dispose();
        _renderer = null;
        _skinContainer?.Dispose();
        _skinContainer = null;
        _localization?.Dispose();
        _localization = null;
        _fakeContext?.Dispose();
        _fakeContext = null;
    }

    private void BuildUi()
    {
        HSplitContainer hSplit = new()
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            SplitOffset = 280,
        };
        AddChild(hSplit);

        // Left panel: layout list + tree inspector
        VBoxContainer leftPanel = new();
        hSplit.AddChild(leftPanel);

        leftPanel.AddChild(
            new Label
            {
                Text = "Layouts",
            }
        );

        _layoutList = new ItemList
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(260, 0),
        };
        _layoutList.ItemSelected += OnLayoutSelected;
        leftPanel.AddChild(_layoutList);

        HBoxContainer btnRow = new();
        leftPanel.AddChild(btnRow);

        Button btnLoadAll = new()
        {
            Text = "Load All",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        btnLoadAll.Pressed += OnLoadAllPressed;
        btnRow.AddChild(btnLoadAll);

        Button btnClear = new()
        {
            Text = "Clear",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        btnClear.Pressed += OnClearPressed;
        btnRow.AddChild(btnClear);

        Button btnSkinTest = new()
        {
            Text = "Skin Test",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        btnSkinTest.Pressed += OnSkinTestPressed;
        btnRow.AddChild(btnSkinTest);

        leftPanel.AddChild(
            new Label
            {
                Text = "IWindow Tree",
            }
        );

        _treeView = new Tree
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(260, 200),
            HideRoot = true,
        };
        leftPanel.AddChild(_treeView);

        // Right panel: viewport + stats + log
        VBoxContainer rightVBox = new();
        hSplit.AddChild(rightVBox);

        _statsLabel = new Label
        {
            Text = "TrackedImage: 0 | Bytes: 0",
        };
        _statsLabel.AddThemeFontSizeOverride("font_size", 11);
        rightVBox.AddChild(_statsLabel);

        VSplitContainer rightSplit = new()
        {
            SplitOffset = -120,
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        rightVBox.AddChild(rightSplit);

        // SubViewport for the real GraphicContext Node2D display
        SubViewportContainer viewportContainer = new()
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            StretchShrink = 1,
            Stretch = true,
        };
        rightSplit.AddChild(viewportContainer);

        _viewport = new SubViewport
        {
            Size = new Vector2I(RENDER_WIDTH, RENDER_HEIGHT),
            TransparentBg = false,
            RenderTargetClearMode = SubViewport.ClearMode.Always,
        };
        viewportContainer.AddChild(_viewport);

        // Dark background for the viewport
        ColorRect bg = new()
        {
            Color = new Color(0.1f, 0.1f, 0.18f, 1f),
            Size = new Vector2(RENDER_WIDTH, RENDER_HEIGHT),
        };
        _viewport.AddChild(bg);

        // Log output
        _logOutput = new RichTextLabel
        {
            BbcodeEnabled = true,
            ScrollFollowing = true,
            CustomMinimumSize = new Vector2(0, 100),
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        rightSplit.AddChild(_logOutput);
    }

    private SkinContainer CreateAndLoadSkinContainer()
    {
        SkinContainer container = new();
        XElement? xml = HabboAssetResolver.LoadXmlAsset("habbo_element_description_xml");

        if (xml != null)
        {
            // Godot adaptation: use HabboFileSystemAssetLibrary so Parse receives an IAssetLibrary
            // (mirrors the component path where HabboWindowManagerComponent passes its own assets).
            SkinParserUtil.Parse(xml, new HabboFileSystemAssetLibrary(), container);
            Log("Skins loaded from habbo_element_description.xml");
        }
        else
        {
            Log("[color=yellow]habbo_element_description.xml not found — fallback colors only.[/color]");
        }

        return container;
    }

    private void InitRenderPipeline()
    {
        _skinContainer = CreateAndLoadSkinContainer();
        _renderer = new WindowRenderer(_skinContainer);

        // @see HabboWindowManagerComponent.EnsureInitialized — init text style registry and parse CSS
        InitTextSystem();

        // Initialize localization so ${key} captions resolve
        InitLocalization();

        Rect2 bounds = new(0, 0, RENDER_WIDTH, RENDER_HEIGHT);
        ResourceManager resourceManager = new(null);
        _context = new WindowContext("visual_test", _renderer, null, null, resourceManager, _localization, null, null, bounds, null);

        // Attach desktop's GraphicContext Node2D to the viewport
        AttachDesktopToViewport();

        // Init text-to-image renderer using the viewport as parent node
        TextImageRenderer.Initialize(_viewport!);
    }

    private void InitTextSystem()
    {
        TextStyleManager.Init();

        string? cssText = HabboAssetResolver.LoadTextAsset("text_styles_css");

        if (cssText != null)
        {
            List<TextStyle> parsedStyles = TextStyleManager.ParseCss(cssText);
            TextStyleManager.SetStyles(parsedStyles, true);
            Log($"Loaded {TextStyleManager.StyleCount} text styles from CSS.");
        }
        else
        {
            Log("[color=yellow]text_styles.css not found — text will use fallback styles.[/color]");
        }
    }

    private void InitLocalization()
    {
        _fakeContext = new FakeContext(null);
        _localization = new HabboLocalizationManager(_fakeContext, HabboLocalizationManager.SKIP_EXTERNAL_LOCALIZATIONS);

        if (_localization.LoadDefaultEmbedLocalizations("en"))
        {
            Log($"Localization loaded — {_localization.GetKeys().Length} keys.");
        }
        else
        {
            Log("[color=yellow]Localization files not found — ${key} captions won't resolve.[/color]");
        }
    }

    private void AttachDesktopToViewport()
    {
        IDesktopWindow? desktop = _context?.GetDesktopWindow();

        if (desktop is IGraphicContextHost desktopHost)
        {
            IGraphicContext? gc = desktopHost.GetGraphicContext(true);

            if (gc?.DisplayNode != null)
            {
                _viewport!.AddChild(gc.DisplayNode);
            }
        }
    }

    private void LoadLayouts()
    {
        string? layoutDir = ProjectSettings.GlobalizePath("res://data/layouts");

        if (!Directory.Exists(layoutDir))
        {
            Log("[color=red]data/layouts/ not found.[/color]");
            return;
        }

        string[] files = Directory.GetFiles(layoutDir, "*.xml", SearchOption.AllDirectories);
        Array.Sort(files);

        foreach (string file in files)
        {
            _layoutPaths.Add(file);
            _layoutList!.AddItem(Path.GetFileNameWithoutExtension(file));
        }
    }

    private void OnLayoutSelected(long index)
    {
        ClearCanvas();

        int idx = (int)index;

        if (idx < 0 || idx >= _layoutPaths.Count)
        {
            return;
        }

        try
        {
            string content = File.ReadAllText(_layoutPaths[idx]);
            XElement xml = XElement.Parse(content);
            string name = Path.GetFileName(_layoutPaths[idx]);
            ParseAndRender(xml);
            Log($"Loaded: [b]{name}[/b]");
        }
        catch (Exception e)
        {
            Log($"[color=red]Error: {e.Message}[/color]");
        }
    }

    private void OnLoadAllPressed()
    {
        ClearCanvas();

        int loaded = 0;
        int errors = 0;

        foreach (string path in _layoutPaths)
        {
            try
            {
                XElement xml = XElement.Parse(File.ReadAllText(path));
                ParseAndRender(xml);
                loaded++;
            }
            catch (Exception e)
            {
                Log($"[color=red]{Path.GetFileName(path)}: {e.Message}[/color]");
                errors++;
            }
        }

        Log($"Loaded {loaded}/{_layoutPaths.Count} layouts.{(errors > 0 ? $" {errors} error(s)." : "")}");
    }

    private void OnClearPressed()
    {
        ClearCanvas();
        Log("Canvas cleared.");
    }

    private void OnSkinTestPressed()
    {
        ClearCanvas();

        if (_context == null || _renderer == null || _skinContainer == null)
        {
            return;
        }

        // Type IDs from Class3652: frame=35, header=6, button=60, border=30
        (string name, uint type, uint style, float x, float y, float w, float h)[] typedWindows =
            new (string name, uint type, uint style, float x, float y, float w, float h)[]
            {
                ("skin_frame", 35, 0, 20, 20, 350, 250),
                ("skin_header", 6, 0, 20, 290, 350, 30),
                ("skin_button", 60, 0, 20, 340, 120, 30),
                ("skin_border", 30, 0, 400, 20, 300, 200),
            };

        int created = 0;

        foreach ((string name, uint type, uint style, float x, float y, float w, float h) in typedWindows)
        {
            bool hasRenderer = _skinContainer.SkinRendererExists(type, style);
            Rect2 bounds = new(x, y, w, h);

            // param=0 → no flag 16, window gets its own GC (like a top-level window)
            IWindow? window = _context.Create(
                name, "", type, style, 0, bounds, null, null, (uint)created
            );

            if (window == null)
            {
                Log($"[color=red]Failed to create typed window: {name} (type={type})[/color]");
                continue;
            }

            // GC wiring is handled automatically by Render() — just invalidate.
            _context.Invalidate(window, new Rect2(0, 0, w, h), Class3655.REDRAW);

            string renderPath = hasRenderer ? "SKIN" : "FILL";
            Log($"Created [b]{name}[/b] type={type} style={style} → {renderPath} ({w}x{h})");

            created++;
        }

        RefreshTreeView();
        Log($"Skin test: {created} typed windows created");
    }

    private void ParseAndRender(XElement xml)
    {
        if (_context == null || _renderer == null)
        {
            return;
        }

        // In-game, layout roots always have flag 16 (use parent GC) because they
        // are inserted into a host window by the calling component. Replicate that:
        // create a host container (param=0 → owns its GC), then parse into it.
        Rect2 hostBounds = new(0, 0, RENDER_WIDTH, RENDER_HEIGHT);
        IWindow? host = _context.Create(
            "_layout_host_" + _hostCounter++, "", 4 /* container */, 0, 0,
            hostBounds, null, null, 0
        );

        if (host == null)
        {
            Log("[color=red]Failed to create host container.[/color]");
            return;
        }

        IWindow? root = _context.GetWindowParser().ParseAndConstruct(xml, host);

        if (root == null)
        {
            Log("[color=yellow]WindowParser returned null.[/color]");
            return;
        }

        // Invalidate the host — the render pipeline handles GC wiring automatically.
        _context.Invalidate(host, new Rect2(0, 0, host.width, host.height), Class3655.REDRAW);

        RefreshTreeView();

        int totalNodes = CountDescendants(host);
        Log($"Parsed: [b]{root.name}[/b] ({root.width}x{root.height}) — {totalNodes} nodes total");
    }

    private static int CountDescendants(IWindow window)
    {
        int count = 1;

        for (int i = 0;
             i < window.numChildren;
             i++)
        {
            IWindow? child = window.GetChildAt(i);

            if (child != null)
            {
                count += CountDescendants(child);
            }
        }

        return count;
    }

    private void RefreshTreeView()
    {
        _treeView!.Clear();
        TreeItem? root = _treeView.CreateItem();

        if (_context == null)
        {
            return;
        }

        IDesktopWindow desktop = _context.GetDesktopWindow();

        if (desktop is IWindow desktopWindow)
        {
            BuildTreeItem(root, desktopWindow);
        }
    }

    private void BuildTreeItem(TreeItem parent, IWindow window)
    {
        TreeItem? item = _treeView!.CreateItem(parent);
        string typeName = window.GetType().Name;
        string label = !string.IsNullOrEmpty(window.name) ? window.name : $"<unnamed:{typeName}>";

        if (!string.IsNullOrEmpty(window.caption) && window.caption != window.name)
        {
            label += $" \"{window.caption}\"";
        }

        string bgInfo = window.background ? $" bg:0x{window.color:X6}" : "";
        string gcInfo = "";

        if (window is IGraphicContextHost host && host.HasGraphicsContext())
        {
            IGraphicContext? gc = host.GetGraphicContext(false);
            gcInfo = gc != null ? " [GC]" : "";
        }

        item.SetText(0, $"{label} ({typeName}) [{window.width}x{window.height}]{bgInfo}{gcInfo}");
        item.SetTooltipText(
            0, $"Controller: {typeName} | Type: {window.type} | Style: {window.style} | Param: 0x{window.param:X}\n" +
               $"Position: ({window.x}, {window.y})\n" +
               $"State: {window.state} | Visible: {window.visible}\n" +
               $"Background: {window.background} | Color: 0x{window.color:X8}"
        );

        for (int i = 0;
             i < window.numChildren;
             i++)
        {
            IWindow? child = window.GetChildAt(i);

            if (child != null)
            {
                BuildTreeItem(item, child);
            }
        }
    }

    private void ClearCanvas()
    {
        _context?.Dispose();
        _renderer?.Dispose();
        _skinContainer?.Dispose();

        if (_viewport != null)
        {
            for (int i = _viewport.GetChildCount() - 1;
                 i >= 0;
                 i--)
            {
                Node? child = _viewport.GetChild(i);

                if (child is not ColorRect && child is not SubViewport)
                {
                    child.QueueFree();
                }
            }
        }

        TextImageRenderer.ClearCache();

        _skinContainer = CreateAndLoadSkinContainer();
        _renderer = new WindowRenderer(_skinContainer);

        Rect2 bounds = new(0, 0, RENDER_WIDTH, RENDER_HEIGHT);
        ResourceManager resourceManager = new(null);
        _context = new WindowContext("visual_test", _renderer, null, null, resourceManager, _localization, null, null, bounds, null);

        AttachDesktopToViewport();

        _treeView?.Clear();
    }

    private void UpdateStats()
    {
        if (_statsLabel == null)
        {
            return;
        }

        bool hasFrame = _skinContainer?.SkinRendererExists(35, 0) ?? false;
        bool hasButton = _skinContainer?.SkinRendererExists(60, 0) ?? false;
        bool hasBorder = _skinContainer?.SkinRendererExists(30, 0) ?? false;

        _statsLabel.Text = $"TrackedImage: {TrackedImage.NumInstances} | " +
                           $"Bytes: {TrackedImage.AllocatedByteCount / 1024}KB | " +
                           $"GraphicContext: {GraphicContext.NumGraphicContexts} | " +
                           $"SkinRenderer[frame]: {hasFrame} [button]: {hasButton} [border]: {hasBorder}";
    }

    private void Log(string message)
    {
        _logOutput?.AppendText(message + "\n");
        GD.Print($"[WindowSystemTest] {message}");
    }
}
