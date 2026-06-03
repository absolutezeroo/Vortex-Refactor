// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/InputField.as

using Godot;

using Vortex.Core.Utils;

namespace Vortex.OnBoardingHcUi;

/// @see WIN63-202111081545-75921380-Source-main/src/onBoardingHcUi/InputField.as
public partial class InputField : Control
{
    private readonly int _style = LoaderUI.STYLE_HITCH;
    private IUIContext? _context;
    private Control? _frame;
    private Label? _promptField;
    private LineEdit? _field;
    private bool _inputClickedAlready;
    private string _inputDefaultString = string.Empty;
    private int _dialogWidth;
    private bool _isPassword;
    private string _caption = string.Empty;
    private string _subCaption = string.Empty;
    private float _maxWidth;
    private string _prompt = string.Empty;

    public event System.Action<string>? Changed;
    public event System.Action<InputEventKey>? KeyDown;

    public InputField() { }

    public InputField
    (
        IUIContext context,
        int dialogWidth,
        string prompt,
        string? inputDefaultString,
        string caption,
        string subCaption,
        bool isPassword = false
    )
    {
        _context = context;
        _dialogWidth = dialogWidth;
        _prompt = prompt;
        _inputDefaultString = inputDefaultString ?? string.Empty;
        _caption = caption;
        _subCaption = subCaption;
        _isPassword = isPassword;

        Init();
    }

    public bool Disposed { get; private set; }

    public string Text => _field?.Text ?? string.Empty;

    public override void _ExitTree()
    {
        base._ExitTree();
    }

    public void DisposeField()
    {
        if (Disposed)
        {
            return;
        }

        if (_field != null)
        {
            _field.TextChanged -= OnInputChange;
            _field.GuiInput -= OnFieldGuiInput;
            _field.FocusEntered -= OnInputClicked;
        }

        _context = null;
        _field = null;
        _promptField = null;
        _frame = null;
        Disposed = true;
    }

    private void Init()
    {
        if (_frame != null)
        {
            return;
        }

        _frame = LoaderUI.CreateFrame(_caption, _subCaption, new Rect2(0, 0, _dialogWidth, 1), _style);
        AddChild(_frame);

        Control inputBackgroundHolder = new()
        {
            Name = "InputBackgroundHolder",
            MouseFilter = MouseFilterEnum.Stop,
        };

        inputBackgroundHolder.GuiInput += OnBackgroundGuiInput;

        NinePatchRect background = NineSplitSprite.INPUT_FIELD_HITCH.Render(_dialogWidth, 31);
        inputBackgroundHolder.Size = background.Size;
        inputBackgroundHolder.CustomMinimumSize = background.CustomMinimumSize;
        inputBackgroundHolder.AddChild(background);
        _frame.AddChild(inputBackgroundHolder);

        _maxWidth = inputBackgroundHolder.Size.X - 30;

        _promptField = LoaderUI.CreateTextField(_prompt, 18, 0x666666, true) as Label;
        if (_promptField != null)
        {
            _promptField.Modulate = new Color(1f, 1f, 1f, 0.8f);
            _promptField.Position = new Vector2(
                inputBackgroundHolder.Position.X + 16,
                inputBackgroundHolder.Position.Y + ((inputBackgroundHolder.Size.Y - _promptField.Size.Y) / 2f)
            );
            _promptField.Size = new Vector2(_maxWidth, _promptField.Size.Y);
            _promptField.Visible = string.IsNullOrWhiteSpace(_inputDefaultString);
            _frame.AddChild(_promptField);
        }

        _field = LoaderUI.CreateTextField(_inputDefaultString, 18, 0x666666, true, false, true) as LineEdit ?? new LineEdit();

        _field.Secret = _isPassword;
        // AS3: _field.y = (_local_1.y + int(((_local_1.height - _field.height) / 2)));
        // LineEdit.Size.Y is 0 before entering the tree, so we match the background height
        // and let Godot vertically center the text within the LineEdit.
        _field.Position = new Vector2(inputBackgroundHolder.Position.X + 16, inputBackgroundHolder.Position.Y);
        _field.Size = new Vector2(_maxWidth, inputBackgroundHolder.Size.Y);
        _field.CustomMinimumSize = new Vector2(_maxWidth, inputBackgroundHolder.Size.Y);
        _field.TextChanged += OnInputChange;
        _field.GuiInput += OnFieldGuiInput;
        _field.FocusEntered += OnInputClicked;
        _frame.AddChild(_field);

        _frame.Position = new Vector2(_frame.Position.X, _frame.Position.Y + 25);

        // AS3 Sprite.width/height auto-computes from children. Godot Control.Size does not.
        // Set Size to emulate Flash bounds for layout methods (LineUpVertically, AlignAnchors).
        Rect2 bounds = DisplayUtils.ComputeDescendantBounds(this);
        Size = new Vector2(Mathf.Max(_dialogWidth, bounds.Size.X), bounds.Size.Y);
        CustomMinimumSize = Size;
    }

    private void OnInputChange(string text)
    {
        if (_promptField != null)
        {
            _promptField.Visible = text.Length == 0;
        }

        Changed?.Invoke(text);
    }

    private void OnBackgroundGuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
        {
            return;
        }

        _field?.GrabFocus();
        OnInputClicked();
    }

    private void OnFieldGuiInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Echo: false } key)
        {
            KeyDown?.Invoke(key);
        }
    }

    private void OnInputClicked()
    {
        if (_inputClickedAlready)
        {
            return;
        }

        if (_promptField != null)
        {
            _promptField.Visible = false;
        }

        _inputClickedAlready = true;

        _field?.AddThemeColorOverride("font_color", _style == LoaderUI.STYLE_HITCH ? new Color(0.4f, 0.4f, 0.4f) : Colors.Black);

        OnInputChange(_field?.Text ?? string.Empty);
    }
}
