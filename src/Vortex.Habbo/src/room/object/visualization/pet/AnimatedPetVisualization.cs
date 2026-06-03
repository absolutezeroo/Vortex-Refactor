using System;

using Godot;

using Vortex.Core.Assets;
using Vortex.Habbo.Room.Object.Visualization.Data;
using Vortex.Habbo.Room.Object.Visualization.Furniture;
using Vortex.Room.Object;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Object.Visualization.Utils;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Pet;

/// @see com.sulake.habbo.room.object.visualization.pet.AnimatedPetVisualization
public class AnimatedPetVisualization : AnimatedFurnitureVisualization
{
    private const string HEAD_SPRITE_TAG = "head";
    private const string SADDLE_SPRITE_TAG = "saddle";
    private const string HAIR_SPRITE_TAG = "hair";
    private const int ADDITIONAL_SPRITE_COUNT = 1;
    private const int EXPERIENCE_BUBBLE_VISIBLE_IN_MS = 1000;
    private const string EXPERIENCE_BUBBLE_ASSET_NAME = "pet_experience_bubble_png";
    private const int POSTURE_ANIMATION_INDEX = 0;
    private const int GESTURE_ANIMATION_INDEX = 1;

    private string _posture = "";
    private string? _gesture = "";
    private bool _isSleeping;
    private int _headDirection;
    private ExperienceData? _experienceData;
    private int _experienceTimestamp;
    private int _gainedExperience;
    private AnimatedPetVisualizationData? _petData;
    private string _paletteName = "";
    private int _paletteIndex = -1;
    private List<int> _customLayerIds = [];
    private List<int> _customPartIds = [];
    private List<int> _customPaletteIds = [];
    private int _color = 0xFFFFFF;
    private bool _headOnly;
    private bool _isRiding;
    private List<AnimationStateData> _animationStates = [];
    private bool _allAnimationsOver;
    private List<bool?> _headSprites = [];
    private List<bool?> _nonHeadSprites = [];
    private List<bool?> _saddleSprites = [];
    private int _cachedRawDirection = -1;

    public AnimatedPetVisualization()
    {
        while (_animationStates.Count < 2)
        {
            _animationStates.Add(new AnimationStateData());
        }
    }

