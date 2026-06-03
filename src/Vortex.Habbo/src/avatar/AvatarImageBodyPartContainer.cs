// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/AvatarImageBodyPartContainer.as

using Godot;

namespace Vortex.Habbo.Avatar;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/AvatarImageBodyPartContainer.as
public class AvatarImageBodyPartContainer
{
    private Vector2I _regPoint;
    private Vector2I _offset;

    /// @see AvatarImageBodyPartContainer.as::AvatarImageBodyPartContainer
    public AvatarImageBodyPartContainer(Image? image, Vector2I regPoint, bool isCacheable)
    {
        _offset = Vector2I.Zero;
        Image = image;
        _regPoint = regPoint;
        IsCacheable = isCacheable;
    }

    /// @see AvatarImageBodyPartContainer.as::get isCacheable
    public bool IsCacheable { get; }

    /// @see AvatarImageBodyPartContainer.as::dispose
    public void Dispose()
    {
        Image = null;
    }

    /// @see AvatarImageBodyPartContainer.as::set image
    /// @see AvatarImageBodyPartContainer.as::get image
    public Image? Image { get; set; }

    /// @see AvatarImageBodyPartContainer.as::setRegPoint
    public void SetRegPoint(Vector2I point)
    {
        _regPoint = point;
    }

    /// @see AvatarImageBodyPartContainer.as::get regPoint
    public Vector2I RegPoint => _regPoint + _offset;

    /// @see AvatarImageBodyPartContainer.as::set offset
    public Vector2I Offset
    {
        set => _offset = value;
    }
}
