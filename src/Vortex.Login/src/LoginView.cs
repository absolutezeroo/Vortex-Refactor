// @see WIN63-202111081545-75921380-Source-main/src/login/LoginView.as

using Godot;

using Vortex.Habbo.Utils;
using Vortex.OnBoardingHcUi;

using HcButton = Vortex.OnBoardingHcUi.Button;

namespace Vortex.Login;

/// @see WIN63-202111081545-75921380-Source-main/src/login/LoginView.as
public partial class LoginView(ILoginContext context) : Control
{
    private Label? _title;
    private ColouredButton? _saveButton;
    private ColouredButton? _cancelButton;
    private InputField? _emailField;
    private InputField? _passwordField;

    private readonly int _loginAreaWidth = 640;
    private bool _initialized;

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

        AddTitleField();
        AddInputFields();
        AddButtons();
    }

    public void DisposeView()
    {
        _saveButton?.Dispose();
        _cancelButton?.Dispose();
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
        _emailField = new InputField(
            context,
            _loginAreaWidth,
            "${connection.login.email}",
            CommunicationUtils.ReadSOLString(CommunicationUtils.SOL_PROPERTY_LOGIN_NAME),
            "${connection.login.missing_credentials}",
            string.Empty
        );

        AddChild(_emailField);
        _emailField.Position = new Vector2(0, 100);

        _passwordField = new InputField(
            context,
            _loginAreaWidth,
            "${connection.login.password}",
            CommunicationUtils.RestorePassword(),
            string.Empty,
            string.Empty,
            true
        );

        AddChild(_passwordField);
    }

    private void AddButtons()
    {
        _cancelButton = new ColouredButton("red", "${generic.cancel}", new Rect2(0, 300, 0, 40), true, OnCancel, 0xD8D8D8);
        AddChild(_cancelButton);

        _saveButton = new ColouredButton("gfreen", "${connection.login.play}", new Rect2(0, 300, 0, 40), true, SaveOutfit, 0xD8D8D8)
        {
            Active = false,
        };
        AddChild(_saveButton);
    }

    private void SaveOutfit(HcButton _)
    {
        context.InitLogin(_emailField?.Text ?? string.Empty, _passwordField?.Text ?? string.Empty);
    }

    private void OnCancel(HcButton _)
    {
        context.ShowScreen(LoginFlow.SCREEN_ENVIRONMENT);
    }

    private void OnAlignElements()
    {
        if (_emailField == null || _passwordField == null || _saveButton == null || _cancelButton == null)
        {
            return;
        }

        LoaderUI.LineUpVertically(_emailField, -20, _passwordField);
        LoaderUI.AlignAnchors(_emailField, 0, "l", _passwordField);
        LoaderUI.AlignAnchors(_emailField, 0, "r", _saveButton);
        LoaderUI.LineUpHorizontallyRevers(_saveButton, 20, _cancelButton);
    }
}
