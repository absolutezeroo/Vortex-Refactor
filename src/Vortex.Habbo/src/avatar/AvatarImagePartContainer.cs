// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/AvatarImagePartContainer.as

using Vortex.Habbo.Avatar.Actions;
using Vortex.Habbo.Avatar.Structure.Animation;
using Vortex.Habbo.Avatar.Structure.Figure;

namespace Vortex.Habbo.Avatar;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/AvatarImagePartContainer.as
public class AvatarImagePartContainer
{
    private readonly AnimationFrame?[] _frames;

    /// @see AvatarImagePartContainer.as::AvatarImagePartContainer
    public AvatarImagePartContainer
    (
        string bodyPartId,
        string partType,
        string partId,
        IPartColor? color,
        AnimationFrame?[] frames,
        IActionDefinition? action,
        bool isColorable,
        int paletteMapId,
        string flippedPartType = "",
        bool isBlendable = false,
        float blendAlpha = 1f
    )
    {
        BodyPartId = bodyPartId;
        PartType = partType;
        PartId = partId;
        Color = color;
        _frames = frames;
        Action = action;
        IsColorable = isColorable;
        PaletteMapId = paletteMapId;
        FlippedPartType = flippedPartType;
        IsBlendable = isBlendable;
        BlendAlpha = blendAlpha;

        // AS3: eyes are never colorable
        if (PartType == "ey")
        {
            IsColorable = false;
        }
    }

    /// @see AvatarImagePartContainer.as::getFrameIndex
    public int GetFrameIndex(int frame)
    {
        if (_frames.Length == 0)
        {
            return 0;
        }

        int idx = frame % _frames.Length;
        AnimationFrame? animFrame = _frames[idx];

        if (animFrame != null)
        {
            return animFrame.Number;
        }

        return idx;
    }

    /// @see AvatarImagePartContainer.as::getFrameDefinition
    public AnimationFrame? GetFrameDefinition(int frame)
    {
        if (_frames.Length == 0)
        {
            return null;
        }

        int idx = frame % _frames.Length;

        if (idx < _frames.Length)
        {
            return _frames[idx];
        }

        return null;
    }

    /// @see AvatarImagePartContainer.as::getCacheableKey
    public string GetCacheableKey(int frame)
    {
        if (_frames.Length <= 0)
        {
            return PartId + ":" + (frame % System.Math.Max(1, _frames.Length));
        }

        int idx = frame % _frames.Length;

        if (idx >= _frames.Length)
        {
            return PartId + ":" + (frame % System.Math.Max(1, _frames.Length));
        }

        AnimationFrame? animFrame = _frames[idx];

        if (animFrame != null)
        {
            return PartId + ":" + animFrame.AssetPartDefinition + ":" + animFrame.Number;
        }

        return PartId + ":" + (frame % System.Math.Max(1, _frames.Length));
    }

    /// @see AvatarImagePartContainer.as::get bodyPartId
    public string BodyPartId { get; }

    /// @see AvatarImagePartContainer.as::get partType
    public string PartType { get; }

    /// @see AvatarImagePartContainer.as::get partId
    public string PartId { get; }

    /// @see AvatarImagePartContainer.as::get color
    public IPartColor? Color { get; }

    /// @see AvatarImagePartContainer.as::get action
    public IActionDefinition? Action { get; }

    /// @see AvatarImagePartContainer.as::set isColorable
    /// @see AvatarImagePartContainer.as::get isColorable
    public bool IsColorable { get; set; }

    /// @see AvatarImagePartContainer.as::get paletteMapId
    public int PaletteMapId { get; }

    /// @see AvatarImagePartContainer.as::get flippedPartType
    public string FlippedPartType { get; }

    /// @see AvatarImagePartContainer.as::get isBlendable
    public bool IsBlendable { get; }

    /// @see AvatarImagePartContainer.as::get blendTransform
    /// Godot adaptation: returns alpha multiplier directly (AS3 was ColorTransform(1,1,1,alpha))
    public float BlendAlpha { get; }

    /// @see AvatarImagePartContainer.as::toString
    public override string ToString()
    {
        return BodyPartId + ":" + PartType + ":" + PartId;
    }
}
