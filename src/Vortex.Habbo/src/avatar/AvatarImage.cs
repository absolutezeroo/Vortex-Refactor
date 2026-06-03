// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/AvatarImage.as

using System.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Habbo.Avatar.Actions;
using Vortex.Habbo.Avatar.Alias;
using Vortex.Habbo.Avatar.Animation;
using Vortex.Habbo.Avatar.Cache;
using Vortex.Habbo.Avatar.Structure;
using Vortex.Habbo.Avatar.Structure.Figure;

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Habbo.Avatar;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/AvatarImage.as
public class AvatarImage : IAvatarImage, IDisposable, IAvatarEffectListener
{
    private const string DEFAULT_ACTION = "Default";
    private const int DEFAULT_DIR = 2;
    private const string DEFAULT_AVATAR_SET = AvatarGeometryType.FULL;

    protected AvatarStructure? _structure;
    protected string _scale;
    protected int _direction;
    protected int _headDirection;
    protected IActiveActionData? _mainAction;
    protected bool _disposed;
    protected double[]? _canvasOffsets;
    protected AssetAliasCollection? _assets;
    protected AvatarImageCache? _cache;
    protected AvatarFigureContainer? _figure;
    protected IAvatarSpriteData? _avatarSpriteData;
    protected IList<IActiveActionData> _actions;
    protected Image? _image;
    protected bool _isCacheFromFullImageCache;

    private readonly IActiveActionData? _defaultAction;
    private int _frameCounter;
    private int _directionOffset;
    private bool _changed;
    private List<ISpriteDataContainer> _sprites;
    private bool _isAnimating;
    private bool _actionsSorted;
    private IList<IActiveActionData>? _sortedActions;
    private string _actionsSortedByKey;
    private string _actionKeyCache;
    private Dictionary<string, Image>? _fullImageCache;
    private bool _useFullImageCache;
    private int _currentEffectId = -1;
    private int _maxFrames;
    private List<string> _bodyPartsCache;
    private int _bodyPartsCacheDirection = -1;
    private string? _bodyPartsCacheGeometryType;
    private string? _bodyPartsCacheSetId;
    private readonly EffectAssetDownloadManager? _effectManager;
    private readonly IAvatarEffectListener? _effectListener;

    /// @see AvatarImage.as::AvatarImage
    public AvatarImage
    (
        AvatarStructure structure,
        AssetAliasCollection assets,
        AvatarFigureContainer? figure,
        string? scale,
        EffectAssetDownloadManager effectManager,
        IAvatarEffectListener? effectListener
    )
    {
        _canvasOffsets =
        [
        ];
        _actions =
        [
        ];
        _bodyPartsCache =
        [
        ];
        _sprites =
        [
        ];

        _changed = true;
        _effectManager = effectManager;
        _structure = structure;
        _assets = assets;
        _scale = scale ?? AvatarScaleType.LARGE;
        _effectListener = effectListener;

        bool isSmallScale = false;
        if (_scale == AvatarScaleType.LARGE + "_50")
        {
            isSmallScale = true;
            _scale = AvatarScaleType.SMALL;
        }

        if (figure == null)
        {
            figure = new AvatarFigureContainer("hr-893-45.hd-180-2.ch-210-66.lg-270-82.sh-300-91.wa-2007-.ri-1-");
            Logger.Info("Using default avatar figure");
        }

        _figure = figure;
        _cache = new AvatarImageCache(_structure, this, _assets, _scale, isSmallScale);

        SetDirection(DEFAULT_AVATAR_SET, DEFAULT_DIR);

        _actions =
        [
        ];
        _defaultAction = new ActiveActionData(AvatarAction.POSTURE_STAND)
        {
            Definition = _structure.GetActionDefinition(DEFAULT_ACTION),
        };

        ResetActions();
        _fullImageCache = new Dictionary<string, Image>();
    }

    /// @see AvatarImage.as::getServerRenderData
    public object[]? GetServerRenderData()
    {
        GetAvatarPartsForCamera(DEFAULT_AVATAR_SET);
        return
        [
            .. _cache!.GetServerRenderData(),
        ];
    }

