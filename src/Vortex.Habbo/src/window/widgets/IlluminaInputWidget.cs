// @see habbo/window/widgets/IlluminaInputWidget.as

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Iterators;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/IlluminaInputWidget.as
public class IlluminaInputWidget : IIlluminaInputWidget, IWidget
{
    private const int SINGLE_LINE_HEIGHT = 28;

    private IWidgetWindow? _widgetWindow;
    private HabboWindowManagerComponent? _windowManager;
    private IWindowContainer? _container;
    private IWindow? _submitButton;
    private ITextWindow? _inputField;
    private IWindow? _inputWindow;
    private IWindow? _emptyMessageLabel;
    private IIlluminaInputHandler? _submitHandler;

    /// @see habbo/window/widgets/IlluminaInputWidget.as::IlluminaInputWidget
    public IlluminaInputWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;

        // @see IlluminaInputWidget.as — build root from illumina_input_xml asset
        object? xmlAsset = windowManager.FindAssetByName("illumina_input_xml");
        if (xmlAsset is Core.Assets.IAsset { Content: System.Xml.Linq.XElement xml })
        {
            _container = ((IWindowFactory)windowManager).BuildFromXml(xml) as IWindowContainer;
        }

        if (_container == null)
        {
            return;
        }

        IWindow? containerWin = _container;
        containerWin.width = _widgetWindow.width;

        _submitButton = containerWin.FindChildByName("submit");
        _inputWindow = containerWin.FindChildByName("input");
        _inputField = _inputWindow as ITextWindow;
        _emptyMessageLabel = containerWin.FindChildByName("empty_message");

        // @see IlluminaInputWidget.as — set defaults
        if (_submitButton != null)
        {
            _submitButton.caption = "${widgets.chatinput.say}";
            _submitButton.visible = true;
        }
        if (_emptyMessageLabel != null)
        {
            _emptyMessageLabel.caption = "";
        }
        if (_inputField != null)
        {
            _inputField.Multiline = false;
            _inputField.MaxChars = 0;
        }

        Refresh();

        containerWin.procedure = WidgetProcedure;

        _widgetWindow.RootWindow(_container);
    }

    /// @see habbo/window/widgets/IlluminaInputWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/IlluminaInputWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/IlluminaInputWidget.as::message
    string? IIlluminaInputWidget.Message
    {
        get => _inputWindow?.caption ?? "";
        set
        {
            if (value == null || _inputWindow == null)
            {
                return;
            }

            _inputWindow.caption = value;
            Refresh();
        }
    }

    /// @see habbo/window/widgets/IlluminaInputWidget.as::submitHandler
    IIlluminaInputHandler? IIlluminaInputWidget.SubmitHandler
    {
        get => _submitHandler;
        set => _submitHandler = value;
    }

    /// @see habbo/window/widgets/IlluminaInputWidget.as::buttonCaption
    string? IIlluminaInputWidget.ButtonCaption
    {
        get => _submitButton?.caption ?? "";
        set
        {
            if (value == null || _submitButton == null)
            {
                return;
            }

            _submitButton.caption = value;
            _submitButton.visible = !string.IsNullOrEmpty(value);
            Refresh();
        }
    }

    /// @see habbo/window/widgets/IlluminaInputWidget.as::emptyMessage
    string? IIlluminaInputWidget.EmptyMessage
    {
        get => _emptyMessageLabel?.caption ?? "";
        set
        {
            if (value != null && _emptyMessageLabel != null)
            {
                _emptyMessageLabel.caption = value;
            }
        }
    }

    /// @see habbo/window/widgets/IlluminaInputWidget.as::multiline
    bool IIlluminaInputWidget.Multiline
    {
        get => _inputField?.Multiline == true;
        set
        {
            if (_inputField == null || _container == null)
            {
                return;
            }

            _inputField.Multiline = value;
            IWindow? containerWin = _container;
            containerWin.SetParamFlag(2048, value);
            containerWin.height = value && _widgetWindow != null ? _widgetWindow.height : SINGLE_LINE_HEIGHT;
        }
    }

    /// @see habbo/window/widgets/IlluminaInputWidget.as::maxChars
    int IIlluminaInputWidget.MaxChars
    {
        get => _inputField?.MaxChars ?? 0;
        set
        {
            if (_inputField != null)
            {
                _inputField.MaxChars = value;
            }
        }
    }

    /// @see habbo/window/widgets/IlluminaInputWidget.as::widgetProcedure
    private void WidgetProcedure(WindowEvent param1, IWindow param2)
    {
        switch (param1.type)
        {
            case "WE_CHANGE":
                if (param2 == _inputWindow)
                {
                    Refresh();
                }
                break;
            case "WKE_KEY_DOWN":
                // @see IlluminaInputWidget.as — Enter key (charCode 13) submits if button visible
                if (param2 == _inputWindow && _submitButton is { visible: true })
                {
                    SubmitMessage();
                }
                break;
            case "WME_CLICK":
                if (param2 == _submitButton)
                {
                    SubmitMessage();
                }
                break;
        }
    }

    /// @see habbo/window/widgets/IlluminaInputWidget.as::submitMessage
    private void SubmitMessage()
    {
        if (_submitHandler != null && _widgetWindow != null)
        {
            _submitHandler.OnInput(_widgetWindow, _inputWindow?.caption ?? "");
        }
    }

    /// @see habbo/window/widgets/IlluminaInputWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        _inputField = null;
        _inputWindow = null;
        _submitButton = null;
        _emptyMessageLabel = null;

        if (_container is IWindow containerWin)
        {
            containerWin.Destroy();
            _container = null;
        }

        if (_widgetWindow != null)
        {
            _widgetWindow.RootWindow(null);
            _widgetWindow = null;
        }

        _windowManager = null;
        _submitHandler = null;
        disposed = true;
    }

    /// @see habbo/window/widgets/IlluminaInputWidget.as::refresh
    private void Refresh()
    {
        if (_emptyMessageLabel == null || _inputWindow == null || _container == null)
        {
            return;
        }

        // @see IlluminaInputWidget.as — show empty message if input is empty
        int inputLength = _inputField?.Length ?? 0;
        _emptyMessageLabel.visible = inputLength == 0;

        // @see IlluminaInputWidget.as — adjust input width based on button visibility
        IWindow? containerWin = (IWindow)_container;
        float rightEdge = _submitButton is { visible: true } ? _submitButton.x : containerWin.width;
        _inputWindow.width = rightEdge - (_inputWindow.x * 2);
    }
}
