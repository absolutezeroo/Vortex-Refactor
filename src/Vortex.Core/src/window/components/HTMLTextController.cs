// @see core/window/components/HTMLTextController.as

using System;
using System.Text.RegularExpressions;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/HTMLTextController.as
/// Rich HTML text controller with styled links, CSS stylesheets, and URL handling.
public class HtmlTextController : TextFieldController
{
    /// @see HTMLTextController.as — default link target for all instances
    private static string _defaultLinkTarget = "default";

    /// @see HTMLTextController.as — regex for converting links to events
    private static readonly Regex HttpRegex = new(@"href\s*=\s*['""]http://", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex HttpsRegex = new(@"href\s*=\s*['""]https://", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private string _linkTarget = "default";

    /// @see HTMLTextController.as::HTMLTextController (default)
    public HtmlTextController() : base() { }

    /// @see HTMLTextController.as::HTMLTextController (name + rect)
    public HtmlTextController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see HTMLTextController.as::HTMLTextController (full AS3 11-param signature)
    public HtmlTextController
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
    ) : base(param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12)
    {
        // @see HTMLTextController.as — force read-only mode
        Editable = false;
        Selectable = false;
        ImmediateClickMode = true;
    }

    /// @see HTMLTextController.as::get defaultLinkTarget (static)
    public static string DefaultLinkTarget
    {
        get => _defaultLinkTarget;
        set => _defaultLinkTarget = value ?? "default";
    }

    /// @see HTMLTextController.as::get linkTarget
    public string LinkTarget
    {
        get => _linkTarget != "default" ? _linkTarget : _defaultLinkTarget;
        set => _linkTarget = value ?? "default";
    }

    /// @see HTMLTextController.as::get htmlStyleSheetString
    public string? HtmlStyleSheetString { get; set; }

    /// @see HTMLTextController.as::get immediateClickMode
    public bool ImmediateClickMode { get; set; }

    /// @see HTMLTextController.as::set text
    public override void SetText(string? value)
    {
        if (value == null)
        {
            return;
        }

        // @see HTMLTextController.as — remove old localization listener if localized
        if (_localized)
        {
            _context?.RemoveLocalizationListener(ExtractLocalizationKey(_caption), this);
            _localized = false;
        }

        _caption = value;

        // @see HTMLTextController.as — detect ${key} localization pattern (no _displayRaw guard)
        if (value.Length > 2 && value[0] == '$' && value[1] == '{')
        {
            _context?.RegisterLocalizationListener(ExtractLocalizationKey(value), this);
            _localized = true;
        }
        else
        {
            SetHtmlText(ConvertLinksToEvents(value));
        }
    }

    /// @see HTMLTextController.as::set htmlText
    public override void SetHtmlText(string? value)
    {
        if (value == null)
        {
            return;
        }

        base.SetHtmlText(ConvertLinksToEvents(value));
    }

    /// @see HTMLTextController.as::set localization
    public override void SetLocalization(string? value)
    {
        if (value == null)
        {
            return;
        }

        base.SetHtmlText(ConvertLinksToEvents(value));
    }

    /// @see HTMLTextController.as::convertLinksToEvents (static)
    /// Converts HTTP/HTTPS links to event: protocol for click interception.
    public static string ConvertLinksToEvents(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        // @see HTMLTextController.as — convert href='http://...' to href='event:http://...'
        string result = HttpRegex.Replace(text, "href='event:http://");
        result = HttpsRegex.Replace(result, "href='event:https://");

        return result;
    }

    /// @see HTMLTextController.as::dispose — AS3 has empty body, does NOT call super.dispose()
    public override bool Destroy()
    {
        // @see HTMLTextController.as — intentionally blocks disposal
        return false;
    }
}
