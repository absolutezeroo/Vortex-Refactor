// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Habbo.Configuration;
using Vortex.Habbo.Configuration.Enum;
using Vortex.Habbo.Localization;
using Vortex.Splash;

using Timer = Godot.Timer;

namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as
public partial class HabboLoadingScreen : Control, IHabboLoadingScreen
{
    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::LOADING_BAR_WIDTH
    public const int LOADING_BAR_WIDTH = 400;

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::LOADING_BAR_HEIGHT
    public const int LOADING_BAR_HEIGHT = 25;

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::LOADING_BAR_BORDER_WIDTH
    public const int LOADING_BAR_BORDER_WIDTH = 2;

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::LOADING_BAR_BORDER_SPACING
    public const int LOADING_BAR_BORDER_SPACING = 2;

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::CONTAINER
    public const string CONTAINER = "container";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::FILE_LOADING_BAR
    public const string FILE_LOADING_BAR = "fileLoadingBar";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::FILE_BAR_SPRITE
    public const string FILE_BAR_SPRITE = "fileBarSprite";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::PHOTO_SPLASH_SCREEN
    public const string PHOTO_SPLASH_SCREEN = "photoSplashScreen";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::BACKGROUND
    public const string BACKGROUND = "background";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::_SafeStr_4593
    public const string HABBO_LOGO = "habboLogo";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::_SafeStr_4390
    public const string TEXT_FIELD = "textField";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::VERSION_TEXT_FIELD
    public const string VERSION_TEXT_FIELD = "versionTextField";

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::_SafeStr_4594
    public const string LOADING_NUMBER_TEXT_FIELD = "loadingNumberTextField";
    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::_SafeStr_4592
    private static readonly int TEXT_FONT_SIZE = 28;
    private readonly int _initialHeight;
    private readonly int _initialWidth;
    private readonly Dictionary<string, object?> _properties;
    private int _barProgression;
    private bool _timerTimeoutConnected;
    private bool _viewportResizeConnected;
    private FakeContext? _fakeContext;
    private HabboConfigurationManager? _configuration;
    private HabboLocalizationManager? _localization;
    private PhotoSplashScreen? _splashScreen;
    private int _revolvingTextIndex;

