using System;
using System.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Habbo.Avatar;
using Vortex.Habbo.Avatar.Animation;
using Vortex.Habbo.Avatar.Enum;
using Vortex.Habbo.Room.Object.Visualization.Avatar.Additions;
using Vortex.Room.Object;
using Vortex.Room.Object.Enum;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Avatar;

/// @see com.sulake.habbo.room.object.visualization.avatar.AvatarVisualization
public class AvatarVisualization : RoomObjectSpriteVisualization, IAvatarImageListener, IAvatarEffectListener
{
    private const string AVATAR_SPRITE_TAG = "avatar";
    private const double AVATAR_SPRITE_DEFAULT_DEPTH = -0.01;
    private const double AVATAR_OWN_DEPTH_ADJUST = 0.001;
    private const double AVATAR_SPRITE_LAYING_DEPTH = -0.409;
    private const int BASE_Y_SCALE = 1000;
    private const int ANIMATION_FRAME_UPDATE_INTERVAL = 2;
    private const int SNOWBOARDING_EFFECT = 97;
    private const int FREEZE_EFFECT = 218;
    private const int MAX_AVATARS_WITH_EFFECT = 3;
    private const int SPRITE_INDEX_AVATAR = 0;
    private const int SHADOW_SPRITE_INDEX = 1;
    private const int INITIAL_RESERVED_SPRITES = 2;
    private const int ADDITION_ID_IDLE_BUBBLE = 1;
    private const int ADDITION_ID_TYPING_BUBBLE = 2;
    private const int ADDITION_ID_EXPRESSION = 3;
    private const int ADDITION_ID_NUMBER_BUBBLE = 4;
    private const int ADDITION_ID_GAME_CLICK_TARGET = 5;
    private const int ADDITION_ID_MUTED_BUBBLE = 6;
    private const int ADDITION_ID_GUIDE_STATUS_BUBBLE = 7;
    private const int ANIMATION_UPDATE_INTERVAL_MS = 41;

    private static readonly double[] DEFAULT_CANVAS_OFFSETS = [0, 0, 0];

    private int _lastAnimationUpdateTime = -1000;
    private AvatarVisualizationData? _data;
    private Dictionary<string, IAvatarImage> _avatars = new();
    private Dictionary<string, IAvatarImage> _effectAvatars = new();
    private int _updatesUntilFrameUpdate;
    private bool _isAnimating = false;
    private string? _figure;
    private string? _gender;
    private int _remainingFrameUpdates;
    private BitmapDataAsset? _shadowAsset;
    private bool _forceUpdate;
    private int _cacheCleanupCounter = (int)(Random.Shared.NextDouble() * 200 + 200);
    private int _modelHeadDirection = -1;
    private int _computedHeadAngle = -1;
    private int _firstAdditionSpriteIndex = INITIAL_RESERVED_SPRITES;
    private Dictionary<int, IAvatarAddition>? _additions;
    private int _cachedGeometryUpdateId = -1;
    private string _postureParameter = "";
    private bool _isTalking;
    private bool _isSleeping;
    private bool _isBlinking;
    private int _expressionId;
    private int _gestureId;
    private int _danceId;
    private int _highlightId;
    private bool _highlightEnabled;
    private bool _variableHolderHighlight;
    private int _signId = -1;
    private int _carryObject;
    private int _useObject;
    private int _geometryOffset;
    private int _verticalOffset;
    private bool _canStandUp;
    private bool _isLaying;
    private bool _isLayingNegative;
    private IAvatarImage? _avatarImage;
    private bool _isOwnUser;
    private int _effectId;
    private double _cachedScale;
    private int _cachedModelUpdateId;
    private int _cachedObjectUpdateId;

    public bool disposed { get; private set; }

    public int Angle { get; private set; } = -1;

    public string Posture { get; private set; } = "";

    protected int NumAdditions => _additions?.Count ?? 0;

    public override void Dispose()
    {
        if (_avatars != null)
        {
            ResetImages();
            _avatars = null!;
            _effectAvatars = null!;
        }

        _data = null;
        _shadowAsset = null;

        if (_additions != null)
        {
            foreach (IAvatarAddition addition in _additions.Values)
            {
                addition.Dispose();
            }

            _additions = null;
        }

        base.Dispose();
        disposed = true;
    }

