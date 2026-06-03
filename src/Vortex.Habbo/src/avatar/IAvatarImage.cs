// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/class_3374.as

using Godot;

using Vortex.Core.Assets;
using Vortex.Habbo.Avatar.Animation;
using Vortex.Habbo.Avatar.Structure.Figure;

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Habbo.Avatar;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/class_3374.as
public interface IAvatarImage : IDisposable
{
    /// @see class_3374.as::getCroppedImage
    Image? GetCroppedImage(string param1, double param2 = 1);

    /// @see class_3374.as::getImage
    Image? GetImage(string param1, bool param2, double param3 = 1);

    /// @see class_3374.as::getServerRenderData
    object[]? GetServerRenderData();

    /// @see class_3374.as::setDirection
    void SetDirection(string param1, int param2);

    /// @see class_3374.as::setDirectionAngle
    void SetDirectionAngle(string param1, int param2);

    /// @see class_3374.as::updateAnimationByFrames
    void UpdateAnimationByFrames(int param1 = 1);

    /// @see class_3374.as::getScale
    string GetScale();

    /// @see class_3374.as::getSprites
    IList<ISpriteDataContainer> GetSprites();

    /// @see class_3374.as::getLayerData
    IAnimationLayerData? GetLayerData(ISpriteDataContainer param1);

    /// @see class_3374.as::getAsset
    BitmapDataAsset? GetAsset(string param1);

    /// @see class_3374.as::getDirection
    int GetDirection();

    /// @see class_3374.as::getFigure
    IFigureContainer GetFigure();

    /// @see class_3374.as::getPartColor
    IPartColor? GetPartColor(string param1);

    /// @see class_3374.as::isAnimating
    bool IsAnimating();

    /// @see class_3374.as::getCanvasOffsets
    double[]? GetCanvasOffsets();

    /// @see class_3374.as::initActionAppends
    void InitActionAppends();

    /// @see class_3374.as::endActionAppends
    void EndActionAppends();

    /// @see class_3374.as::appendAction
    bool AppendAction(string param1, params object?[] rest);

    /// @see class_3374.as::get avatarSpriteData
    IAvatarSpriteData? AvatarSpriteData { get; }

    /// @see class_3374.as::isPlaceholder
    bool IsPlaceholder();

    /// @see class_3374.as::forceActionUpdate
    void ForceActionUpdate();

    /// @see class_3374.as::get animationHasResetOnToggle
    bool AnimationHasResetOnToggle { get; }

    /// @see class_3374.as::resetAnimationFrameCounter
    void ResetAnimationFrameCounter();

    /// @see class_3374.as::get mainAction
    string? MainAction { get; }

    /// @see class_3374.as::disposeInactiveActionCache
    void DisposeInactiveActionCache();
}