    /// @see AnimatedPetVisualization.as::set direction
    protected new int Direction
    {
        get => base.Direction;
        set
        {
            if (base.Direction != value)
            {
                base.Direction = value;
                _forceFrameUpdate = true;
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        if (_animationStates != null)
        {
            foreach (AnimationStateData state in _animationStates)
            {
                state?.Dispose();
            }

            _animationStates = null!;
        }

        if (_experienceData != null)
        {
            _experienceData.Dispose();
            _experienceData = null;
        }
    }

    /// @see AnimatedPetVisualization.as::getAnimationId
    protected override int GetAnimationId(AnimationStateData animState)
    {
        return animState.AnimationId;
    }

    /// @see AnimatedPetVisualization.as::initialize
    public override bool Initialize(IRoomObjectVisualizationData data)
    {
        if (data is not AnimatedPetVisualizationData petData)
        {
            return false;
        }

        _petData = petData;

        if (_petData.CommonAssets != null)
        {
            BitmapDataAsset? bubbleAsset = _petData.CommonAssets.GetAssetByName(EXPERIENCE_BUBBLE_ASSET_NAME) as BitmapDataAsset;

            if (bubbleAsset != null)
            {
                Image? bitmapClone = (Image?)(bubbleAsset.Content as Image)?.Duplicate();

                if (bitmapClone != null)
                {
                    _experienceData = new ExperienceData(bitmapClone);
                }
            }
        }

        return base.Initialize(data);
    }

    /// @see AnimatedPetVisualization.as::update
    public override void Update(IRoomGeometry geometry, int time, bool full, bool skip)
    {
        base.Update(geometry, time, full, skip);
        UpdateExperienceBubble(time);
    }

    /// @see AnimatedPetVisualization.as::updateAnimation
    protected override int UpdateAnimation(double scale)
    {
        IRoomObject? obj = Object;

        if (obj != null)
        {
            int rawDirection = (int)obj.Direction.X;

            if (rawDirection != _cachedRawDirection)
            {
                _cachedRawDirection = rawDirection;
                ResetAllAnimationFrames();
            }
        }

        return base.UpdateAnimation(scale);
    }

    /// @see AnimatedPetVisualization.as::updateModel
    protected override bool UpdateModel(double scale)
    {
        IRoomObject? obj = Object;

        if (obj == null)
        {
            return false;
        }

        IRoomObjectModel model = obj.Model;

        if (model.UpdateId != _previousDirection)
        {
            string? postureStr = model.GetString("figure_posture");
            string? gestureStr = model.GetString("figure_gesture");

            double postureNum = model.GetNumber("figure_posture");
            if (!double.IsNaN(postureNum))
            {
                int postureCount = _petData!.GetPostureCount(_previousScale);

                if (postureCount > 0)
                {
                    postureStr = _petData.GetPostureForAnimation(_previousScale, (int)postureNum % postureCount, true);
                    gestureStr = null;
                }
            }

            double gestureNum = model.GetNumber("figure_gesture");
            if (!double.IsNaN(gestureNum))
            {
                int gestureCount = _petData!.GetGestureCount(_previousScale);

                if (gestureCount > 0)
                {
                    gestureStr = _petData.GetGestureForAnimation(_previousScale, (int)gestureNum % gestureCount);
                }
            }

            ValidateActions(postureStr, gestureStr);

            double alphaMultiplier = model.GetNumber("furniture_alpha_multiplier");
            if (double.IsNaN(alphaMultiplier))
            {
                alphaMultiplier = 1;
            }

            if (alphaMultiplier != _alphaMultiplier)
            {
                _alphaMultiplier = alphaMultiplier;
                _alphaChanged = true;
            }

            _isSleeping = model.GetNumber("figure_sleep") > 0;

            double headDir = model.GetNumber("head_direction");
            if (!double.IsNaN(headDir) && _petData!.IsAllowedToTurnHead)
            {
                _headDirection = (int)headDir;
            }
            else
            {
                _headDirection = (int)obj.Direction.X;
            }

            _experienceTimestamp = (int)model.GetNumber("figure_experience_timestamp");
            _gainedExperience = (int)model.GetNumber("figure_gained_experience");

            int paletteIndex = (int)model.GetNumber("pet_palette_index");
            if (paletteIndex != _paletteIndex)
            {
                _paletteIndex = paletteIndex;
                _paletteName = _paletteIndex.ToString();
            }

            List<int>? layerIds = model.GetNumberArray("pet_custom_layer_ids");
            _customLayerIds = layerIds ?? [];

            List<int>? partIds = model.GetNumberArray("pet_custom_part_ids");
            _customPartIds = partIds ?? [];

            List<int>? paletteIds = model.GetNumberArray("pet_custom_palette_ids");
            _customPaletteIds = paletteIds ?? [];

            int ridingVal = (int)model.GetNumber("pet_is_riding");
            _isRiding = !double.IsNaN(ridingVal) && ridingVal > 0;

            double colorVal = model.GetNumber("pet_color");
            if (!double.IsNaN(colorVal) && (int)colorVal != _color)
            {
                _color = (int)colorVal;
            }

            _headOnly = model.GetNumber("pet_head_only") > 0;

            _previousDirection = model.UpdateId;
            return true;
        }

        return false;
    }

    /// @see AnimatedPetVisualization.as::updateLayerCount
    protected override void UpdateLayerCount(int count)
    {
        base.UpdateLayerCount(count);
        _headSprites = [];
    }

    /// @see AnimatedPetVisualization.as::getAdditionalSpriteCount
    protected override int GetAdditionalSpriteCount()
    {
        return base.GetAdditionalSpriteCount() + ADDITIONAL_SPRITE_COUNT;
    }

    /// @see AnimatedPetVisualization.as::setAnimation (no-op override)
    protected override void SetAnimation(int animationId)
    {
    }

    /// @see AnimatedPetVisualization.as::resetAllAnimationFrames
    protected new void ResetAllAnimationFrames()
    {
        _allAnimationsOver = false;

        for (int i = _animationStates.Count - 1; i >= 0; i--)
        {
            AnimationStateData? state = _animationStates[i];
            state?.SetLayerCount(AnimatedLayerCount);
        }
    }

    /// @see AnimatedPetVisualization.as::updateAnimations
    protected new int UpdateAnimations(double scale)
    {
        if (_allAnimationsOver)
        {
            return 0;
        }

        bool allDone = true;
        int result = 0;

        for (int i = 0; i < _animationStates.Count; i++)
        {
            AnimationStateData? state = _animationStates[i];

            if (state != null)
            {
                if (!state.AnimationOver)
                {
                    int updated = UpdateFramesForAnimation(state, scale);
                    result |= updated;

                    if (!state.AnimationOver)
                    {
                        allDone = false;
                    }
                    else if (AnimationData.IsTransitionFromAnimation(state.AnimationId)
                             || AnimationData.IsTransitionToAnimation(state.AnimationId))
                    {
                        SetAnimationForIndex(i, state.AnimationAfterTransitionId);
                        allDone = false;
                    }
                }
            }
        }

        _allAnimationsOver = allDone;
        return result;
    }

    /// @see AnimatedPetVisualization.as::getFrameNumber
    protected override int GetFrameNumber(int scale, int layer)
    {
        for (int i = _animationStates.Count - 1; i >= 0; i--)
        {
            AnimationStateData? state = _animationStates[i];

            if (state != null)
            {
                AnimationFrame? frame = state.GetFrame(layer);

                if (frame != null)
                {
                    return frame.Id;
                }
            }
        }

        return base.GetFrameNumber(scale, layer);
    }

    /// @see AnimatedPetVisualization.as::getPostureForAssetFile
    protected override string? GetPostureForAssetFile(int scale, string? assetName)
    {
        if (assetName == null)
        {
            return null;
        }

        string[] parts = assetName.Split('_');
        int animIndex = parts.Length;

        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == "64" || parts[i] == "32")
            {
                animIndex = i + 3;
                break;
            }
        }

        string? posture = null;

        if (animIndex < parts.Length)
        {
            string rawAnimId = parts[animIndex].Split('@')[0];

            if (int.TryParse(rawAnimId, out int animId))
            {
                posture = _petData?.GetPostureForAnimation(scale, animId / 100, false);

                if (posture == null)
                {
                    posture = _petData?.GetGestureForAnimationId(scale, animId / 100);
                }
            }
        }

        return posture;
    }

