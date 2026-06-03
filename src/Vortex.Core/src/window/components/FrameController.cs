// @see core/window/components/FrameController.as

using System;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/FrameController.as
public class FrameController : ContainerController
{
    private IWindow? _title;
    private IWindow? _header;
    private IWindow? _content;

    /// @see FrameController.as::var_185 — gates iterator override
    private readonly bool _var185;

    /// @see FrameController.as::var_2584 — help page URL
    private string _helpPage = "";

    /// @see FrameController.as::FrameController (default)
    public FrameController() : base() { }

    /// @see FrameController.as::FrameController (name + rect)
    public FrameController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see FrameController.as::FrameController (full AS3 11-param signature)
    /// @see FrameController.as — param4 = (param4 | 1) & ~16 before super call
    public FrameController
    (
        string param1,
        uint param2,
        uint param3,
        uint param4,
        IWindowContext param5,
        Rect2 param6,
        IWindow? param7,
        Action<WindowEvent, IWindow>? param8 = null,
        IList<object>? param9 = null,
        IList<string>? param10 = null,
        uint param11 = 0, string param12 = ""
    ) : base(param1, param2, param3, (param4 | 1) & ~16u, param5, param6, param7, param8, param9, param10, param11, param12)
    {
        // @see FrameController.as — var_185 = true gates iterator to use content.iterator
        _var185 = true;
        Activate();
        SetupScaling();

        // @see FrameController.as — wire help button procedure
        IWindow? helpButton = FindChildByName("header_button_help");

        if (helpButton is WindowController helpCtrl)
        {
            helpCtrl.procedure = HelpButtonProcedure;
        }

        // @see FrameController.as — helpPage setter controls help button visibility
        HelpPage = _helpPage;
    }

    /// @see FrameController.as::get title — typed as ILabelWindow (ITextWindow) in AS3
    public IWindow? title => _title ??= FindChildByTag("_TITLE");

    /// @see FrameController.as::get header
    public IWindow? header => _header ??= FindChildByTag("_HEADER");

    /// @see FrameController.as::get content — typed as IWindowContainer in AS3
    public IWindow? content => _content ??= FindChildByTag("_CONTENT");

    /// @see FrameController.as::get scaler — NOT cached per AS3
    public IWindow? scaler => FindChildByTag("_SCALER");

    /// @see FrameController.as::set helpPage
    public string HelpPage
    {
        get => _helpPage;
        set
        {
            _helpPage = value;
            IWindow? helpButton = FindChildByName("header_button_help");

            if (helpButton != null)
            {
                helpButton.visible = !string.IsNullOrEmpty(_helpPage);
            }
        }
    }

    /// @see FrameController.as::set caption
    /// AS3 calls super.caption first (triggers invalidation), then sets title.text
    public override string caption
    {
        get => _caption;
        set
        {
            base.caption = value;

            try
            {
                if (title != null)
                {
                    title.caption = value;
                }
            }
            catch
            {
                // @see FrameController.as — try/catch around title caption set
            }
        }
    }

    /// @see FrameController.as::set color
    /// AS3 uses override, groupChildrenWithTag depth=0 (default, current level only)
    public override uint color
    {
        get => base.color;
        set
        {
            base.color = value;

            List<IWindow> colorizeChildren = new();
            // @see FrameController.as — default depth (0 = current level only)
            GroupChildrenWithTag(TAG_COLORIZE, colorizeChildren);

            foreach (IWindow child in colorizeChildren)
            {
                child.color = value;
            }
        }
    }

    /// @see FrameController.as::get iterator
    public override object? Iterator()
    {
        return content != null && _var185 ? (content as ContainerController)?.Iterator() : new Iterators.ContainerIterator(this);
    }

    /// @see FrameController.as::buildFromXML — returns bool per AS3 signature
    public bool BuildFromXml(XElement param1, Dictionary<string, object?>? param2 = null)
    {
        IWindow? contentWindow = content;

        if (contentWindow == null || _context == null)
        {
            return false;
        }

        IWindow? result = _context.GetWindowParser().ParseAndConstruct(param1, contentWindow, param2);

        return result != null;
    }

    /// @see FrameController.as::set properties
    public override void ApplyProperties(PropertyStruct[] properties)
    {
        foreach (PropertyStruct prop in properties)
        {
            switch (prop.key)
            {
                case "help_page":
                    if (prop.value is string hp)
                    {
                        HelpPage = hp;
                    }
                    break;
                case "margin_left":
                    if (prop.value is int ml && content != null)
                    {
                        content.x = ml;
                        content.width = width - ml - (width - content.x - content.width);
                    }
                    break;
                case "margin_top":
                    if (prop.value is int mt && content != null)
                    {
                        content.y = mt;
                        content.height = height - mt - (height - content.y - content.height);
                    }
                    break;
                case "margin_right":
                    if (prop.value is int mr && content != null)
                    {
                        content.width = width - content.x - mr;
                    }
                    break;
                case "margin_bottom":
                    if (prop.value is int mb && content != null)
                    {
                        content.height = height - content.y - mb;
                    }
                    break;
            }
        }

        base.ApplyProperties(properties);
    }

    /// @see FrameController.as::setParamFlag
    public override void SetParamFlag(uint param1, bool param2 = true)
    {
        base.SetParamFlag(param1, param2);
        SetupScaling();
    }

    /// @see FrameController.as::setupScaling
    private void SetupScaling()
    {
        IWindow? scalerWindow = scaler;
        if (scalerWindow == null)
        {
            return;
        }

        // @see FrameController.as — bit 65536=vertical, 8192=horizontal, 4096=scaler_visible
        bool vertical = TestParamFlag(65536);
        bool horizontal = TestParamFlag(8192);
        bool scalerVisible = TestParamFlag(4096);

        // @see FrameController.as — flags on scaler child
        scalerWindow.SetParamFlag(8192, horizontal || vertical);
        scalerWindow.SetParamFlag(4096, scalerVisible || vertical);

        // @see FrameController.as — visible if any scaling mode active
        scalerWindow.visible = horizontal || scalerVisible || vertical;
    }

    /// @see FrameController.as::helpButtonProcedure
    private void HelpButtonProcedure(WindowEvent param1, IWindow param2)
    {
        // @see FrameController.as — on WME_CLICK, invoke help callback
        // Help callback (var_4001) not yet ported — stub
    }
}