    /// @see AvatarImage.as::dispose
    public virtual void Dispose()
    {
        if (!_disposed)
        {
            _structure = null;
            _assets = null;
            _mainAction = null;
            _figure = null;
            _avatarSpriteData = null;
            _actions = null!;

            _image?.Dispose();

            if (_cache != null)
            {
                _cache.Dispose();
                _cache = null;
            }

            if (_fullImageCache != null)
            {
                foreach (Image img in _fullImageCache.Values)
                {
                    img?.Dispose();
                }
                _fullImageCache.Clear();
                _fullImageCache = null;
            }

            _image = null;
            _canvasOffsets = null;
            _disposed = true;
        }
    }

    /// @see AvatarImage.as::get disposed
    public bool disposed => _disposed;

    /// @see AvatarImage.as::getFigure
    public IFigureContainer GetFigure()
    {
        return _figure!;
    }

    /// @see AvatarImage.as::getScale
    public string GetScale()
    {
        return _scale;
    }

    /// @see AvatarImage.as::getPartColor
    public IPartColor? GetPartColor(string partType)
    {
        return _structure!.GetPartColor(_figure!, partType);
    }

    /// @see AvatarImage.as::setDirection
    public void SetDirection(string avatarSet, int direction)
    {
        direction += _directionOffset;

        if (direction < 0)
        {
            direction = 7 + (direction + 1);
        }

        if (direction > 7)
        {
            direction -= 8;
        }

        if (_structure!.IsMainAvatarSet(avatarSet))
        {
            _direction = direction;
        }

        if (avatarSet is AvatarGeometryType.HEAD or AvatarGeometryType.FULL)
        {
            if (avatarSet == AvatarGeometryType.HEAD && IsHeadTurnPreventedByAction())
            {
                direction = _direction;
            }
            _headDirection = direction;
        }

        _cache!.SetDirection(avatarSet, direction);
        _changed = true;
    }

    /// @see AvatarImage.as::setDirectionAngle
    public void SetDirectionAngle(string avatarSet, int angle)
    {
        int direction = angle / 45;
        SetDirection(avatarSet, direction);
    }

    /// @see AvatarImage.as::getSprites
    public IList<ISpriteDataContainer> GetSprites()
    {
        return _sprites.ToList<ISpriteDataContainer>();
    }

    /// @see AvatarImage.as::getCanvasOffsets
    public double[]? GetCanvasOffsets()
    {
        return _canvasOffsets;
    }

    /// @see AvatarImage.as::getLayerData
    public IAnimationLayerData? GetLayerData(ISpriteDataContainer spriteDataContainer)
    {
        return _structure!.GetBodyPartData(spriteDataContainer.Animation.Id, _frameCounter, spriteDataContainer.Id);
    }

    /// @see AvatarImage.as::updateAnimationByFrames
    public void UpdateAnimationByFrames(int frames = 1)
    {
        _frameCounter += frames;
        _changed = true;
    }

    /// @see AvatarImage.as::resetAnimationFrameCounter
    public void ResetAnimationFrameCounter()
    {
        _frameCounter = 0;
        _changed = true;
    }

    /// @see AvatarImage.as::getFullImageCacheKey
    private string? GetFullImageCacheKey()
    {
        if (!_useFullImageCache)
        {
            return null;
        }

        if (_sortedActions is
            {
                Count: 1,
            } && _direction == _headDirection)
        {
            if (_mainAction?.ActionType == AvatarAction.POSTURE_STAND)
            {
                return _direction + _actionKeyCache;
            }
            return _direction + _actionKeyCache + (_frameCounter % 4);
        }

        if (_sortedActions is
            {
                Count: 2,
            })
        {
            foreach (IActiveActionData action in _sortedActions)
            {
                if (action.ActionType == AvatarAction.EFFECT)
                {
                    string param = action.ActionParameter;
                    if (param is "33" or "34" or "35" or "36")
                    {
                        return _direction + _actionKeyCache + "0";
                    }
                    if (param is "38" or "39")
                    {
                        int frameIdx = _frameCounter % 11;
                        return _direction + "_" + _headDirection + _actionKeyCache + frameIdx;
                    }
                }
            }
        }

        return null;
    }

