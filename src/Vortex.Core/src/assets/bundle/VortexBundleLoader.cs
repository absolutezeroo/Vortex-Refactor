// Godot adaptation: utility methods for .vortex bundle data.

using Godot;

using Vortex.Bundle.Data;

namespace Vortex.Core.Assets.Bundle;

/// <summary>
/// Utility class for .vortex bundle operations in the Godot runtime.
/// Provides spritesheet creation from decoded bundle data.
/// </summary>
public static class VortexBundleLoader
{
    /// <summary>
    /// Creates a VortexSpritesheet from loaded bundle data.
    /// Decodes the spritesheet image bytes into a Godot Image.
    /// </summary>
    public static VortexSpritesheet? CreateSpritesheet(VortexBundleData data)
    {
        if (data.SpritesheetImage == null || data.SpritesheetMeta == null || data.StringTable == null)
        {
            return null;
        }

        Image image = new();
        Error err;

        if (data.UsesWebP)
        {
            err = image.LoadWebpFromBuffer(data.SpritesheetImage);
        }
        else
        {
            err = image.LoadPngFromBuffer(data.SpritesheetImage);
        }

        if (err == Error.Ok)
        {
            return new VortexSpritesheet(image, data.SpritesheetMeta, data.StringTable);
        }

        GD.PrintErr($"[VortexBundleLoader] Failed to decode spritesheet image: {err}");

        return null;

    }
}
