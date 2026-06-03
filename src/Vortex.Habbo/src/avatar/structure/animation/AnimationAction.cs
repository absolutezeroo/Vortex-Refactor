// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/animation/AnimationAction.as

using System;
using System.Xml.Linq;

using Godot;

namespace Vortex.Habbo.Avatar.Structure.Animation;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/animation/AnimationAction.as
public class AnimationAction
{
    /// @see AnimationAction.as::DEFAULT_OFFSET
    public static readonly Vector2I DefaultOffset = new(0, 0);

    private readonly Dictionary<int, Dictionary<int, Dictionary<string, Vector2I>>> _offsets;
    private readonly List<int> _frameSequence;

    /// @see AnimationAction.as::AnimationAction
    public AnimationAction(XElement xml)
    {
        _offsets = new Dictionary<int, Dictionary<int, Dictionary<string, Vector2I>>>();
        Id = xml.Attribute("id")?.Value ?? "";
        Parts = new Dictionary<string, AnimationActionPart>();

        foreach (XElement partXml in xml.Elements("part"))
        {
            AnimationActionPart actionPart = new(partXml);
            string setType = partXml.Element("set-type")?.Value ?? "";
            Parts[setType] = actionPart;
            FrameCount = Math.Max(FrameCount, actionPart.Frames.Count);
        }

        _frameSequence = new List<int>();

        XElement? offsetsElement = xml.Element("offsets");
        if (offsetsElement != null)
        {
            foreach (XElement frameXml in offsetsElement.Elements("frame"))
            {
                int frameId = int.Parse(frameXml.Attribute("id")?.Value ?? "0");
                FrameCount = Math.Max(FrameCount, frameId);

                Dictionary<int, Dictionary<string, Vector2I>> directionMap = new();
                _offsets[frameId] = directionMap;

                XElement? directionsElement = frameXml.Element("directions");
                if (directionsElement != null)
                {
                    foreach (XElement directionXml in directionsElement.Elements("direction"))
                    {
                        int directionId = int.Parse(directionXml.Attribute("id")?.Value ?? "0");
                        Dictionary<string, Vector2I> bodyPartMap = new();
                        directionMap[directionId] = bodyPartMap;

                        foreach (XElement bodyPartXml in directionXml.Elements("bodypart"))
                        {
                            string bodyPartId = bodyPartXml.Attribute("id")?.Value ?? "";
                            int dx = bodyPartXml.Attribute("dx") != null
                                ? int.Parse(bodyPartXml.Attribute("dx")!.Value)
                                : 0;
                            int dy = bodyPartXml.Attribute("dy") != null
                                ? int.Parse(bodyPartXml.Attribute("dy")!.Value)
                                : 0;
                            bodyPartMap[bodyPartId] = new Vector2I(dx, dy);
                        }
                    }
                }

                _frameSequence.Add(frameId);

                int repeats = int.Parse(frameXml.Attribute("repeats")?.Value ?? "0");
                if (repeats > 1)
                {
                    while (true)
                    {
                        repeats--;
                        if (repeats <= 0)
                        {
                            break;
                        }
                        _frameSequence.Add(frameId);
                    }
                }
            }
        }
    }

    /// @see AnimationAction.as::getPart
    public AnimationActionPart? GetPart(string setType)
    {
        Parts.TryGetValue(setType, out AnimationActionPart? part);
        return part;
    }

    /// @see AnimationAction.as::get id
    public string Id { get; }

    /// @see AnimationAction.as::get parts
    public Dictionary<string, AnimationActionPart> Parts { get; }

    /// @see AnimationAction.as::get frameCount
    public int FrameCount { get; }

    /// @see AnimationAction.as::getFrameBodyPartOffset
    public Vector2I GetFrameBodyPartOffset(int direction, int frameIndex, string bodyPart)
    {
        if (_frameSequence.Count == 0)
        {
            return DefaultOffset;
        }

        int sequenceIndex = frameIndex % _frameSequence.Count;
        int frameId = _frameSequence[sequenceIndex];

        if (_offsets.TryGetValue(frameId, out Dictionary<int, Dictionary<string, Vector2I>>? directionMap))
        {
            if (directionMap.TryGetValue(direction, out Dictionary<string, Vector2I>? bodyPartMap))
            {
                if (bodyPartMap.TryGetValue(bodyPart, out Vector2I offset))
                {
                    return offset;
                }
            }
        }

        return DefaultOffset;
    }
}
