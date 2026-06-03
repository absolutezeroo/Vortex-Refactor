namespace Vortex.Bundle.Data;

/// <summary>
/// Complete visualization data for a furniture/object at a specific size.
/// </summary>
public sealed class VisualizationData
{
    public ushort Size { get; set; }

    public ushort LayerCount { get; set; }

    public ushort Angle { get; set; }

    public VisualizationLayer[] Layers { get; set; } = [];

    public VisualizationColor[] Colors { get; set; } = [];

    public VisualizationDirection[] Directions { get; set; } = [];

    public VisualizationAnimation[] Animations { get; set; } = [];

    public VisualizationPosture[] Postures { get; set; } = [];

    public VisualizationGesture[] Gestures { get; set; } = [];
}

/// <summary>
/// A single layer entry within a visualization.
/// </summary>
public struct VisualizationLayer
{
    public ushort Id;
    public short X;
    public short Y;
    public short Z;
    public ushort Alpha;
    /// <summary>String table index for ink mode (e.g. "ADD"). NULL_STRING if none.</summary>
    public uint InkIndex;
    /// <summary>String table index for tag. NULL_STRING if none.</summary>
    public uint TagIndex;
    /// <summary>Bit 0: ignoreMouse.</summary>
    public byte Flags;

    public const byte FLAG_IGNORE_MOUSE = 1 << 0;

    public bool IgnoreMouse => (Flags & FLAG_IGNORE_MOUSE) != 0;
}

/// <summary>
/// Color overlay: maps a color ID to per-layer color tints.
/// </summary>
public sealed class VisualizationColor
{
    public uint ColorId { get; set; }

    public VisualizationColorLayer[] Layers { get; set; } = [];
}

public struct VisualizationColorLayer
{
    public ushort LayerId;
    /// <summary>ARGB packed color value.</summary>
    public uint Color;
}

/// <summary>
/// Direction override: allows per-direction z-order adjustments for layers.
/// </summary>
public sealed class VisualizationDirection
{
    public ushort DirectionId { get; set; }

    public VisualizationDirectionLayer[] LayerOverrides { get; set; } = [];
}

public struct VisualizationDirectionLayer
{
    public ushort LayerId;
    public short Z;
}

/// <summary>
/// Animation sequence within a visualization.
/// </summary>
public sealed class VisualizationAnimation
{
    public uint AnimationId { get; set; }

    public VisualizationAnimSequence[] Sequences { get; set; } = [];
}

/// <summary>
/// A layer-level animation sequence: which layer, and the frame indices.
/// </summary>
public sealed class VisualizationAnimSequence
{
    public ushort LayerId { get; set; }

    public ushort LoopCount { get; set; }

    public bool Random { get; set; }

    public VisualizationAnimFrame[] Frames { get; set; } = [];
}

public struct VisualizationAnimFrame
{
    /// <summary>Frame index (sprite suffix).</summary>
    public ushort Id;
    /// <summary>X offset for this frame.</summary>
    public short OffsetX;
    /// <summary>Y offset for this frame.</summary>
    public short OffsetY;
}

/// <summary>
/// Named posture referencing an animation ID.
/// </summary>
public struct VisualizationPosture
{
    /// <summary>String table index for posture name.</summary>
    public uint NameIndex;
    /// <summary>Animation ID to play.</summary>
    public uint AnimationId;
}

/// <summary>
/// Named gesture referencing an animation ID.
/// </summary>
public struct VisualizationGesture
{
    /// <summary>String table index for gesture name.</summary>
    public uint NameIndex;
    /// <summary>Animation ID to play.</summary>
    public uint AnimationId;
}
