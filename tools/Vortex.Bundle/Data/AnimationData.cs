namespace Vortex.Bundle.Data;

/// <summary>
/// Animation data for figure parts (distinct from visualization animations).
/// Used by avatar figure rendering.
/// </summary>
public sealed class AnimationData
{
    /// <summary>String table index for the animation name.</summary>
    public uint NameIndex { get; set; }

    /// <summary>String table index for the description.</summary>
    public uint DescriptionIndex { get; set; }

    public bool ResetOnToggle { get; set; }

    public AnimSprite[] Sprites { get; set; } = [];

    public AnimFrame[] Frames { get; set; } = [];

    public AnimOverride[] Overrides { get; set; } = [];

    public AnimAdd[] Adds { get; set; } = [];

    public AnimRemove[] Removes { get; set; } = [];

    public AnimShadow[] Shadows { get; set; } = [];

    public AnimAvatar[] Avatars { get; set; } = [];
}

public struct AnimSprite
{
    /// <summary>String table index for sprite ID.</summary>
    public uint IdIndex;
    /// <summary>String table index for member.</summary>
    public uint MemberIndex;
    public byte Directions;
    public short StaticY;
    /// <summary>String table index for ink mode.</summary>
    public uint InkIndex;
}

public struct AnimFrame
{
    public ushort Number;
    public AnimFramePart[] Parts;
}

public struct AnimFramePart
{
    /// <summary>String table index for set type.</summary>
    public uint SetTypeIndex;
    /// <summary>String table index for set ID.</summary>
    public uint SetIdIndex;
    /// <summary>String table index for action.</summary>
    public uint ActionIndex;
    public short Dx;
    public short Dy;
    public short Dd;
}

public struct AnimOverride
{
    /// <summary>String table index for the override name.</summary>
    public uint NameIndex;
    /// <summary>String table index for override value.</summary>
    public uint OverrideIndex;
    public AnimFrame[] Frames;
}

public struct AnimAdd
{
    /// <summary>String table index for add ID.</summary>
    public uint IdIndex;
    /// <summary>String table index for align.</summary>
    public uint AlignIndex;
    /// <summary>String table index for base.</summary>
    public uint BaseIndex;
}

public struct AnimRemove
{
    /// <summary>String table index for remove ID.</summary>
    public uint IdIndex;
}

public struct AnimShadow
{
    /// <summary>String table index for shadow ID.</summary>
    public uint IdIndex;
}

public struct AnimAvatar
{
    /// <summary>String table index for ink mode.</summary>
    public uint InkIndex;
    /// <summary>String table index for foreground.</summary>
    public uint ForegroundIndex;
    /// <summary>String table index for background.</summary>
    public uint BackgroundIndex;
}
