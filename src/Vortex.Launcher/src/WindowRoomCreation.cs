using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;
using Vortex.Habbo.Room;
using Vortex.Habbo.Room.Object;
using Vortex.Habbo.Room.Object.Visualization.Room;
using Vortex.Room.Object;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Object.Visualization.Utils;
using Vortex.Room.Utils;

namespace Vortex.Test;

/// <summary>
/// Visual test harness for the Room rendering pipeline.
/// Parses a heightmap string, creates room planes, and renders isometric room views
/// using the flat-color fallback path (no rasterizer textures needed).
/// </summary>
public partial class WindowRoomCreation : Control
{
    private TextureRect? _roomDisplay;
    private TextEdit? _heightmapInput;
    private SpinBox? _wallHeightSpin;
    private OptionButton? _scaleOption;
    private SpinBox? _directionSpin;
    private CheckButton? _autoRotateToggle;
    private ColorPickerButton? _bgColorPicker;
    private Label? _statsLabel;
    private RichTextLabel? _logOutput;

    private RoomObject? _roomObject;
    private RoomVisualization? _visualization;
    private RoomGeometry? _geometry;

    private LineEdit? _bundleUrlInput;
    private Label? _bundleStatus;
    private RoomContentLoader? _contentLoader;
    private EventDispatcherWrapper? _contentEvents;
    private bool _contentLoaded;

    private bool _autoRotate;
    private double _autoRotateTimer;
    private int _autoRotateDir;

    private const string DEFAULT_HEIGHTMAP =
        "xxxx00\n" +
        "xxxx00\n" +
        "xxx000\n" +
        "xxx000\n" +
        "x00000\n" +
        "xx0000\n" +
        "000000\n" +
        "000000";

    private const int DISPLAY_SIZE = 400;

    public override void _Ready()
    {
        BuildUi();
        RenderRoom();
    }

    public override void _Process(double delta)
    {
        if (!_autoRotate)
        {
            return;
        }

        _autoRotateTimer += delta;

        if (!(_autoRotateTimer >= 1.0))
        {
            return;
        }

        _autoRotateTimer = 0;
        _autoRotateDir = (_autoRotateDir + 2) % 8;
        _directionSpin!.Value = _autoRotateDir;
        RenderRoom();
    }

    public override void _ExitTree()
    {
        DisposeRoom();
        _contentLoader?.Dispose();
        _contentLoader = null;
        _contentEvents = null;
    }

