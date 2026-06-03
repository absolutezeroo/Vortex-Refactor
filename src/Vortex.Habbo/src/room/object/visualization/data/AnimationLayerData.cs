using System;

namespace Vortex.Habbo.Room.Object.Visualization.Data;

/// <summary>
/// Contains multiple frame sequences for a single animation layer.
/// Manages loop count, frame repeat (duration per frame), and random playback.
/// </summary>
/// @see com.sulake.habbo.room.object.visualization.data.AnimationLayerData
public class AnimationLayerData
{
    private List<AnimationFrameSequenceData> _sequences = new();
    private int _cachedFrameCount = -1;
    private readonly int _loopCount;
    private readonly int _frameRepeat;
    private readonly bool _isRandom;

    public AnimationLayerData(int loopCount, int frameRepeat, bool isRandom)
    {
        if (loopCount < 0)
        {
            loopCount = 0;
        }

        if (frameRepeat < 1)
        {
            frameRepeat = 1;
        }

        _loopCount = loopCount;
        _frameRepeat = frameRepeat;
        _isRandom = isRandom;
    }

    public int FrameCount
    {
        get
        {
            if (_cachedFrameCount < 0)
            {
                CalculateLength();
            }

            return _cachedFrameCount;
        }
    }

    public void Dispose()
    {
        foreach (AnimationFrameSequenceData seq in _sequences)
        {
            seq.Dispose();
        }

        _sequences = new List<AnimationFrameSequenceData>();
    }

    public AnimationFrameSequenceData AddFrameSequence(int loopCount, bool isRandom)
    {
        AnimationFrameSequenceData sequence = new(loopCount, isRandom);

        _sequences.Add(sequence);

        return sequence;
    }

    public void CalculateLength()
    {
        _cachedFrameCount = 0;

        foreach (AnimationFrameSequenceData seq in _sequences)
        {
            _cachedFrameCount += seq.FrameCount;
        }
    }

    public AnimationFrame? GetFrame(int direction, int frameCounter)
    {
        if (_cachedFrameCount < 1)
        {
            return null;
        }

        AnimationFrameSequenceData? sequence = null;
        frameCounter /= _frameRepeat;
        bool isLastFrame = false;
        int seqIndex;
        if (!_isRandom)
        {
            int loopNum = frameCounter / _cachedFrameCount;
            frameCounter %= _cachedFrameCount;

            if ((_loopCount > 0 && loopNum >= _loopCount) || (_loopCount <= 0 && _cachedFrameCount == 1))
            {
                frameCounter = _cachedFrameCount - 1;
                isLastFrame = true;
            }

            int offset = 0;
            seqIndex = 0;

            while (seqIndex < _sequences.Count)
            {
                sequence = _sequences[seqIndex];

                if (frameCounter < offset + sequence.FrameCount)
                {
                    break;
                }

                offset += sequence.FrameCount;
                seqIndex++;
            }

            return GetFrameFromSpecificSequence(direction, sequence, seqIndex, frameCounter - offset, isLastFrame);
        }

        seqIndex = (int)(Random.Shared.NextDouble() * _sequences.Count);
        sequence = _sequences[seqIndex];

        if (sequence.FrameCount < 1)
        {
            return null;
        }

        frameCounter = 0;

        return GetFrameFromSpecificSequence(direction, sequence, seqIndex, frameCounter, false);
    }

    public AnimationFrame? GetFrameFromSequence(int direction, int sequenceIndex, int frameIndex, int overallFrame)
    {
        if (sequenceIndex < 0 || sequenceIndex >= _sequences.Count)
        {
            return null;
        }

        AnimationFrameSequenceData sequence = _sequences[sequenceIndex];

        if (frameIndex >= sequence.FrameCount)
        {
            return GetFrame(direction, overallFrame);
        }

        return GetFrameFromSpecificSequence(direction, sequence, sequenceIndex, frameIndex, false);
    }

    private AnimationFrame? GetFrameFromSpecificSequence
    (
        int direction,
        AnimationFrameSequenceData? sequence,
        int sequenceIndex,
        int frameIndex,
        bool isOver
    )
    {
        if (sequence == null)
        {
            return null;
        }

        int resolvedIndex = sequence.GetFrameIndex(frameIndex);
        AnimationFrameData? frameData = sequence.GetFrame(resolvedIndex);

        if (frameData == null)
        {
            return null;
        }

        int x = frameData.GetX(direction);
        int y = frameData.GetY(direction);
        int randomX = frameData.RandomX;
        int randomY = frameData.RandomY;

        if (randomX != 0)
        {
            x += (int)(randomX * Random.Shared.NextDouble());
        }

        if (randomY != 0)
        {
            y += (int)(randomY * Random.Shared.NextDouble());
        }

        int repeats = frameData.Repeats;

        if (repeats > 1)
        {
            repeats = sequence.GetRepeats(resolvedIndex);
        }

        int frameRepeats = _frameRepeat * repeats;

        if (isOver)
        {
            frameRepeats = -1;
        }

        bool isLastInLayer = false;

        if (_isRandom || sequence.IsRandom)
        {
            return AnimationFrame.Allocate(
                frameData.Id, x, y, repeats, frameRepeats,
                isLastInLayer, sequenceIndex, frameIndex
            );
        }

        if (sequenceIndex == _sequences.Count - 1 && frameIndex == sequence.FrameCount - 1)
        {
            isLastInLayer = true;
        }

        return AnimationFrame.Allocate(
            frameData.Id, x, y, repeats, frameRepeats,
            isLastInLayer, sequenceIndex, frameIndex
        );
    }
}
