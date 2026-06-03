// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/ISpriteDataContainer.as

namespace Vortex.Habbo.Avatar.Animation;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/ISpriteDataContainer.as
public interface ISpriteDataContainer
{
    /// @see ISpriteDataContainer.as::get animation
    IAnimation Animation { get; }

    /// @see ISpriteDataContainer.as::get id
    string Id { get; }

    /// @see ISpriteDataContainer.as::get ink
    int Ink { get; }

    /// @see ISpriteDataContainer.as::get member
    string Member { get; }

    /// @see ISpriteDataContainer.as::get hasDirections
    bool HasDirections { get; }

    /// @see ISpriteDataContainer.as::get hasStaticY
    bool HasStaticY { get; }

    /// @see ISpriteDataContainer.as::getDirectionOffsetX
    int GetDirectionOffsetX(int direction);

    /// @see ISpriteDataContainer.as::getDirectionOffsetY
    int GetDirectionOffsetY(int direction);

    /// @see ISpriteDataContainer.as::getDirectionOffsetZ
    int GetDirectionOffsetZ(int direction);
}
