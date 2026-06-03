// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/PlaceholderAvatarImage.as

using Godot;

using Vortex.Habbo.Avatar.Alias;

namespace Vortex.Habbo.Avatar;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/PlaceholderAvatarImage.as
public class PlaceholderAvatarImage : AvatarImage
{
    /// AS3: static shared cache across all placeholder instances.
    private static readonly Dictionary<string, Image> SharedFullImageCache = new();

    /// @see PlaceholderAvatarImage.as::PlaceholderAvatarImage
    public PlaceholderAvatarImage
    (
        AvatarStructure structure,
        AssetAliasCollection assets,
        AvatarFigureContainer? figure,
        string? scale,
        EffectAssetDownloadManager effectManager
    )
        : base(structure, assets, figure, scale, effectManager, null)
    {
    }

    /// @see PlaceholderAvatarImage.as::dispose
    public override void Dispose()
    {
        if (!_disposed)
        {
            if (_cache != null)
            {
                _cache.Dispose();
                _cache = null;
            }

            _structure = null;
            _assets = null;
            _mainAction = null;
            _figure = null;
            _avatarSpriteData = null;
            _actions = null!;

            if (!_isCacheFromFullImageCache && _image != null)
            {
                _image.Dispose();
            }

            _image = null;
            _canvasOffsets = null;
            _disposed = true;
        }
    }

    /// @see PlaceholderAvatarImage.as::getFullImage
    protected override Image? GetFullImage(string key)
    {
        SharedFullImageCache.TryGetValue(key, out Image? img);
        return img;
    }

    /// @see PlaceholderAvatarImage.as::cacheFullImage
    protected override void CacheFullImage(string key, Image image)
    {
        if (SharedFullImageCache.TryGetValue(key, out Image? existing))
        {
            existing.Dispose();
            SharedFullImageCache.Remove(key);
        }

        SharedFullImageCache[key] = image;
    }

    /// @see PlaceholderAvatarImage.as::appendAction
    public override bool AppendAction(string actionType, params object?[] rest)
    {
        string? param = null;
        if (rest.Length > 0 && rest[0] != null)
        {
            param = rest[0]!.ToString();
        }

        switch (actionType)
        {
            case AvatarAction.POSTURE:
                switch (param)
                {
                    case AvatarAction.POSTURE_LAY:
                    case AvatarAction.POSTURE_WALK:
                    case AvatarAction.POSTURE_STAND:
                    case AvatarAction.POSTURE_SWIM:
                    case AvatarAction.POSTURE_FLOAT:
                    case AvatarAction.POSTURE_SIT:
                        base.AppendAction(actionType, rest);
                        break;
                }
                break;

            case AvatarAction.EFFECT:
            case AvatarAction.DANCE:
            case AvatarAction.WAVE:
            case AvatarAction.SIGN:
            case AvatarAction.CARRY_OBJECT:
            case AvatarAction.USE_OBJECT:
            case AvatarAction.BLOW:
                AddActionData(actionType, param ?? "");
                break;
        }

        return true;
    }

    /// @see PlaceholderAvatarImage.as::isPlaceholder
    public override bool IsPlaceholder()
    {
        return true;
    }
}
