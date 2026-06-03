using System;

namespace Vortex.Habbo.Room.Object.Visualization.Data;

/// <summary>
/// Pooled animation frame instance with lifecycle management. Uses a static pool to recycle objects.
/// </summary>
/// @see com.sulake.habbo.room.object.visualization.data.AnimationFrame
public class AnimationFrame
{
    public const int FRAME_REPEAT_FOREVER = -1;
    public const int SEQUENCE_NOT_DEFINED = -1;

    private const int POOL_SIZE_LIMIT = 3000;

    private static readonly List<AnimationFrame> s_pool = new();

    public static AnimationFrame Allocate
    (
        int id,
        int x,
        int y,
        int repeats,
        int frameRepeats,
        bool isLastFrame,
        int activeSequence = -1,
        int activeSequenceOffset = 0
    )
    {
        AnimationFrame frame;

        if (s_pool.Count > 0)
        {
            frame = s_pool[^1];
            s_pool.RemoveAt(s_pool.Count - 1);
        }
        else
        {
            frame = new AnimationFrame();
        }

        frame._recycled = false;
        frame._id = id;
        frame.X = x;
        frame.Y = y;
        frame.IsLastFrame = isLastFrame;

        if (repeats < 1)
        {
            repeats = 1;
        }

        frame.Repeats = repeats;


        if (frameRepeats < 0)
        {
            frameRepeats = -1;
        }
        frame.FrameRepeats = frameRepeats;
        frame._remainingFrameRepeats = frameRepeats;

        if (activeSequence < 0)
        {
            return frame;
        }

        frame.ActiveSequence = activeSequence;
        frame.ActiveSequenceOffset = activeSequenceOffset;

        return frame;
    }

    private int _id;
    private int _remainingFrameRepeats = 1;
    private bool _recycled;

    private AnimationFrame() { }

    public int ActiveSequenceOffset { get; private set; }

    public bool IsLastFrame { get; private set; }

    public int Id
    {
        get
        {
            if (_id >= 0)
            {
                return _id;
            }
            return (int)(-_id * Random.Shared.NextDouble());
        }
    }

    public int X { get; private set; }

    public int Y { get; private set; }

    public int Repeats { get; private set; } = 1;

    public int FrameRepeats { get; private set; } = 1;

    public int RemainingFrameRepeats
    {
        get => FrameRepeats < 0 ? -1 : _remainingFrameRepeats;
        set
        {
            if (value < 0)
            {
                value = 0;
            }
            if (FrameRepeats > 0 && value > FrameRepeats)
            {
                value = FrameRepeats;
            }
            _remainingFrameRepeats = value;
        }
    }

    public int ActiveSequence { get; private set; } = -1;

    public void Recycle()
    {
        if (_recycled)
        {
            return;
        }

        _recycled = true;

        if (s_pool.Count < POOL_SIZE_LIMIT)
        {
            s_pool.Add(this);
        }
    }
}