    public override bool Initialize(IRoomObjectVisualizationData data)
    {
        _data = data as AvatarVisualizationData;
        CreateSprites(INITIAL_RESERVED_SPRITES);
        return true;
    }

    /// @see AvatarVisualization.as::update
    public override void Update(IRoomGeometry? geometry, int time, bool full, bool skip)
    {
        IRoomObject? obj = Object;

        if (obj == null || geometry == null || _data == null)
        {
            return;
        }

        if (--_cacheCleanupCounter <= 0 && _avatarImage != null)
        {
            _avatarImage.DisposeInactiveActionCache();
            _cacheCleanupCounter = 500;
        }

        bool animationTimeTick;
        if (time >= _lastAnimationUpdateTime + ANIMATION_UPDATE_INTERVAL_MS)
        {
            animationTimeTick = true;
            _lastAnimationUpdateTime += ANIMATION_UPDATE_INTERVAL_MS;

            if (_lastAnimationUpdateTime + ANIMATION_UPDATE_INTERVAL_MS < time)
            {
                _lastAnimationUpdateTime = time - ANIMATION_UPDATE_INTERVAL_MS;
            }
        }
        else
        {
            animationTimeTick = false;
        }

        IRoomObjectModel model = obj.Model;
        double scale = geometry.Scale;
        int previousEffect = _effectId;
        bool effectChanged = false;
        bool modelChanged = UpdateModel(model, scale, full);

        if (_forceUpdate)
        {
            ResetImages();
            _forceUpdate = false;
        }

        bool scaleChanged = false;
        bool needsNewImage = false;
        bool objectChanged;

        if (modelChanged || scale != _cachedScale || _avatarImage == null)
        {
            if (scale != _cachedScale)
            {
                scaleChanged = true;
                ValidateActions(scale);
            }

            if (previousEffect != _effectId)
            {
                effectChanged = true;
            }

            if (scaleChanged || _avatarImage == null || effectChanged)
            {
                _avatarImage = GetAvatarImage(scale, _effectId);

                if (_avatarImage == null)
                {
                    return;
                }

                needsNewImage = true;

                IRoomObjectSprite? avatarSprite = GetSprite(SPRITE_INDEX_AVATAR);
                if (avatarSprite != null && _avatarImage != null && _avatarImage.IsPlaceholder())
                {
                    avatarSprite.Alpha = 150;
                }
                else if (avatarSprite != null)
                {
                    avatarSprite.Alpha = 255;
                }
            }

            if (_avatarImage == null)
            {
                return;
            }

            if (effectChanged && _avatarImage.AnimationHasResetOnToggle)
            {
                _avatarImage.ResetAnimationFrameCounter();
            }

            UpdateShadow(scale);
            objectChanged = UpdateObject(obj, geometry, full, true);
            UpdateActions(_avatarImage);

            if (_additions != null)
            {
                int spriteIdx = _firstAdditionSpriteIndex;
                foreach (IAvatarAddition addition in _additions.Values)
                {
                    addition.Update(GetSprite(spriteIdx++)!, scale);
                }
            }

            _cachedScale = scale;
        }
        else
        {
            objectChanged = UpdateObject(obj, geometry, full);
        }

        if (animationTimeTick && _additions != null)
        {
            int spriteIdx = _firstAdditionSpriteIndex;
            foreach (IAvatarAddition addition in _additions.Values)
            {
                if (addition.Animate(GetSprite(spriteIdx++)!))
                {
                    IncreaseUpdateId();
                }
            }
        }

        bool stateChanged = objectChanged || modelChanged || scaleChanged;
        bool shouldAnimate = (_isAnimating || _remainingFrameUpdates > 0) && full && animationTimeTick;

        if (stateChanged)
        {
            _remainingFrameUpdates = ANIMATION_FRAME_UPDATE_INTERVAL;
        }

        if (stateChanged || shouldAnimate)
        {
            IncreaseUpdateId();

            if (animationTimeTick)
            {
                _remainingFrameUpdates--;
                _updatesUntilFrameUpdate--;
            }

            if (!(_updatesUntilFrameUpdate <= 0 || scaleChanged || modelChanged || needsNewImage))
            {
                return;
            }

            _avatarImage!.UpdateAnimationByFrames(1);
            _updatesUntilFrameUpdate = ANIMATION_FRAME_UPDATE_INTERVAL;

            double[] canvasOffsets = _avatarImage.GetCanvasOffsets() ?? DEFAULT_CANVAS_OFFSETS;
            if (canvasOffsets.Length < 3)
            {
                canvasOffsets = DEFAULT_CANVAS_OFFSETS;
            }

            IRoomObjectSprite? mainSprite = GetSprite(SPRITE_INDEX_AVATAR);

            if (mainSprite != null)
            {
                Image? fullImage = _avatarImage.GetImage("full", _highlightId != 0 || _variableHolderHighlight);

                // TODO: Highlight/glow filters (Godot shader equivalent deferred)

                if (fullImage != null)
                {
                    mainSprite.Asset = fullImage;
                }

                if (mainSprite.Asset != null)
                {
                    mainSprite.OffsetX = (int)(-1 * scale / 2 + canvasOffsets[0] - (mainSprite.Asset.GetWidth() - scale) / 2);
                    mainSprite.OffsetY = (int)(-mainSprite.Asset.GetHeight() + scale / 4 + canvasOffsets[1] + _geometryOffset);

                    if (Posture == "swdieback" || Posture == "swdiefront")
                    {
                        mainSprite.OffsetY += (int)(20 * scale / 32);
                    }
                }

                if (_isLaying)
                {
                    if (_isLayingNegative)
                    {
                        mainSprite.RelativeDepth = -0.5;
                    }
                    else
                    {
                        mainSprite.RelativeDepth = AVATAR_SPRITE_LAYING_DEPTH + canvasOffsets[2];
                    }
                }
                else
                {
                    mainSprite.RelativeDepth = AVATAR_SPRITE_DEFAULT_DEPTH + canvasOffsets[2];
                }

                if (_isOwnUser)
                {
                    mainSprite.RelativeDepth -= AVATAR_OWN_DEPTH_ADJUST;
                    mainSprite.SpriteType = RoomObjectSpriteType.AVATAR_OWN;
                }
                else
                {
                    mainSprite.SpriteType = RoomObjectSpriteType.AVATAR;
                }
            }

            if (GetAddition(ADDITION_ID_TYPING_BUBBLE) is TypingBubble typingBubble)
            {
                if (!_isLaying)
                {
                    typingBubble.RelativeDepth = AVATAR_SPRITE_DEFAULT_DEPTH - 0.01 + canvasOffsets[2];
                }
                else
                {
                    typingBubble.RelativeDepth = AVATAR_SPRITE_LAYING_DEPTH - 0.01 + canvasOffsets[2];
                }
            }

            _isAnimating = _avatarImage.IsAnimating();

            int effectSpriteIndex = INITIAL_RESERVED_SPRITES;
            int direction = _avatarImage.GetDirection();

            foreach (ISpriteDataContainer spriteData in _avatarImage.GetSprites())
            {
                if (spriteData.Id == AVATAR_SPRITE_TAG)
                {
                    IRoomObjectSprite? avatarSprite = GetSprite(SPRITE_INDEX_AVATAR);
                    IAnimationLayerData? layerData = _avatarImage.GetLayerData(spriteData);

                    int dirOffsetX = spriteData.GetDirectionOffsetX(direction);
                    int dirOffsetY = spriteData.GetDirectionOffsetY(direction);

                    if (layerData != null)
                    {
                        dirOffsetX += layerData.Dx;
                        dirOffsetY += layerData.Dy;
                    }

                    if (scale < 48)
                    {
                        dirOffsetX /= 2;
                        dirOffsetY /= 2;
                    }

                    if (!_canStandUp && avatarSprite != null)
                    {
                        avatarSprite.OffsetX += dirOffsetX;
                        avatarSprite.OffsetY += dirOffsetY;
                    }
                }
                else
                {
                    IRoomObjectSprite? effectSprite = GetSprite(effectSpriteIndex);

                    if (effectSprite != null)
                    {
                        effectSprite.AlphaTolerance = 256;
                        effectSprite.Visible = true;

                        IAnimationLayerData? layerData = _avatarImage.GetLayerData(spriteData);

                        int animFrame = 0;
                        int dirX = spriteData.GetDirectionOffsetX(direction);
                        int dirY = spriteData.GetDirectionOffsetY(direction);
                        int dirZ = spriteData.GetDirectionOffsetZ(direction);
                        int spriteDir = 0;

                        if (spriteData.HasDirections)
                        {
                            spriteDir = direction;
                        }

                        if (layerData != null)
                        {
                            animFrame = layerData.AnimationFrame;
                            dirX += layerData.Dx;
                            dirY += layerData.Dy;
                            spriteDir += layerData.DirectionOffset;
                        }

                        if (scale < 48)
                        {
                            dirX /= 2;
                            dirY /= 2;
                        }

                        if (spriteDir < 0)
                        {
                            spriteDir += 8;
                        }
                        else if (spriteDir > 7)
                        {
                            spriteDir -= 8;
                        }

                        string assetName = _avatarImage.GetScale() + "_" + spriteData.Member + "_" + spriteDir + "_" + animFrame;
                        BitmapDataAsset? asset = _avatarImage.GetAsset(assetName);
                        int spriteSize = _avatarImage.GetScale() == "sh" ? 32 : 64;
                        bool halfResolution = false;

                        if (asset == null)
                        {
                            if (_avatarImage.GetScale() == "sh")
                            {
                                assetName = "h_" + spriteData.Member + "_" + spriteDir + "_" + animFrame;
                                asset = _avatarImage.GetAsset(assetName);
                                halfResolution = true;
                            }

                            if (asset == null)
                            {
                                effectSpriteIndex++;
                                continue;
                            }
                        }

                        effectSprite.Asset = halfResolution
                            ? AvatarBitmapUtils.ResampleBitmapData(
                                asset.Content as Image ?? Image.CreateEmpty(1, 1, false, Image.Format.Rgba8), 0.5)
                            : asset.Content as Image;

                        int offsetX = halfResolution ? (int)(asset.Offset.X / 2) : (int)asset.Offset.X;
                        int offsetY = halfResolution ? (int)(asset.Offset.Y / 2) : (int)asset.Offset.Y;

                        effectSprite.OffsetX = -offsetX - spriteSize / 2 + dirX;
                        effectSprite.OffsetY = -offsetY + dirY;

                        if (spriteData.HasStaticY)
                        {
                            effectSprite.OffsetY += (int)(_verticalOffset * scale / (2 * BASE_Y_SCALE));
                        }
                        else
                        {
                            effectSprite.OffsetY += _geometryOffset;
                        }

                        if (_isLaying)
                        {
                            effectSprite.RelativeDepth = AVATAR_SPRITE_LAYING_DEPTH - 0.001 * SpriteCount * dirZ;
                        }
                        else
                        {
                            effectSprite.RelativeDepth = AVATAR_SPRITE_DEFAULT_DEPTH - 0.001 * SpriteCount * dirZ;
                        }

                        if (spriteData.Ink == 33)
                        {
                            effectSprite.BlendMode = "add";
                        }
                        else
                        {
                            effectSprite.BlendMode = "normal";
                        }
                    }

                    effectSpriteIndex++;
                }
            }
        }
    }

