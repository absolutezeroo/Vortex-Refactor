using System;
using System.Globalization;
using System.Xml.Linq;

using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Data;

/// <summary>
/// Top-level animation definition indexed by layer ID. Manages animation layers,
/// transition IDs and immediate-change tracking.
/// </summary>
/// @see com.sulake.habbo.room.object.visualization.data.AnimationData
public class AnimationData
{
    public const int DEFAULT_FRAME_NUMBER = 0;

    private const int TRANSITION_TO_ANIMATION_OFFSET = 1000000;
    private const int TRANSITION_FROM_ANIMATION_OFFSET = 2000000;

    public static int GetTransitionToAnimationId(int id)
    {
        return TRANSITION_TO_ANIMATION_OFFSET + id;
    }

    public static int GetTransitionFromAnimationId(int id)
    {
        return TRANSITION_FROM_ANIMATION_OFFSET + id;
    }

    public static bool IsTransitionToAnimation(int id)
    {
        return id is >= TRANSITION_TO_ANIMATION_OFFSET and < TRANSITION_FROM_ANIMATION_OFFSET;
    }

    public static bool IsTransitionFromAnimation(int id)
    {
        return id >= TRANSITION_FROM_ANIMATION_OFFSET;
    }

    private Dictionary<int, AnimationLayerData>? _layers = new();
    private int _cachedMaxFrameCount = -1;
    private bool _randomStart;
    private List<int>? _immediateChangeFrom;

    public void Dispose()
    {
        if (_layers != null)
        {
            foreach (AnimationLayerData layer in _layers.Values)
            {
                layer.Dispose();
            }

            _layers.Clear();
            _layers = null;
        }

        _immediateChangeFrom = null;
    }

    public void SetImmediateChanges(List<int> changes)
    {
        _immediateChangeFrom = changes;
    }

    public bool IsImmediateChange(int animationId)
    {
        return _immediateChangeFrom != null && _immediateChangeFrom.Contains(animationId);
    }

    public int GetStartFrame(int layerId)
    {
        if (!_randomStart)
        {
            return 0;
        }

        return (int)(Random.Shared.NextDouble() * _cachedMaxFrameCount);
    }

    public bool Initialize(XElement xml)
    {
        _randomStart = false;
        XAttribute? randomStartAttr = xml.Attribute("randomStart");

        if (randomStartAttr != null && int.Parse(randomStartAttr.Value, CultureInfo.InvariantCulture) != 0)
        {
            _randomStart = true;
        }

        foreach (XElement layerElement in xml.Elements("animationLayer"))
        {
            if (!XMLValidator.CheckRequiredAttributes(layerElement, ["id"]))
            {
                return false;
            }

            int layerId = int.Parse(layerElement.Attribute("id")!.Value, CultureInfo.InvariantCulture);
            int loopCount = 1;
            int frameRepeat = 1;
            bool isRandom = false;

            XAttribute? loopAttr = layerElement.Attribute("loopCount");
            if (loopAttr is
                {
                    Value.Length: > 0,
                })
            {
                loopCount = int.Parse(loopAttr.Value, CultureInfo.InvariantCulture);
            }

            XAttribute? repeatAttr = layerElement.Attribute("frameRepeat");

            if (repeatAttr is
                {
                    Value.Length: > 0,
                })
            {
                frameRepeat = int.Parse(repeatAttr.Value, CultureInfo.InvariantCulture);
            }

            XAttribute? randomAttr = layerElement.Attribute("random");

            if (randomAttr != null)
            {
                isRandom = int.Parse(randomAttr.Value, CultureInfo.InvariantCulture) != 0;
            }

            if (!AddLayer(layerId, loopCount, frameRepeat, isRandom, layerElement))
            {
                return false;
            }
        }
        return true;
    }

    public AnimationFrame? GetFrame(int direction, int layerId, int frameCounter)
    {
        if (_layers != null && _layers.TryGetValue(layerId, out AnimationLayerData? layer))
        {
            return layer.GetFrame(direction, frameCounter);
        }

        return null;
    }

