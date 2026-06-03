// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/animation/AnimationActionPart.as

using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Structure.Animation;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/animation/AnimationActionPart.as
public class AnimationActionPart
{
    /// @see AnimationActionPart.as::AnimationActionPart
    public AnimationActionPart(XElement xml)
    {
        Frames = new List<AnimationFrame>();

        foreach (XElement frameXml in xml.Elements("frame"))
        {
            AnimationFrame frame = new(frameXml);
            Frames.Add(frame);

            int repeats = int.Parse(frameXml.Attribute("repeats")?.Value ?? "0");

            if (repeats <= 1)
            {
                continue;
            }

            while (true)
            {
                repeats--;

                if (repeats <= 0)
                {
                    break;
                }
                // Push same reference as AS3
                Frames.Add(Frames[^1]);
            }
        }
    }

    /// @see AnimationActionPart.as::get frames
    public List<AnimationFrame> Frames { get; }
}