    /// @see AvatarVisualization.as::getAvatarRendererAsset
    public IAsset? GetAvatarRendererAsset(string name)
    {
        return _data?.GetAvatarRendererAsset(name);
    }

    /// @see IAvatarImageListener
    public void AvatarImageReady(string param1)
    {
        _forceUpdate = true;
    }

    /// @see IAvatarEffectListener
    public void AvatarEffectReady(int param1)
    {
        _forceUpdate = true;
    }

    /// @see AvatarVisualization.as::addAddition
    public IAvatarAddition AddAddition(IAvatarAddition addition)
    {
        _additions ??= new Dictionary<int, IAvatarAddition>();

        if (_additions.ContainsKey(addition.Id))
        {
            throw new InvalidOperationException("Avatar addition with index " + addition.Id + " already exists!");
        }

        _additions[addition.Id] = addition;
        return addition;
    }

    /// @see AvatarVisualization.as::getAddition
    public IAvatarAddition? GetAddition(int id)
    {
        if (_additions == null)
        {
            return null;
        }

        _additions.TryGetValue(id, out IAvatarAddition? addition);
        return addition;
    }

    /// @see AvatarVisualization.as::removeAddition
    public void RemoveAddition(int id)
    {
        IAvatarAddition? addition = GetAddition(id);

        if (addition == null)
        {
            return;
        }

        _additions!.Remove(id);
        addition.Dispose();
    }

