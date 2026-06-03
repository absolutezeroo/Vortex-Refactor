// @see WIN63-202111081545-75921380-Source-main/src/login/EnvironmentView.as

using System;

using Godot;

using Vortex.Core.Utils;
using Vortex.OnBoardingHcUi;

using HcButton = Vortex.OnBoardingHcUi.Button;

namespace Vortex.Login;

/// @see WIN63-202111081545-75921380-Source-main/src/login/EnvironmentView.as
public partial class EnvironmentView(LoginFlow context) : Control
{
    private static readonly string[] FlagImageIds = ["en", "pt", "de", "es", "fi", "fr", "it", "nl", "tr", "dev"];
    private static readonly string[] DefaultEnvironmentIds = ["en", "pt", "de", "es", "fi", "fr", "it", "nl", "tr", "dev"];
    private const int ItemsPerRow = 9;
    private const int ItemSize = 80;
    private const int Spacing = 5;
    private const float ThumbScale = 0.5f; // AS3: THUMB_SCALE = 0.5

    private readonly List<TextureButton> _environmentButtons = [];
    private readonly List<string> _environmentIds = [];

    private Label? _title;
    private Control? _balloon;
    private Control? _selectedMarker;
    private Label? _environmentName;
    private ColouredButton? _loginButton;
    private ColouredButton? _loginWithCodeButton;

    private int _selectedIndex;
    private bool _initialized;

    public override void _EnterTree()
    {
        base._EnterTree();

        ScheduleAlign();
    }

    public bool Disposed => !IsInsideTree() && _initialized;

    public string EnvironmentId =>
        _selectedIndex >= 0 && _selectedIndex < _environmentIds.Count ? _environmentIds[_selectedIndex] : string.Empty;

    public bool EnvironmentAvailable
    {
        get
        {
            string current = context.GetProperty("environment.id");

            return _environmentIds.Exists(env => string.Equals(env, current, StringComparison.Ordinal));
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/login/EnvironmentView.as::init
    public void Init()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        BuildEnvironmentList();
        UpdateEnvironment();
        InitView();
    }

    public void DisposeView()
    {
        _loginButton?.Dispose();

        _loginWithCodeButton?.Dispose();
    }

    public void UpdateEnvironment()
    {
        string environment = context.GetProperty("environment.id");
        int index = _environmentIds.IndexOf(environment);
        _selectedIndex = index < 0 ? 0 : index;

        ChooseEnvironment();
    }

    private void BuildEnvironmentList()
    {
        _environmentIds.Clear();

        string configured = context.GetProperty("live.environment.list");

        if (!string.IsNullOrWhiteSpace(configured))
        {
            foreach (string entry in configured.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                _environmentIds.Add(entry);
            }
        }

        if (_environmentIds.Count == 0)
        {
            _environmentIds.AddRange(DefaultEnvironmentIds);
        }
    }

    private void InitView()
    {
        AddTitleField();

        _balloon = LoaderUI.CreateBalloon(640, 100, 0, false, 995918, "none");
        _balloon.Visible = false;
        AddChild(_balloon);

        _selectedMarker = new Control
        {
            Name = "SelectedMarker",
            Visible = false,
            MouseFilter = MouseFilterEnum.Ignore,
        };

        if (ResourceLoader.Exists("res://assets/images/flag_icon_selected.png"))
        {
            Texture2D? markerTexture = GD.Load<Texture2D>("res://assets/images/flag_icon_selected.png");

            if (markerTexture != null)
            {
                Vector2 markerSize = new(
                    markerTexture.GetWidth() * ThumbScale,
                    markerTexture.GetHeight() * ThumbScale
                );

                _selectedMarker.Size = markerSize;
                _selectedMarker.CustomMinimumSize = markerSize;

                TextureRect marker = new()
                {
                    Texture = markerTexture,
                    MouseFilter = MouseFilterEnum.Ignore,
                    ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                    StretchMode = TextureRect.StretchModeEnum.Scale,
                    Size = markerSize,
                    CustomMinimumSize = markerSize,
                };

                _selectedMarker.AddChild(marker);
            }
        }

        AddChild(_selectedMarker);

        int rowOriginY = 100;

        for (int i = 0;
             i < _environmentIds.Count;
             i++)
        {
            string flagImageId = i < FlagImageIds.Length ? FlagImageIds[i] : _environmentIds[i];

            TextureButton flagButton = new()
            {
                Name = $"Environment_{i}",
                IgnoreTextureSize = true,
                StretchMode = TextureButton.StretchModeEnum.Scale,
                MouseFilter = MouseFilterEnum.Stop,
                FocusMode = FocusModeEnum.None,
                Size = new Vector2(ItemSize, ItemSize),
                CustomMinimumSize = new Vector2(ItemSize, ItemSize),
            };

            string flagPath = $"res://assets/images/flag_icon_{flagImageId}.png";

            if (ResourceLoader.Exists(flagPath))
            {
                Texture2D? texture = GD.Load<Texture2D>(flagPath);
                flagButton.TextureNormal = texture;
                flagButton.TextureHover = texture;
                flagButton.TexturePressed = texture;
                flagButton.TextureDisabled = texture;

                if (texture != null)
                {
                    Vector2 scaledSize = new(
                        texture.GetWidth() * ThumbScale,
                        texture.GetHeight() * ThumbScale
                    );
                    flagButton.Size = scaledSize;
                    flagButton.CustomMinimumSize = scaledSize;
                }
            }

            int row = i / ItemsPerRow;
            int column = i % ItemsPerRow;

            flagButton.Position = new Vector2(column * (ItemSize + Spacing), rowOriginY + (row * (ItemSize + Spacing)));
            int localIndex = i;
            flagButton.Pressed += () => OnEnvironmentClick(localIndex);

            _environmentButtons.Add(flagButton);

            AddChild(flagButton);
        }

        _environmentName = LoaderUI.CreateTextField("Title", 20, 0xFFFFFF, false, true) as Label;

        if (_environmentName != null)
        {
            _environmentName.Size = new Vector2(260, _environmentName.Size.Y);
            _environmentName.Position = new Vector2(0, 300);

            AddChild(_environmentName);
        }

        _loginButton = new ColouredButton("gfreen", "${connection.login.login}", new Rect2(0, 300, 0, 40), true, OnButtonSelect);
        AddChild(_loginButton);

        _loginWithCodeButton =
            new ColouredButton("gfreen", "${connection.login.useTicket}", new Rect2(0, 300, 0, 40), true, OnButtonSelectToken);
        AddChild(_loginWithCodeButton);

        ChooseEnvironment();

        Rect2 bounds = DisplayUtils.ComputeDescendantBounds(this);
        Size = bounds.Size;
        CustomMinimumSize = Size;
    }

    private void AddTitleField()
    {
        if (_title != null)
        {
            return;
        }

        _title = LoaderUI.CreateTextField("${connection.login.environment.choose}", 40, 0xFFFFFF, false, true) as Label;

        if (_title == null)
        {
            return;
        }

        _title.Position = Vector2.Zero;
        _title.Size = new Vector2(500, _title.Size.Y);
        AddChild(_title);
    }

    private void OnEnvironmentClick(int index)
    {
        if (index < 0 || index >= _environmentIds.Count)
        {
            return;
        }

        _selectedIndex = index;

        ChooseEnvironment();

        context.UpdateEnvironment(_environmentIds[_selectedIndex], true);

        OnAlignElements();
    }

    private void ChooseEnvironment()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _environmentButtons.Count)
        {
            return;
        }

