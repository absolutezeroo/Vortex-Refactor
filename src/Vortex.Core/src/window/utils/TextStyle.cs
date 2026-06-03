// @see core/window/utils/class_3613.as

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/class_3613.as
public class TextStyle
{
    public const string NORMAL = "normal";
    public const string ITALIC = "italic";
    public const string BOLD = "bold";
    public const string UNDERLINE = "underline";
    public const string NONE = "none";
    public const string ADVANCED = "advanced";

    public const string TOP_LEFT = "top-left";
    public const string TOP = "top";
    public const string TOP_RIGHT = "top-right";
    public const string LEFT = "left";
    public const string RIGHT = "right";
    public const string BOTTOM_LEFT = "bottom-left";
    public const string BOTTOM = "bottom";
    public const string BOTTOM_RIGHT = "bottom-right";

    public string? Name;
    public uint? Color;
    public string? FontFamily;
    public int? FontSize;
    public string? FontStyle;
    public string? FontWeight;
    public bool? Kerning;
    public int? Leading;
    public int? LetterSpacing;
    public string? TextDecoration;
    public int? TextIndent;
    public string? AntiAliasType;
    public int? Sharpness;
    public int? Thickness;
    public uint? EtchingColor;
    public string? EtchingPosition;

    /// @see class_3613.as::class_3613
    public TextStyle() { }

    /// @see class_3613.as::toString
    public override string ToString()
    {
        string s = (Name ?? "") + " {\n";

        if (Color != null)
        {
            s += $"\tcolor: #{Color:x};\n";
        }

        if (FontFamily != null)
        {
            s += $"\tfont-family: {FontFamily};\n";
        }

        if (FontSize != null)
        {
            s += $"\tfont-size: {FontSize};\n";
        }

        if (FontStyle != null)
        {
            s += $"\tfont-style: {FontStyle};\n";
        }

        if (FontWeight != null)
        {
            s += $"\tfont-weight: {FontWeight};\n";
        }

        if (Kerning != null)
        {
            s += $"\tkerning: {Kerning};\n";
        }

        if (Leading != null)
        {
            s += $"\tleading: {Leading};\n";
        }

        if (LetterSpacing != null)
        {
            s += $"\tletter-spacing: {LetterSpacing};\n";
        }

        if (TextDecoration != null)
        {
            s += $"\ttext-decoration: {TextDecoration};\n";
        }

        if (TextIndent != null)
        {
            s += $"\ttext-indent: {TextIndent};\n";
        }

        if (AntiAliasType != null)
        {
            s += $"\tanti-alias-type: {AntiAliasType};\n";
        }

        if (Sharpness != null)
        {
            s += $"\tsharpness: {Sharpness};\n";
        }

        if (Thickness != null)
        {
            s += $"\tthickness: {Thickness};\n";
        }

        if (EtchingColor != null)
        {
            s += $"\tetching-color: #{EtchingColor:x};\n";
        }

        if (EtchingPosition != null)
        {
            s += $"\tetching-direction: {EtchingPosition};\n";
        }

        return s + "}";
    }

    /// @see class_3613.as::equals
    public bool Equals(TextStyle other)
    {
        return Color == other.Color &&
               FontFamily == other.FontFamily &&
               FontSize == other.FontSize &&
               FontStyle == other.FontStyle &&
               FontWeight == other.FontWeight &&
               Kerning == other.Kerning &&
               Leading == other.Leading &&
               LetterSpacing == other.LetterSpacing &&
               TextDecoration == other.TextDecoration &&
               TextIndent == other.TextIndent &&
               AntiAliasType == other.AntiAliasType &&
               Sharpness == other.Sharpness &&
               Thickness == other.Thickness &&
               EtchingColor == other.EtchingColor &&
               EtchingPosition == other.EtchingPosition;
    }

    /// @see class_3613.as::clone
    public TextStyle Clone()
    {
        return new TextStyle
        {
            Name = Name,
            Color = Color,
            FontFamily = FontFamily,
            FontSize = FontSize,
            FontStyle = FontStyle,
            FontWeight = FontWeight,
            Kerning = Kerning,
            Leading = Leading,
            LetterSpacing = LetterSpacing,
            TextDecoration = TextDecoration,
            TextIndent = TextIndent,
            AntiAliasType = AntiAliasType,
            Sharpness = Sharpness,
            Thickness = Thickness,
            EtchingColor = EtchingColor,
            EtchingPosition = EtchingPosition,
        };
    }
}
