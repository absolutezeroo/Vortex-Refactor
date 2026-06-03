// @see WIN63-202407091256-704579380-Source-main/core/window/theme/IThemeManager.as

namespace Vortex.Core.Window.Theme;

/// @see WIN63-202407091256-704579380-Source-main/core/window/theme/IThemeManager.as
public interface IThemeManager
{
    /// @see IThemeManager.as::getStyle
    uint GetStyle(string param1, uint param2, string param3);

    /// @see IThemeManager.as::getThemeAndIntent
    (string theme, string? intent) GetThemeAndIntent(uint param1, uint param2);

    /// @see IThemeManager.as::getIntents
    string[] GetIntents(uint param1, string param2, uint param3);

    /// @see IThemeManager.as::getPropertyDefaults
    IPropertyMap? GetPropertyDefaults(uint param1);

    /// @see IThemeManager.as::getThemes
    string[] GetThemes();
}
