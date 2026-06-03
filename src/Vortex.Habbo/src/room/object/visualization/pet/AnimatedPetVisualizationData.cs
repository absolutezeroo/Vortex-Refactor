using System.Xml.Linq;

using Vortex.Core.Assets;
using Vortex.Habbo.Room.Object.Visualization.Data;
using Vortex.Habbo.Room.Object.Visualization.Furniture;

namespace Vortex.Habbo.Room.Object.Visualization.Pet;

/// @see com.sulake.habbo.room.object.visualization.pet.AnimatedPetVisualizationData
public class AnimatedPetVisualizationData : AnimatedFurnitureVisualizationData
{
    public IAssetLibrary? CommonAssets { get; set; }

    public bool IsAllowedToTurnHead { get; private set; } = true;

    /// @see AnimatedPetVisualizationData.as::defineVisualizations
    protected override bool DefineVisualizations(XElement xml)
    {
        string? disableHeadTurn = xml.Element("graphics")?.Attribute("disableheadturn")?.Value;
        IsAllowedToTurnHead = disableHeadTurn != "1";

        return base.DefineVisualizations(xml);
    }

    /// @see AnimatedPetVisualizationData.as::createSizeData
    protected override SizeData CreateSizeData(int size, int layerCount, int angle)
    {
        if (size > 1)
        {
            return new PetAnimationSizeData(layerCount, angle);
        }

        return new AnimationSizeData(layerCount, angle);
    }

    /// @see AnimatedPetVisualizationData.as::processVisualizationElement
    protected override bool ProcessVisualizationElement(SizeData? sizeData, XElement? element)
    {
        if (sizeData == null || element == null)
        {
            return false;
        }

        string elementName = element.Name.LocalName;

        switch (elementName)
        {
            case "postures":
                {
                    if (sizeData is PetAnimationSizeData petData)
                    {
                        if (!petData.DefinePostures(element))
                        {
                            return false;
                        }
                    }

                    break;
                }

            case "gestures":
                {
                    if (sizeData is PetAnimationSizeData petData)
                    {
                        if (!petData.DefineGestures(element))
                        {
                            return false;
                        }
                    }

                    break;
                }

            default:
                if (!base.ProcessVisualizationElement(sizeData, element))
                {
                    return false;
                }

                break;
        }

        return true;
    }

    public int GetAnimationForPosture(int scale, string? posture)
    {
        if (GetSizeData(scale) is PetAnimationSizeData petData)
        {
            return petData.GetAnimationForPosture(posture);
        }

        return -1;
    }

    public bool GetGestureDisabled(int scale, string? posture)
    {
        if (GetSizeData(scale) is PetAnimationSizeData petData)
        {
            return petData.GetGestureDisabled(posture);
        }

        return false;
    }

    public int GetAnimationForGesture(int scale, string? gesture)
    {
        if (GetSizeData(scale) is PetAnimationSizeData petData)
        {
            return petData.GetAnimationForGesture(gesture);
        }

        return -1;
    }

    public string? GetPostureForAnimation(int scale, int index, bool fallbackToDefault)
    {
        if (GetSizeData(scale) is PetAnimationSizeData petData)
        {
            return petData.GetPostureForAnimation(index, fallbackToDefault);
        }

        return null;
    }

    public string? GetGestureForAnimation(int scale, int index)
    {
        if (GetSizeData(scale) is PetAnimationSizeData petData)
        {
            return petData.GetGestureForAnimation(index);
        }

        return null;
    }

    public string? GetGestureForAnimationId(int scale, int animationId)
    {
        if (GetSizeData(scale) is PetAnimationSizeData petData)
        {
            return petData.GetGestureForAnimationId(animationId);
        }

        return null;
    }

    public int GetPostureCount(int scale)
    {
        if (GetSizeData(scale) is PetAnimationSizeData petData)
        {
            return petData.GetPostureCount();
        }

        return 0;
    }

    public int GetGestureCount(int scale)
    {
        if (GetSizeData(scale) is PetAnimationSizeData petData)
        {
            return petData.GetGestureCount();
        }

        return 0;
    }
}
