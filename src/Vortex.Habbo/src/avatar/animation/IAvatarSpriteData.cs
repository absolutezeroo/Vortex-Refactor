// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/class_3581.as

namespace Vortex.Habbo.Avatar.Animation;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/class_3581.as
public interface IAvatarSpriteData
{
    /// @see class_3581.as::get ink
    int Ink { get; }

    /// @see class_3581.as::get colorTransform
    /// Godot adaptation: float[4] = { rMultiplier, gMultiplier, bMultiplier, aMultiplier }
    float[] ColorTransform { get; }

    /// @see class_3581.as::get paletteIsGrayscale
    bool PaletteIsGrayscale { get; }

    /// @see class_3581.as::get reds
    int[] Reds { get; }

    /// @see class_3581.as::get greens
    int[] Greens { get; }

    /// @see class_3581.as::get blues
    int[] Blues { get; }

    /// @see class_3581.as::get alphas
    int[] Alphas { get; }
}
