// @see WIN63-202407091256-704579380-Source-main/habbo/window/theme/Theme.as

using Vortex.Core.Window.Theme;

namespace Vortex.Habbo.Window.Theme;

/// @see WIN63-202407091256-704579380-Source-main/habbo/window/theme/Theme.as
public class Theme
{
    public const string NONE = "None";
    public const string ICON = "Icon";
    public const string LEGACY_BORDER = "Legacy border";
    public const string VOLTER = "Volter";
    public const string UBUNTU = "Ubuntu";
    public const string ILLUMINA_LIGHT = "Illumina Light";
    public const string ILLUMINA_DARK = "Illumina Dark";

    /// @see Theme.as::Theme
    public Theme(string param1, bool param2, uint param3, uint param4, PropertyMap param5)
    {
        name = param1;
        isReal = param2;
        baseStyle = param3;
        styleCount = param4;
        propertyDefaults = param5;
    }

    /// @see Theme.as::get name
    public string name { get; }

    /// @see Theme.as::get isReal
    public bool isReal { get; }

    /// @see Theme.as::get baseStyle
    public uint baseStyle { get; }

    /// @see Theme.as::get styleCount
    public uint styleCount { get; }

    /// @see Theme.as::get propertyDefaults
    public PropertyMap propertyDefaults { get; }

    /// @see Theme.as::coversStyle
    public bool CoversStyle(uint param1)
    {
        return param1 >= baseStyle && param1 < baseStyle + styleCount;
    }
}
