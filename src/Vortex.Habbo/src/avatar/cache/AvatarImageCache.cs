// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/cache/AvatarImageCache.as

using System.Diagnostics;
using System.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Habbo.Avatar.Actions;
using Vortex.Habbo.Avatar.Alias;
using Vortex.Habbo.Avatar.Animation;
using Vortex.Habbo.Avatar.Enum;
using Vortex.Habbo.Avatar.Structure;
using Vortex.Habbo.Avatar.Structure.Animation;

namespace Vortex.Habbo.Avatar.Cache;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/cache/AvatarImageCache.as
public class AvatarImageCache
{
    private const int DEFAULT_MAX_CACHE_STORAGE_TIME_MS = 60000;
    private const string PART_FACE = "fc";
    private const string PART_EYES = "ey";
    private const string PART_RIGHT_ITEM = "ri";
    private const string ACTION_WAVE = "wav";
    private const string ACTION_DRINK = "drk";
    private const string ACTION_BLOW = "blw";
    private const string ACTION_SIGN = "sig";
    private const string ACTION_RESPECT = "respect";
    private const string BASE_ACTION = "std";

    private static readonly Stopwatch Timer = Stopwatch.StartNew();

    /// AS3 pre-cached int→string for 0..9999
    private static readonly string[] IntStrings;

    static AvatarImageCache()
    {
        IntStrings = new string[10000];
        for (int i = 0;
             i < 10000;
             i++)
        {
            IntStrings[i] = i.ToString();
        }
    }

    private AvatarStructure? _structure;
    private IAvatarImage? _avatarImage;
    private AssetAliasCollection? _assets;
    private readonly string _scale;
    private Dictionary<string, AvatarImageBodyPartCache>? _cache;
    private AvatarCanvas? _canvas;
    private bool _disposed;
    private string? _geometryType;
    private readonly List<ImageData> _renderList;
    private readonly List<object[]> _serverRenderData;
    private readonly bool _isSmallScale;

    /// @see AvatarImageCache.as::AvatarImageCache
    public AvatarImageCache
    (
        AvatarStructure structure,
        IAvatarImage avatarImage,
        AssetAliasCollection assets,
        string scale,
        bool isSmallScale
    )
    {
        _renderList = [];
        _serverRenderData = [];
        _structure = structure;
        _avatarImage = avatarImage;
        _assets = assets;
        _scale = scale;
        _cache = new Dictionary<string, AvatarImageBodyPartCache>();
        _isSmallScale = isSmallScale;
    }

    /// @see AvatarImageCache.as::dispose
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _structure = null;
        _avatarImage = null;
        _assets = null;

        if (_cache != null)
        {
            foreach (AvatarImageBodyPartCache bodyPartCache in _cache.Values)
            {
                bodyPartCache.Dispose();
            }
            _cache = null;
        }