    /// @see AvatarVisualization.as::updateModel
    private bool UpdateModel(IRoomObjectModel model, double scale, bool animated)
    {

        if (model.UpdateId == _cachedModelUpdateId)
        {
            return false;
        }

        bool changed = false;

        bool boolVal = model.GetNumber("figure_talk") > 0 && animated;

        if (boolVal != _isTalking)
        {
            _isTalking = boolVal;
            changed = true;
        }

        int intVal = (int)model.GetNumber("figure_expression");

        if (intVal != _expressionId)
        {
            _expressionId = intVal;
            changed = true;
        }

        boolVal = model.GetNumber("figure_sleep") > 0;

        if (boolVal != _isSleeping)
        {
            _isSleeping = boolVal;
            changed = true;
        }

        boolVal = model.GetNumber("figure_blink") > 0 && animated;

        if (boolVal != _isBlinking)
        {
            _isBlinking = boolVal;
            changed = true;
        }

        intVal = (int)model.GetNumber("figure_gesture");

        if (intVal != _gestureId)
        {
            _gestureId = intVal;
            changed = true;
        }

        string? stringVal = model.GetString("figure_posture") ?? "";

        if (stringVal != Posture)
        {
            Posture = stringVal;
            changed = true;
        }

        stringVal = model.GetString("figure_posture_parameter") ?? "";

        if (stringVal != _postureParameter)
        {
            _postureParameter = stringVal;
            changed = true;
        }

        boolVal = model.GetNumber("figure_can_stand_up") > 0;

        if (boolVal != _canStandUp)
        {
            _canStandUp = boolVal;
            changed = true;
        }

        intVal = (int)(model.GetNumber("figure_vertical_offset") * BASE_Y_SCALE);

        if (intVal != _verticalOffset)
        {
            _verticalOffset = intVal;
            changed = true;
        }

        intVal = (int)model.GetNumber("figure_dance");

        if (intVal != _danceId)
        {
            _danceId = intVal;
            changed = true;
        }

        intVal = (int)model.GetNumber("figure_effect");

        if (intVal != _effectId)
        {
            _effectId = intVal;
            changed = true;
        }

        intVal = (int)model.GetNumber("figure_carry_object");

        if (intVal != _carryObject)
        {
            _carryObject = intVal;
            changed = true;
        }

        intVal = (int)model.GetNumber("figure_use_object");

        if (intVal != _useObject)
        {
            _useObject = intVal;
            changed = true;
        }

        intVal = (int)model.GetNumber("head_direction");

        if (intVal != _modelHeadDirection)
        {
            _modelHeadDirection = intVal;
            changed = true;
        }

        if (_carryObject > 0 && model.GetNumber("figure_use_object") > 0)
        {
            if (_useObject != _carryObject)
            {
                _useObject = _carryObject;
                changed = true;
            }
        }
        else if (_useObject != 0)
        {
            _useObject = 0;
            changed = true;
        }

        // Idle Z addition
        IAvatarAddition? addition = GetAddition(ADDITION_ID_IDLE_BUBBLE) as FloatingIdleZ;

        if (_isSleeping)
        {
            if (addition == null)
            {
                AddAddition(new FloatingIdleZ(ADDITION_ID_IDLE_BUBBLE, this));
            }

            changed = true;
        }
        else if (addition != null)
        {
            RemoveAddition(ADDITION_ID_IDLE_BUBBLE);
        }

        // Muted bubble
        boolVal = model.GetNumber("figure_is_muted") > 0;
        addition = GetAddition(ADDITION_ID_MUTED_BUBBLE) as MutedBubble;

        if (boolVal)
        {
            if (addition == null)
            {
                AddAddition(new MutedBubble(ADDITION_ID_MUTED_BUBBLE, this));
            }

            RemoveAddition(ADDITION_ID_TYPING_BUBBLE);

            changed = true;
        }
        else
        {
            if (addition != null)
            {
                RemoveAddition(ADDITION_ID_MUTED_BUBBLE);
                changed = true;
            }

            // Typing bubble (only if not muted)
            boolVal = model.GetNumber("figure_is_typing") > 0;
            addition = GetAddition(ADDITION_ID_TYPING_BUBBLE) as TypingBubble;

            if (boolVal)
            {
                if (addition == null)
                {
                    AddAddition(new TypingBubble(ADDITION_ID_TYPING_BUBBLE, this));
                }

                changed = true;
            }
            else if (addition != null)
            {
                RemoveAddition(ADDITION_ID_TYPING_BUBBLE);
            }
        }

        // Guide status bubble
        intVal = (int)model.GetNumber("figure_guide_status");

        if (intVal != 0)
        {
            RemoveAddition(ADDITION_ID_GUIDE_STATUS_BUBBLE);
            AddAddition(new GuideStatusBubble(ADDITION_ID_GUIDE_STATUS_BUBBLE, this, intVal));
            changed = true;
        }
        else if (GetAddition(ADDITION_ID_GUIDE_STATUS_BUBBLE) is GuideStatusBubble)
        {
            RemoveAddition(ADDITION_ID_GUIDE_STATUS_BUBBLE);
            changed = true;
        }

        // Game click target
        boolVal = model.GetNumber("figure_is_playing_game") > 0;
        addition = GetAddition(ADDITION_ID_GAME_CLICK_TARGET) as GameClickTarget;

        if (boolVal)
        {
            if (addition == null)
            {
                AddAddition(new GameClickTarget(ADDITION_ID_GAME_CLICK_TARGET));
            }

            changed = true;
        }
        else if (addition != null)
        {
            RemoveAddition(ADDITION_ID_GAME_CLICK_TARGET);
        }

        // Number bubble
        intVal = (int)model.GetNumber("figure_number_value");
        addition = GetAddition(ADDITION_ID_NUMBER_BUBBLE) as NumberBubble;

        if (intVal > 0)
        {
            if (addition == null)
            {
                AddAddition(new NumberBubble(ADDITION_ID_NUMBER_BUBBLE, intVal, this));
            }

            changed = true;
        }
        else if (addition != null)
        {
            RemoveAddition(ADDITION_ID_NUMBER_BUBBLE);
        }

        // Expression
        intVal = (int)model.GetNumber("figure_expression");
        addition = GetAddition(ADDITION_ID_EXPRESSION);

        if (intVal > 0)
        {
            if (addition == null)
            {
                IAvatarAddition? expr = ExpressionAdditionFactory.Make(ADDITION_ID_EXPRESSION, intVal, this);

                if (expr != null)
                {
                    AddAddition(expr);
                }
            }
        }
        else if (addition != null)
        {
            RemoveAddition(ADDITION_ID_EXPRESSION);
        }

        ValidateActions(scale);

        stringVal = model.GetString("gender") ?? "";
        if (stringVal != _gender)
        {
            _gender = stringVal;
            changed = true;
        }

        string? figureStr = model.GetString("figure");
        if (UpdateFigure(figureStr))
        {
            changed = true;
        }

        if (model.HasNumber("figure_sign"))
        {
            intVal = (int)model.GetNumber("figure_sign");
            if (intVal != _signId)
            {
                changed = true;
                _signId = intVal;
            }
        }

        boolVal = model.GetNumber("figure_highlight_enable") > 0;
        if (boolVal != _highlightEnabled)
        {
            _highlightEnabled = boolVal;
            changed = true;
        }

        if (_highlightEnabled)
        {
            intVal = (int)model.GetNumber("figure_highlight");
            if (intVal != _highlightId)
            {
                _highlightId = intVal;
                changed = true;
            }
        }

        boolVal = model.GetNumber("figure_highlight_variable_holder") > 0;
        if (boolVal != _variableHolderHighlight)
        {
            _variableHolderHighlight = boolVal;
            changed = true;
        }

        boolVal = model.GetNumber("own_user") > 0;
        if (boolVal != _isOwnUser)
        {
            _isOwnUser = boolVal;
            changed = true;
        }

        _cachedModelUpdateId = model.UpdateId;
        return changed;

    }

