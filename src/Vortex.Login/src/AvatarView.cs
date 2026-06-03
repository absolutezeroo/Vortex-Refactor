// @see WIN63-202111081545-75921380-Source-main/src/login/AvatarView.as

using Godot;

using Vortex.Habbo.Communication.Login;
using Vortex.OnBoardingHcUi;

using HcButton = Vortex.OnBoardingHcUi.Button;

namespace Vortex.Login;

/// @see WIN63-202111081545-75921380-Source-main/src/login/AvatarView.as
public partial class AvatarView(ILoginContext context) : Control
{
    private readonly List<TextureButton> _avatarButtons = [];

    private Label? _title;
    private ColouredButton? _saveButton;
    private ColouredButton? _cancelButton;
    private Control? _infoPanel;
    private Label? _avatarDescription;
    private Label? _avatarName;
    private TextureRect? _halo;
    private TextureRect? _glow;

    private bool _initialized;
    private List<AvatarData> _avatars = [];
    private int _selectedIndex;
    private string _baseUrl = string.Empty;

    public string BaseUrl
    {
        set => _baseUrl = value ?? string.Empty;
    }

    public override void _EnterTree()
    {
        base._EnterTree();

        ScheduleAlign();
    }

    public void Init()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        _selectedIndex = 0;

        _infoPanel = new Control();
        AddChild(_infoPanel);

        Control panelBackground = LoaderUI.CreateBalloon(640, 100, 0, false, 995918, "none");

        _infoPanel.AddChild(panelBackground);
        _infoPanel.Position = new Vector2(0, 180);

        _avatarDescription = LoaderUI.CreateTextField(string.Empty, 18, 8309486) as Label;
        _avatarName = LoaderUI.CreateTextField(string.Empty, 20, 0xFFFFFF, false, true) as Label;

        if (_avatarName != null)
        {
            _avatarName.Size = new Vector2(260, _avatarName.Size.Y);
            _avatarName.Position = new Vector2(50, _avatarName.Position.Y);
            _infoPanel.AddChild(_avatarName);
        }

        if (_avatarDescription != null)
        {
            _avatarDescription.Position = new Vector2(50, _avatarDescription.Position.Y);
            _avatarDescription.Size = new Vector2(260, _avatarDescription.Size.Y);
            _infoPanel.AddChild(_avatarDescription);
        }

        if (_avatarName != null && _avatarDescription != null)
        {
            LoaderUI.LineUpVertically(panelBackground, 15 - (int)panelBackground.Size.Y, _avatarName, 20, _avatarDescription);
        }

        // AS3 z-order: halo first (behind), then glow on top.
        // AS3: _SafeStr_4552.blendMode = "overlay" — no native overlay in Godot.
        _halo = LoadTextureRect("res://assets/images/avatar_halo.png");

        if (_halo != null)
        {
            _halo.Visible = false;

            AddChild(_halo);
        }

        // AS3: _avatarGlow.blendMode = "add"
        _glow = LoadTextureRect("res://assets/images/avatar_glow.png");

        if (_glow != null)
        {
            _glow.Visible = false;

            _glow.Material = new CanvasItemMaterial
            {
                BlendMode = CanvasItemMaterial.BlendModeEnum.Add,
            };

            AddChild(_glow);
        }

