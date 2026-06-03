namespace Vortex.Bundle.Data;

/// <summary>
/// Logic metadata for a furniture/object: dimensions, valid directions, actions, sounds.
/// </summary>
public sealed class LogicData
{
    public bool HasModel { get; set; }

    public bool HasAction { get; set; }

    public bool HasSound { get; set; }

    public bool HasParticles { get; set; }

    // --- Model ---
    public short DimensionX { get; set; }

    public short DimensionY { get; set; }

    public float DimensionZ { get; set; }

    public float CenterZ { get; set; }

    public ushort[] Directions { get; set; } = [];

    // --- Action ---
    /// <summary>String table index for the action link. NULL_STRING if none.</summary>
    public uint ActionLinkIndex { get; set; } = VortexBundleFormat.NULL_STRING;

    public int ActionStartState { get; set; }

    // --- Sound ---
    public uint SoundSampleId { get; set; }

    /// <summary>String table index for the sound name. NULL_STRING if none.</summary>
    public uint SoundNameIndex { get; set; } = VortexBundleFormat.NULL_STRING;

    // --- Particles ---
    public byte[][] ParticleBlobs { get; set; } = [];
}