    private void BuildUi()
    {
        HSplitContainer hSplit = new()
        {
            LayoutMode = 1, AnchorsPreset = (int)LayoutPreset.FullRect, SplitOffset = 320,
        };
        AddChild(hSplit);

        // Left panel: controls
        ScrollContainer scrollContainer = new()
        {
            CustomMinimumSize = new Vector2(320, 0), SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        hSplit.AddChild(scrollContainer);

        VBoxContainer leftPanel = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        scrollContainer.AddChild(leftPanel);

        // --- Heightmap ---
        leftPanel.AddChild(new Label
        {
            Text = "Heightmap",
        });

        _heightmapInput = new TextEdit
        {
            Text = DEFAULT_HEIGHTMAP, CustomMinimumSize = new Vector2(300, 160), SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        _heightmapInput.AddThemeFontSizeOverride("font_size", 14);
        leftPanel.AddChild(_heightmapInput);

        // --- Wall Height ---
        leftPanel.AddChild(new Label
        {
            Text = "Wall Height",
        });

        _wallHeightSpin = new SpinBox
        {
            MinValue = 0,
            MaxValue = 20,
            Value = 3.6,
            Step = 0.1,
            CustomMinimumSize = new Vector2(120, 0),
        };
        leftPanel.AddChild(_wallHeightSpin);

        // --- Scale ---
        leftPanel.AddChild(new Label
        {
            Text = "Scale",
        });

        _scaleOption = new OptionButton();
        _scaleOption.AddItem("64 (Zoomed In)", 0);
        _scaleOption.AddItem("32 (Zoomed Out)", 1);
        _scaleOption.Selected = 0;
        _scaleOption.ItemSelected += _ => RenderRoom();
        leftPanel.AddChild(_scaleOption);

        // --- Direction ---
        leftPanel.AddChild(new Label
        {
            Text = "Direction (0-7)",
        });

        _directionSpin = new SpinBox
        {
            MinValue = 0,
            MaxValue = 7,
            Value = 0,
            Step = 1,
            CustomMinimumSize = new Vector2(70, 0),
        };
        _directionSpin.ValueChanged += _ => RenderRoom();
        leftPanel.AddChild(_directionSpin);

        // --- Render button ---
        Button btnRender = new()
        {
            Text = "Render", SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        btnRender.Pressed += RenderRoom;
        leftPanel.AddChild(btnRender);

        // --- Auto-rotate ---
        _autoRotateToggle = new CheckButton
        {
            Text = "Auto Rotate",
        };
        _autoRotateToggle.Toggled += toggled =>
        {
            _autoRotate = toggled;
            _autoRotateTimer = 0;
        };
        leftPanel.AddChild(_autoRotateToggle);

        leftPanel.AddChild(new HSeparator());

        // --- Background Color ---
        leftPanel.AddChild(new Label
        {
            Text = "Background Color",
        });

        _bgColorPicker = new ColorPickerButton
        {
            Color = Colors.White, CustomMinimumSize = new Vector2(80, 30),
        };
        _bgColorPicker.ColorChanged += _ => RenderRoom();
        leftPanel.AddChild(_bgColorPicker);

        leftPanel.AddChild(new HSeparator());

        // --- Bundle URL ---
        leftPanel.AddChild(new Label
        {
            Text = "Bundle URL",
        });

        _bundleUrlInput = new LineEdit
        {
            PlaceholderText = "https://host/path/", SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        leftPanel.AddChild(_bundleUrlInput);

        Button btnLoadBundles = new()
        {
            Text = "Load Bundles", SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        btnLoadBundles.Pressed += LoadBundles;
        leftPanel.AddChild(btnLoadBundles);

        _bundleStatus = new Label
        {
            Text = "Not loaded",
        };
        _bundleStatus.AddThemeFontSizeOverride("font_size", 11);
        leftPanel.AddChild(_bundleStatus);

        leftPanel.AddChild(new HSeparator());

        // --- Stats ---
        _statsLabel = new Label
        {
            Text = "Not rendered",
        };
        _statsLabel.AddThemeFontSizeOverride("font_size", 11);
        leftPanel.AddChild(_statsLabel);

        // Right panel: room display + log
        VBoxContainer rightVBox = new();
        hSplit.AddChild(rightVBox);

        // Room image display with dark background
        PanelContainer displayPanel = new()
        {
            CustomMinimumSize = new Vector2(DISPLAY_SIZE * 2, DISPLAY_SIZE * 2), SizeFlagsVertical = SizeFlags.ExpandFill,
        };

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

        _roomDisplay = new TextureRect
        {
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize = new Vector2(DISPLAY_SIZE * 2, DISPLAY_SIZE * 2),
        };
        displayPanel.AddChild(_roomDisplay);

        // Log output
        rightVBox.AddChild(new Label
        {
            Text = "Log",
        });

        _logOutput = new RichTextLabel
        {
            BbcodeEnabled = true, ScrollFollowing = true, CustomMinimumSize = new Vector2(0, 200), SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        rightVBox.AddChild(_logOutput);
    }

    private void RenderRoom()
    {
        try
        {
            Stopwatch sw = Stopwatch.StartNew();

            string heightmap = _heightmapInput?.Text ?? DEFAULT_HEIGHTMAP;
            double wallHeight = _wallHeightSpin?.Value ?? 3.6;
            double scale = _scaleOption?.Selected == 1 ? 32 : 64;
            int dirIndex = (int)(_directionSpin?.Value ?? 0);

            // Get background color as uint ARGB
            Color bgGodot = _bgColorPicker?.Color ?? Colors.White;
            uint bgColor = ((uint)(bgGodot.R8) << 16) | ((uint)(bgGodot.G8) << 8) | (uint)(bgGodot.B8);

            // Dispose previous
            DisposeRoom();

            // 1. Build the plane XML from the heightmap
            string planeXml = BuildPlaneXml(heightmap, wallHeight);

            if (string.IsNullOrEmpty(planeXml))
            {
                Log("[color=red]Failed to parse heightmap.[/color]");

                return;
            }

            // 2. Create RoomObject with model data
            _roomObject = new RoomObject(1, 1, "room");
            _roomObject.ModelController.SetString("room_plane_xml", planeXml);
            _roomObject.ModelController.SetNumber("room_random_seed", 12345);
            _roomObject.ModelController.SetNumber("room_background_color", bgColor);
            _roomObject.ModelController.SetNumber("room_floor_visibility", 1);
            _roomObject.ModelController.SetNumber("room_wall_visibility", 1);
            _roomObject.ModelController.SetNumber("room_landscape_visibility", 1);

            // 3. Create visualization data — use real textures if bundles loaded, else flat-color fallback
            RoomVisualizationData vizData;

            if (_contentLoaded && _contentLoader != null)
            {
                XElement? vizXml = _contentLoader.GetVisualizationXml("room");
                IGraphicAssetCollection? assetCollection = _contentLoader.GetGraphicAssetCollection("room");

                if (vizXml != null && assetCollection != null)
                {
                    vizData = new RoomVisualizationData();
                    vizData.Initialize(vizXml);
                    vizData.InitializeAssetCollection(assetCollection);
                    Log("[color=green]Using real room textures from bundle[/color]");
                }
                else
                {
                    vizData = CreateFlatColorVisualizationData();
                    Log(
                        $"[color=yellow]Bundle loaded but vizXml={vizXml != null}, assets={assetCollection != null}, using flat color[/color]");
                }
            }
            else
            {
                vizData = CreateFlatColorVisualizationData();
            }

            // 4. Create visualization and initialize
            _visualization = new RoomVisualization();
            _visualization.Initialize(vizData);
            _roomObject.SetVisualization(_visualization);

            // 5. Create room geometry
            double rotationAngle = dirIndex * 45.0;
            Vector3d direction = new(-135 + rotationAngle, 30);
            Vector3d location = new(11, 11, 5);
            Vector3d depthDir = new(-135 + rotationAngle, 0.5);
            _geometry = new RoomGeometry(scale, direction, location, depthDir);

            // 6. Run the visualization update (creates planes, renders textures)
            _visualization.Update(_geometry, 0, true, false);

            sw.Stop();

            Image? image = _visualization.GetImage();

            if (image != null)
            {
                ImageTexture texture = ImageTexture.CreateFromImage(image);
                _roomDisplay!.Texture = texture;

                Log($"[color=green]Rendered room:[/color] {image.GetWidth()}x{image.GetHeight()}px");
            }
            else
            {
                _roomDisplay!.Texture = null;
                Log("[color=yellow]GetImage returned null (no visible sprites?).[/color]");
            }
        }
        catch (Exception e)
        {
            Log($"[color=red]Render error: {e.Message}[/color]");
            Log($"[color=red]{e.StackTrace}[/color]");
        }
    }

    /// <summary>
    /// Converts a heightmap string into the XML format expected by RoomPlaneParser.
    /// Chars: 0-9 = floor height, a-z = heights 10-35, x = void tile (-1).
    /// </summary>
    private static string BuildPlaneXml(string heightmap, double wallHeight)
    {
        string[] rows = heightmap.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (rows.Length == 0)
        {
            return "";
        }

        int height = rows.Length;
        int width = rows.Select(row => row.Length).Prepend(0).Max();

        if (width == 0)
        {
            return "";
        }

        StringBuilder sb = new();
        sb.Append("<roomData>");
        sb.Append(CultureInfo.InvariantCulture,
            $"<tileMap width=\"{width}\" height=\"{height}\" wallHeight=\"{wallHeight}\" fixedWallsHeight=\"-1\">");

        for (int y = 0; y < height; y++)
        {
            sb.Append("<tileRow>");
            string row = y < rows.Length ? rows[y] : "";

            for (int x = 0; x < width; x++)
            {
                double tileHeight;

                if (x < row.Length)
                {
                    char c = row[x];

                    tileHeight = c switch
                    {
                        'x' or 'X' => -110,
                        >= '0' and <= '9' => c - '0',
                        >= 'a' and <= 'z' => 10 + (c - 'a'),
                        _ => -110,
                    };
                }
                else
                {
                    tileHeight = -110;
                }

                sb.Append(CultureInfo.InvariantCulture, $"<tile height=\"{tileHeight}\"/>");
            }

            sb.Append("</tileRow>");
        }

        sb.Append("</tileMap>");
        sb.Append("</roomData>");

        return sb.ToString();
    }

    /// <summary>
    /// Creates a RoomVisualizationData with all rasterizers disposed/nulled.
    /// This activates the flat-color fallback in RoomPlane.GetTexture(), which renders
    /// solid-color planes without needing texture bundles.
    /// </summary>
    private static RoomVisualizationData CreateFlatColorVisualizationData()
    {
        RoomVisualizationData vizData = new();

        // Dispose nulls all rasterizers and MaskManager, activating flat-color fallback.
        // RoomPlane.GetTexture() creates solid-color bitmaps when rasterizer is null.
        // RoomPlane.UpdateMask() safely returns when MaskManager is null.
        vizData.Dispose();

        return vizData;
    }

    private void DisposeRoom()
    {
        if (_roomObject != null)
        {
            _roomObject.Dispose();
            _roomObject = null;
        }

        _visualization = null; // Disposed by RoomObject.SetVisualization(null)

        if (_geometry != null)
        {
            _geometry.Dispose();
            _geometry = null;
        }
    }

    private void LoadBundles()
    {
        string baseUrl = _bundleUrlInput?.Text?.Trim() ?? "";

        if (string.IsNullOrEmpty(baseUrl))
        {
            return;
        }

        if (!baseUrl.EndsWith('/'))
        {
            baseUrl += "/";
        }

        // Dispose previous
        _contentLoader?.Dispose();
        _contentEvents = null;
        _contentLoaded = false;

        _contentEvents = new EventDispatcherWrapper();
        TestConfiguration config = new(baseUrl);
        _contentLoader = new RoomContentLoader("");
        _contentLoader.VisualizationFactory = new RoomObjectVisualizationFactory(new FakeContext(null), 0);
        _contentLoader.Initialize(_contentEvents, config);

        bool roomOk = _contentLoader.LoadObjectContent("room", _contentEvents);
        bool cursorOk = _contentLoader.LoadObjectContent("tile_cursor", _contentEvents);

        _contentLoaded = roomOk;
        _bundleStatus!.Text = $"Room: {(roomOk ? "OK" : "FAIL")} | Cursor: {(cursorOk ? "OK" : "FAIL")}";
        Log($"[Bundles] room={roomOk}, tile_cursor={cursorOk}, baseUrl={baseUrl}");
    }

    private sealed class TestConfiguration(string baseUrl) : ICoreConfiguration
    {
        public bool disposed { get; private set; }

        public string GetProperty(string param1, IDictionary<string, string>? param2 = null)
        {
            return param1 switch
            {
                "flash.dynamic.download.url" => baseUrl,
                _ => "",
            };
        }

        public bool PropertyExists(string param1)
        {
            return param1 == "flash.dynamic.download.url";
        }

        public void SetProperty(string param1, string param2, bool param3 = false, bool param4 = false) { }

        public bool GetBoolean(string param1)
        {
            return false;
        }
 public int GetInteger(string param1, int param2)
        {
            return param2;
        }

        public string? Interpolate(string param1)
        {
            return param1;
        }

        public string UpdateUrlProtocol(string param1)
        {
            return param1;
        }

        public IUnknown? QueueInterface(Core.Runtime.IID param1, Action<Core.Runtime.IID, IUnknown?>? param2 = null)
        {
            return null;
        }

        public uint Release(Core.Runtime.IID param1)
        {
            return 0;
        }

        public void Dispose()
        {
            disposed = true;
        }
    }

    private void Log(string message)
    {
        _logOutput?.AppendText(message + "\n");
        GD.Print($"[RoomTest] {message}");
    }
}
