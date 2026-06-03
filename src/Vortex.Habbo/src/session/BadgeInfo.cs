// @see com.sulake.habbo.session.BadgeInfo

using Godot;

namespace Vortex.Habbo.Session;

/// @see com.sulake.habbo.session.BadgeInfo
public class BadgeInfo
{
    /// @see BadgeInfo.as::BadgeInfo
    public BadgeInfo(Image? image, bool isPlaceholder)
    {
        Image = image;
        IsPlaceholder = isPlaceholder;
    }

    public Image? Image { get; }
    public bool IsPlaceholder { get; }
}