    /// @see AvatarImage.as::getBodyParts
    private List<string> GetBodyParts(string setId, string geometryType, int direction)
    {
        if (direction != _bodyPartsCacheDirection || geometryType != _bodyPartsCacheGeometryType || setId != _bodyPartsCacheSetId)
        {
            _bodyPartsCacheDirection = direction;
            _bodyPartsCacheGeometryType = geometryType;
            _bodyPartsCacheSetId = setId;
            _bodyPartsCache = _structure!.GetBodyParts(setId, geometryType, direction);
        }
        return _bodyPartsCache;
    }

    /// @see AvatarImage.as::getAvatarPartsForCamera
    public void GetAvatarPartsForCamera(string avatarSet)
    {
        if (_mainAction == null)
        {
            return;
        }

        AvatarCanvas? canvas = _structure!.GetCanvas(_scale, _mainAction.Definition!.GeometryType);
        if (canvas == null)
        {
            return;
        }

        List<string> bodyParts = GetBodyParts(avatarSet, _mainAction.Definition.GeometryType, _direction);

        for (int i = bodyParts.Count - 1;
             i >= 0;
             i--)
        {
            _cache!.GetImageContainer(bodyParts[i], _frameCounter, true);
        }
    }

    /// @see AvatarImage.as::getImage
    public Image? GetImage(string avatarSet, bool clone, double scaleFactor = 1)
    {
        if (!_changed)
        {
            return _image;
        }

        if (_mainAction == null)
        {
            return null;
        }

        if (!_actionsSorted)
        {
            EndActionAppends();
        }

        // Try full image cache
        string? cacheKey = GetFullImageCacheKey();
        if (cacheKey != null)
        {
            Image? cached = GetFullImage(cacheKey);
            if (cached != null)
            {
                _changed = false;
                if (clone)
                {
                    return (Image)cached.Duplicate();
                }
                _image = cached;
                _isCacheFromFullImageCache = true;
                return _image;
            }
        }

        AvatarCanvas? canvas = _structure!.GetCanvas(_scale, _mainAction.Definition!.GeometryType);
        if (canvas == null)
        {
            return null;
        }

        // Allocate or reuse bitmap
        if (_isCacheFromFullImageCache || _image == null ||
            _image.GetWidth() != canvas.Width || _image.GetHeight() != canvas.Height)
        {
            if (_image != null && !_isCacheFromFullImageCache)
            {
                _image.Dispose();
            }
            _image = Image.CreateEmpty(canvas.Width, canvas.Height, false, Image.Format.Rgba8);
            _isCacheFromFullImageCache = false;
        }

        List<string> bodyParts = GetBodyParts(avatarSet, _mainAction.Definition.GeometryType, _direction);

        Logger.Info(
            $"[AvatarImage] GetImage: avatarSet={avatarSet}, geomType={_mainAction.Definition.GeometryType}, dir={_direction}, bodyParts={bodyParts.Count} [{string.Join(",", bodyParts)}], canvas={canvas.Width}x{canvas.Height}");

        // Clear the image
        _image.Fill(new Color(0, 0, 0, 0));

        bool allCacheable = true;
        int drawnParts = 0;

        for (int i = bodyParts.Count - 1;
             i >= 0;
             i--)
        {
            string partId = bodyParts[i];
            AvatarImageBodyPartContainer? container = _cache!.GetImageContainer(partId, _frameCounter);

            if (container == null)
            {
                Logger.Warn($"[AvatarImage] GetImageContainer returned null for part '{partId}'");
                continue;
            }

            allCacheable &= container.IsCacheable;

            Image? partImage = container.Image;
            Vector2I regPoint = container.RegPoint;

            if (partImage != null)
            {
                Vector2I drawPoint = regPoint + canvas.Offset + canvas.RegPoint;
                BlitWithAlpha(_image, partImage, drawPoint);
                drawnParts++;
            }
            else
            {
                Logger.Warn($"[AvatarImage] Part '{partId}' container.Image is null (regPoint={regPoint})");
            }
        }

        Logger.Info($"[AvatarImage] Composited {drawnParts}/{bodyParts.Count} parts");

        _changed = false;

        // Apply grayscale palette mapping if avatar sprite data is set
        if (_avatarSpriteData != null)
        {
            if (_avatarSpriteData.PaletteIsGrayscale)
            {
                Image grayscale = ConvertToGrayscale(_image);
                _image.Dispose();
                _image = grayscale;
                ApplyPaletteMap(_image, _avatarSpriteData.Reds);
            }
            else
            {
                // AS3: copyChannel(src, src.rect, ZERO, GREEN, ALPHA)
                CopyGreenToAlpha(_image);
            }
        }

        // Cache if all parts were cacheable
        if (cacheKey != null && allCacheable)
        {
            CacheFullImage(cacheKey, (Image)_image.Duplicate());
        }

        if (scaleFactor != 1 && _image != null)
        {
            Image resampled = AvatarBitmapUtils.ResampleBitmapData(_image, scaleFactor);
            if (!_isCacheFromFullImageCache)
            {
                _image.Dispose();
            }
            _image = resampled;
            _isCacheFromFullImageCache = false;
        }

        if (_image != null && clone)
        {
            return (Image)_image.Duplicate();
        }

        return _image;
    }

