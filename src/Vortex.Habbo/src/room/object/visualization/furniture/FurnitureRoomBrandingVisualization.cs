using System;

using Godot;

using Vortex.Room.Object;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Object.Visualization.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureRoomBrandingVisualization
public class FurnitureRoomBrandingVisualization : FurnitureVisualization
{
    private const string BRANDED_IMAGE_SPRITE_TAG = "branded_image";
    private const int OBJECT_STATE_DEFAULT = 0;
    private const int OBJECT_STATE_FLIPH = 1;
    private const int OBJECT_STATE_FLIP_BOTH = 2;
    private const int OBJECT_STATE_FLIPV = 3;

    protected string? _imageUrl;
    protected bool _imageReady;
    protected int _brandingOffsetX;
    protected int _brandingOffsetY;
    protected int _brandingOffsetZ;
    private string? _dynamicAssetName;

    public override void Dispose()
    {
        if (_dynamicAssetName != null && AssetCollection != null)
        {
            AssetCollection.DisposeAsset(_dynamicAssetName);
            _dynamicAssetName = null;
        }

        base.Dispose();
        _imageUrl = null;
    }

    public virtual void SetExternalBaseUrls(string baseUrl, string extraDataUrl, bool useExtraData)
    {
    }

    protected override bool UpdateObject(double scale, double geometryDirection)
    {
        if (base.UpdateObject(scale, geometryDirection))
        {
            if (_imageReady)
            {
                CheckAndCreateImageForCurrentState((int)scale);
            }

            return true;
        }

        return false;
    }

    protected override bool UpdateModel(double scale)
    {
        IRoomObject? obj = Object;
        bool changed = base.UpdateModel(scale);

        if (changed && obj != null)
        {
            IRoomObjectModel model = obj.Model;
            _brandingOffsetX = (int)model.GetNumber("furniture_branding_offset_x");
            _brandingOffsetY = (int)model.GetNumber("furniture_branding_offset_y");
            _brandingOffsetZ = (int)model.GetNumber("furniture_branding_offset_z");
        }

        if (!_imageReady)
        {
            _imageReady = CheckIfImageReady();

            if (_imageReady)
            {
                CheckAndCreateImageForCurrentState((int)scale);
                return true;
            }
        }
        else if (CheckIfImageChanged())
        {
            _imageReady = false;
            _imageUrl = null;
            return true;
        }

        return changed;
    }

    protected override string? GetSpriteAssetName(int scale, int spriteIndex)
    {
        int size = GetSize(scale);
        string layerCode;

        if (spriteIndex < SpriteCount - 1)
        {
            layerCode = FurnitureVisualizationData.LAYER_NAMES[spriteIndex];
        }
        else
        {
            layerCode = "sd";
        }

        string baseName;

        if (size == 1)
        {
            baseName = Type + "_icon_" + layerCode;
        }
        else
        {
            int frame = GetFrameNumber(scale, spriteIndex);
            baseName = Type + "_" + size + "_" + layerCode + "_" + Direction + "_" + frame;
        }

        string tag = GetSpriteTag(scale, Direction, spriteIndex);

        if (_imageUrl != null && tag == BRANDED_IMAGE_SPRITE_TAG)
        {
            return _imageUrl + "_" + size + "_" + Object?.GetState(0);
        }

        return baseName;
    }

    protected override string? GetLibraryAssetNameForSprite(IGraphicAsset asset, IRoomObjectSprite sprite)
    {
        if (sprite.Tag != BRANDED_IMAGE_SPRITE_TAG)
        {
            return base.GetLibraryAssetNameForSprite(asset, sprite);
        }

        string? url = Object?.Model.GetString("furniture_branding_image_url");

        if (!string.IsNullOrEmpty(url))
        {
            return url;
        }

        return base.GetLibraryAssetNameForSprite(asset, sprite);
    }

    protected virtual string? GetThumbnailUrl()
    {
        return null;
    }

    protected virtual bool CheckIfImageChanged()
    {
        IRoomObject? obj = Object;

        if (obj == null)
        {
            return false;
        }

        string? url = obj.Model.GetString("furniture_branding_image_url");

        return url != null && url != _imageUrl;
    }