        TextureButton selected = _environmentButtons[_selectedIndex];

        if (_selectedMarker != null)
        {
            _selectedMarker.Position = new Vector2(
                selected.Position.X - ((_selectedMarker.Size.X - selected.Size.X) / 2f) - 1,
                selected.Position.Y - ((_selectedMarker.Size.Y - selected.Size.Y) / 2f) - 1
            );
            _selectedMarker.Visible = true;
        }

        if (_loginButton != null)
        {
            _loginButton.Active = true;
        }

        UpdateDescription();
    }

    private void OnButtonSelect(HcButton _)
    {
        if (_selectedIndex < 0 || _selectedIndex >= _environmentIds.Count)
        {
            return;
        }

        context.UpdateEnvironment(_environmentIds[_selectedIndex], false);
        context.ShowScreen(LoginFlow.SCREEN_LOGIN);
    }

    private void OnButtonSelectToken(HcButton _)
    {
        if (_selectedIndex < 0 || _selectedIndex >= _environmentIds.Count)
        {
            return;
        }

        context.UpdateEnvironment(_environmentIds[_selectedIndex], false);
        context.ShowScreen(LoginFlow.SCREEN_SSO_TOKEN);
    }

    private void UpdateDescription()
    {
        if (_environmentName == null || _selectedIndex < 0 || _selectedIndex >= _environmentIds.Count)
        {
            return;
        }

        string environmentId = _environmentIds[_selectedIndex];
        _environmentName.Text = context.GetProperty($"connection.info.name.{environmentId}");
    }

    private void OnAlignElements()
    {
        if (_balloon == null || _loginButton == null || _loginWithCodeButton == null)
        {
            return;
        }

        LoaderUI.AlignAnchors(this, 0, "c", _balloon);
        LoaderUI.AlignAnchors(_balloon, 0, "r", _loginButton);
        LoaderUI.LineUpHorizontallyRevers(_loginButton, 20, _loginWithCodeButton);
    }

    private async void ScheduleAlign()
    {
        SceneTree? tree = GetTree();

        if (tree == null)
        {
            return;
        }

        await ToSignal(tree.CreateTimer(0.02), SceneTreeTimer.SignalName.Timeout);

        if (!IsInsideTree())
        {
            return;
        }

        OnAlignElements();
    }
}
