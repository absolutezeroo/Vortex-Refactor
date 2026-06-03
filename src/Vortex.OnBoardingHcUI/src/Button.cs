// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/Button.as

using System;

using Godot;

namespace Vortex.OnBoardingHcUi;

/// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/Button.as
public partial class Button : LocalizedSprite
{
    private readonly Action<Button>? _action;
    private readonly bool _fitWidthToText;
    private readonly uint _glowColour;
    private bool _active;
    private bool _alignRight;

    private Control? _background;
    private Label? _captionElement;
    private bool _centred;
    private bool _currentlyEditing;
    private Control? _defaultBackground;
    private Control? _editingBackground;
    private bool _hover;
    private TextureRect? _icon;
    private Control? _inactiveBackground;
    private string _localizedText;

    private bool _pressed;
    private Control? _pressedBackground;
    private bool _ready;
    protected Rect2 _rectangle;
    private Control? _rolloverBackground;
    private bool _selected;

    public Button() : this(string.Empty, new Rect2(0, 0, 120, 40), false, null) { }

    public Button(string label, Rect2 rectangle, bool fitWidthToText, Action<Button>? action, uint glowColour = 0xFFFFFF)
    {
        RemoveOldLocalization(label);

        Label = label;
        _localizedText = label;

        CheckLocalization(Label);

        _rectangle = rectangle;
        _fitWidthToText = fitWidthToText;
        _action = action;
        _glowColour = glowColour;

        MouseFilter = MouseFilterEnum.Stop;
        MouseEntered += OnMouseOver;
        MouseExited += OnMouseOut;
        GuiInput += OnGuiInput;

        Active = true;
    }

    public bool Centred
    {
        get => _centred;
        set
        {
            _centred = value;
            ApplyAlignment();
        }
    }

    public bool Active
    {
        get => _active;
        set
        {
            _active = value;
            MouseDefaultCursorShape = _active ? CursorShape.PointingHand : CursorShape.Arrow;
            Refresh();
        }
    }

    public bool AlignRight
    {
        set
        {
            _alignRight = value;
            ApplyAlignment();
        }
    }

    public string Label { get; }

    public string LocalizedText
    {
        get => _localizedText;
        set
        {
            _localizedText = value;
            if (_ready)
            {
                RebuildVisuals();
            }
        }
    }

    public override string Localization
    {
        set => LocalizedText = value;
    }

    protected virtual string DefaultBackgroundPath => "res://assets/images/HabboButton_button.png";
    protected virtual Rect2 DefaultBackgroundScale9 => new(5, 5, 1, 2);
    protected virtual string PressedBackgroundPath => "res://assets/images/HabboButton_button_pressed.png";
    protected virtual Rect2 PressedBackgroundScale9 => new(6, 10, 1, 3);
    protected virtual string InactiveBackgroundPath => "res://assets/images/HabboButton_button_inactive.png";
    protected virtual Rect2 InactiveBackgroundScale9 => new(5, 6, 1, 2);
    protected virtual string CurrentBackgroundPath => "res://assets/images/HabboButton_button.png";
    protected virtual Rect2 CurrentBackgroundScale9 => new(5, 6, 1, 2);
    protected virtual string? RolloverBackgroundPath => null;
    protected virtual Rect2 RolloverBackgroundScale9 => new(5, 5, 1, 2);
    protected virtual Texture2D? IconTexture => null;
    protected virtual string? IconPath => null;
    protected virtual bool Etching => true;
    protected virtual int Padding => 24;
    protected virtual uint TextColour => 0x000000;
    protected virtual bool Italic => false;
    protected virtual bool Underline => false;

    public event Action<Button>? Pressed;

    public override void _Ready()
    {
        base._Ready();
        _ready = true;
        RebuildVisuals();
    }

    public override void _Process(double delta)
    {
        _ = delta;

        if (_pressed && !Input.IsMouseButtonPressed(MouseButton.Left))
        {
            _pressed = false;
            Refresh();
        }
    }

    public override void _ExitTree()
    {
        _ready = false;

        base._ExitTree();
    }

    public void Unselect()
    {
        _currentlyEditing = false;
        _selected = false;
        Refresh();
    }

    public void CurrentlyEditing()
    {
        _currentlyEditing = true;
        Refresh();
    }

    public void Select()
    {
        _selected = true;
        Refresh();
    }

