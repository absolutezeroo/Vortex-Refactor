// @see habbo/window/enum/class_3821.as

namespace Vortex.Habbo.Window.Enum;

/// @see habbo/window/enum/class_3821.as
public class Class3821
{
    public const string UP_LEFT = "up, left";
    public const string UP_CENTER = "up, center";
    public const string UP_RIGHT = "up, right";
    public const string DOWN_LEFT = "down, left";
    public const string DOWN_CENTER = "down, center";
    public const string DOWN_RIGHT = "down, right";
    public const string LEFT_TOP = "left, top";
    public const string LEFT_MIDDLE = "left, middle";
    public const string LEFT_BOTTOM = "left, bottom";
    public const string RIGHT_TOP = "right, top";
    public const string RIGHT_MIDDLE = "right, middle";
    public const string RIGHT_BOTTOM = "right, bottom";

    public static readonly string[] ALL =
    {
        UP_LEFT,
        UP_CENTER,
        UP_RIGHT,
        DOWN_LEFT,
        DOWN_CENTER,
        DOWN_RIGHT,
        LEFT_TOP,
        LEFT_MIDDLE,
        LEFT_BOTTOM,
        RIGHT_TOP,
        RIGHT_MIDDLE,
        RIGHT_BOTTOM,
    };

    public const string UP = "up";
    public const string DOWN = "down";
    public const string LEFT = "left";
    public const string RIGHT = "right";

    public const string MINIMUM = "minimum";
    public const string MIDDLE = "middle";
    public const string MAXIMUM = "maximum";

    /// @see class_3821.as::directionFromPivot
    public static string DirectionFromPivot(string pivot)
    {
        return pivot[..pivot.IndexOf(',')];
    }

    /// @see class_3821.as::positionFromPivot
    public static string PositionFromPivot(string pivot)
    {
        return pivot switch
        {
            UP_LEFT or DOWN_LEFT or LEFT_TOP or RIGHT_TOP => MINIMUM,
            UP_RIGHT or DOWN_RIGHT or LEFT_BOTTOM or RIGHT_BOTTOM => MAXIMUM,
            _ => MIDDLE,
        };
    }
}
