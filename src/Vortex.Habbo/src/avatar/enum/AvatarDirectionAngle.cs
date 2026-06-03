// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/enum/AvatarDirectionAngle.as

namespace Vortex.Habbo.Avatar.Enum;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/enum/AvatarDirectionAngle.as
public static class AvatarDirectionAngle
{
    /// @see AvatarDirectionAngle.as::const_439
    public static readonly int[] ANGLES = [45, 90, 135, 180, 225, 270, 315, 0];

    /// @see AvatarDirectionAngle.as::const_444
    public static readonly bool[] MIRRORED = [false, false, false, false, true, true, true, false];

    /// @see AvatarDirectionAngle.as::const_832
    public const int MIN_DIRECTION = 0;

    /// @see AvatarDirectionAngle.as::MAX_DIRECTION
    public const int MAX_DIRECTION = 7;
}