    /// @see AvatarVisualization.as::updateFigure
    private bool UpdateFigure(string? figure)
    {
        if (_figure != figure)
        {
            _figure = figure;
            ResetImages();
            return true;
        }

        return false;
    }

    /// @see AvatarVisualization.as::resetImages
    private void ResetImages()
    {
        foreach (IAvatarImage avatar in _avatars.Values)
        {
            avatar.Dispose();
        }

        foreach (IAvatarImage avatar in _effectAvatars.Values)
        {
            avatar.Dispose();
        }

        _avatars.Clear();
        _effectAvatars.Clear();
        _avatarImage = null;

        IRoomObjectSprite? sprite = GetSprite(SPRITE_INDEX_AVATAR);
        if (sprite != null)
        {
            sprite.Asset = null;
            sprite.Alpha = 255;
        }
    }

    /// @see AvatarVisualization.as::validateActions
    private void ValidateActions(double scale)
    {
        if (scale < 48)
        {
            _isBlinking = false;
        }

        if (Posture == "sit" || Posture == "lay")
        {
            _geometryOffset = (int)(scale / 2);
        }
        else
        {
            _geometryOffset = 0;
        }

        _isLayingNegative = false;
        _isLaying = false;

        if (Posture == "lay")
        {
            _isLaying = true;

            if (int.TryParse(_postureParameter, out int paramValue) && paramValue < 0)
            {
                _isLayingNegative = true;
            }
        }
    }