    private void RebuildVisuals()
    {
        Position = _rectangle.Position;

        _captionElement = BuildCaption();
        if (_fitWidthToText && _captionElement != null)
        {
            float width = _captionElement.GetCombinedMinimumSize().X + Padding;
            _rectangle = new Rect2(_rectangle.Position, new Vector2(width, _rectangle.Size.Y));
        }

        Vector2 buttonSize = _rectangle.Size;
        Size = buttonSize;
        CustomMinimumSize = buttonSize;

        while (GetChildCount() > 0)
        {
            Node? child = GetChild(0);
            RemoveChild(child);
            child.QueueFree();
        }

        _background = new Control
        {
            Name = "Background",
            MouseFilter = MouseFilterEnum.Ignore,
            Size = buttonSize,
            CustomMinimumSize = buttonSize,
        };
        AddChild(_background);

        _defaultBackground = CreateState(DefaultBackgroundPath, DefaultBackgroundScale9, buttonSize);
        _editingBackground = CreateState(CurrentBackgroundPath, CurrentBackgroundScale9, buttonSize);
        _pressedBackground = CreateState(PressedBackgroundPath, PressedBackgroundScale9, buttonSize);
        _inactiveBackground = CreateState(InactiveBackgroundPath, InactiveBackgroundScale9, buttonSize);
        _rolloverBackground = string.IsNullOrWhiteSpace(RolloverBackgroundPath)
            ? null
            : CreateState(RolloverBackgroundPath, RolloverBackgroundScale9, buttonSize);

        _background.AddChild(_defaultBackground);
        _background.AddChild(_pressedBackground);
        _background.AddChild(_inactiveBackground);
        _background.AddChild(_editingBackground);
        if (_rolloverBackground != null)
        {
            _background.AddChild(_rolloverBackground);
        }

        if (_captionElement != null)
        {
            AddChild(_captionElement);
            _captionElement.Position = new Vector2(
                ((buttonSize.X - _captionElement.GetCombinedMinimumSize().X) / 2f) - 2,
                ((buttonSize.Y - _captionElement.GetCombinedMinimumSize().Y) / 2f) - 2
            );
        }

        _icon = BuildIcon();
        if (_icon != null)
        {
            _background.AddChild(_icon);
            _icon.Position = new Vector2(10, (buttonSize.Y - _icon.Size.Y) / 2f);
        }

        Refresh();
        ApplyAlignment();
    }

    private Label? BuildCaption()
    {
        if (Label.Length == 0)
        {
            return null;
        }

        if (LoaderUI.CreateTextField(_localizedText, 18, TextColour, true, false, false, Italic, "left", false, Underline) is not Label label)
        {
            return null;
        }

        if (Etching)
        {
            LoaderUI.AddEtching(label);
        }

        return label;
    }

    private TextureRect? BuildIcon()
    {
        Texture2D? texture = IconTexture;

        if (texture == null && !string.IsNullOrWhiteSpace(IconPath) && ResourceLoader.Exists(IconPath))
        {
            texture = GD.Load<Texture2D>(IconPath);
        }

        if (texture == null)
        {
            return null;
        }

        return new TextureRect
        {
            Name = "Icon",
            Texture = texture,
            MouseFilter = MouseFilterEnum.Ignore,
            Size = texture.GetSize(),
            CustomMinimumSize = texture.GetSize(),
        };
    }

    private static Control CreateState(string texturePath, Rect2 scale9, Vector2 size)
    {
        StyleBoxTexture? style = LoaderUI.CreateScale9GridShapeFromPath(texturePath, scale9);
        if (style != null)
        {
            PanelContainer panel = new()
            {
                MouseFilter = MouseFilterEnum.Ignore,
                Size = size,
                CustomMinimumSize = size,
            };
            panel.AddThemeStyleboxOverride("panel", style);
            return panel;
        }

        return new ColorRect
        {
            Color = new Color(0.2f, 0.2f, 0.2f, 0.8f),
            MouseFilter = MouseFilterEnum.Ignore,
            Size = size,
            CustomMinimumSize = size,
        };
    }

    private void OnMouseOver()
    {
        if (!_active)
        {
            return;
        }

        _hover = true;
        Refresh();
    }

    private void OnMouseOut()
    {
        _hover = false;
        Refresh();
    }

    private void OnGuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseEvent || mouseEvent.ButtonIndex != MouseButton.Left)
        {
            return;
        }

        if (!_active)
        {
            return;
        }

        if (mouseEvent.Pressed)
        {
            _pressed = true;
            Refresh();
            return;
        }

        bool wasPressed = _pressed;
        _pressed = false;
        Refresh();

        if (wasPressed && _hover)
        {
            _action?.Invoke(this);
            Pressed?.Invoke(this);
        }
    }

    private void Refresh()
    {
        if (_background == null || _defaultBackground == null || _pressedBackground == null || _inactiveBackground == null
            || _editingBackground == null)
        {
            return;
        }

        int state = _active ? (_pressed && _hover) || _selected ? 2 : 1 : 3;
        if (_currentlyEditing)
        {
            state = 4;
        }

        _defaultBackground.Visible = state == 1 && (_rolloverBackground == null || !_hover);
        _pressedBackground.Visible = state == 2;
        _inactiveBackground.Visible = state == 3;
        _editingBackground.Visible = state == 4;

        if (_rolloverBackground != null)
        {
            _rolloverBackground.Visible = state == 1 && _hover;
            Modulate = Colors.White;
        }
        else
        {
            Modulate = _hover ? new Color(ToColor(_glowColour), 0.95f) : Colors.White;
        }

        _captionElement?.AddThemeColorOverride("font_color", _active ? ToColor(TextColour) : new Color(0.6f, 0.6f, 0.6f));
    }

    private void ApplyAlignment()
    {
        if (GetParent() is not Control parent)
        {
            return;
        }

        if (_centred)
        {
            Position = new Vector2((parent.Size.X - Size.X) / 2f, Position.Y);
        }

        if (_alignRight)
        {
            Position = new Vector2(parent.Size.X - Size.X, Position.Y);
        }
    }

    private static Color ToColor(uint value)
    {
        return new Color(((value >> 16) & 0xFF) / 255f, ((value >> 8) & 0xFF) / 255f, (value & 0xFF) / 255f);
    }
}
