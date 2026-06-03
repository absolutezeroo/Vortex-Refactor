using System.Text;

using Vortex.Bundle.Data;

namespace Vortex.Bundle.IO;

/// <summary>
/// Writes a .vortex bundle file from in-memory data.
/// All multi-byte values are little-endian.
/// </summary>
public sealed class VortexBundleWriter
{
    /// <summary>
    /// Writes a complete .vortex bundle to the given stream.
    /// </summary>
    public static void Write(Stream output, VortexBundleData data, string[] stringTable)
    {
        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms, Encoding.UTF8, leaveOpen: true);

        // Build section buffers
        List<(ushort id, byte[] buffer)> sections = new();

        // String table must be first
        sections.Add((VortexBundleFormat.SECTION_STRING_TABLE, WriteStringTable(stringTable)));

        if (data.Assets is { Length: > 0 })
        {
            sections.Add((VortexBundleFormat.SECTION_ASSETS, WriteAssets(data.Assets)));
        }

        if (data.Aliases is { Length: > 0 })
        {
            sections.Add((VortexBundleFormat.SECTION_ALIASES, WriteAliases(data.Aliases)));
        }

        if (data.Visualizations is { Length: > 0 })
        {
            sections.Add((VortexBundleFormat.SECTION_VISUALIZATION, WriteVisualizations(data.Visualizations)));
        }

        if (data.Logic is not null)
        {
            sections.Add((VortexBundleFormat.SECTION_LOGIC, WriteLogic(data.Logic)));
        }

        if (data.Animations is { Length: > 0 })
        {
            sections.Add((VortexBundleFormat.SECTION_ANIMATION, WriteAnimations(data.Animations)));
        }

        if (data.Palettes is { Length: > 0 })
        {
            sections.Add((VortexBundleFormat.SECTION_PALETTES, WritePalettes(data.Palettes)));
        }

        if (data.SpritesheetMeta is not null)
        {
            sections.Add((VortexBundleFormat.SECTION_SPRITESHEET_META, WriteSpritesheetMeta(data.SpritesheetMeta)));
        }

        if (data.SpritesheetImage is { Length: > 0 })
        {
            sections.Add((VortexBundleFormat.SECTION_SPRITESHEET_IMAGE, WriteSpritesheetImage(data.SpritesheetImage)));
        }

        if (data.RawData is { Count: > 0 })
        {
            sections.Add((VortexBundleFormat.SECTION_RAW_DATA, WriteRawData(data.RawData, stringTable)));
        }

        // Calculate layout: header → ToC → sections
        uint tocOffset = VortexBundleFormat.HeaderSize;
        uint tocSize = (uint)(sections.Count * VortexBundleFormat.TocEntrySize);
        uint sectionStart = tocOffset + tocSize;

        // Build ToC entries with absolute offsets
        TocEntry[] tocEntries = new TocEntry[sections.Count];
        uint currentOffset = sectionStart;

        for (int i = 0; i < sections.Count; i++)
        {
            tocEntries[i] = new TocEntry
            {
                SectionId = sections[i].id,
                Reserved = 0,
                Offset = currentOffset,
                Length = (uint)sections[i].buffer.Length,
            };

            currentOffset += (uint)sections[i].buffer.Length;
        }

        // Write header
        bw.Write(VortexBundleFormat.Magic);
        bw.Write(data.Version);
        bw.Write(data.Flags);
        bw.Write(tocOffset);
        bw.Write((uint)sections.Count);

        // Write ToC
        foreach (TocEntry entry in tocEntries)
        {
            bw.Write(entry.SectionId);
            bw.Write(entry.Reserved);
            bw.Write(entry.Offset);
            bw.Write(entry.Length);
        }

        // Write section data
        foreach ((ushort _, byte[] buffer) in sections)
        {
            bw.Write(buffer);
        }