    /// @see AvatarVisualization.as::getAvatarImage
    private IAvatarImage? GetAvatarImage(double scale, int effectId)
    {
        string key = "avatarImage" + scale;

        if (effectId == 0)
        {
            if (_avatars.TryGetValue(key, out IAvatarImage? cached))
            {
                return cached;
            }
        }
        else
        {
            key += "-" + effectId;

            if (_effectAvatars.TryGetValue(key, out IAvatarImage? cached))
            {
                cached.ForceActionUpdate();
                return cached;
            }
        }

        IAvatarImage? avatar = _data!.GetAvatar(_figure, scale, _gender, this, this);

        if (avatar != null)
        {
            if (effectId == 0)
            {
                _avatars[key] = avatar;
            }
            else
            {
                if (_effectAvatars.Count >= MAX_AVATARS_WITH_EFFECT)
                {
                    // Remove oldest entry (LRU eviction)
                    using IEnumerator<KeyValuePair<string, IAvatarImage>> enumerator = _effectAvatars.GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        KeyValuePair<string, IAvatarImage> oldest = enumerator.Current;
                        _effectAvatars.Remove(oldest.Key);
                        oldest.Value.Dispose();
                    }
                }

                _effectAvatars[key] = avatar;
            }
        }

        return avatar;
    }

    /// @see AvatarVisualization.as::updateObject
    private bool UpdateObject(IRoomObject obj, IRoomGeometry geometry, bool animated, bool force = false)
    {
        if (force || _cachedObjectUpdateId != obj.UpdateId || _cachedGeometryUpdateId != geometry.UpdateId)
        {
            bool changed = animated;
            int bodyAngle = ((int)(obj.Direction.X - geometry.Direction.X) % 360 + 360) % 360;

            if (Posture == "sit" && _canStandUp)
            {
                bodyAngle -= bodyAngle % 90 - 45;
            }

            int headAngle = _modelHeadDirection;
            if (Posture == "float")
            {
                headAngle = bodyAngle;
            }
            else
            {
                headAngle -= (int)geometry.Direction.X;
            }

            headAngle = (headAngle % 360 + 360) % 360;

            if ((Posture == "sit" && _canStandUp) || Posture == "swdieback" || Posture == "swdiefront")
            {
                headAngle -= headAngle % 90 - 45;
            }

            if (bodyAngle != Angle || force)
            {
                changed = true;
                Angle = bodyAngle;
                int adjustedBody = (bodyAngle - (int)112.5 + 360) % 360;
                _avatarImage!.SetDirectionAngle("full", adjustedBody);
            }

            if (headAngle != _computedHeadAngle || force)
            {
                changed = true;
                _computedHeadAngle = headAngle;

                if (_computedHeadAngle != Angle)
                {
                    int adjustedHead = (headAngle - (int)112.5 + 360) % 360;
                    _avatarImage!.SetDirectionAngle("head", adjustedHead);
                }
            }

            _cachedObjectUpdateId = obj.UpdateId;
            _cachedGeometryUpdateId = geometry.UpdateId;
            return changed;
        }

        return false;
    }

    /// @see AvatarVisualization.as::updateShadow
    private void UpdateShadow(double scale)
    {
        IRoomObjectSprite? shadowSprite = GetSprite(SHADOW_SPRITE_INDEX);
        _shadowAsset = null;

        bool showShadow = Posture == "mv" || Posture == "std" || (Posture == "sit" && _canStandUp);

        if (_effectId is SNOWBOARDING_EFFECT or FREEZE_EFFECT)
        {
            showShadow = false;
        }

        if (showShadow)
        {
            shadowSprite!.Visible = true;

            int offsetX;
            int offsetY;

            if (scale < 48)
            {
                shadowSprite.LibraryAssetName = "sh_std_sd_1_0_0";
                _shadowAsset = _avatarImage!.GetAsset(shadowSprite.LibraryAssetName);
                offsetX = -8;
                offsetY = _canStandUp ? 6 : -3;
            }
            else
            {
                shadowSprite.LibraryAssetName = "h_std_sd_1_0_0";
                _shadowAsset = _avatarImage!.GetAsset(shadowSprite.LibraryAssetName);
                offsetX = -17;
                offsetY = _canStandUp ? 10 : -7;
            }

            if (_shadowAsset != null)
            {
                shadowSprite.Asset = _shadowAsset.Content as Image;
                shadowSprite.OffsetX = offsetX;
                shadowSprite.OffsetY = offsetY;
                shadowSprite.Alpha = 50;
                shadowSprite.RelativeDepth = 1;
            }
            else
            {
                shadowSprite.Visible = false;
            }
        }
        else
        {
            _shadowAsset = null;
            shadowSprite!.Visible = false;
        }
    }

    /// @see AvatarVisualization.as::updateActions
    private void UpdateActions(IAvatarImage? avatar)
    {
        if (avatar == null)
        {
            return;
        }

        avatar.InitActionAppends();
        avatar.AppendAction("posture", Posture, _postureParameter);

        if (_gestureId > 0)
        {
            avatar.AppendAction("gest", AvatarExpressionEnum.GetGesture(_gestureId));
        }

        if (_danceId > 0)
        {
            avatar.AppendAction("dance", _danceId);
        }

        if (_signId > -1)
        {
            avatar.AppendAction("sign", _signId);
        }

        if (_carryObject > 0)
        {
            avatar.AppendAction("cri", _carryObject);
        }

        if (_useObject > 0)
        {
            avatar.AppendAction("usei", _useObject);
        }

        if (_isTalking)
        {
            avatar.AppendAction("talk");
        }

        if (_isSleeping || _isBlinking)
        {
            avatar.AppendAction("Sleep");
        }

        if (_expressionId > 0)
        {
            string expression = AvatarExpressionEnum.GetExpression(_expressionId);

            if (expression != "")
            {
                if (expression == "dance")
                {
                    avatar.AppendAction("dance", 2);
                }
                else
                {
                    avatar.AppendAction(expression);
                }
            }
        }

        if (_effectId > 0)
        {
            avatar.AppendAction("fx", _effectId);
        }

        avatar.EndActionAppends();
        _isAnimating = avatar.IsAnimating();

        // Count effect sprites (non-avatar sprite layers)
        int totalSprites = INITIAL_RESERVED_SPRITES + _avatarImage!.GetSprites().Count(spriteData => spriteData.Id != AVATAR_SPRITE_TAG);

        if (totalSprites != SpriteCount)
        {
            CreateSprites(totalSprites);
        }

        _firstAdditionSpriteIndex = totalSprites;

        // Add sprites for additions
        if (_additions == null)
        {
            return;
        }

        foreach (IAvatarAddition unused in _additions.Values)
        {
            AddSprite();
        }
    }
}