    /// @see AvatarImage.as::getCroppedImage
    public Image? GetCroppedImage(string avatarSet, double scaleFactor = 1)
    {
        if (_mainAction == null)
        {
            return null;
        }

        if (!_actionsSorted)
        {
            EndActionAppends();
        }

        AvatarCanvas? canvas = _structure!.GetCanvas(_scale, _mainAction.Definition!.GeometryType);
        if (canvas == null)
        {
            return null;
        }

        Image? tempImage = Image.CreateEmpty(canvas.Width, canvas.Height, true, Image.Format.Rgba8);
        tempImage.Fill(new Color(1, 1, 1, 0));

        List<string> bodyParts = _structure.GetBodyParts(avatarSet, _mainAction.Definition.GeometryType, _direction);
        Rect2I? unionRect = null;

        for (int i = bodyParts.Count - 1;
             i >= 0;
             i--)
        {
            string partId = bodyParts[i];
            AvatarImageBodyPartContainer? container = _cache!.GetImageContainer(partId, _frameCounter);

            if (container == null)
            {
                continue;
            }

            Image? partImage = container.Image;
            if (partImage == null)
            {
                tempImage.Dispose();
                return null;
            }

            Vector2I regPoint = container.RegPoint;
            {
                BlitWithAlpha(tempImage, partImage, regPoint);

                Rect2I partRect = new(
                    regPoint.X, regPoint.Y,
                    partImage.GetWidth(), partImage.GetHeight()
                );

                if (unionRect == null)
                {
                    unionRect = partRect;
                }
                else
                {
                    unionRect = unionRect.Value.Merge(partRect);
                }
            }
        }

        if (unionRect == null)
        {
            unionRect = new Rect2I(0, 0, 1, 1);
        }

        Image? cropped = Image.CreateEmpty(unionRect.Value.Size.X, unionRect.Value.Size.Y, true, Image.Format.Rgba8);
        cropped.BlitRect(tempImage, unionRect.Value, Vector2I.Zero);
        tempImage.Dispose();

        if (scaleFactor == 1)
        {
            return cropped;
        }

        Image resampled = AvatarBitmapUtils.ResampleBitmapData(cropped, scaleFactor);
        cropped.Dispose();
        cropped = resampled;

        return cropped;
    }

    /// @see AvatarImage.as::getFullImage
    protected virtual Image? GetFullImage(string key)
    {
        if (_fullImageCache != null && _fullImageCache.TryGetValue(key, out Image? img))
        {
            return img;
        }
        return null;
    }

    /// @see AvatarImage.as::cacheFullImage
    protected virtual void CacheFullImage(string key, Image image)
    {
        if (_fullImageCache == null)
        {
            return;
        }

        if (_fullImageCache.TryGetValue(key, out Image? existing))
        {
            existing.Dispose();
            _fullImageCache.Remove(key);
        }

        _fullImageCache[key] = image;
    }