        bw.Flush();
        ms.Position = 0;
        ms.CopyTo(output);
    }

    private static byte[] WriteStringTable(string[] strings)
    {
        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms, Encoding.UTF8);

        bw.Write((uint)strings.Length);

        foreach (string s in strings)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);

            bw.Write((ushort)bytes.Length);
            bw.Write(bytes);
        }

        return ms.ToArray();
    }

    private static byte[] WriteAssets(AssetEntry[] assets)
    {
        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms);

        bw.Write((uint)assets.Length);

        foreach (AssetEntry a in assets)
        {
            bw.Write(a.NameIndex);
            bw.Write(a.OffsetX);
            bw.Write(a.OffsetY);
            bw.Write(a.SourceIndex);
            bw.Write(a.Flags);
        }

        return ms.ToArray();
    }

    private static byte[] WriteAliases(AliasEntry[] aliases)
    {
        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms);

        bw.Write((uint)aliases.Length);
        foreach (AliasEntry a in aliases)
        {
            bw.Write(a.NameIndex);
            bw.Write(a.LinkIndex);
            bw.Write(a.Flags);
        }

        return ms.ToArray();
    }

    private static byte[] WriteVisualizations(VisualizationData[] vizs)
    {
        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms);

        bw.Write((ushort)vizs.Length);
        foreach (VisualizationData viz in vizs)
        {
            bw.Write(viz.Size);
            bw.Write(viz.LayerCount);
            bw.Write(viz.Angle);

            // Layers
            bw.Write((ushort)viz.Layers.Length);
            foreach (VisualizationLayer layer in viz.Layers)
            {
                bw.Write(layer.Id);
                bw.Write(layer.X);
                bw.Write(layer.Y);
                bw.Write(layer.Z);
                bw.Write(layer.Alpha);
                bw.Write(layer.InkIndex);
                bw.Write(layer.TagIndex);
                bw.Write(layer.Flags);
            }

            // Colors
            bw.Write((ushort)viz.Colors.Length);
            foreach (VisualizationColor color in viz.Colors)
            {
                bw.Write(color.ColorId);
                bw.Write((ushort)color.Layers.Length);
                foreach (VisualizationColorLayer cl in color.Layers)
                {
                    bw.Write(cl.LayerId);
                    bw.Write(cl.Color);
                }
            }

            // Directions
            bw.Write((ushort)viz.Directions.Length);
            foreach (VisualizationDirection dir in viz.Directions)
            {
                bw.Write(dir.DirectionId);
                bw.Write((ushort)dir.LayerOverrides.Length);
                foreach (VisualizationDirectionLayer over in dir.LayerOverrides)
                {
                    bw.Write(over.LayerId);
                    bw.Write(over.Z);
                }
            }

            // Animations
            bw.Write((ushort)viz.Animations.Length);
            foreach (VisualizationAnimation anim in viz.Animations)
            {
                bw.Write(anim.AnimationId);
                bw.Write((ushort)anim.Sequences.Length);
                foreach (VisualizationAnimSequence seq in anim.Sequences)
                {
                    bw.Write(seq.LayerId);
                    bw.Write(seq.LoopCount);
                    bw.Write((byte)(seq.Random ? 1 : 0));
                    bw.Write((ushort)seq.Frames.Length);
                    foreach (VisualizationAnimFrame frame in seq.Frames)
                    {
                        bw.Write(frame.Id);
                        bw.Write(frame.OffsetX);
                        bw.Write(frame.OffsetY);
                    }
                }
            }

            // Postures
            bw.Write((ushort)viz.Postures.Length);
            foreach (VisualizationPosture posture in viz.Postures)
            {
                bw.Write(posture.NameIndex);
                bw.Write(posture.AnimationId);
            }

            // Gestures
            bw.Write((ushort)viz.Gestures.Length);
            foreach (VisualizationGesture gesture in viz.Gestures)
            {
                bw.Write(gesture.NameIndex);
                bw.Write(gesture.AnimationId);
            }
        }

        return ms.ToArray();
    }

    private static byte[] WriteLogic(LogicData logic)
    {
        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms);

        bw.Write((byte)(logic.HasModel ? 1 : 0));
        bw.Write((byte)(logic.HasAction ? 1 : 0));
        bw.Write((byte)(logic.HasSound ? 1 : 0));
        bw.Write((byte)(logic.HasParticles ? 1 : 0));

        if (logic.HasModel)
        {
            bw.Write(logic.DimensionX);
            bw.Write(logic.DimensionY);
            bw.Write(logic.DimensionZ);
            bw.Write(logic.CenterZ);
            bw.Write((ushort)logic.Directions.Length);
            foreach (ushort dir in logic.Directions)
            {
                bw.Write(dir);
            }
        }

        if (logic.HasAction)
        {
            bw.Write(logic.ActionLinkIndex);
            bw.Write(logic.ActionStartState);
        }

        if (logic.HasSound)
        {
            bw.Write(logic.SoundSampleId);
            bw.Write(logic.SoundNameIndex);
        }

        if (logic.HasParticles)
        {
            bw.Write((ushort)logic.ParticleBlobs.Length);
            foreach (byte[] blob in logic.ParticleBlobs)
            {
                bw.Write((uint)blob.Length);
                bw.Write(blob);
            }
        }

        return ms.ToArray();
    }

    private static byte[] WriteAnimations(AnimationData[] anims)
    {
        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms);

        bw.Write((ushort)anims.Length);
        foreach (AnimationData anim in anims)
        {
            bw.Write(anim.NameIndex);
            bw.Write(anim.DescriptionIndex);
            bw.Write((byte)(anim.ResetOnToggle ? 1 : 0));

            // Sprites
            bw.Write((ushort)anim.Sprites.Length);
            foreach (AnimSprite sprite in anim.Sprites)
            {
                bw.Write(sprite.IdIndex);
                bw.Write(sprite.MemberIndex);
                bw.Write(sprite.Directions);
                bw.Write(sprite.StaticY);
                bw.Write(sprite.InkIndex);
            }

            // Frames
            bw.Write((ushort)anim.Frames.Length);
            foreach (AnimFrame frame in anim.Frames)
            {
                bw.Write(frame.Number);
                bw.Write((ushort)frame.Parts.Length);
                foreach (AnimFramePart part in frame.Parts)
                {
                    bw.Write(part.SetTypeIndex);
                    bw.Write(part.SetIdIndex);
                    bw.Write(part.ActionIndex);
                    bw.Write(part.Dx);
                    bw.Write(part.Dy);
                    bw.Write(part.Dd);
                }
            }

            // Overrides
            bw.Write((ushort)anim.Overrides.Length);
            foreach (AnimOverride over in anim.Overrides)
            {
                bw.Write(over.NameIndex);
                bw.Write(over.OverrideIndex);
                bw.Write((ushort)over.Frames.Length);
                foreach (AnimFrame frame in over.Frames)
                {
                    bw.Write(frame.Number);
                    bw.Write((ushort)frame.Parts.Length);
                    foreach (AnimFramePart part in frame.Parts)
                    {
                        bw.Write(part.SetTypeIndex);
                        bw.Write(part.SetIdIndex);
                        bw.Write(part.ActionIndex);
                        bw.Write(part.Dx);
                        bw.Write(part.Dy);
                        bw.Write(part.Dd);
                    }
                }
            }

            // Adds
            bw.Write((ushort)anim.Adds.Length);
            foreach (AnimAdd add in anim.Adds)
            {
                bw.Write(add.IdIndex);
                bw.Write(add.AlignIndex);
                bw.Write(add.BaseIndex);
            }

            // Removes
            bw.Write((ushort)anim.Removes.Length);
            foreach (AnimRemove rem in anim.Removes)
            {
                bw.Write(rem.IdIndex);
            }

            // Shadows
            bw.Write((ushort)anim.Shadows.Length);
            foreach (AnimShadow shadow in anim.Shadows)
            {
                bw.Write(shadow.IdIndex);
            }

            // Avatars
            bw.Write((ushort)anim.Avatars.Length);
            foreach (AnimAvatar avatar in anim.Avatars)
            {
                bw.Write(avatar.InkIndex);
                bw.Write(avatar.ForegroundIndex);
                bw.Write(avatar.BackgroundIndex);
            }
        }

        return ms.ToArray();
    }

    private static byte[] WritePalettes(PaletteData[] palettes)
    {
        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms);

        bw.Write((ushort)palettes.Length);
        foreach (PaletteData pal in palettes)
        {
            bw.Write(pal.Id);
            bw.Write(pal.SourceIndex);
            bw.Write((byte)(pal.Master ? 1 : 0));
            bw.Write(pal.Breed);
            bw.Write(pal.ColorTag);
            bw.Write(pal.Color1Index);
            bw.Write(pal.Color2Index);

            bw.Write((ushort)pal.TagIndices.Length);
            foreach (uint idx in pal.TagIndices)
            {
                bw.Write(idx);
            }

            bw.Write((ushort)pal.Colors.Length);
            foreach (PaletteColor c in pal.Colors)
            {
                bw.Write(c.R);
                bw.Write(c.G);
                bw.Write(c.B);
            }
        }

        return ms.ToArray();
    }

    private static byte[] WriteSpritesheetMeta(SpritesheetMeta meta)
    {
        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms);

        bw.Write(meta.Width);
        bw.Write(meta.Height);
        bw.Write((uint)meta.Frames.Length);
        foreach (FrameData f in meta.Frames)
        {
            bw.Write(f.NameIndex);
            bw.Write(f.X);
            bw.Write(f.Y);
            bw.Write(f.Width);
            bw.Write(f.Height);
            bw.Write(f.SourceWidth);
            bw.Write(f.SourceHeight);
            bw.Write(f.TrimX);
            bw.Write(f.TrimY);
        }

        return ms.ToArray();
    }

    private static byte[] WriteSpritesheetImage(byte[] imageData)
    {
        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms);

        bw.Write((uint)imageData.Length);
        bw.Write(imageData);

        return ms.ToArray();
    }

    private static byte[] WriteRawData(Dictionary<string, byte[]> rawData, string[] stringTable)
    {
        // Build reverse lookup: name → index
        Dictionary<string, uint> nameToIndex = new(stringTable.Length);

        for (int i = 0; i < stringTable.Length; i++)
        {
            nameToIndex[stringTable[i]] = (uint)i;
        }

        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms);

        bw.Write((uint)rawData.Count);

        foreach ((string name, byte[] data) in rawData)
        {
            if (!nameToIndex.TryGetValue(name, out uint nameIndex))
            {
                continue;
            }

            bw.Write(nameIndex);
            bw.Write((uint)data.Length);
            bw.Write(data);
        }

        return ms.ToArray();
    }
}