    protected virtual bool CheckIfImageReady()
    {
        string? thumbnailUrl = GetThumbnailUrl();

        if (thumbnailUrl != null)
        {
            return CheckThumbnailAsset(thumbnailUrl);
        }

        return CheckBrandingAsset();
    }

    protected virtual void ImageReady(Image? image, string url)
    {
        Logger.Debug("billboard visualization got image from url = " + url);

        _imageUrl = image != null ? url : null;
    }

    protected string GetFullThumbnailAssetName(int direction, int size)
    {
        return (_imageUrl ?? "") + "_" + size + "_" + (Object?.GetState(0) ?? 0);
    }

    private bool CheckBrandingAsset()
    {
        IRoomObject? obj = Object;

        if (obj == null)
        {
            return false;
        }

        IRoomObjectModel model = obj.Model;
        string? url = model.GetString("furniture_branding_image_url");

        if (url == null)
        {
            return false;
        }

        if (_imageUrl != null && _imageUrl == url)
        {
            return false;
        }

        double status = model.GetNumber("furniture_branding_image_status");

        if (status != 1)
        {
            return false;
        }

        IGraphicAsset? asset = AssetCollection?.GetAsset(url);

        if (asset?.Asset?.Content is not Image image)
        {
            return false;
        }

        ImageReady(image, url);

        return true;

    }

    private bool CheckThumbnailAsset(string url)
    {
        if (_imageUrl != null && _imageUrl == url)
        {
            return false;
        }

        IGraphicAsset? asset = AssetCollection?.GetAsset(url);

        if (asset?.Asset?.Content is not Image image)
        {
            return false;
        }

        ImageReady(image, url);

        return true;

    }

    private void CheckAndCreateImageForCurrentState(int scale)
    {
        IRoomObject? obj = Object;

        if (obj == null || _imageUrl == null || AssetCollection == null)
        {
            return;
        }

        IGraphicAsset? sourceAsset = AssetCollection.GetAsset(_imageUrl);

        if (sourceAsset == null)
        {
            return;
        }

        int state = obj.GetState(0);
        int size = GetSize(scale);
        string assetName = _imageUrl + "_" + size + "_" + state;

        if (AssetCollection.GetAsset(assetName) != null)
        {
            return;
        }

        if (sourceAsset.Asset?.Content is not Image sourceImage)
        {
            Logger.Debug("could not find bitmap data for image " + assetName);
            return;
        }

        bool doScale = !_imageUrl.Contains("noscale");

        if (_imageUrl.Contains("force32"))
        {
            size = 32;
        }

        Image processed;

        if (size == 32 && doScale)
        {
            int halfW = sourceImage.GetWidth() / 2;
            int halfH = sourceImage.GetHeight() / 2;
            processed = (Image)sourceImage.Duplicate();
            processed.Resize(halfW, halfH);
        }
        else
        {
            processed = (Image)sourceImage.Duplicate();
        }

        int offsetX = 0;
        int offsetY = 0;
        bool flipH = false;
        bool flipV = false;

        switch (state)
        {
            case OBJECT_STATE_DEFAULT:
                break;
            case OBJECT_STATE_FLIPH:
                offsetX = -processed.GetWidth();
                flipH = true;
                break;
            case OBJECT_STATE_FLIP_BOTH:
                offsetX = -processed.GetWidth();
                offsetY = -processed.GetHeight();
                flipH = true;
                flipV = true;
                break;
            case OBJECT_STATE_FLIPV:
                offsetY = -processed.GetHeight();
                flipV = true;
                break;
            default:
                Logger.Debug("could not handle unknown state " + state);
                break;
        }

        if (_dynamicAssetName != null)
        {
            AssetCollection.DisposeAsset(_dynamicAssetName);
        }

        _dynamicAssetName = assetName;

        if (!AssetCollection.AddAsset(assetName, processed, true, offsetX, offsetY, flipH, flipV))
        {
            Logger.Debug("could not add asset for image " + assetName);
        }
    }
}