    /// @see AnimatedPetVisualization.as::getSpriteXOffset
    protected override int GetSpriteXOffset(int scale, int direction, int layer)
    {
        int offset = base.GetSpriteXOffset(scale, direction, layer);

        for (int i = _animationStates.Count - 1; i >= 0; i--)
        {
            AnimationStateData? state = _animationStates[i];

            if (state != null)
            {
                AnimationFrame? frame = state.GetFrame(layer);

                if (frame != null)
                {
                    offset += frame.X;
                }
            }
        }

        return offset;
    }

    /// @see AnimatedPetVisualization.as::getSpriteYOffset
    protected override int GetSpriteYOffset(int scale, int direction, int layer)
    {
        int offset = base.GetSpriteYOffset(scale, direction, layer);

        for (int i = _animationStates.Count - 1; i >= 0; i--)
        {
            AnimationStateData? state = _animationStates[i];

            if (state != null)
            {
                AnimationFrame? frame = state.GetFrame(layer);

                if (frame != null)
                {
                    offset += frame.Y;
                }
            }
        }

        return offset;
    }

    /// @see AnimatedPetVisualization.as::getAsset
    protected override IGraphicAsset? GetAsset(string name, int layer = -1)
    {
        if (AssetCollection == null)
        {
            return null;
        }

        int customIndex = _customLayerIds.IndexOf(layer);
        string paletteName = _paletteName;
        int customPartId = -1;

        if (customIndex > -1)
        {
            customPartId = customIndex < _customPartIds.Count ? _customPartIds[customIndex] : -1;
            int customPaletteId = customIndex < _customPaletteIds.Count ? _customPaletteIds[customIndex] : -1;
            paletteName = customPaletteId > -1 ? customPaletteId.ToString() : _paletteName;
        }

        if (customPartId > -1)
        {
            name += "_" + customPartId;
        }

        return AssetCollection.GetAssetWithPalette(name, paletteName);
    }

