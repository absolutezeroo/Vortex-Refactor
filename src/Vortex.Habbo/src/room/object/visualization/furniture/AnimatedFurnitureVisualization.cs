using System;

using Vortex.Habbo.Room.Object.Visualization.Data;
using Vortex.Room.Object;
using Vortex.Room.Object.Visualization;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.AnimatedFurnitureVisualization
public class AnimatedFurnitureVisualization : FurnitureVisualization
{
    public const int DEFAULT_ANIMATION_ID = 0;

    private AnimatedFurnitureVisualizationData? _animationData;
    private int _state = -1;
    private AnimationStateData _animationState = new();
    private int _animationChangeTime;
    private double _cachedScale;

    protected bool _forceFrameUpdate;

    public int AnimationId => _animationState.AnimationId;

    protected int AnimatedLayerCount { get; private set; }

    protected int FrameIncrease { get; } = 1;

    public override void Dispose()
    {
        base.Dispose();
        _animationData = null;

        if (_animationState != null)
        {
            _animationState.Dispose();
            _animationState = null!;
        }
    }

    public override bool Initialize(IRoomObjectVisualizationData data)
    {
        if (data is not AnimatedFurnitureVisualizationData animData)
        {
            return false;
        }

        _animationData = animData;
        return base.Initialize(data);
    }

    protected override bool UpdateObject(double scale, double geometryDirection)
    {
        if (base.UpdateObject(scale, geometryDirection))
        {
            IRoomObject? obj = Object;
            if (obj == null)
            {
                return false;
            }

            int state = obj.GetState(0);
            if (state != _state)
            {
                SetAnimation(state);
                _state = state;

                IRoomObjectModel? model = obj.Model;
                if (model != null)
                {
                    _animationChangeTime = (int)model.GetNumber("furniture_state_update_time");
                }
            }

            return true;
        }

        return false;
    }

    protected override bool UpdateModel(double scale)
    {
        if (base.UpdateModel(scale))
        {
            IRoomObject? obj = Object;
            if (obj != null)
            {
                IRoomObjectModel model = obj.Model;

                if (UsesAnimationResetting())
                {
                    int updateTime = (int)model.GetNumber("furniture_state_update_time");
                    if (updateTime > _animationChangeTime)
                    {
                        _animationChangeTime = updateTime;
                        SetAnimation(_state);
                    }
                }

                double autoIndex = model.GetNumber("furniture_automatic_state_index");
                if (!double.IsNaN(autoIndex))
                {
                    int animId = _animationData!.GetAnimationId(_cachedScale, (int)autoIndex);
                    SetAnimation(animId);
                }
            }

            return true;
        }

        return false;
    }

    protected override int UpdateAnimation(double scale)
    {
        if (_animationData == null)
        {
            return 0;
        }

        if (scale != _cachedScale)
        {
            _cachedScale = scale;
            AnimatedLayerCount = _animationData.GetLayerCount(scale);
            ResetAllAnimationFrames();
        }

        int result = UpdateAnimations(scale);
        _forceFrameUpdate = false;
        return result;
    }

    protected override int GetFrameNumber(int scale, int layer)
    {
        AnimationFrame? frame = _animationState.GetFrame(layer);
        if (frame != null)
        {
            return frame.Id;
        }
        return base.GetFrameNumber(scale, layer);
    }

    protected override int GetSpriteXOffset(int scale, int direction, int layer)
    {
        int offset = base.GetSpriteXOffset(scale, direction, layer);
        AnimationFrame? frame = _animationState.GetFrame(layer);
        if (frame != null)
        {
            offset += frame.X;
        }
        return offset;
    }

    protected override int GetSpriteYOffset(int scale, int direction, int layer)
    {
        int offset = base.GetSpriteYOffset(scale, direction, layer);
        AnimationFrame? frame = _animationState.GetFrame(layer);
        if (frame != null)
        {
            offset += frame.Y;
        }
        return offset;
    }

    protected virtual int GetAnimationId(AnimationStateData animState)
    {
        int id = AnimationId;
        if (id != 0 && _animationData!.HasAnimation(_cachedScale, id))
        {
            return id;
        }
        return 0;
    }

    protected virtual void SetAnimation(int animationId)
    {
        if (_animationData != null)
        {
            SetSubAnimation(_animationState, animationId, _state >= 0);
        }
    }