    /// @see AvatarImage.as::getAsset
    public BitmapDataAsset? GetAsset(string name)
    {
        return _assets!.GetAssetByName(name) as BitmapDataAsset;
    }

    /// @see AvatarImage.as::getDirection
    public int GetDirection()
    {
        return _direction;
    }

    /// @see AvatarImage.as::initActionAppends
    public void InitActionAppends()
    {
        _actions =
        [
        ];
        _actionsSorted = false;
        _actionKeyCache = "";
        _useFullImageCache = false;
    }

    /// @see AvatarImage.as::endActionAppends
    public void EndActionAppends()
    {
        if (SortActions())
        {
            foreach (IActiveActionData action in _sortedActions!)
            {
                if (action.ActionType == AvatarAction.EFFECT && _effectManager != null)
                {
                    int effectId = int.TryParse(action.ActionParameter, out int eid) ? eid : 0;
                    if (!_effectManager.IsReady(effectId))
                    {
                        _effectManager.LoadEffectData(effectId, this);
                    }
                }
            }

            ResetActions();
            SetActionsToParts();
        }
    }

    /// @see AvatarImage.as::appendAction
    public virtual bool AppendAction(string actionType, params object?[] rest)
    {
        string? param = null;
        _actionsSorted = false;

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
                        if (_direction == 0)
                        {
                            SetDirection(AvatarGeometryType.FULL, 4);
                        }
                        else
                        {
                            SetDirection(AvatarGeometryType.FULL, 2);
                        }
                        goto case AvatarAction.POSTURE_WALK; // AS3 fall-through
                    case AvatarAction.POSTURE_WALK:
                        _useFullImageCache = true;
                        goto case AvatarAction.POSTURE_STAND; // AS3 fall-through
                    case AvatarAction.POSTURE_STAND:
                        _useFullImageCache = true;
                        goto case AvatarAction.POSTURE_SWIM; // AS3 fall-through
                    case AvatarAction.POSTURE_SWIM:
                    case AvatarAction.POSTURE_FLOAT:
                    case AvatarAction.POSTURE_SIT:
                    case AvatarAction.POSTURE_SNOWWAR_RUN:
                    case AvatarAction.POSTURE_SNOWWAR_DIE_FRONT:
                    case AvatarAction.POSTURE_SNOWWAR_DIE_BACK:
                    case AvatarAction.POSTURE_SNOWWAR_PICK:
                    case AvatarAction.POSTURE_SNOWWAR_THROW:
                        AddActionData(param!);
                        break;
                    default:
                        Logger.Warn("[AvatarImage] appendAction() >> UNKNOWN POSTURE TYPE: " + param);
                        break;
                }
                break;

            case AvatarAction.GESTURE:
                switch (param)
                {
                    case AvatarAction.GESTURE_ANGRY:
                    case AvatarAction.GESTURE_SAD:
                    case AvatarAction.GESTURE_SMILE:
                    case AvatarAction.GESTURE_SURPRISED:
                        AddActionData(param!);
                        break;
                    default:
                        Logger.Warn("[AvatarImage] appendAction() >> UNKNOWN GESTURE TYPE: " + param);
                        break;
                }
                break;

            case AvatarAction.EFFECT:
                if (param is "33" or "34" or "35" or "36" or "38" or "39")
                {
                    _useFullImageCache = true;
                }
                goto case AvatarAction.DANCE; // AS3 fall-through to shared handler

            case AvatarAction.DANCE:
            case AvatarAction.TALK:
            case AvatarAction.WAVE:
            case AvatarAction.SLEEP:
            case AvatarAction.SIGN:
            case AvatarAction.RESPECT:
            case AvatarAction.BLOW:
            case AvatarAction.LAUGH:
            case AvatarAction.CRY:
            case AvatarAction.IDLE:
            case AvatarAction.SNOWBOARD_OLLIE:
            case AvatarAction.SNOWBOARD_360:
            case AvatarAction.RIDE_JUMP:
                AddActionData(actionType, param ?? "");
                break;

