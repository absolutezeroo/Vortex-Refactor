// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/class_3543.as::resampleBitmapData

using Godot;

namespace Vortex.Habbo.Avatar;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/class_3543.as
public static class AvatarBitmapUtils
{
    /// Iterative half-step downsample per AS3 class_3543.resampleBitmapData.
    /// Halves dimensions repeatedly until within 2× of target, then does a final resize.
    /// This produces much better quality than a single large-ratio resize.
    /// @see class_3543.as::resampleBitmapData
    public static Image ResampleBitmapData(Image source, double scaleFactor)
    {
        if (scaleFactor <= 0.0)
        {
            return (Image)source.Duplicate();
        }

        int targetW = System.Math.Max(1, (int)(source.GetWidth() * scaleFactor));
        int targetH = System.Math.Max(1, (int)(source.GetHeight() * scaleFactor));

        // Upscale: simple resize (AS3 Matrix scale with smoothing=true)
        if (scaleFactor >= 1.0)
        {
            if (targetW == source.GetWidth() && targetH == source.GetHeight())
            {
                return (Image)source.Duplicate();
            }

            Image? upscaled = (Image)source.Duplicate();
            upscaled.Resize(targetW, targetH, Image.Interpolation.Bilinear);
            return upscaled;
        }

        Image? current = (Image)source.Duplicate();

        // Iterative half-step: halve until within 2× of target
        while (current.GetWidth() / 2 > targetW && current.GetHeight() / 2 > targetH)
        {
            int halfW = System.Math.Max(1, current.GetWidth() / 2);
            int halfH = System.Math.Max(1, current.GetHeight() / 2);

            Image? halved = Image.CreateEmpty(halfW, halfH, false, current.GetFormat());
            halved.CopyFrom(current);
            halved.Resize(halfW, halfH, Image.Interpolation.Bilinear);

            current.Dispose();
            current = halved;
        }

        // Final resize to exact target dimensions
        if (current.GetWidth() == targetW && current.GetHeight() == targetH)
        {
            return current;
        }

        Image? final = Image.CreateEmpty(targetW, targetH, false, current.GetFormat());

        final.CopyFrom(current);
        final.Resize(targetW, targetH);

        current.Dispose();
        current = final;

        return current;
    }
}