    private Timer? _progressTimer;
    private string? _revolvingText;
    private bool _textSwapReady;

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::HabboLoadingScreen
    public HabboLoadingScreen(int param1, int param2, Dictionary<string, object?>? param3)
    {
        Name = nameof(HabboLoadingScreen);
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _initialWidth = param1;
        _initialHeight = param2;
        _properties = param3 != null ? Clone(param3) : new Dictionary<string, object?>(StringComparer.Ordinal);

        CreateFakeContext(_properties);

        ColorRect background = new()
        {
            Name = BACKGROUND,
            Color = new Color(14 / 255f, 21 / 255f, 28 / 255f),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(background);

        _splashScreen = new PhotoSplashScreen(this)
        {
            Name = PHOTO_SPLASH_SCREEN,
        };
        AddChild(_splashScreen);

        Control loadingBarContainer = new()
        {
            Name = FILE_LOADING_BAR,
            Size = new Vector2(LOADING_BAR_WIDTH, LOADING_BAR_HEIGHT),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(loadingBarContainer);

        ColorRect loadingBarFrame = new()
        {
            Name = "fileBarFrame",
            Color = Colors.White,
            Size = new Vector2(LOADING_BAR_WIDTH, LOADING_BAR_HEIGHT),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        loadingBarContainer.AddChild(loadingBarFrame);

        ColorRect loadingBarBackground = new()
        {
            Name = "fileBarBackground",
            Color = new Color(0x26 / 255f, 0x1F / 255f, 0x2F / 255f),
            Position = new Vector2(LOADING_BAR_BORDER_WIDTH, LOADING_BAR_BORDER_WIDTH),
            Size = new Vector2(
                LOADING_BAR_WIDTH - (LOADING_BAR_BORDER_WIDTH * 2),
                LOADING_BAR_HEIGHT - (LOADING_BAR_BORDER_WIDTH * 2)
            ),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        loadingBarContainer.AddChild(loadingBarBackground);

        Control fileBarSprite = new()
        {
            Name = FILE_BAR_SPRITE,
            Position = new Vector2(LOADING_BAR_BORDER_WIDTH + LOADING_BAR_BORDER_SPACING,
                LOADING_BAR_BORDER_WIDTH + LOADING_BAR_BORDER_SPACING),
            Size = Vector2.Zero,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        loadingBarContainer.AddChild(fileBarSprite);

        ColorRect topFill = new()
        {
            Name = "top",
            Color = new Color(186 / 255f, 204 / 255f, 83 / 255f),
            Position = Vector2.Zero,
            Size = Vector2.Zero,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        fileBarSprite.AddChild(topFill);

        ColorRect bottomFill = new()
        {
            Name = "bottom",
            Color = new Color(140 / 255f, 158 / 255f, 45 / 255f),
            Position = Vector2.Zero,
            Size = Vector2.Zero,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        fileBarSprite.AddChild(bottomFill);

        string? rotating = GetLocalization("client.starting.revolving");
        string fallback = GetLocalization("client.starting") ?? "Starting client";
        string text = fallback;

        if (rotating != null)
        {
            string[] options = rotating.Split('/');
            _revolvingTextIndex = (int)RandomNumber(0, options.Length - 1);
            _revolvingText = rotating;
            text = options[_revolvingTextIndex];
        }

        Label textField = new()
        {
            Name = TEXT_FIELD,
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center,
            ThemeTypeVariation = "HeaderSmall",
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(textField);

        Label loadingNumber = new()
        {
            Name = LOADING_NUMBER_TEXT_FIELD,
            Text = "0%",
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(loadingNumber);

        Label versionText = new()
        {
            Name = VERSION_TEXT_FIELD,
            Text = "Habbo Air for Flash",
            HorizontalAlignment = HorizontalAlignment.Right,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(versionText);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::get disposed
    public bool disposed { get; private set; }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::dispose
    public new void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        Viewport? viewport = GetViewport();
        if (viewport != null && _viewportResizeConnected)
        {
            viewport.SizeChanged -= OnResize;
            _viewportResizeConnected = false;
        }

        RemoveElement(PHOTO_SPLASH_SCREEN);
        RemoveElement(BACKGROUND);
        RemoveElement(TEXT_FIELD);
        RemoveElement(HABBO_LOGO);
        RemoveElement(FILE_LOADING_BAR);
        RemoveElement(CONTAINER);

        if (_progressTimer != null)
        {
            _progressTimer.Stop();

            if (_timerTimeoutConnected)
            {
                _progressTimer.Timeout -= OnBarProgressEvent;
                _timerTimeoutConnected = false;
            }

            _progressTimer.GetParent()?.RemoveChild(_progressTimer);

            _progressTimer.QueueFree();
            _progressTimer = null;
        }

        _configuration?.Dispose();
        _configuration = null;

        _localization?.Dispose();
        _localization = null;

        _fakeContext?.Dispose();
        _fakeContext = null;

        GetParent()?.RemoveChild(this);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::updateLoadingBar
    public void UpdateLoadingBar(double param1)
    {
        Label? loadingNumber = GetNodeOrNull<Label>(LOADING_NUMBER_TEXT_FIELD);

        if (loadingNumber != null)
        {
            loadingNumber.Text = $"{Math.Round(param1 * 100, MidpointRounding.AwayFromZero)}%";
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::createFakeContext
    private void CreateFakeContext(Dictionary<string, object?> param1)
    {
        try
        {
            _fakeContext = new FakeContext(param1);

            AssetLibrary tempAssetLibrary = new("_assetsTemp@");
            _fakeContext.AssetLibraryCollection?.AddAssetLibrary(tempAssetLibrary);

            _configuration = CreateConfiguration(_fakeContext);
            _localization = CreateLocalization(_fakeContext);

            string? environmentId = _configuration?.GetProperty("environment.id");
            if (string.IsNullOrWhiteSpace(environmentId))
            {
                environmentId = "en";
            }

            _localization?.LoadDefaultEmbedLocalizations(environmentId);
        }
        catch (Exception exception)
        {
            Logger.Warn("[HabboLoadingScreen] Failed to create fake context: " + exception.Message);
            _fakeContext?.Dispose();
            _fakeContext = null;
            _configuration = null;
            _localization = null;
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::createConfiguration
    private static HabboConfigurationManager? CreateConfiguration(FakeContext context)
    {
        AssetLibrary? assetLibrary = BuildAssetLibraryFromManifest("_assetsConfiguration@", typeof(HabboConfigurationCom));
        if (assetLibrary != null)
        {
            context.AssetLibraryCollection?.AddAssetLibrary(assetLibrary);
        }

        return new HabboConfigurationManager(
            context,
            HabboConfigurationFlags.SKIP_EXTERNAL_VARIABLES | HabboConfigurationFlags.SKIP_LOCALIZATIONS,
            assetLibrary
        );
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::createLocalization
    private static HabboLocalizationManager? CreateLocalization(FakeContext context)
    {
        AssetLibrary? assetLibrary = BuildAssetLibraryFromManifest("_assetsLocalization@", typeof(HabboLocalizationCom));
        if (assetLibrary != null)
        {
            context.AssetLibraryCollection?.AddAssetLibrary(assetLibrary);
        }

        return new HabboLocalizationManager(
            context,
            HabboLocalizationManager.SKIP_EXTERNAL_LOCALIZATIONS,
            assetLibrary
        );
    }

    /// Extracts <assets> from a manifest XML for the given Com type, builds an AssetLibrary, and loads it.
    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::createConfiguration (shared pattern)
    private static AssetLibrary? BuildAssetLibraryFromManifest(string libraryName, Type comType)
    {
        if (!TryGetManifestXml(comType, out XElement manifestXml))
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

        AssetLibrary assetLibrary = new(libraryName, libraryManifest);

        assetLibrary.LoadFromResource(libraryManifest, comType);

        return assetLibrary;
    }

    /// Resolves a manifest XML from a Com type's static property or from data/manifests/ files.
    /// @see ComponentContext.TryGetManifestXml — same pattern, duplicated here to avoid coupling
    /// HabboLoadingScreen to ComponentContext internals.
    private static bool TryGetManifestXml(Type param1, out XElement param2)
    {
        param2 = default!;

        // Try static manifest property first
        PropertyInfo? manifestProperty =
            param1.GetProperty("manifest", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        if (manifestProperty != null)
        {
            try
            {
                object? value = manifestProperty.GetValue(null);
                switch (value)
                {
                    case XElement manifestElement:
                        param2 = manifestElement;
                        return true;
                    case string manifestXmlText when !string.IsNullOrWhiteSpace(manifestXmlText):
                        param2 = XElement.Parse(manifestXmlText);
                        return true;
                    case byte[] { Length: > 0 } manifestBytes:
                        param2 = XElement.Parse(Encoding.UTF8.GetString(manifestBytes));
                        return true;
                }
            }
            catch
            {
                // Fall through to file-based lookup.
            }
        }

        // Try file-based manifest lookup
        string manifestDirectory = Path.Combine("data", "manifests");
        if (!Directory.Exists(manifestDirectory))
        {
            return false;
        }

        string canonicalName = "Habbo" + param1.Name + "_manifest_xml";
        string[] candidates =
        [
            Path.Combine(manifestDirectory, canonicalName + ".xml"),
            Path.Combine(manifestDirectory, canonicalName),
        ];

        string? manifestPath = candidates.FirstOrDefault(File.Exists)
                               ?? Directory.EnumerateFiles(manifestDirectory, "*" + param1.Name + "_manifest.xml").FirstOrDefault();

        if (string.IsNullOrWhiteSpace(manifestPath) || !File.Exists(manifestPath))
        {
            return false;
        }

        try
        {
            param2 = XElement.Parse(File.ReadAllText(manifestPath, Encoding.UTF8));
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::_Ready
    public override void _Ready()
    {
        OnAddedToStage();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::_ExitTree
    public override void _ExitTree()
    {
        OnRemovedFromStage();

        base._ExitTree();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::onRemovedFromStage
    private static void OnRemovedFromStage()
    {
        Logger.Debug("Habbo Loading Screen was removed from stage.");
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::onAddedToStage
    private void OnAddedToStage()
    {
        Viewport? viewport = GetViewport();
        if (viewport != null && !_viewportResizeConnected)
        {
            viewport.SizeChanged += OnResize;
            _viewportResizeConnected = true;
        }

        PositionLoadingScreenDisplayElements();

        _progressTimer ??= new Timer
        {
            Name = "loadingBarTimer",
            WaitTime = 0.75,
            OneShot = false,
            Autostart = false,
            ProcessCallback = Timer.TimerProcessCallback.Idle,
        };

        if (_progressTimer.GetParent() == null)
        {
            AddChild(_progressTimer);
        }

        if (!_timerTimeoutConnected)
        {
            _progressTimer.Timeout += OnBarProgressEvent;
            _timerTimeoutConnected = true;
        }

        _progressTimer.Start();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::onResize
    private void OnResize()
    {
        PositionLoadingScreenDisplayElements();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::onBarProgressEvent
    private void OnBarProgressEvent()
    {
        if (_barProgression == 100)
        {
            if (_textSwapReady)
            {
                string[] rotating = _revolvingText!.Split('/');
                Label? oldLabel = GetNodeOrNull<Label>(TEXT_FIELD);

                if (oldLabel != null)
                {
                    int previousY = (int)oldLabel.Position.Y;

                    RemoveElement(TEXT_FIELD);

                    Label newLabel = new()
                    {
                        Name = TEXT_FIELD,
                        Text = rotating[_revolvingTextIndex],
                        HorizontalAlignment = HorizontalAlignment.Center,
                        ThemeTypeVariation = "HeaderSmall",
                        MouseFilter = MouseFilterEnum.Ignore,
                    };

                    AddChild(newLabel);
                    // Godot/C# adaptation: position after AddChild so the label has a valid size.
                    newLabel.Position = new Vector2((Size.X - newLabel.Size.X) / 2f, previousY);
                }

                _textSwapReady = false;
            }

            _barProgression = 0;
        }
        else
        {
            _barProgression += (int)Math.Min(RandomNumber(35, RandomNumber(45, 55)), 100 - _barProgression);
        }

        if (_barProgression == 100 && _revolvingText != null)
        {
            _textSwapReady = true;
            _revolvingTextIndex = (_revolvingTextIndex + 1) % (_revolvingText.Split('/').Length - 1);
        }

        UpdateLoadingBarProgression(_barProgression / 100.0);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::randomNumber
    private static double RandomNumber(double param1, double param2)
    {
        return Math.Floor(GD.RandRange(param1, param2 + 1));
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::removeElement
    private void RemoveElement(string param1)
    {
        Node? node = GetNodeOrNull(param1);

        if (node == null)
        {
            return;
        }

        RemoveChild(node);

        node.QueueFree();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::positionLoadingScreenDisplayElements
    public void PositionLoadingScreenDisplayElements()
    {
        int stageWidth;
        int stageHeight;
        Rect2 viewportRect = GetViewportRect();

        if (viewportRect.Size is { X: > 0, Y: > 0 })
        {
            stageWidth = (int)viewportRect.Size.X;
            stageHeight = (int)viewportRect.Size.Y;
        }
        else
        {
            stageWidth = _initialWidth;
            stageHeight = _initialHeight;
        }

        ColorRect? background = GetNodeOrNull<ColorRect>(BACKGROUND);

        if (background != null)
        {
            background.Position = Vector2.Zero;
            background.Size = new Vector2(stageWidth, stageHeight);
        }

        int contentBottom = 0;
        int spacing = 10;

        Control? photo = GetNodeOrNull<Control>(PHOTO_SPLASH_SCREEN);

        if (photo != null)
        {
            photo.Position = new Vector2((stageWidth - photo.Size.X) / 2f, photo.Position.Y);
            contentBottom = (int)(photo.Position.Y + photo.Size.Y);
        }

        Label? text = GetNodeOrNull<Label>(TEXT_FIELD);

        if (text != null)
        {
            text.Position = new Vector2((stageWidth - text.Size.X) / 2f, text.Position.Y);
        }

        Label? version = GetNodeOrNull<Label>(VERSION_TEXT_FIELD);

        if (version != null)
        {
            version.Position = new Vector2(stageWidth - version.Size.X, 0);
        }

        Control? bar = GetNodeOrNull<Control>(FILE_LOADING_BAR);

        if (bar != null)
        {
            bar.Position = new Vector2((stageWidth - bar.Size.X) / 2f, contentBottom);
            contentBottom = (int)(bar.Position.Y + bar.Size.Y);
        }

        Label? loadingNumber = GetNodeOrNull<Label>(LOADING_NUMBER_TEXT_FIELD);

        if (loadingNumber != null)
        {
            loadingNumber.Position = new Vector2((stageWidth - loadingNumber.Size.X) / 2f, loadingNumber.Position.Y);
        }

        int centerTop = (stageHeight - contentBottom) / 2;
        centerTop -= spacing * 2;

        if (photo != null)
        {
            photo.Position = new Vector2(photo.Position.X, centerTop);
            contentBottom = (int)(photo.Position.Y + photo.Size.Y);
        }

        if (text != null)
        {
            text.Position = new Vector2(text.Position.X, contentBottom + 50);
            contentBottom = (int)(text.Position.Y + text.Size.Y) + spacing;
        }

        if (bar != null)
        {
            bar.Position = new Vector2(bar.Position.X, contentBottom);
            contentBottom = (int)(bar.Position.Y + bar.Size.Y + (spacing / 2f));
        }

        if (loadingNumber != null)
        {
            loadingNumber.Position = new Vector2(loadingNumber.Position.X, contentBottom);
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::updateLoadingBarProgression
    public void UpdateLoadingBarProgression(double param1)
    {
        Control? loadingBar = GetNodeOrNull<Control>(FILE_LOADING_BAR);

        if (loadingBar == null)
        {
            return;
        }

        Control? fileBarSprite = loadingBar.GetNodeOrNull<Control>(FILE_BAR_SPRITE);

        if (fileBarSprite == null)
        {
            return;
        }

        fileBarSprite.Position = new Vector2(
            LOADING_BAR_BORDER_WIDTH + LOADING_BAR_BORDER_SPACING,
            LOADING_BAR_BORDER_WIDTH + LOADING_BAR_BORDER_SPACING
        );

        int innerHeight = LOADING_BAR_HEIGHT - (LOADING_BAR_BORDER_WIDTH * 2) - (LOADING_BAR_BORDER_SPACING * 2);
        int innerWidth = LOADING_BAR_WIDTH - (LOADING_BAR_BORDER_WIDTH * 2) - (LOADING_BAR_BORDER_SPACING * 2);
        int fillWidth = (int)(innerWidth * param1);

        fileBarSprite.Size = new Vector2(fillWidth, innerHeight);

        ColorRect? top = fileBarSprite.GetNodeOrNull<ColorRect>("top");
        ColorRect? bottom = fileBarSprite.GetNodeOrNull<ColorRect>("bottom");

        if (top != null)
        {
            top.Position = Vector2.Zero;
            top.Size = new Vector2(fillWidth, innerHeight / 2f);
        }

        if (bottom == null)
        {
            return;
        }

        bottom.Position = new Vector2(0, innerHeight / 2f);
        bottom.Size = new Vector2(fillWidth, (innerHeight / 2f) + 1);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::getLocalization
    private string? GetLocalization(string param1)
    {
        if (_localization != null)
        {
            string localized = _localization.GetLocalization(param1);
            if (!string.IsNullOrEmpty(localized) && !string.Equals(localized, param1, StringComparison.Ordinal))
            {
                return localized;
            }
        }

        if (_properties.TryGetValue(param1, out object? value) && value != null)
        {
            return Convert.ToString(value);
        }

        return param1 switch
        {
            "client.starting.revolving" => "Loading rooms/Loading inventory/Loading windows",
            "client.starting" => "Starting Habbo",
            _ => null,
        };
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboLoadingScreen.as::clone
    private static Dictionary<string, object?> Clone(Dictionary<string, object?> param1)
    {
        Dictionary<string, object?> result = new(StringComparer.Ordinal);

        foreach (KeyValuePair<string, object?> pair in param1)
        {
            if (pair.Value is Dictionary<string, object?> nested)
            {
                result[pair.Key] = Clone(nested);
            }
            else
            {
                result[pair.Key] = pair.Value;
            }
        }

        return result;
    }
}
