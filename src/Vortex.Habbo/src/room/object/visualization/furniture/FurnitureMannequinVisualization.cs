using Godot;

using Vortex.Habbo.Avatar;
using Vortex.Room.Object;
using Vortex.Room.Object.Visualization;

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureMannequinVisualization
public class FurnitureMannequinVisualization : FurnitureVisualization, IAvatarImageListener
{
    private const string AVATAR_IMAGE_SPRITE_TAG = "avatar_image";
    private const string MANNEQUIN_BODY = "hd-99999-99998";

    private static Dictionary<int, IAvatarImage>? s_customPlaceholders;
    private static int s_instanceCount;

    private string? _figure;
    private string? _gender;
    private int _size;
    private bool _needsUpdate;
    private string? _dynamicAssetName;
    private AvatarFurnitureVisualizationData? _avatarData;
    private bool _disposed;

    public FurnitureMannequinVisualization()
    {
        s_instanceCount++;
    }

    bool IDisposable.disposed => _disposed;

    public override void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _avatarData = null;
        _disposed = true;

        if (_dynamicAssetName != null && AssetCollection != null)
        {
            AssetCollection.DisposeAsset(_dynamicAssetName);
            _dynamicAssetName = null;
        }

        base.Dispose();

        s_instanceCount--;

        if (s_instanceCount == 0 && s_customPlaceholders != null)
        {
            foreach (IAvatarImage placeholder in s_customPlaceholders.Values)
            {
                placeholder.Dispose();
            }

            s_customPlaceholders = null;
        }
    }

    public override bool Initialize(IRoomObjectVisualizationData? data)
    {
        _avatarData = data as AvatarFurnitureVisualizationData;
        return base.Initialize(data);
    }

    public void AvatarImageReady(string figure)
    {
        if (figure == _figure)
        {
            AddAvatarAsset(true);
        }
    }

    protected override bool UpdateObject(double scale, double geometryDirection)
    {
        bool changed = base.UpdateObject(scale, geometryDirection);

        if (changed)
        {
            if (_size != (int)scale)
            {
                _size = (int)scale;
                AddAvatarAsset();
            }
        }

        return changed;
    }

    protected override bool UpdateModel(double scale)
    {
        bool changed = base.UpdateModel(scale);

        if (changed)
        {
            IRoomObject? obj = Object;

            if (obj != null)
            {
                IRoomObjectModel model = obj.Model;
                string? figure = model.GetString("furniture_mannequin_figure");

                if (!string.IsNullOrEmpty(figure))
                {
                    _gender = model.GetString("furniture_mannequin_gender");
                    _figure = figure + "." + MANNEQUIN_BODY;
                    AddAvatarAsset();
                }
            }
        }

        if (!changed)
        {
            changed = _needsUpdate;
        }

        _needsUpdate = false;
        return changed;
    }

    protected override string? GetSpriteAssetName(int scale, int spriteIndex)
    {
        string tag = GetSpriteTag(scale, Direction, spriteIndex);

        if (_figure != null && tag == AVATAR_IMAGE_SPRITE_TAG && IsAvatarAssetReady())
        {
            return GetAvatarAssetName();
        }

        return base.GetSpriteAssetName(scale, spriteIndex);
    }

    protected override int GetSpriteXOffset(int scale, int direction, int layer)
    {
        string tag = GetSpriteTag(scale, direction, layer);

        if (tag == AVATAR_IMAGE_SPRITE_TAG && IsAvatarAssetReady())
        {
            IRoomObjectSprite? sprite = GetSprite(layer);

            if (sprite != null)
            {
                return -sprite.Width / 2;
            }
        }

        return base.GetSpriteXOffset(scale, direction, layer);
    }

    protected override int GetSpriteYOffset(int scale, int direction, int layer)
    {
        string tag = GetSpriteTag(scale, direction, layer);

        if (tag == AVATAR_IMAGE_SPRITE_TAG && IsAvatarAssetReady())
        {
            IRoomObjectSprite? sprite = GetSprite(layer);

            if (sprite != null)
            {
                return -sprite.Height;
            }
        }

        return base.GetSpriteYOffset(scale, direction, layer);
    }

    private void AddAvatarAsset(bool forceUpdate = false)
    {
        if (_avatarData == null || _figure == null)
        {
            return;
        }

        if (IsAvatarAssetReady() && !forceUpdate)
        {
            return;
        }

        IAvatarImage? avatar = _avatarData.GetAvatar(_figure, _size, _gender, this);

        if (avatar == null)
        {
            return;
        }

        if (avatar.IsPlaceholder())
        {
            avatar.Dispose();
            IAvatarImage? placeholder = GetCustomPlaceholder(_size);

            if (placeholder != null)
            {
                placeholder.SetDirection("full", Direction);
                Image? image = placeholder.GetImage("full", true);

                if (image != null && AssetCollection != null)
                {
                    string assetName = GetAvatarAssetName() ?? "";
                    AssetCollection.AddAsset(assetName, image, true);
                }
            }

            _needsUpdate = true;
            return;
        }

        avatar.SetDirection("full", Direction);

        if (_dynamicAssetName != null && AssetCollection != null)
        {
            AssetCollection.DisposeAsset(_dynamicAssetName);
        }

        string name = GetAvatarAssetName() ?? "";
        Image? avatarImage = avatar.GetImage("full", true);

        if (avatarImage != null && AssetCollection != null)
        {
            AssetCollection.AddAsset(name, avatarImage, true);
        }

        _dynamicAssetName = name;
        _needsUpdate = true;
        avatar.Dispose();
    }

    private IAvatarImage? GetCustomPlaceholder(int size)
    {
        s_customPlaceholders ??= new Dictionary<int, IAvatarImage>();

        if (s_customPlaceholders.TryGetValue(size, out IAvatarImage? cached))
        {
            return cached;
        }

        if (_avatarData == null)
        {
            return null;
        }

        IAvatarImage? placeholder = _avatarData.GetAvatar(MANNEQUIN_BODY, size);
        if (placeholder != null)
        {
            s_customPlaceholders[size] = placeholder;
        }

        return placeholder;
    }

    private bool IsAvatarAssetReady()
    {
        if (_figure == null)
        {
            return false;
        }

        string? name = GetAvatarAssetName();
        return name != null && GetAsset(name) != null;
    }

    private string? GetAvatarAssetName()
    {
        IRoomObject? obj = Object;

        if (obj == null)
        {
            return null;
        }

        return "mannequin_" + _figure + "_" + _size + "_" + Direction + "_" + obj.Id;
    }
}