    protected bool SetSubAnimation(AnimationStateData animState, int targetAnimId, bool hasCurrentState = true)
    {
        int currentAnimId = animState.AnimationId;

        if (hasCurrentState)
        {
            if (IsPlayingTransition(animState, targetAnimId))
            {
                return false;
            }

            int currentState = GetCurrentState(animState);

            if (targetAnimId != currentState)
            {
                if (!_animationData!.IsImmediateChange(_cachedScale, targetAnimId, currentState))
                {
                    int transFromId = AnimationData.GetTransitionFromAnimationId(currentState);
                    if (_animationData.HasAnimation(_cachedScale, transFromId))
                    {
                        animState.AnimationAfterTransitionId = targetAnimId;
                        targetAnimId = transFromId;
                    }
                    else
                    {
                        int transToId = AnimationData.GetTransitionToAnimationId(targetAnimId);
                        if (_animationData.HasAnimation(_cachedScale, transToId))
                        {
                            animState.AnimationAfterTransitionId = targetAnimId;
                            targetAnimId = transToId;
                        }
                    }
                }
            }
            else if (AnimationData.IsTransitionFromAnimation(currentAnimId))
            {
                int transToId = AnimationData.GetTransitionToAnimationId(targetAnimId);
                if (_animationData!.HasAnimation(_cachedScale, transToId))
                {
                    animState.AnimationAfterTransitionId = targetAnimId;
                    targetAnimId = transToId;
                }
            }
            else if (!AnimationData.IsTransitionToAnimation(currentAnimId))
            {
                if (UsesAnimationResetting())
                {
                    int transFromId = AnimationData.GetTransitionFromAnimationId(currentState);
                    if (_animationData!.HasAnimation(_cachedScale, transFromId))
                    {
                        animState.AnimationAfterTransitionId = targetAnimId;
                        targetAnimId = transFromId;
                    }
                    else
                    {
                        int transToId = AnimationData.GetTransitionToAnimationId(targetAnimId);
                        if (_animationData!.HasAnimation(_cachedScale, transToId))
                        {
                            animState.AnimationAfterTransitionId = targetAnimId;
                            targetAnimId = transToId;
                        }
                    }
                }
            }
        }

        if (currentAnimId != targetAnimId)
        {
            animState.AnimationId = targetAnimId;
            return true;
        }

        return false;
    }

    protected bool GetLastFramePlayed(int layer)
    {
        return _animationState.GetLastFramePlayed(layer);
    }

    protected void ResetAllAnimationFrames()
    {
        _animationState?.SetLayerCount(AnimatedLayerCount);
    }

    protected int UpdateAnimations(double scale)
    {
        int result = 0;

        if (!_animationState.AnimationOver || _forceFrameUpdate)
        {
            result = UpdateFramesForAnimation(_animationState, scale);

            if (_animationState.AnimationOver)
            {
                int animId = _animationState.AnimationId;
                if (AnimationData.IsTransitionFromAnimation(animId) || AnimationData.IsTransitionToAnimation(animId))
                {
                    SetAnimation(_animationState.AnimationAfterTransitionId);
                    _animationState.AnimationOver = false;
                }
            }
        }

        return result;
    }

    protected int UpdateFramesForAnimation(AnimationStateData animState, double scale)
    {
        if (animState.AnimationOver && !_forceFrameUpdate)
        {
            return 0;
        }

        int frameCounter = animState.FrameCounter;
        int animId = GetAnimationId(animState);

        if (frameCounter == 0)
        {
            frameCounter = _animationData!.GetStartFrame(scale, animId, Direction);
        }

        frameCounter += FrameIncrease;
        animState.FrameCounter = frameCounter;

        int updatedLayers = 0;
        animState.AnimationOver = true;
        int layerBit = 1 << (AnimatedLayerCount - 1);

        for (int layer = AnimatedLayerCount - 1; layer >= 0; layer--)
        {
            bool animationPlayed = animState.GetAnimationPlayed(layer);

            if (!animationPlayed || _forceFrameUpdate)
            {
                bool lastFramePlayed = animState.GetLastFramePlayed(layer);
                AnimationFrame? currentFrame = animState.GetFrame(layer);

                if (currentFrame != null)
                {
                    if (currentFrame.IsLastFrame && currentFrame.RemainingFrameRepeats <= FrameIncrease)
                    {
                        lastFramePlayed = true;
                    }
                }

                if (_forceFrameUpdate || currentFrame == null ||
                    (currentFrame.RemainingFrameRepeats >= 0 &&
                     (currentFrame.RemainingFrameRepeats -= FrameIncrease) <= 0))
                {
                    int activeSequence = -1;
                    if (currentFrame != null)
                    {
                        activeSequence = currentFrame.ActiveSequence;
                    }

                    AnimationFrame? newFrame;
                    if (activeSequence == -1)
                    {
                        newFrame = _animationData!.GetFrame(scale, animId, Direction, layer, frameCounter);
                    }
                    else
                    {
                        newFrame = _animationData!.GetFrameFromSequence(
                            scale, animId, Direction, layer,
                            activeSequence, currentFrame!.ActiveSequenceOffset + currentFrame.Repeats,
                            frameCounter);
                    }

                    animState.SetFrame(layer, newFrame);
                    currentFrame = newFrame;
                    updatedLayers |= layerBit;
                }

                if (currentFrame == null || currentFrame.RemainingFrameRepeats == -1)
                {
                    lastFramePlayed = true;
                    animationPlayed = true;
                }
                else
                {
                    animState.AnimationOver = false;
                }

                animState.SetLastFramePlayed(layer, lastFramePlayed);
                animState.SetAnimationPlayed(layer, animationPlayed);
            }

            layerBit >>= 1;
        }

        return updatedLayers;
    }

    protected virtual bool UsesAnimationResetting()
    {
        return false;
    }

    private bool IsPlayingTransition(AnimationStateData animState, int targetAnimId)
    {
        int currentAnimId = animState.AnimationId;
        if (AnimationData.IsTransitionFromAnimation(currentAnimId) || AnimationData.IsTransitionToAnimation(currentAnimId))
        {
            if (targetAnimId == animState.AnimationAfterTransitionId)
            {
                if (!animState.AnimationOver)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private int GetCurrentState(AnimationStateData animState)
    {
        int animId = animState.AnimationId;
        if (AnimationData.IsTransitionFromAnimation(animId) || AnimationData.IsTransitionToAnimation(animId))
        {
            return animState.AnimationAfterTransitionId;
        }
        return animId;
    }
}