        AddTitleField();
        AddButtons();
    }

    public void DisposeView()
    {
        _saveButton?.Dispose();
        _cancelButton?.Dispose();
    }

    public void PopulateAvatars(IReadOnlyList<AvatarData> avatars)
    {
        foreach (TextureButton avatarButton in _avatarButtons)
        {
            if (avatarButton.GetParent() == this)
            {
                RemoveChild(avatarButton);
            }

            avatarButton.QueueFree();
        }

        _avatarButtons.Clear();
        _avatars = new List<AvatarData>(avatars);

        for (int i = 0;
             i < _avatars.Count && i <= 6;
             i++)
        {
            AvatarData avatar = _avatars[i];

            TextureButton avatarButton = new()
            {
                Name = $"Avatar_{i}",
                IgnoreTextureSize = false,
                MouseFilter = MouseFilterEnum.Stop,
            };

            if (ResourceLoader.Exists("res://assets/images/placeholder_avatar.png"))
            {
                avatarButton.TextureNormal = GD.Load<Texture2D>("res://assets/images/placeholder_avatar.png");
            }

            int localIndex = i;
            avatarButton.Pressed += () => OnAvatarClick(localIndex);

            int x = ((i + 1) * 10) + (i * 100);
            avatarButton.Position = new Vector2(x, 50);

            AddChild(avatarButton);
            _avatarButtons.Add(avatarButton);

            TextureRect loaderTarget = new()
            {
                MouseFilter = MouseFilterEnum.Ignore,
            };
            avatarButton.AddChild(loaderTarget);

            ImageLoader.CreateLoader(loaderTarget, GetAvatarUrl(avatar), args => OnAvatarImageLoaded(localIndex, args.Loader));
        }

        if (_avatars.Count > 0)
        {
            _selectedIndex = 0;

            UpdateDescription();

            if (_saveButton != null)
            {
                _saveButton.Active = true;
            }

            if (_glow != null)
            {
                _glow.Visible = true;
            }

            if (_halo != null)
            {
                _halo.Visible = true;
            }

            HighlightAvatar(_selectedIndex);
        }
        else
        {
            if (_saveButton != null)
            {
                _saveButton.Active = false;
            }
        }
    }

    private void OnAvatarClick(int index)
    {
        _selectedIndex = index;

        UpdateDescription();
        HighlightAvatar(index);

        if (_saveButton != null)
        {
            _saveButton.Active = true;
        }
    }

    private void OnAvatarImageLoaded(int index, TextureRect loaderTarget)
    {
        if (index < 0 || index >= _avatarButtons.Count)
        {
            return;
        }

        TextureButton avatarButton = _avatarButtons[index];
        if (loaderTarget.Texture != null)
        {
            avatarButton.TextureNormal = loaderTarget.Texture;
        }

        loaderTarget.QueueFree();

        if (_glow != null)
        {
            _glow.Visible = true;
        }

        if (_halo != null)
        {
            _halo.Visible = true;
        }

        HighlightAvatar(_selectedIndex);
    }

    private void UpdateDescription()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _avatars.Count)
        {
            return;
        }

        AvatarData avatar = _avatars[_selectedIndex];

        if (_avatarName != null)
        {
            _avatarName.Text = avatar.Name;
        }

        if (_avatarDescription != null)
        {
            _avatarDescription.Text = avatar.Motto;
        }
    }

    private void HighlightAvatar(int index)
    {
        if (index < 0 || index >= _avatarButtons.Count)
        {
            return;
        }

        TextureButton avatar = _avatarButtons[index];
        Vector2 center = avatar.Position + (avatar.Size / 2f);

        if (_glow != null)
        {
            _glow.Position = new Vector2(center.X - (_glow.Size.X / 2f), center.Y - (_glow.Size.Y / 2f) + 15);
        }

        if (_halo != null)
        {
            _halo.Position = new Vector2(center.X - (_halo.Size.X / 2f), center.Y + _halo.Size.Y - 40);
        }
    }

    private string GetAvatarUrl(AvatarData avatarData)
    {
        string url = $"{_baseUrl}/habbo-imaging/avatarimage?user={avatarData.Name}";

        if (_baseUrl.Contains("local", System.StringComparison.OrdinalIgnoreCase) ||
            _baseUrl.Contains("127.0.0.1", System.StringComparison.OrdinalIgnoreCase))
        {
            url = $"https://www.habbo.com/habbo-imaging/avatarimage?size=m&figure={avatarData.Figure}&direction=2";
        }

        return url;
    }

    private void AddTitleField()
    {
        if (_title != null)
        {
            return;
        }

        _title = LoaderUI.CreateTextField("${connection.login.account.choose}", 40, 0xFFFFFF, false, true) as Label;

        if (_title == null)
        {
            return;
        }

        _title.Position = Vector2.Zero;
        _title.Size = new Vector2(500, _title.Size.Y);

        AddChild(_title);
    }

    private void AddButtons()
    {
        _cancelButton = new ColouredButton("red", "${generic.cancel}", new Rect2(0, 300, 0, 40), true, OnCancel, 0xD8D8D8);

        AddChild(_cancelButton);

        _saveButton = new ColouredButton("gfreen", "${connection.login.play}", new Rect2(0, 300, 0, 40), true, OnChooseAvatar, 0xD8D8D8)
        {
            Active = false,
        };

        AddChild(_saveButton);
    }

    private void OnCancel(HcButton _)
    {
        context.ShowScreen(LoginFlow.SCREEN_LOGIN);
    }

    private void OnChooseAvatar(HcButton _)
    {
        if (_selectedIndex < 0 || _selectedIndex >= _avatars.Count)
        {
            return;
        }

        context.LoginWithAvatar(_avatars[_selectedIndex]);
    }

    private void OnAlignElements()
    {
        if (_saveButton == null || _cancelButton == null || _infoPanel == null)
        {
            return;
        }

        LoaderUI.LineUpVerticallyRevers(_saveButton, 20, _infoPanel);
        LoaderUI.AlignAnchors(_infoPanel, 0, "r", _saveButton);
        LoaderUI.LineUpHorizontallyRevers(_saveButton, 20, _cancelButton);
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

    private static TextureRect? LoadTextureRect(string path)
    {
        if (!ResourceLoader.Exists(path))
        {
            return null;
        }

        Texture2D? texture = GD.Load<Texture2D>(path);

        if (texture == null)
        {
            return null;
        }

        return new TextureRect
        {
            Texture = texture,
            MouseFilter = MouseFilterEnum.Ignore,
        };
    }
}