    public AnimationFrame? GetFrameFromSequence
    (
        int direction,
        int layerId,
        int sequenceIndex,
        int frameIndex,
        int overallFrame
    )
    {
        if (_layers != null && _layers.TryGetValue(layerId, out AnimationLayerData? layer))
        {
            return layer.GetFrameFromSequence(direction, sequenceIndex, frameIndex, overallFrame);
        }

        return null;
    }

    private bool AddLayer(int layerId, int loopCount, int frameRepeat, bool isRandom, XElement xml)
    {
        AnimationLayerData layerData = new(loopCount, frameRepeat, isRandom);

        foreach (XElement seqElement in xml.Elements("frameSequence"))
        {
            int seqLoop = 1;
            bool seqRandom = false;

            XAttribute? seqLoopAttr = seqElement.Attribute("loopCount");

            if (seqLoopAttr is
                {
                    Value.Length: > 0,
                })
            {
                seqLoop = int.Parse(seqLoopAttr.Value, CultureInfo.InvariantCulture);
            }

            XAttribute? seqRandomAttr = seqElement.Attribute("random");

            if (seqRandomAttr != null)
            {
                seqRandom = int.Parse(seqRandomAttr.Value, CultureInfo.InvariantCulture) != 0;
            }

            AnimationFrameSequenceData sequence = layerData.AddFrameSequence(seqLoop, seqRandom);

            foreach (XElement frameElement in seqElement.Elements("frame"))
            {
                if (!XMLValidator.CheckRequiredAttributes(frameElement, ["id"]))
                {
                    layerData.Dispose();

                    return false;
                }

                int frameId = int.Parse(frameElement.Attribute("id")!.Value, CultureInfo.InvariantCulture);
                int frameX = 0;
                int frameY = 0;
                int randomX = 0;
                int randomY = 0;

                XAttribute? xAttr = frameElement.Attribute("x");

                if (xAttr != null)
                {
                    frameX = int.Parse(xAttr.Value, CultureInfo.InvariantCulture);
                }

                XAttribute? yAttr = frameElement.Attribute("y");

                if (yAttr != null)
                {
                    frameY = int.Parse(yAttr.Value, CultureInfo.InvariantCulture);
                }

                XAttribute? rxAttr = frameElement.Attribute("randomX");

                if (rxAttr != null)
                {
                    randomX = int.Parse(rxAttr.Value, CultureInfo.InvariantCulture);
                }

                XAttribute? ryAttr = frameElement.Attribute("randomY");

                if (ryAttr != null)
                {
                    randomY = int.Parse(ryAttr.Value, CultureInfo.InvariantCulture);
                }

                DirectionalOffsetData? directionalOffsets = ReadDirectionalOffsets(frameElement);

                sequence.AddFrame(frameId, frameX, frameY, randomX, randomY, directionalOffsets);
            }

            sequence.Initialize();
        }

        layerData.CalculateLength();
        _layers![layerId] = layerData;

        int frameCount = layerData.FrameCount;

        if (frameCount > _cachedMaxFrameCount)
        {
            _cachedMaxFrameCount = frameCount;
        }

        return true;
    }

    private static DirectionalOffsetData? ReadDirectionalOffsets(XElement frameElement)
    {
        DirectionalOffsetData? result = null;

        XElement? offsetsElement = frameElement.Element("offsets");

        if (offsetsElement == null)
        {
            return result;
        }

        foreach (XElement offsetElement in offsetsElement.Elements("offset"))
        {
            if (!XMLValidator.CheckRequiredAttributes(offsetElement, ["direction"]))
            {
                continue;
            }

            int direction = int.Parse(offsetElement.Attribute("direction")!.Value, CultureInfo.InvariantCulture);
            int x = 0;
            int y = 0;

            XAttribute? xAttr = offsetElement.Attribute("x");

            if (xAttr != null)
            {
                x = int.Parse(xAttr.Value, CultureInfo.InvariantCulture);
            }

            XAttribute? yAttr = offsetElement.Attribute("y");

            if (yAttr != null)
            {
                y = int.Parse(yAttr.Value, CultureInfo.InvariantCulture);
            }

            result ??= new DirectionalOffsetData();

            result.SetOffset(direction, x, y);
        }

        return result;
    }
}
