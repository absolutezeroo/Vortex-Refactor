namespace Vortex.Habbo.Room.Object.Visualization.Data;

/// <summary>
/// Runtime state for playing animations. Tracks current animation ID, frame counter,
/// per-layer frame instances, and animation completion status.
/// </summary>
/// @see com.sulake.habbo.room.object.visualization.data.AnimationStateData
public class AnimationStateData
{
    private int _animationId = -1;
    private List<AnimationFrame?> _frames = new();
    private List<bool> _lastFramePlayed = new();
    private List<bool> _animationPlayed = new();
    private int _layerCount;

    public bool AnimationOver { get; set; }

    public int FrameCounter { get; set; }

    public int AnimationId
    {
        get => _animationId;
        set
        {
            if (value != _animationId)
            {
                _animationId = value;
                ResetAnimationFrames(false);
            }
        }
    }

    public int AnimationAfterTransitionId { get; set; }

    public void Dispose()
    {
        RecycleFrames();
        _frames = null!;
        _lastFramePlayed = null!;
        _animationPlayed = null!;
    }

    public void SetLayerCount(int count)
    {
        _layerCount = count;
        ResetAnimationFrames();
    }

    public void ResetAnimationFrames(bool fullReset = true)
    {
        if (fullReset || _frames == null)
        {
            RecycleFrames();
            _frames = new List<AnimationFrame?>();
        }

        _lastFramePlayed = new List<bool>();
        _animationPlayed = new List<bool>();
        AnimationOver = false;
        FrameCounter = 0;

        for (int i = 0;
             i < _layerCount;
             i++)
        {
            if (fullReset || _frames.Count <= i)
            {
                if (_frames.Count <= i)
                {
                    _frames.Add(null);
                }
                else
                {
                    _frames[i] = null;
                }
            }
            else
            {
                AnimationFrame? existingFrame = _frames[i];

                if (existingFrame != null)
                {
                    existingFrame.Recycle();

                    _frames[i] = AnimationFrame.Allocate(
                        existingFrame.Id, existingFrame.X, existingFrame.Y,
                        existingFrame.Repeats, 0, existingFrame.IsLastFrame
                    );
                }
            }

            _lastFramePlayed.Add(false);
            _animationPlayed.Add(false);
        }
    }

    public AnimationFrame? GetFrame(int layerIndex)
    {
        if (layerIndex >= 0 && layerIndex < _layerCount)
        {
            return _frames[layerIndex];
        }

        return null;
    }

    public void SetFrame(int layerIndex, AnimationFrame? frame)
    {
        if (layerIndex < 0 || layerIndex >= _layerCount)
        {
            return;
        }

        AnimationFrame? existing = _frames[layerIndex];

        existing?.Recycle();

        _frames[layerIndex] = frame;
    }

    public bool GetAnimationPlayed(int layerIndex)
    {
        if (layerIndex >= 0 && layerIndex < _layerCount)
        {
            return _animationPlayed[layerIndex];
        }

        return true;
    }

    public void SetAnimationPlayed(int layerIndex, bool value)
    {
        if (layerIndex >= 0 && layerIndex < _layerCount)
        {
            _animationPlayed[layerIndex] = value;
        }
    }

    public bool GetLastFramePlayed(int layerIndex)
    {
        if (layerIndex >= 0 && layerIndex < _layerCount)
        {
            return _lastFramePlayed[layerIndex];
        }

        return true;
    }

    public void SetLastFramePlayed(int layerIndex, bool value)
    {
        if (layerIndex >= 0 && layerIndex < _layerCount)
        {
            _lastFramePlayed[layerIndex] = value;
        }
    }

    private void RecycleFrames()
    {
        if (_frames == null)
        {
            return;
        }

        foreach (AnimationFrame? frame in _frames)
        {
            frame?.Recycle();
        }
    }
}
