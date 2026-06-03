namespace Vortex.Bundle.Data;

/// <summary>
/// Spritesheet metadata: atlas dimensions and frame descriptors.
/// </summary>
public sealed class SpritesheetMeta
{
    public ushort Width { get; set; }

    public ushort Height { get; set; }

    public FrameData[] Frames { get; set; } = [];
}
