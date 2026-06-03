using System;

namespace Vortex.Habbo.Localization;

/// @see WIN63-202407091256-704579380-Source-main/habbo/localization/BadgeBaseAndLevel.as
public class BadgeBaseAndLevel
{
    private int _level = 1;

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/BadgeBaseAndLevel.as::BadgeBaseAndLevel
    public BadgeBaseAndLevel(string param1)
    {
        int cursor = param1.Length - 1;

        while (cursor > 0 && char.IsAsciiDigit(param1[cursor]))
        {
            cursor--;
        }

        @base = param1[..(cursor + 1)];
        string levelStr = param1[(cursor + 1)..];

        if (!string.IsNullOrEmpty(levelStr) && int.TryParse(levelStr, out int parsed))
        {
            _level = parsed;
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/BadgeBaseAndLevel.as::get base
    public string @base { get; } = string.Empty;

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/BadgeBaseAndLevel.as::get level
    public int level
    {
        get => _level;
        set => _level = Math.Max(1, value);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/localization/BadgeBaseAndLevel.as::get badgeId
    public string badgeId => @base + _level;
}
