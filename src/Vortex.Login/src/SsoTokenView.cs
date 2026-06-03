// @see WIN63-202111081545-75921380-Source-main/src/login/SsoTokenView.as

using Godot;

using Vortex.OnBoardingHcUi;

using HcButton = Vortex.OnBoardingHcUi.Button;

namespace Vortex.Login;

/// @see WIN63-202111081545-75921380-Source-main/src/login/SsoTokenView.as
public partial class SsoTokenView(LoginFlow context) : Control
{
    private Label? _title;
    private ColouredButton? _saveButton;
    private ColouredButton? _cancelButton;
    private readonly int _loginAreaWidth = 640;
    private InputField? _tokenField;
    private bool _initialized;

    public override void _EnterTree()
    {
        base._EnterTree();

        ScheduleAlign();
    }

    public void Init()
    {
        if (!_initialized)
        {
            _initialized = true;
            AddTitleField();
            AddInputFields();
            AddButtons();
        }

        ResetState();
    }

    private void ResetState()
    {
        if (_saveButton != null)
        {
            _saveButton.Active = !string.IsNullOrWhiteSpace(_tokenField?.Text);
        }
    }

    public void DisposeView()
    {
        if (_tokenField == null)
        {
            return;
        }

        _tokenField.Changed -= OnInputChange;
        _tokenField.KeyDown -= OnInputKeyboardEvent;
    }

    public new void Ready()
    {
        if (_saveButton != null)
        {
            _saveButton.Active = true;
        }
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

    private void AddTitleField()
    {
        if (_title != null)
        {
            return;
        }

        _title = LoaderUI.CreateTextField("${connection.login.title}", 40, 0xFFFFFF, false, true) as Label;

        if (_title == null)
        {
            return;
        }

        _title.Position = Vector2.Zero;
        _title.Size = new Vector2(500, _title.Size.Y);

        AddChild(_title);
    }

    private void AddInputFields()
    {
        _tokenField = new InputField(
            context,
            _loginAreaWidth,
            "${connection.login.code.prompt}",
            string.Empty,
            "${connection.login.useTicket}",
            string.Empty,
            true
        );

        AddChild(_tokenField);
        _tokenField.Position = new Vector2(0, 100);
        _tokenField.Changed += OnInputChange;
        _tokenField.KeyDown += OnInputKeyboardEvent;
    }

    private void OnInputKeyboardEvent(InputEventKey key)
    {
        if (key.Keycode == Key.Enter && _saveButton is { Active: true })
        {
            OnLogin(null!);
        }
    }

    private void OnInputChange(string _)
    {
        if (_saveButton != null)
        {
            _saveButton.Active = !string.IsNullOrWhiteSpace(_tokenField?.Text);
        }
    }

    private void AddButtons()
    {
        _cancelButton = new ColouredButton("red", "${generic.cancel}", new Rect2(0, 300, 0, 40), true, OnCancel, 0xD8D8D8);

        AddChild(_cancelButton);

        _saveButton = new ColouredButton("gfreen", "${connection.login.play}", new Rect2(0, 300, 0, 40), true, OnLogin, 0xD8D8D8)
        {
            Active = false,
        };

        AddChild(_saveButton);
    }

    private void OnLogin(HcButton _)
    {
        string tokenValue = _tokenField?.Text?.Trim() ?? string.Empty;

        if (tokenValue.Length == 0)
        {
            if (_saveButton != null)
            {
                _saveButton.Active = false;
            }

            return;
        }

        if (ValidateToken(tokenValue, out string environmentId, out string token))
        {
            context.InitLoginWithSsoToken(environmentId, token);
            return;
        }

        context.InitLoginWithSsoToken(context.GetProperty("environment.id"), tokenValue);
    }

    private static bool ValidateToken(string value, out string environmentId, out string token)
    {
        environmentId = string.Empty;
        token = value;

        string[] parts = value.Split('.');

        if (parts.Length != 3)
        {
            return false;
        }

        string parsedEnvironment = parts[0].Replace("hh", string.Empty, System.StringComparison.Ordinal);

        parsedEnvironment = parsedEnvironment.Replace("br", "pt", System.StringComparison.Ordinal);
        parsedEnvironment = parsedEnvironment.Replace("us", "en", System.StringComparison.Ordinal);

        environmentId = parsedEnvironment;
        token = $"{parts[1]}.{parts[2]}";
        return true;
    }

    private void OnCancel(HcButton _)
    {
        context.ShowScreen(LoginFlow.SCREEN_ENVIRONMENT);
    }

    private void OnAlignElements()
    {
        if (_tokenField == null || _saveButton == null || _cancelButton == null)
        {
            return;
        }

        LoaderUI.AlignAnchors(_tokenField, 0, "r", _saveButton);
        LoaderUI.AlignAnchors(_saveButton, -20 - (int)_cancelButton.Size.X, "l", _cancelButton);
    }
}
