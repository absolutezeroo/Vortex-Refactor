using System.Globalization;
using System.Xml.Linq;

using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Data;

/// <summary>
/// Extends SizeData with animation definitions indexed by animation ID.
/// Handles transition animations (to/from) and immediate change tracking.
/// </summary>
/// @see com.sulake.habbo.room.object.visualization.data.AnimationSizeData
public class AnimationSizeData(int layerCount, int directionCount) : SizeData(layerCount, directionCount)
{
    private Dictionary<int, AnimationData>? _animations = new();
    private readonly List<int> _baseAnimationIds = new();

    public override void Dispose()
    {
        base.Dispose();

        if (_animations == null)
        {
            return;
        }

        foreach (AnimationData anim in _animations.Values)
        {
            anim.Dispose();
        }

        _animations.Clear();
        _animations = null;
    }

    public bool DefineAnimations(XElement? xml)
    {
        if (xml == null)
        {
            return true;
        }

        foreach (XElement animElement in xml.Elements("animation"))
        {
            if (!XMLValidator.CheckRequiredAttributes(animElement, ["id"]))
            {
                return false;
            }

            int animId = int.Parse(animElement.Attribute("id")!.Value, CultureInfo.InvariantCulture);
            bool isTransition = false;

            XAttribute? transToAttr = animElement.Attribute("transitionTo");

            if (transToAttr is
                {
                    Value.Length: > 0,
                })
            {
                int targetId = int.Parse(transToAttr.Value, CultureInfo.InvariantCulture);
                animId = AnimationData.GetTransitionToAnimationId(targetId);
                isTransition = true;
            }

            XAttribute? transFromAttr = animElement.Attribute("transitionFrom");

            if (transFromAttr is
                {
                    Value.Length: > 0,
                })
            {
                int sourceId = int.Parse(transFromAttr.Value, CultureInfo.InvariantCulture);
                animId = AnimationData.GetTransitionFromAnimationId(sourceId);
                isTransition = true;
            }

            AnimationData animData = CreateAnimationData();

            if (!animData.Initialize(animElement))
            {
                animData.Dispose();
                return false;
            }

            XAttribute? immediateAttr = animElement.Attribute("immediateChangeFrom");

            if (immediateAttr is
                {
                    Value.Length: > 0,
                })
            {
                string[] parts = immediateAttr.Value.Split(',');
                List<int> immediateIds = new();

                foreach (string part in parts)
                {
                    int id = int.Parse(part.Trim(), CultureInfo.InvariantCulture);

                    if (!immediateIds.Contains(id))
                    {
                        immediateIds.Add(id);
                    }
                }

                animData.SetImmediateChanges(immediateIds);
            }

            _animations![animId] = animData;

            if (!isTransition)
            {
                _baseAnimationIds.Add(animId);
            }
        }
        return true;
    }

    public bool HasAnimation(int animationId)
    {
        return _animations != null && _animations.ContainsKey(animationId);
    }

    public int GetAnimationCount()
    {
        return _baseAnimationIds.Count;
    }

    public int GetAnimationId(int index)
    {
        int count = GetAnimationCount();

        if (index >= 0 && count > 0)
        {
            return _baseAnimationIds[index % count];
        }

        return 0;
    }

    public bool IsImmediateChange(int animationId, int fromAnimationId)
    {
        if (_animations != null && _animations.TryGetValue(animationId, out AnimationData? animData))
        {
            return animData.IsImmediateChange(fromAnimationId);
        }

        return false;
    }

    public int GetStartFrame(int animationId, int layerId)
    {
        if (_animations != null && _animations.TryGetValue(animationId, out AnimationData? animData))
        {
            return animData.GetStartFrame(layerId);
        }

        return 0;
    }

    public AnimationFrame? GetFrame(int animationId, int direction, int layerId, int frameCounter)
    {
        if (_animations != null && _animations.TryGetValue(animationId, out AnimationData? animData))
        {
            return animData.GetFrame(direction, layerId, frameCounter);
        }

        return null;
    }

    public AnimationFrame? GetFrameFromSequence
    (
        int animationId,
        int direction,
        int layerId,
        int sequenceIndex,
        int frameIndex,
        int overallFrame
    )
    {
        if (_animations != null && _animations.TryGetValue(animationId, out AnimationData? animData))
        {
            return animData.GetFrameFromSequence(direction, layerId, sequenceIndex, frameIndex, overallFrame);
        }

        return null;
    }

    protected virtual AnimationData CreateAnimationData()
    {
        return new AnimationData();
    }
}