        _canvas = null;
        _disposed = true;
    }

    /// @see AvatarImageCache.as::disposeInactiveActions
    public void DisposeInactiveActions(int maxAgeMs = DEFAULT_MAX_CACHE_STORAGE_TIME_MS)
    {
        if (_cache == null)
        {
            return;
        }

        long now = Timer.ElapsedMilliseconds;

        foreach (AvatarImageBodyPartCache bodyPartCache in _cache.Values)
        {
            bodyPartCache.DisposeActions(maxAgeMs, now);
        }
    }

    /// @see AvatarImageCache.as::resetBodyPartCache
    public void ResetBodyPartCache(IActiveActionData action)
    {
        if (_cache == null)
        {
            return;
        }

        foreach (AvatarImageBodyPartCache bodyPartCache in _cache.Values)
        {
            bodyPartCache.SetAction(action, 0);
        }
    }

    /// @see AvatarImageCache.as::setDirection
    public void SetDirection(string avatarSet, int direction)
    {
        if (_structure == null)
        {
            return;
        }

        string[] bodyParts = _structure.GetBodyPartsUnordered(avatarSet);

        foreach (string partId in bodyParts)
        {
            AvatarImageBodyPartCache bodyPartCache = GetBodyPartCache(partId);
            bodyPartCache.SetDirection(direction);
        }
    }

    /// @see AvatarImageCache.as::setAction
    public void SetAction(IActiveActionData action, int frameNumber)
    {
        if (_structure == null || _avatarImage == null)
        {
            return;
        }

        long timeMs = Timer.ElapsedMilliseconds;
        string[] bodyPartIds = _structure.GetActiveBodyPartIds(action, _avatarImage);

        foreach (string partId in bodyPartIds)
        {
            AvatarImageBodyPartCache bodyPartCache = GetBodyPartCache(partId);
            bodyPartCache.SetAction(action, timeMs);
        }
    }

    /// @see AvatarImageCache.as::setGeometryType
    public void SetGeometryType(string geometryType)
    {
        if (_geometryType == geometryType)
        {
            return;
        }

        if ((_geometryType == "sitting" && geometryType == "vertical") ||
            (_geometryType == "vertical" && geometryType == "sitting") ||
            _geometryType == "swhorizontal" || geometryType == "swhorizontal")
        {
            _geometryType = geometryType;
            _canvas = null;
            return;
        }

        DisposeInactiveActions(0);
        _geometryType = geometryType;
        _canvas = null;
    }

    /// @see AvatarImageCache.as::getImageContainer
    public AvatarImageBodyPartContainer? GetImageContainer(string bodyPartId, int frameNumber, bool preCache = false)
    {
        if (_structure == null || _avatarImage == null || _assets == null)
        {
            return null;
        }

        AvatarImageBodyPartCache bodyPartCache = GetBodyPartCache(bodyPartId);
        int direction = bodyPartCache.GetDirection();
        int frame = frameNumber;

        IActiveActionData? action = bodyPartCache.GetAction();

        if (action?.Definition == null)
        {
            return null;
        }

        if (action.Definition.StartFromFrameZero)
        {
            frame -= action.StartFrame;
        }

        IActiveActionData? cacheAction = action;
        IList<string> removeData = [];
        Dictionary<string, string> itemsMap = new();
        Vector2I animOffset = Vector2I.Zero;

        if (action.Definition.IsAnimation)
        {
            int adjustedDirection = direction;
            string animKey = action.Definition.State + "." + action.ActionParameter;
            int animFrame = frameNumber - action.StartFrame;

            if (_structure.GetAnimation(animKey) is Animation.Animation anim)
            {
                AnimationLayerData? layerData = anim.GetLayerData(animFrame, bodyPartId, action.OverridingAction);

                if (layerData != null)
                {
                    adjustedDirection = direction + layerData.DirectionOffset;

                    // Wrap direction to 0-7
                    if (layerData.DirectionOffset < 0)
                    {
                        switch (adjustedDirection)
                        {
                            case < 0:
                                adjustedDirection += 8;
                                break;
                            case > 7:
                                adjustedDirection = 8 - adjustedDirection;
                                break;
                        }
                    }
                    else
                    {
                        switch (adjustedDirection)
                        {
                            case < 0:
                                adjustedDirection += 8;
                                break;
                            case > 7:
                                adjustedDirection -= 8;
                                break;
                        }
                    }

                    if (_scale == AvatarScaleType.LARGE)
                    {
                        animOffset = new Vector2I(layerData.Dx, layerData.Dy);
                    }
                    else
                    {
                        animOffset = new Vector2I(layerData.Dx / 2, layerData.Dy / 2);
                    }

                    frame = layerData.AnimationFrame;

                    if (layerData.Action != null)
                    {
                        action = layerData.Action;
                    }

                    switch (layerData.Type)
                    {
                        case AnimationLayerData.TYPE_BODYPART:
                            {
                                if (layerData.Action != null)
                                {
                                    cacheAction = layerData.Action;
                                }
                                direction = adjustedDirection;
                                break;
                            }
                        case AnimationLayerData.TYPE_FX:
                            direction = adjustedDirection;
                            break;
                    }

                    itemsMap = layerData.Items;
                }

                removeData = anim.RemoveData;
            }
        }

        // Get or create action cache
        AvatarImageActionCache? actionCache = bodyPartCache.GetActionCache(cacheAction);
        if (actionCache == null || preCache)
        {
            actionCache = new AvatarImageActionCache();
            bodyPartCache.UpdateActionCache(cacheAction, actionCache);
        }

        // Get or create direction cache
        AvatarImageDirectionCache? directionCache = actionCache.GetDirectionCache(direction);
        if (directionCache == null || preCache)
        {
            List<AvatarImagePartContainer>? parts = _structure.GetParts(
                bodyPartId, _avatarImage.GetFigure(), cacheAction,
                _geometryType!, direction, removeData as List<string> ?? [.. removeData],
                _avatarImage, itemsMap
            );

            if (parts is not List<AvatarImagePartContainer> partList)
            {
                Logger.Warn(
                    $"[AvatarImageCache] GetParts returned null/empty for bodyPart='{bodyPartId}', action='{cacheAction?.Definition?.Id}', dir={direction}, geom='{_geometryType}'");
                return null;
            }

            Logger.Info(
                $"[AvatarImageCache] GetParts for '{bodyPartId}': {partList.Count} parts [{string.Join(",", partList.Select(p => p.PartType + ":" + p.PartId))}]");

            directionCache = new AvatarImageDirectionCache(partList);
            actionCache.UpdateDirectionCache(direction, directionCache);
        }

        // Get or render image container
        AvatarImageBodyPartContainer? imageContainer = directionCache.GetImageContainer(frame);

        if (imageContainer == null || preCache)
        {
            List<AvatarImagePartContainer>? partList = directionCache.GetPartList();
            imageContainer = RenderBodyPart(direction, partList!, frame, action, preCache);

            if (imageContainer == null || preCache)
            {
                return null;
            }

            if (imageContainer.IsCacheable)
            {
                directionCache.UpdateImageContainer(imageContainer, frame);
            }
        }

        // Apply frame body part offset
        (int X, int Y) frameOffset = _structure.GetFrameBodyPartOffset(cacheAction, direction, frame, bodyPartId);
        imageContainer.Offset = animOffset + new Vector2I(frameOffset.X, frameOffset.Y);

        return imageContainer;
    }

    /// @see AvatarImageCache.as::getServerRenderData
    public List<object[]> GetServerRenderData()
    {
        List<object[]> result = new(_serverRenderData);
        _serverRenderData.Clear();
        return result;
    }

    /// @see AvatarImageCache.as::getBodyPartCache
    private AvatarImageBodyPartCache GetBodyPartCache(string bodyPartId)
    {
        if (_cache!.TryGetValue(bodyPartId, out AvatarImageBodyPartCache? cache))
        {
            return cache;
        }

        cache = new AvatarImageBodyPartCache();
        _cache[bodyPartId] = cache;

        return cache;
    }

    /// @see AvatarImageCache.as::renderBodyPart
    private AvatarImageBodyPartContainer? RenderBodyPart
    (
        int direction,
        List<AvatarImagePartContainer> parts,
        int frame,
        IActiveActionData action,
        bool preCache = false
    )
    {
        if (parts.Count == 0)
        {
            return null;
        }

        if (_canvas == null)
        {
            _canvas = _structure!.GetCanvas(_scale, _geometryType!);
            if (_canvas == null)
            {
                return null;
            }
        }

        bool isMirrored = AvatarDirectionAngle.MIRRORED[direction];
        string assetPartDef = action.Definition!.AssetPartDefinition;
        bool allCacheable = true;

        // Render parts back-to-front (reverse order)
        for (int i = parts.Count - 1;
             i >= 0;
             i--)
        {
            AvatarImagePartContainer part = parts[i];

            // Skip face/eyes at direction 7
            if (direction == 7 && part.PartType is PART_FACE or PART_EYES)
            {
                continue;
            }

            // Skip right-item with null partId
            if (part is { PartType: PART_RIGHT_ITEM, PartId: null })
            {
                continue;
            }

            string partType = part.PartType;
            string? partId = part.PartId;

            AnimationFrame? frameDef = part.GetFrameDefinition(frame);
            int frameIndex;

            if (frameDef != null)
            {
                frameIndex = frameDef.Number;
                if (!string.IsNullOrEmpty(frameDef.AssetPartDefinition))
                {
                    assetPartDef = frameDef.AssetPartDefinition;
                }
            }
            else
            {
                frameIndex = part.GetFrameIndex(frame);
            }

            int adjustedDirection = direction;
            bool flipPart = false;

            if (isMirrored)
            {
                flipPart = ShouldFlipPart(assetPartDef, partType);

                if (!flipPart)
                {
                    // Mirror direction
                    adjustedDirection = direction switch
                    {
                        4 => 2,
                        5 => 1,
                        6 => 0,
                        _ => adjustedDirection,
                    };

                    if (part.FlippedPartType != partType)
                    {
                        partType = part.FlippedPartType;
                    }
                }
            }

            // Build asset name: scale_assetPartDef_partType_partId_direction_frame
            string scaleStr = _isSmallScale ? AvatarScaleType.LARGE : _scale;
            string dirStr = adjustedDirection < IntStrings.Length ? IntStrings[adjustedDirection] : adjustedDirection.ToString();
            string frameStr = frameIndex < IntStrings.Length ? IntStrings[frameIndex] : frameIndex.ToString();
            string assetName = scaleStr + "_" + assetPartDef + "_" + partType + "_" + partId + "_" + dirStr + "_" + frameStr;

            // Fallback to std action, frame 0
            if (_assets!.GetAssetByName(assetName) is not BitmapDataAsset asset)
            {
                assetName = scaleStr + "_" + BASE_ACTION + "_" + partType + "_" + partId + "_" + dirStr + "_" + IntStrings[0];
                asset = _assets.GetAssetByName(assetName) as BitmapDataAsset;
            }

            if (asset == null)
            {
                Logger.Info(
                    $"[AvatarImageCache] Missing sprite: {assetName} (partType={partType}, partId={partId}, dir={adjustedDirection}, frame={frameIndex})");
                continue;
            }

            if (asset.Content is not Image bitmap)
            {
                Logger.Info(
                    $"[AvatarImageCache] Asset has no Image content: {assetName} (content={asset.Content?.GetType().Name ?? "null"})");
                allCacheable = false;
                continue;
            }

            // Build color transform
            float[]? colorTransform = null;
            bool hasColor = false;

            if (part is { IsColorable: true, Color: not null })
            {
                colorTransform = [(float)part.Color.RedMultiplier, (float)part.Color.GreenMultiplier, (float)part.Color.BlueMultiplier, 1f];
                hasColor = true;
            }

            if (part.IsBlendable)
            {
                colorTransform ??= [1f, 1f, 1f, 1f];
                colorTransform[3] *= part.BlendAlpha;
                hasColor = true;
            }

            Vector2I offset = new((int)asset.Offset.X, (int)asset.Offset.Y);
            if (flipPart)
            {
                offset = new Vector2I(offset.X + (_scale == AvatarScaleType.LARGE ? 65 : 31), offset.Y);
            }

            Rect2I rect = asset.Rectangle.HasValue
                ? new Rect2I(
                    (int)asset.Rectangle.Value.Position.X, (int)asset.Rectangle.Value.Position.Y,
                    (int)asset.Rectangle.Value.Size.X, (int)asset.Rectangle.Value.Size.Y
                )
                : new Rect2I(0, 0, bitmap.GetWidth(), bitmap.GetHeight());

            _renderList.Add(new ImageData(bitmap, rect, offset, flipPart, hasColor ? colorTransform : null));
        }

        if (_renderList.Count == 0)
        {
            return null;
        }

        // Compose all parts into a single image
        ImageData unionImage = CreateUnionImage(_renderList, isMirrored);

        int canvasBottom = _scale == AvatarScaleType.LARGE ? _canvas.Height - 16 : _canvas.Height - 8;
        Vector2I regPoint = unionImage.RegPoint;

        if (_isSmallScale)
        {
            regPoint = new Vector2I(regPoint.X / 2, regPoint.Y / 2);
        }

        Vector2I position = new(-regPoint.X, canvasBottom - regPoint.Y);

        if (isMirrored && assetPartDef != "lay")
        {
            position = new Vector2I(position.X + (_scale == AvatarScaleType.LARGE ? 67 : 31), position.Y);
        }

        // Clean up render list
        for (int i = _renderList.Count - 1;
             i >= 0;
             i--)
        {
            _renderList[i].Dispose();
        }
        _renderList.Clear();

        Image? resultBitmap = unionImage.Bitmap;

        if (!_isSmallScale || resultBitmap == null)
        {
            return new AvatarImageBodyPartContainer(resultBitmap, position, allCacheable);
        }

        Image resampled = AvatarBitmapUtils.ResampleBitmapData(resultBitmap, 0.5);
        resultBitmap.Dispose();
        resultBitmap = resampled;

        return new AvatarImageBodyPartContainer(resultBitmap, position, allCacheable);
    }

    /// @see AvatarImageCache.as — flip logic extracted for clarity
    private static bool ShouldFlipPart(string assetPartDef, string partType)
    {
        switch (assetPartDef)
        {
            case ACTION_WAVE when partType is "lh" or "ls" or "lc":
            case ACTION_DRINK when partType is "rh" or "rs" or "rc":
            case ACTION_BLOW when partType == "rh":
            case ACTION_SIGN when partType == "lh":
            case ACTION_RESPECT when partType == "lh":
                return true;
        }

        return partType is "ri" or "li" or "cp";
    }

    /// @see AvatarImageCache.as::createUnionImage
    private static ImageData CreateUnionImage(List<ImageData> parts, bool isMirrored)
    {
        // Calculate union rectangle
        Rect2I unionRect = new();
        bool first = true;

        foreach (Rect2I offsetRect in parts.Select(part => part.OffsetRect))
        {
            if (first)
            {
                unionRect = offsetRect;
                first = false;
            }
            else
            {
                unionRect = unionRect.Merge(offsetRect);
            }
        }

        if (unionRect.Size.X <= 0 || unionRect.Size.Y <= 0)
        {
            return new ImageData(null, new Rect2I(), Vector2I.Zero, isMirrored, null);
        }

        Vector2I regPoint = new(-unionRect.Position.X, -unionRect.Position.Y);
        Image? result = Image.CreateEmpty(unionRect.Size.X, unionRect.Size.Y, false, Image.Format.Rgba8);

        foreach (ImageData part in parts)
        {
            if (part.Bitmap == null)
            {
                continue;
            }

            Vector2I drawPos = regPoint - part.RegPoint;

            if (isMirrored)
            {
                drawPos = new Vector2I(result.GetWidth() - (drawPos.X + part.Rect.Size.X), drawPos.Y);
            }

            bool shouldFlip = (isMirrored && !part.FlipH) || (!isMirrored && part.FlipH);

            if (shouldFlip || part.ColorTransform != null)
            {
                // Use per-pixel compositing for flip or color transform
                BlitWithTransform(result, part.Bitmap, part.Rect, drawPos, shouldFlip, part.ColorTransform);
            }
            else
            {
                // Simple blit with alpha
                BlitWithAlpha(result, part.Bitmap, part.Rect, drawPos);
            }
        }

        Rect2I resultRect = new(0, 0, result.GetWidth(), result.GetHeight());
        return new ImageData(result, resultRect, regPoint, isMirrored, null);
    }

    /// Blit source onto dest with alpha compositing (no transform).
    private static void BlitWithAlpha(Image dest, Image src, Rect2I srcRect, Vector2I destPos)
    {
        int srcX0 = srcRect.Position.X;
        int srcY0 = srcRect.Position.Y;
        int w = srcRect.Size.X;
        int h = srcRect.Size.Y;

        for (int y = 0;
             y < h;
             y++)
        {
            int dy = destPos.Y + y;
            if (dy < 0 || dy >= dest.GetHeight())
            {
                continue;
            }

            for (int x = 0;
                 x < w;
                 x++)
            {
                int dx = destPos.X + x;
                if (dx < 0 || dx >= dest.GetWidth())
                {
                    continue;
                }

                Color srcPixel = src.GetPixel(srcX0 + x, srcY0 + y);
                if (srcPixel.A <= 0f)
                {
                    continue;
                }

                if (srcPixel.A >= 1f)
                {
                    dest.SetPixel(dx, dy, srcPixel);
                }
                else
                {
                    Color dstPixel = dest.GetPixel(dx, dy);
                    float outA = srcPixel.A + (dstPixel.A * (1f - srcPixel.A));

                    if (!(outA > 0f))
                    {
                        continue;
                    }

                    float invA = 1f / outA;

                    dest.SetPixel(
                        dx, dy, new Color(
                            ((srcPixel.R * srcPixel.A) + (dstPixel.R * dstPixel.A * (1f - srcPixel.A))) * invA,
                            ((srcPixel.G * srcPixel.A) + (dstPixel.G * dstPixel.A * (1f - srcPixel.A))) * invA,
                            ((srcPixel.B * srcPixel.A) + (dstPixel.B * dstPixel.A * (1f - srcPixel.A))) * invA,
                            outA
                        )
                    );
                }
            }
        }
    }

    /// Blit with horizontal flip and/or color transform.
    private static void BlitWithTransform
    (
        Image dest,
        Image src,
        Rect2I srcRect,
        Vector2I destPos,
        bool flipH,
        float[]? colorTransform
    )
    {
        int srcX0 = srcRect.Position.X;
        int srcY0 = srcRect.Position.Y;
        int w = srcRect.Size.X;
        int h = srcRect.Size.Y;

        float rMul = colorTransform?[0] ?? 1f;
        float gMul = colorTransform?[1] ?? 1f;
        float bMul = colorTransform?[2] ?? 1f;
        float aMul = colorTransform?[3] ?? 1f;

        for (int y = 0;
             y < h;
             y++)
        {
            int dy = destPos.Y + y;
            if (dy < 0 || dy >= dest.GetHeight())
            {
                continue;
            }

            for (int x = 0;
                 x < w;
                 x++)
            {
                int sx = flipH ? (w - 1 - x) : x;
                int dx = destPos.X + x;
                if (dx < 0 || dx >= dest.GetWidth())
                {
                    continue;
                }

                Color srcPixel = src.GetPixel(srcX0 + sx, srcY0 + y);
                if (srcPixel.A <= 0f)
                {
                    continue;
                }

                Color transformed = new(
                    System.Math.Clamp(srcPixel.R * rMul, 0f, 1f),
                    System.Math.Clamp(srcPixel.G * gMul, 0f, 1f),
                    System.Math.Clamp(srcPixel.B * bMul, 0f, 1f),
                    System.Math.Clamp(srcPixel.A * aMul, 0f, 1f)
                );

                switch (transformed.A)
                {
                    case >= 1f:
                        dest.SetPixel(dx, dy, transformed);
                        break;
                    case > 0f:
                        {
                            Color dstPixel = dest.GetPixel(dx, dy);
                            float outA = transformed.A + (dstPixel.A * (1f - transformed.A));

                            if (!(outA > 0f))
                            {
                                continue;
                            }

                            float invA = 1f / outA;
                            dest.SetPixel(
                                dx, dy, new Color(
                                    ((transformed.R * transformed.A) + (dstPixel.R * dstPixel.A * (1f - transformed.A))) * invA,
                                    ((transformed.G * transformed.A) + (dstPixel.G * dstPixel.A * (1f - transformed.A))) * invA,
                                    ((transformed.B * transformed.A) + (dstPixel.B * dstPixel.A * (1f - transformed.A))) * invA,
                                    outA
                                )
                            );
                            break;
                        }
                }
            }
        }
    }
}
