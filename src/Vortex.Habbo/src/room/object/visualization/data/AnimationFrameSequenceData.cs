using System;

namespace Vortex.Habbo.Room.Object.Visualization.Data;

/// <summary>
/// Sequence of animation frames with optional random playback, loop count and repeat tracking.
/// </summary>
/// @see com.sulake.habbo.room.object.visualization.data.AnimationFrameSequenceData
public class AnimationFrameSequenceData
{
    private List<AnimationFrameData> _frames = new();
    private readonly List<int> _frameIndexes = new();
    private readonly List<int> _repeats = new();
    private readonly int _loopCount;

    public AnimationFrameSequenceData(int loopCount, bool isRandom)
    {
        if (loopCount < 1)
        {
            loopCount = 1;
        }
        _loopCount = loopCount;
        IsRandom = isRandom;
    }

    public bool IsRandom { get; }

    public int FrameCount => _frameIndexes.Count * _loopCount;

    public void Dispose()
    {
        _frames = [];
    }

    public void Initialize()
    {
        int repeat = 1;
        int lastIndex = -1;

        for (int i = _frameIndexes.Count - 1;
             i >= 0;
             i--)
        {
            if (_frameIndexes[i] == lastIndex)
            {
                repeat++;
            }
            else
            {
                lastIndex = _frameIndexes[i];
                repeat = 1;
            }

            _repeats[i] = repeat;
        }
    }

    public void AddFrame(int id, int x, int y, int randomX, int randomY, DirectionalOffsetData? directionalOffsets)
    {
        int repeats = 1;

        if (_frames.Count > 0)
        {
            AnimationFrameData last = _frames[^1];

            if (last.Id == id && !last.HasDirectionalOffsets() &&
                last.X == x && last.Y == y &&
                last.RandomX == randomX && randomX == 0 &&
                last.RandomY == randomY && randomY == 0)
            {
                repeats += last.Repeats;

                _frames.RemoveAt(_frames.Count - 1);
            }
        }

        AnimationFrameData frameData;

        if (directionalOffsets == null)
        {
            frameData = new AnimationFrameData(id, x, y, randomX, randomY, repeats);
        }
        else
        {
            frameData = new AnimationFrameDirectionalData(id, x, y, randomX, randomY, directionalOffsets, repeats);
        }

        _frames.Add(frameData);
        _frameIndexes.Add(_frames.Count - 1);
        _repeats.Add(1);
    }

    public AnimationFrameData? GetFrame(int index)
    {
        if (_frames.Count == 0 || index < 0 || index >= FrameCount)
        {
            return null;
        }

        index = _frameIndexes[index % _frameIndexes.Count];

        return _frames[index];
    }

    public int GetFrameIndex(int index)
    {
        if (index < 0 || index >= FrameCount)
        {
            return -1;
        }

        if (!IsRandom)
        {
            return index;
        }

        index = (int)(Random.Shared.NextDouble() * _frameIndexes.Count);

        if (index == _frameIndexes.Count)
        {
            index--;
        }

        return index;
    }

    public int GetRepeats(int index)
    {
        if (index < 0 || index >= FrameCount)
        {
            return 0;
        }

        return _repeats[index % _repeats.Count];
    }
}
