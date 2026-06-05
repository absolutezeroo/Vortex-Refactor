using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Habbo.Avatar;
using Vortex.Habbo.Avatar.Events;

namespace Vortex.Test;

/// <summary>
/// Visual test harness for the Avatar rendering pipeline.
/// Bootstraps AvatarRenderManager with local XML data, triggers the real download pipeline
/// to load .vortex bundles, then renders avatar images using figure strings, directions, and actions.
/// </summary>
public partial class WindowAvatarCreation : Control
{
    private FakeContext? _fakeContext;
    private TestAvatarRenderManager? _renderManager;

    private TextureRect? _avatarDisplay;
    private LineEdit? _figureInput;
    private LineEdit? _bundlePathInput;
    private LineEdit? _figureDataUrlInput;
    private SpinBox? _directionSpin;
    private OptionButton? _scaleOption;
    private OptionButton? _actionOption;
    private RichTextLabel? _logOutput;
    private Label? _statsLabel;
    private CheckButton? _autoRotateToggle;

    private IAvatarImage? _currentAvatar;
    private bool _autoRotate;
    private double _autoRotateTimer;
    private int _autoRotateDir;

    private const string DEFAULT_FIGURE = "hd-180-1.ch-255-66.lg-280-110.sh-305-62.ha-1012-110.hr-828-61";
    private const int DISPLAY_SIZE = 200;
    private const string DEFAULT_DOWNLOAD_URL = "http://vortex-assets.local/gordon/vortex-assets-PRODUCTION-202601121522-867048149/";
    private const string DEFAULT_FIGUREDATA_URL = "http://vortex-assets.local/gamedata/figuredata.xml";

    // Action presets: label -> (actionType, actionParam)
    private static readonly (string Label, string ActionType, string? Param)[] ACTION_PRESETS =
    [
        ("Stand", AvatarAction.POSTURE, AvatarAction.POSTURE_STAND),
        ("Walk", AvatarAction.POSTURE, AvatarAction.POSTURE_WALK),
        ("Sit", AvatarAction.POSTURE, AvatarAction.POSTURE_SIT),
        ("Lay", AvatarAction.POSTURE, AvatarAction.POSTURE_LAY),
        ("Wave", AvatarAction.WAVE, null),
        ("Dance", AvatarAction.DANCE, "1"),
        ("Smile", AvatarAction.GESTURE, AvatarAction.GESTURE_SMILE),
        ("Angry", AvatarAction.GESTURE, AvatarAction.GESTURE_ANGRY),
        ("Sad", AvatarAction.GESTURE, AvatarAction.GESTURE_SAD),
        ("Surprised", AvatarAction.GESTURE, AvatarAction.GESTURE_SURPRISED),
        ("Talk", AvatarAction.TALK, null),
        ("Sleep", AvatarAction.SLEEP, null),
        ("Blow", AvatarAction.BLOW, null),
        ("Laugh", AvatarAction.LAUGH, null),
        ("Respect", AvatarAction.RESPECT, null),
        ("Idle", AvatarAction.IDLE, null),
        ("Swim", AvatarAction.POSTURE, AvatarAction.POSTURE_SWIM),
        ("Carry (1)", AvatarAction.CARRY_OBJECT, "1"),
        ("Carry (2)", AvatarAction.CARRY_OBJECT, "2"),
    ];

    public override void _Ready()
    {
        BuildUi();
        InitAvatarSystem();
    }

    public override void _Process(double delta)
    {
        if (!_autoRotate)
        {
            return;
        }

        _autoRotateTimer += delta;

        if (!(_autoRotateTimer >= 0.5))
        {
            return;
        }

        _autoRotateTimer = 0;
        _autoRotateDir = (_autoRotateDir + 1) % 8;
        _directionSpin!.Value = _autoRotateDir;

        RenderAvatar();
    }

    public override void _ExitTree()
    {
        DisposeCurrentAvatar();

        _renderManager?.Dispose();
        _renderManager = null;

        _fakeContext?.Dispose();
        _fakeContext = null;
    }

    private void BuildUi()
    {
        HSplitContainer hSplit = new()
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            SplitOffset = 320,
        };
        AddChild(hSplit);