    /// @see AnimatedPetVisualization.as::getSpriteZOffset
    protected override double GetSpriteZOffset(int scale, int direction, int layer)
    {
        if (_petData == null)
        {
            return 0;
        }

        return _petData.GetZOffset(scale, GetDirection(scale, layer), layer);
    }

    /// @see AnimatedPetVisualization.as::getSpriteAssetName
    protected override string? GetSpriteAssetName(int scale, int spriteIndex)
    {
        if (_headOnly && IsNonHeadSprite(spriteIndex))
        {
            return null;
        }

        if (_isRiding && IsSaddleSprite(spriteIndex))
        {
            return null;
        }

        int totalSprites = SpriteCount;

        if (spriteIndex < totalSprites - ADDITIONAL_SPRITE_COUNT)
        {
            int size = GetSize(scale);

            if (spriteIndex < totalSprites - (ADDITIONAL_SPRITE_COUNT + 1))
            {
                if (spriteIndex >= FurnitureVisualizationData.LAYER_NAMES.Length)
                {
                    return null;
                }

                string layerCode = FurnitureVisualizationData.LAYER_NAMES[spriteIndex];

                if (size == 1)
                {
                    return Type + "_icon_" + layerCode;
                }

                return Type + "_" + size + "_" + layerCode + "_" + GetDirection(scale, spriteIndex) + "_" + GetFrameNumber(size, spriteIndex);
            }

            // Shadow sprite
            return Type + "_" + size + "_sd_" + GetDirection(scale, spriteIndex) + "_0";
        }

        return null;
    }

    /// @see AnimatedPetVisualization.as::getSpriteColor
    protected override uint GetSpriteColor(int scale, int layer, int colorId)
    {
        if (layer < SpriteCount - ADDITIONAL_SPRITE_COUNT)
        {
            return (uint)_color;
        }

        return 0xFFFFFF;
    }

    /// @see AnimatedPetVisualization.as::updateExperienceBubble
    private void UpdateExperienceBubble(int time)
    {
        if (_experienceData == null)
        {
            return;
        }

        _experienceData.Alpha = 0;

        if (_experienceTimestamp > 0)
        {
            int elapsed = time - _experienceTimestamp;

            if (elapsed < EXPERIENCE_BUBBLE_VISIBLE_IN_MS)
            {
                _experienceData.Alpha = (int)(Math.Sin((double)elapsed / EXPERIENCE_BUBBLE_VISIBLE_IN_MS * Math.PI) * 255);
                _experienceData.SetExperience(_gainedExperience);
            }
            else
            {
                _experienceTimestamp = 0;
            }

            IRoomObjectSprite? sprite = GetSprite(SpriteCount - 1);

            if (sprite != null)
            {
                if (_experienceData.Alpha > 0)
                {
                    sprite.Asset = _experienceData.ImageData;
                    sprite.OffsetX = -20;
                    sprite.OffsetY = -80;
                    sprite.Alpha = _experienceData.Alpha;
                    sprite.Visible = true;
                }
                else
                {
                    sprite.Asset = null;
                    sprite.Visible = false;
                }
            }
        }
    }

