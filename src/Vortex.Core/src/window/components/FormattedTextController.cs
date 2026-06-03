// @see core/window/components/FormattedTextController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/FormattedTextController.as
/// Subclass of TextController that treats text as HTML markup.
public class FormattedTextController : TextController
{
    /// @see FormattedTextController.as::FormattedTextController (default)
    public FormattedTextController() : base() { }

    /// @see FormattedTextController.as::FormattedTextController (name + rect)
    public FormattedTextController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see FormattedTextController.as::FormattedTextController (full AS3 11-param signature)
    public FormattedTextController
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
    }

    /// @see FormattedTextController.as::set text
    /// Overrides to set htmlText instead of plain text.
    public override void SetText(string? value)
    {
        if (value == null)
        {
            return;
        }

        // @see FormattedTextController.as — remove old localization listener if localized
        if (_localized)
        {
            _context?.RemoveLocalizationListener(ExtractLocalizationKey(_caption), this);
            _localized = false;
        }

        _caption = value;

        // @see FormattedTextController.as — detect ${key} localization pattern
        if (value.Length > 2 && value[0] == '$' && value[1] == '{')
        {
            _localized = true;
            _context?.RegisterLocalizationListener(ExtractLocalizationKey(value), this);
        }
        else
        {
            SetHtmlText(value);
        }
    }

    /// @see FormattedTextController.as::set localization
    public override void SetLocalization(string? value)
    {
        if (value == null)
        {
            return;
        }

        SetHtmlText(value);
    }
}