            case AvatarAction.CARRY_OBJECT:
            case AvatarAction.USE_OBJECT:
                ActionDefinition? actionDef = _structure!.GetActionDefinitionWithState(actionType);
                if (actionDef != null && param != null)
                {
                    param = actionDef.GetParameterValue(param);
                }
                AddActionData(actionType, param ?? "");
                break;

            default:
                Logger.Warn("[AvatarImage] appendAction() >> UNKNOWN ACTION TYPE: " + actionType);
                break;
        }

        return true;
    }

    /// @see AvatarImage.as::addActionData
    protected void AddActionData(string actionType, string actionParameter = "")
    {
        _actions ??=
        [
        ];

        for (int i = 0;
             i < _actions.Count;
             i++)
        {
            IActiveActionData existing = _actions[i];
            if (existing.ActionType == actionType && existing.ActionParameter == actionParameter)
            {
                return;
            }
        }

        _actions.Add(new ActiveActionData(actionType, actionParameter, _frameCounter));
    }

    /// @see AvatarImage.as::isAnimating
    public bool IsAnimating()
    {
        return _isAnimating || _maxFrames > 1;
    }

    /// @see AvatarImage.as::resetActions
    private bool ResetActions()
    {
        AnimationHasResetOnToggle = false;
        _isAnimating = false;
        _sprites =
        [
        ];
        _avatarSpriteData = null;
        _directionOffset = 0;

        _structure!.RemoveDynamicItems(this);

        _mainAction = _defaultAction;
        _mainAction!.Definition = _defaultAction!.Definition;

        ResetBodyPartCache(_defaultAction);
        return true;
    }

    /// @see AvatarImage.as::isHeadTurnPreventedByAction
    private bool IsHeadTurnPreventedByAction()
    {
        if (_sortedActions == null)
        {
            return false;
        }

        foreach (IActiveActionData action in _sortedActions)
        {
            ActionDefinition? actionDef = _structure!.GetActionDefinitionWithState(action.ActionType);

            // AS3: skip Sleep action when main action is not "lay"
            if (action.ActionType == AvatarAction.SLEEP && _mainAction != null && _mainAction.ActionType != AvatarAction.POSTURE_LAY)
            {
                continue;
            }

            if (actionDef != null && actionDef.GetPreventHeadTurn(action.ActionParameter))
            {
                return true;
            }
        }

        return false;
    }

    /// @see AvatarImage.as::sortActions
    private bool SortActions()
    {
        bool effectChanged = false;
        bool hasEffect = false;
        string newActionKey = "";

        _actionKeyCache = "";
        _sortedActions = _structure!.SortActions(_actions);
        _maxFrames = _structure.MaxFrames(_sortedActions);

        if (_sortedActions == null)
        {
            _canvasOffsets =
            [
                0, 0, 0,
            ];
            if (_actionsSortedByKey != "")
            {
                _actionsSortedByKey = "";
            }
        }
        else
        {
            _canvasOffsets = _structure.GetCanvasOffsets(_sortedActions, _scale, _direction);

            foreach (IActiveActionData action in _sortedActions)
            {
                _actionKeyCache += action.ActionType + action.ActionParameter;

                if (action.ActionType == AvatarAction.EFFECT)
                {
                    int effectId = int.TryParse(action.ActionParameter, out int eid) ? eid : 0;
                    if (_currentEffectId != effectId)
                    {
                        effectChanged = true;
                    }
                    _currentEffectId = effectId;
                    hasEffect = true;
                }
            }

            if (!hasEffect)
            {
                if (_currentEffectId > -1)
                {
                    effectChanged = true;
                }
                _currentEffectId = -1;
            }

            if (effectChanged)
            {
                _cache!.DisposeInactiveActions(0);
            }

            if (_actionsSortedByKey != _actionKeyCache)
            {
                newActionKey = _actionKeyCache;
                _actionsSortedByKey = _actionKeyCache;
            }
        }

        _actionsSorted = true;
        // AS3 only returns whether the action key changed, NOT the effect
        return newActionKey != "";
    }

    /// @see AvatarImage.as::setActionsToParts
    private void SetActionsToParts()
    {
        if (_sortedActions == null)
        {
            return;
        }

        // Collect all action types
        List<string> actionTypes = new();
        foreach (IActiveActionData action in _sortedActions)
        {
            actionTypes.Add(action.ActionType);
        }

        // First pass: resolve overriding actions
        foreach (IActiveActionData action in _sortedActions)
        {
            if (action?.Definition == null || !action.Definition.IsAnimation)
            {
                continue;
            }

            string animKey = action.Definition.State + "." + action.ActionParameter;
            Animation.Animation? anim = _structure!.GetAnimation(animKey) as Animation.Animation;

            if (anim != null && anim.HasOverriddenActions())
            {
                IList<string>? overriddenNames = anim.OverriddenActionNames();
                if (overriddenNames != null)
                {
                    foreach (string overriddenAction in overriddenNames)
                    {
                        if (actionTypes.Contains(overriddenAction))
                        {
                            action.OverridingAction = anim.OverridingAction(overriddenAction);
                        }
                    }
                }
            }

            if (anim is
                {
                    ResetOnToggle: true,
                })
            {
                AnimationHasResetOnToggle = true;
            }
        }

        // Second pass: set actions and collect animation data
        foreach (IActiveActionData action in _sortedActions)
        {
            if (action?.Definition == null)
            {
                continue;
            }

            if (action.Definition.IsAnimation && action.ActionParameter == "")
            {
                action.ActionParameter = "1";
            }

            SetActionToParts(action);

            if (action.Definition.IsAnimation)
            {
                _isAnimating = action.Definition.IsAnimated(action.ActionParameter);

                string animKey = action.Definition.State + "." + action.ActionParameter;

                if (_structure!.GetAnimation(animKey) is Animation.Animation anim)
                {
                    IList<ISpriteDataContainer>? spriteData = anim.SpriteData;
                    if (spriteData != null)
                    {
                        _sprites.AddRange(spriteData);
                    }

                    if (anim.HasDirectionData())
                    {
                        _directionOffset = anim.DirectionData!.Offset;
                    }

                    if (anim.HasAvatarData())
                    {
                        _avatarSpriteData = anim.AvatarData;
                    }
                }
            }
        }
    }

    /// @see AvatarImage.as::setActionToParts
    private void SetActionToParts(IActiveActionData action)
    {
        if (action?.Definition == null)
        {
            return;
        }

        if (action.Definition.AssetPartDefinition == "")
        {
            return;
        }

        if (action.Definition.IsMain)
        {
            _mainAction = action;
            _cache!.SetGeometryType(action.Definition.GeometryType);
        }

        _cache!.SetAction(action, _frameCounter);
        _changed = true;
    }

    /// @see AvatarImage.as::resetBodyPartCache
    private void ResetBodyPartCache(IActiveActionData action)
    {
        if (action?.Definition == null)
        {
            return;
        }

        if (action.Definition.AssetPartDefinition == "")
        {
            return;
        }

        if (action.Definition.IsMain)
        {
            _mainAction = action;
            _cache!.SetGeometryType(action.Definition.GeometryType);
        }

        _cache!.ResetBodyPartCache(action);
        _changed = true;
    }

    /// @see AvatarImage.as::get avatarSpriteData
    public IAvatarSpriteData? AvatarSpriteData => _avatarSpriteData;

    /// @see AvatarImage.as::isPlaceholder
    public virtual bool IsPlaceholder()
    {
        return false;
    }

    /// @see AvatarImage.as::forceActionUpdate
    public void ForceActionUpdate()
    {
        _actionsSortedByKey = "";
    }

    /// @see AvatarImage.as::get animationHasResetOnToggle
    public bool AnimationHasResetOnToggle { get; private set; }

    /// @see AvatarImage.as::get mainAction
    public string? MainAction => _mainAction?.ActionType;

    /// @see AvatarImage.as::disposeInactiveActionCache
    public void DisposeInactiveActionCache()
    {
        _cache?.DisposeInactiveActions();
    }

    /// @see AvatarImage.as::avatarEffectReady
    public void AvatarEffectReady(int effectId)
    {
        if (effectId == _currentEffectId)
        {
            ResetActions();
            SetActionsToParts();
            AnimationHasResetOnToggle = true;
            _changed = true;

            if (_effectListener is { disposed: false })
            {
                _effectListener.AvatarEffectReady(effectId);
            }
        }
    }

    /// @see AvatarImage.as::convertToGrayscale
    private static Image ConvertToGrayscale(Image source, string mode = "CHANNELS_EQUAL")
    {
        float rWeight = 0.33f;
        float gWeight = 0.33f;
        float bWeight = 0.33f;

        switch (mode)
        {
            case "CHANNELS_UNIQUE":
                rWeight = 0.3f;
                gWeight = 0.59f;
                bWeight = 0.11f;
                break;
            case "CHANNELS_RED":
                rWeight = 1f;
                gWeight = 0f;
                bWeight = 0f;
                break;
            case "CHANNELS_GREEN":
                rWeight = 0f;
                gWeight = 1f;
                bWeight = 0f;
                break;
            case "CHANNELS_BLUE":
                rWeight = 0f;
                gWeight = 0f;
                bWeight = 1f;
                break;
            case "CHANNELS_DESATURATED":
                rWeight = 0.3086f;
                gWeight = 0.6094f;
                bWeight = 0.082f;
                break;
        }

        Image? result = Image.CreateEmpty(source.GetWidth(), source.GetHeight(), source.HasMipmaps(), Image.Format.Rgba8);

        for (int y = 0;
             y < source.GetHeight();
             y++)
        {
            for (int x = 0;
                 x < source.GetWidth();
                 x++)
            {
                Color pixel = source.GetPixel(x, y);
                float gray = (pixel.R * rWeight) + (pixel.G * gWeight) + (pixel.B * bWeight);
                result.SetPixel(x, y, new Color(gray, gray, gray, pixel.A));
            }
        }

        return result;
    }

    /// Godot adaptation: applies palette map (AS3 paletteMap with reds array).
    /// Maps grayscale intensity (0-255) → palette color.
    private static void ApplyPaletteMap(Image image, int[] reds)
    {
        if (reds.Length < 256)
        {
            return;
        }

        for (int y = 0;
             y < image.GetHeight();
             y++)
        {
            for (int x = 0;
                 x < image.GetWidth();
                 x++)
            {
                Color pixel = image.GetPixel(x, y);
                if (pixel.A <= 0f)
                {
                    continue;
                }

                int index = System.Math.Clamp((int)(pixel.R * 255f), 0, 255);
                uint argb = (uint)reds[index];

                float a = ((argb >> 24) & 0xFF) / 255f;
                float r = ((argb >> 16) & 0xFF) / 255f;
                float g = ((argb >> 8) & 0xFF) / 255f;
                float b = (argb & 0xFF) / 255f;

                image.SetPixel(x, y, new Color(r, g, b, pixel.A * a));

            }
        }
    }

    /// Godot adaptation: copies green channel to alpha channel (AS3 copyChannel GREEN→ALPHA).
    private static void CopyGreenToAlpha(Image image)
    {
        for (int y = 0;
             y < image.GetHeight();
             y++)
        {
            for (int x = 0;
                 x < image.GetWidth();
                 x++)
            {
                Color pixel = image.GetPixel(x, y);
                image.SetPixel(x, y, new Color(pixel.R, pixel.G, pixel.B, pixel.G));
            }
        }
    }

    /// Blit source image onto destination with alpha compositing.
    private static void BlitWithAlpha(Image dest, Image src, Vector2I destPos)
    {
        int w = src.GetWidth();
        int h = src.GetHeight();

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

                Color srcPixel = src.GetPixel(x, y);

                switch (srcPixel.A)
                {
                    case <= 0f:
                        continue;
                    case >= 1f:
                        dest.SetPixel(dx, dy, srcPixel);
                        break;
                    default:
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
                            break;
                        }
                }

            }
        }
    }
}