    /// @see AnimatedPetVisualization.as::validateActions
    private void ValidateActions(string? posture, string? gesture)
    {
        if (posture != _posture)
        {
            _posture = posture ?? "";
            int animId = _petData!.GetAnimationForPosture(_previousScale, posture);
            SetAnimationForIndex(POSTURE_ANIMATION_INDEX, animId);
        }

        if (_petData!.GetGestureDisabled(_previousScale, posture))
        {
            gesture = null;
        }

        if (gesture != _gesture)
        {
            _gesture = gesture;
            int animId = _petData.GetAnimationForGesture(_previousScale, gesture);
            SetAnimationForIndex(GESTURE_ANIMATION_INDEX, animId);
        }
    }

    /// @see AnimatedPetVisualization.as::getAnimationStateData
    private AnimationStateData? GetAnimationStateData(int index)
    {
        if (index >= 0 && index < _animationStates.Count)
        {
            return _animationStates[index];
        }

        return null;
    }

    /// @see AnimatedPetVisualization.as::setAnimationForIndex
    private void SetAnimationForIndex(int index, int animationId)
    {
        AnimationStateData? state = GetAnimationStateData(index);

        if (state != null)
        {
            if (SetSubAnimation(state, animationId))
            {
                _allAnimationsOver = false;
            }
        }
    }

    /// @see AnimatedPetVisualization.as::getDirection
    private int GetDirection(int scale, int spriteIndex)
    {
        if (IsHeadSprite(spriteIndex))
        {
            return _petData!.GetDirectionValue(scale, _headDirection);
        }

        return Direction;
    }

    /// @see AnimatedPetVisualization.as::isHeadSprite
    private bool IsHeadSprite(int spriteIndex)
    {
        while (_headSprites.Count <= spriteIndex)
        {
            _headSprites.Add(null);
        }

        if (_headSprites[spriteIndex] == null)
        {
            string tag = _petData!.GetTag(_previousScale, -1, spriteIndex);
            bool isHead = tag == HEAD_SPRITE_TAG;
            bool isHair = tag == HAIR_SPRITE_TAG;
            _headSprites[spriteIndex] = isHead || isHair;
        }

        return _headSprites[spriteIndex]!.Value;
    }

    /// @see AnimatedPetVisualization.as::isNonHeadSprite
    private bool IsNonHeadSprite(int spriteIndex)
    {
        while (_nonHeadSprites.Count <= spriteIndex)
        {
            _nonHeadSprites.Add(null);
        }

        if (_nonHeadSprites[spriteIndex] == null)
        {
            if (spriteIndex < SpriteCount - (ADDITIONAL_SPRITE_COUNT + 1))
            {
                string tag = _petData!.GetTag(_previousScale, -1, spriteIndex);

                if (!string.IsNullOrEmpty(tag) && tag != HEAD_SPRITE_TAG && tag != HAIR_SPRITE_TAG)
                {
                    _nonHeadSprites[spriteIndex] = true;
                }
                else
                {
                    _nonHeadSprites[spriteIndex] = false;
                }
            }
            else
            {
                _nonHeadSprites[spriteIndex] = true;
            }
        }

        return _nonHeadSprites[spriteIndex]!.Value;
    }

    /// @see AnimatedPetVisualization.as::isSaddleSprite
    private bool IsSaddleSprite(int spriteIndex)
    {
        while (_saddleSprites.Count <= spriteIndex)
        {
            _saddleSprites.Add(null);
        }

        if (_saddleSprites[spriteIndex] == null)
        {
            _saddleSprites[spriteIndex] = _petData!.GetTag(_previousScale, -1, spriteIndex) == SADDLE_SPRITE_TAG;
        }

        return _saddleSprites[spriteIndex]!.Value;
    }
}