        // Left panel: controls
        VBoxContainer leftPanel = new();
        hSplit.AddChild(leftPanel);

        // --- Download URL ---
        leftPanel.AddChild(
            new Label
            {
                Text = "Download URL",
            }
        );

        _bundlePathInput = new LineEdit
        {
            Text = DEFAULT_DOWNLOAD_URL,
            PlaceholderText = "https://images.habbo.com/gordon/PRODUCTION-.../",
            CustomMinimumSize = new Vector2(300, 0),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        leftPanel.AddChild(_bundlePathInput);

        // --- Figure Data URL ---
        leftPanel.AddChild(
            new Label
            {
                Text = "Figure Data URL",
            }
        );

        _figureDataUrlInput = new LineEdit
        {
            Text = DEFAULT_FIGUREDATA_URL,
            PlaceholderText = "https://www.habbo.com/gamedata/figuredata/1",
            CustomMinimumSize = new Vector2(300, 0),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        leftPanel.AddChild(_figureDataUrlInput);

        // --- Figure string ---
        leftPanel.AddChild(
            new Label
            {
                Text = "Figure String",
            }
        );

        _figureInput = new LineEdit
        {
            Text = DEFAULT_FIGURE,
            CustomMinimumSize = new Vector2(300, 0),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        _figureInput.TextSubmitted += _ => OnRenderPressed();
        leftPanel.AddChild(_figureInput);

        // --- Direction + Scale row ---
        HBoxContainer dirScaleRow = new();
        leftPanel.AddChild(dirScaleRow);

        dirScaleRow.AddChild(
            new Label
            {
                Text = "Dir:",
            }
        );
        _directionSpin = new SpinBox
        {
            MinValue = 0,
            MaxValue = 7,
            Value = 2,
            Step = 1,
            CustomMinimumSize = new Vector2(70, 0),
        };
        _directionSpin.ValueChanged += _ => RenderAvatar();
        dirScaleRow.AddChild(_directionSpin);

        dirScaleRow.AddChild(
            new Label
            {
                Text = "Scale:",
            }
        );
        _scaleOption = new OptionButton();
        _scaleOption.AddItem("Large (h)", 0);
        _scaleOption.AddItem("Small (sh)", 1);
        _scaleOption.Selected = 0;
        _scaleOption.ItemSelected += _ => RenderAvatar();
        dirScaleRow.AddChild(_scaleOption);

        // --- Auto-rotate ---
        _autoRotateToggle = new CheckButton
        {
            Text = "Auto-rotate",
        };
        _autoRotateToggle.Toggled += toggled =>
        {
            _autoRotate = toggled;
            _autoRotateTimer = 0;
        };
        leftPanel.AddChild(_autoRotateToggle);

        // --- Action ---
        leftPanel.AddChild(
            new Label
            {
                Text = "Action",
            }
        );
        _actionOption = new OptionButton();

        for (int i = 0;
             i < ACTION_PRESETS.Length;
             i++)
        {
            _actionOption.AddItem(ACTION_PRESETS[i].Label, i);
        }

        _actionOption.Selected = 0;
        _actionOption.ItemSelected += _ => RenderAvatar();
        leftPanel.AddChild(_actionOption);

        // --- Render button ---
        Button btnRender = new()
        {
            Text = "Render",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        btnRender.Pressed += OnRenderPressed;
        leftPanel.AddChild(btnRender);

        // --- Frame advance (for animations) ---
        HBoxContainer frameRow = new();
        leftPanel.AddChild(frameRow);

        Button btnPrevFrame = new()
        {
            Text = "<< Frame",
        };
        // Re-render at current frame (no advance)
        btnPrevFrame.Pressed += RenderAvatar;
        frameRow.AddChild(btnPrevFrame);

        Button btnNextFrame = new()
        {
            Text = "Frame >>",
        };
        btnNextFrame.Pressed += () =>
        {
            if (_currentAvatar != null)
            {
                _currentAvatar.UpdateAnimationByFrames(1);
                RenderCurrentFrame();
            }
        };
        frameRow.AddChild(btnNextFrame);

        Button btnResetFrame = new()
        {
            Text = "Reset",
        };
        btnResetFrame.Pressed += () =>
        {
            if (_currentAvatar != null)
            {
                _currentAvatar.ResetAnimationFrameCounter();
                RenderCurrentFrame();
            }
        };
        frameRow.AddChild(btnResetFrame);

        // --- Stats ---
        _statsLabel = new Label
        {
            Text = "Not initialized",
        };
        _statsLabel.AddThemeFontSizeOverride("font_size", 11);
        leftPanel.AddChild(_statsLabel);

        // Right panel: avatar display + log
        VBoxContainer rightVBox = new();
        hSplit.AddChild(rightVBox);

        // Avatar image display with dark background
        PanelContainer displayPanel = new()
        {
            CustomMinimumSize = new Vector2(DISPLAY_SIZE * 2, DISPLAY_SIZE * 2),
        };

        // Style the panel with dark background
        StyleBoxFlat styleBox = new()
        {
            BgColor = new Color(0.1f, 0.1f, 0.18f, 1f),
            CornerRadiusBottomLeft = 4,
            CornerRadiusBottomRight = 4,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
        };
        displayPanel.AddThemeStyleboxOverride("panel", styleBox);
        rightVBox.AddChild(displayPanel);

        _avatarDisplay = new TextureRect
        {
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize = new Vector2(DISPLAY_SIZE * 2, DISPLAY_SIZE * 2),
        };
        displayPanel.AddChild(_avatarDisplay);

        // Log output
        rightVBox.AddChild(
            new Label
            {
                Text = "Log",
            }
        );

        _logOutput = new RichTextLabel
        {
            BbcodeEnabled = true,
            ScrollFollowing = true,
            CustomMinimumSize = new Vector2(0, 200),
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        rightVBox.AddChild(_logOutput);
    }

    private void InitAvatarSystem()
    {
        try
        {
            string downloadUrl = _bundlePathInput?.Text ?? DEFAULT_DOWNLOAD_URL;

            // Ensure trailing slash
            if (!downloadUrl.EndsWith('/'))
            {
                downloadUrl += "/";
            }

            Log("Initializing avatar system...");
            Log($"Download URL: {downloadUrl}");

            string figuremapUrl = downloadUrl + "figuremap.xml";
            string figurePartListUrl = _figureDataUrlInput?.Text ?? DEFAULT_FIGUREDATA_URL;

            // 1. Create FakeContext
            _fakeContext = new FakeContext(null);

            // 2. Build AssetLibrary from manifest
            AssetLibrary? assetLibrary = BuildAvatarAssetLibrary();

            if (assetLibrary == null)
            {
                Log("[color=red]Failed to build avatar asset library from manifest.[/color]");
                return;
            }

            _fakeContext.AssetLibraryCollection?.AddAssetLibrary(assetLibrary);
            Log($"Asset library built. Has HabboAvatarGeometry: {assetLibrary.HasAsset("HabboAvatarGeometry")}");

            // 3. Inject XML content into the asset library
            InjectXmlContent(assetLibrary);

            // 4. Create TestAvatarRenderManager — InitComponent fires immediately (inNuxFlow=true)
            _renderManager = new TestAvatarRenderManager(_fakeContext, 0, assetLibrary);

            // 5. Set configuration properties for the download pipeline
            _renderManager.SetProperty("flash.dynamic.avatar.download.url", downloadUrl, true);
            _renderManager.SetProperty("flash.dynamic.avatar.download.name.template", "%libname%.vortex", true);
            _renderManager.SetProperty("flash.dynamic.avatar.download.configuration", figuremapUrl, true);
            // Point figurepartlist to the figuredata endpoint (server or Habbo CDN).
            // The local habbo_avatar_figure.xml is only a stub (swim suits + placeholder).
            // The real figure data with all clothing types comes from this URL.
            _renderManager.SetProperty("external.figurepartlist.txt", figurePartListUrl, true);
            Log($"  figurepartlist = {figurePartListUrl}");

            Log("Configuration properties set:");
            Log($"  download.url = {downloadUrl}");
            Log($"  download.configuration = {figuremapUrl}");
            Log($"  name.template = %libname%.vortex");

            // 6. Listen for readiness event
            _renderManager.Events.AddEventListener(AvatarRenderEvent.AVATAR_RENDER_READY, OnAvatarRenderReady);

            // 7. Trigger the real download pipeline (OnConfigurationComplete)
            Log("Triggering configuration complete (download pipeline)...");
            _renderManager.TriggerConfigurationComplete();

            Log($"AvatarRenderManager ready: {_renderManager.IsReady}");
            Log($"FigureData available: {_renderManager.GetFigureData() != null}");
            Log($"AnimationManager available: {_renderManager.GetAnimationManager() != null}");

            UpdateStats();

            // If already ready (all bundles loaded synchronously), render immediately
            if (_renderManager.IsReady)
            {
                Log("Pipeline ready immediately (synchronous load). Rendering...");
                RenderAvatar();
            }
            else
            {
                Log("[color=yellow]Waiting for download pipeline to complete...[/color]");
            }
        }
        catch (Exception e)
        {
            Log($"[color=red]Init failed: {e.Message}[/color]");
            Log($"[color=red]{e.StackTrace}[/color]");
        }
    }

    private void OnAvatarRenderReady(object? param)
    {
        Log("[color=green]Avatar render pipeline ready![/color]");
        UpdateStats();
        RenderAvatar();
    }

    /// Builds an AssetLibrary from the HabboAvatarRenderLib manifest.
    /// Reuses the pattern from HabboLoadingScreen.BuildAssetLibraryFromManifest.
    private static AssetLibrary? BuildAvatarAssetLibrary()
    {
        string manifestPath = Path.Combine("data", "manifests", "HabboAvatarRenderLib_manifest.xml");

        if (!File.Exists(manifestPath))
        {
            return null;
        }

        XElement manifestXml;

        try
        {
            manifestXml = XElement.Parse(File.ReadAllText(manifestPath, System.Text.Encoding.UTF8));
        }
        catch
        {
            return null;
        }

        XElement? componentElement = manifestXml.Elements("component").FirstOrDefault();

        if (componentElement == null)
        {
            return null;
        }

        List<XElement> assetsElements = componentElement.Elements("assets").ToList();

        if (assetsElements.Count == 0)
        {
            return null;
        }

        XElement libraryManifest = new("manifest", new XElement("library"));
        XElement libraryElement = libraryManifest.Element("library")!;

        foreach (XElement assetsElement in assetsElements)
        {
            libraryElement.Add(assetsElement);
        }

        foreach (XElement aliasesElement in componentElement.Elements("aliases"))
        {
            libraryElement.Add(aliasesElement);
        }

        AssetLibrary assetLibrary = new("_avatarRenderLib@", libraryManifest);
        assetLibrary.LoadFromResource(libraryManifest, typeof(AvatarRenderManager));

        return assetLibrary;
    }

    /// Loads the 4 avatar XML data files from disk and injects them into the asset library.
    /// The manifest creates XmlAsset stubs with null content (lazy). We populate them here.
    private void InjectXmlContent(AssetLibrary library)
    {
        (string AssetName, string FileName)[] xmlAssets = new (string AssetName, string FileName)[]
        {
            ("HabboAvatarGeometry", "habbo_avatar_geometry"),
            ("HabboAvatarPartSets", "habbo_avatar_part_sets"),
            ("HabboAvatarAnimation", "habbo_avatar_animation"),
            ("HabboAvatarFigure", "habbo_avatar_figure"),
        };

        foreach ((string assetName, string fileName) in xmlAssets)
        {
            IAsset? asset = library.GetAssetByName(assetName);

            if (asset is not XmlAsset xmlAsset)
            {
                Log($"[color=yellow]Asset '{assetName}' not found or not XmlAsset.[/color]");
                continue;
            }

            string filePath = Path.Combine("data", "layouts", $"{fileName}.xml");

            if (!File.Exists(filePath))
            {
                Log($"[color=yellow]XML file not found: {filePath}[/color]");
                continue;
            }

            try
            {
                XElement xml = XElement.Load(filePath);
                xmlAsset.SetUnknownContent(xml);
                Log($"Injected XML: {assetName} ({xml.Name.LocalName}, {xml.Elements().Count()} children)");
            }
            catch (Exception e)
            {
                Log($"[color=red]Failed to load {filePath}: {e.Message}[/color]");
            }
        }
    }

    private void OnRenderPressed()
    {
        RenderAvatar();
    }

    private void RenderAvatar()
    {
        if (_renderManager == null || !_renderManager.IsReady)
        {
            Log("[color=yellow]Render manager not ready.[/color]");
            return;
        }

        string figureString = _figureInput?.Text ?? DEFAULT_FIGURE;
        int direction = (int)(_directionSpin?.Value ?? 2);
        string scale = _scaleOption?.Selected == 1 ? AvatarScaleType.SMALL : AvatarScaleType.LARGE;
        int actionIdx = (int)(_actionOption?.Selected ?? 0);

        try
        {
            DisposeCurrentAvatar();

            // Pre-download figure assets before creating the image.
            // CreateAvatarImage returns PlaceholderAvatarImage if assets aren't ready,
            // but since downloads are synchronous, DownloadFigure completes immediately.
            IFigureContainer figure = _renderManager.CreateFigureContainer(figureString);

            if (!_renderManager.IsFigureReady(figure))
            {
                Log("Downloading figure assets...");
                _renderManager.DownloadFigure(figure, null);

                if (_renderManager.IsFigureReady(figure))
                {
                    Log("[color=green]Figure assets downloaded.[/color]");
                }
                else
                {
                    Log("[color=yellow]Some figure assets may still be pending.[/color]");
                }
            }

            // Create avatar image — figure should now be ready
            _currentAvatar = _renderManager.CreateAvatarImage(figureString, scale);

            if (_currentAvatar == null)
            {
                Log("[color=red]CreateAvatarImage returned null.[/color]");
                return;
            }

            // Set direction
            _currentAvatar.SetDirection(AvatarGeometryType.FULL, direction);

            // Apply action
            _currentAvatar.InitActionAppends();

            if (actionIdx >= 0 && actionIdx < ACTION_PRESETS.Length)
            {
                (string Label, string ActionType, string? Param) preset = ACTION_PRESETS[actionIdx];

                if (preset.Param != null)
                {
                    _currentAvatar.AppendAction(preset.ActionType, preset.Param);
                }
                else
                {
                    _currentAvatar.AppendAction(preset.ActionType);
                }

                Log(
                    $"Action: {preset.Label} ({preset.ActionType}" +
                    (preset.Param != null ? $", {preset.Param}" : "") + ")"
                );
            }

            _currentAvatar.EndActionAppends();

            RenderCurrentFrame();

            Log($"Figure: {figureString} | Dir: {direction} | Scale: {scale} | Animating: {_currentAvatar.IsAnimating()}");
        }
        catch (Exception e)
        {
            Log($"[color=red]Render error: {e.Message}[/color]");
            Log($"[color=red]{e.StackTrace}[/color]");
        }

        UpdateStats();
    }

    private void RenderCurrentFrame()
    {
        if (_currentAvatar == null)
        {
            return;
        }

        try
        {
            Image? image = _currentAvatar.GetImage(AvatarGeometryType.FULL, true);

            if (image != null)
            {
                ImageTexture? texture = ImageTexture.CreateFromImage(image);
                _avatarDisplay!.Texture = texture;
                image.Dispose();
            }
            else
            {
                Log("[color=yellow]GetImage returned null (missing sprite assets?).[/color]");
                _avatarDisplay!.Texture = null;
            }
        }
        catch (Exception e)
        {
            Log($"[color=red]Frame render error: {e.Message}[/color]");
        }
    }

    private void DisposeCurrentAvatar()
    {
        if (_currentAvatar is Vortex.Core.Runtime.IDisposable disposable)
        {
            disposable.Dispose();
        }

        _currentAvatar = null;
    }

    private void UpdateStats()
    {
        if (_statsLabel == null)
        {
            return;
        }

        bool ready = _renderManager?.IsReady ?? false;
        bool figData = _renderManager?.GetFigureData() != null;
        bool animMgr = _renderManager?.GetAnimationManager() != null;

        _statsLabel.Text = $"Ready: {ready} | FigureData: {figData} | AnimMgr: {animMgr}";
    }

    private void Log(string message)
    {
        _logOutput?.AppendText(message + "\n");
        GD.Print($"[AvatarSystemTest] {message}");
    }
}
