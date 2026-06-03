using System.Xml.Linq;

using Vortex.Habbo.Room.Object.Visualization.Data;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.AnimatedFurnitureVisualizationData
public class AnimatedFurnitureVisualizationData : FurnitureVisualizationData
{
    protected override SizeData CreateSizeData(int size, int layerCount, int angle)
    {
        return new AnimationSizeData(layerCount, angle);
    }

    protected override bool ProcessVisualizationElement(SizeData sizeData, XElement element)
    {
        if (element.Name.LocalName != "animations")
        {
            return base.ProcessVisualizationElement(sizeData, element);
        }

        return sizeData is not AnimationSizeData animSizeData || animSizeData.DefineAnimations(element);

    }

    public bool HasAnimation(double scale, int animationId)
    {
        AnimationSizeData? sizeData = GetSizeData((int)scale) as AnimationSizeData;

        return sizeData?.HasAnimation(animationId) ?? false;
    }

    public int GetAnimationCount(double scale)
    {
        AnimationSizeData? sizeData = GetSizeData((int)scale) as AnimationSizeData;

        return sizeData?.GetAnimationCount() ?? 0;
    }

    public int GetAnimationId(double scale, int index)
    {
        AnimationSizeData? sizeData = GetSizeData((int)scale) as AnimationSizeData;

        return sizeData?.GetAnimationId(index) ?? 0;
    }

    public bool IsImmediateChange(double scale, int animationId, int fromAnimationId)
    {
        AnimationSizeData? sizeData = GetSizeData((int)scale) as AnimationSizeData;

        return sizeData?.IsImmediateChange(animationId, fromAnimationId) ?? false;
    }

    public int GetStartFrame(double scale, int animationId, int direction)
    {
        AnimationSizeData? sizeData = GetSizeData((int)scale) as AnimationSizeData;

        return sizeData?.GetStartFrame(animationId, direction) ?? 0;
    }

    public AnimationFrame? GetFrame(double scale, int animationId, int direction, int layer, int frameCounter)
    {
        AnimationSizeData? sizeData = GetSizeData((int)scale) as AnimationSizeData;

        return sizeData?.GetFrame(animationId, direction, layer, frameCounter);
    }

    public AnimationFrame? GetFrameFromSequence(
        double scale,
        int animationId,
        int direction,
        int layer,
        int sequenceIndex,
        int frameIndex,
        int overallFrame)
    {
        AnimationSizeData? sizeData = GetSizeData((int)scale) as AnimationSizeData;

        return sizeData?.GetFrameFromSequence(animationId, direction, layer, sequenceIndex, frameIndex, overallFrame);
    }

    public int GetLayerCount(double scale)
    {
        return base.GetLayerCount((int)scale);
    }
}
