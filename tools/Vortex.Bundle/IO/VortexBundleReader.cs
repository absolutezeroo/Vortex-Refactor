using System.Text;

using Vortex.Bundle.Data;

namespace Vortex.Bundle.IO;

/// <summary>
/// Reads a .vortex bundle file into in-memory data structures.
/// Supports selective section loading (e.g., metadata only, skip spritesheet image).
/// </summary>
public sealed class VortexBundleReader
{
    /// <summary>
    /// Reads all sections from a .vortex bundle.
    /// </summary>
    public VortexBundleData Read(Stream input)
    {
        return Read(input, readImage: true);
    }

    /// <summary>
    /// Reads a .vortex bundle, optionally skipping the spritesheet image.
    /// </summary>
    public VortexBundleData Read(Stream input, bool readImage)
    {
        using BinaryReader br = new(input, Encoding.UTF8, leaveOpen: true);

        VortexBundleData data = new();

        // --- Header (16 bytes) ---
        Span<byte> magic = stackalloc byte[4];
        if (br.Read(magic) < 4)
        {
            throw new InvalidDataException("Unexpected end of stream reading header.");
        }

        if (!magic.SequenceEqual(VortexBundleFormat.Magic))
        {
            throw new InvalidDataException(
                $"Invalid magic: expected VRTX, got {Encoding.ASCII.GetString(magic)}.");
        }

        data.Version = br.ReadUInt16();
        if (data.Version > VortexBundleFormat.CurrentVersion)
        {
            throw new InvalidDataException(
                $"Unsupported version {data.Version} (max={VortexBundleFormat.CurrentVersion}).");
        }

        data.Flags = br.ReadUInt16();
        uint tocOffset = br.ReadUInt32();
        uint tocCount = br.ReadUInt32();

        // --- Table of Contents ---
        input.Seek(tocOffset, SeekOrigin.Begin);
        TocEntry[] toc = new TocEntry[tocCount];
        for (int i = 0; i < tocCount; i++)
        {
            toc[i] = new TocEntry
            {
                SectionId = br.ReadUInt16(), Reserved = br.ReadUInt16(), Offset = br.ReadUInt32(), Length = br.ReadUInt32(),
            };
        }

        // --- Read sections by ID ---

        // String table must be read first
        foreach (TocEntry entry in toc)
        {
            if (entry.SectionId != VortexBundleFormat.SECTION_STRING_TABLE)
            {
                continue;
            }

            input.Seek(entry.Offset, SeekOrigin.Begin);

            StringTable? stringTable = ReadStringTable(br);
            data.StringTable = stringTable;

            break;
        }

        foreach (TocEntry entry in toc)
        {
            switch (entry.SectionId)
            {
                case VortexBundleFormat.SECTION_STRING_TABLE:
                case VortexBundleFormat.SECTION_SPRITESHEET_IMAGE when !readImage:
                    continue; // Already read
                default:
                    input.Seek(entry.Offset, SeekOrigin.Begin);

                    switch (entry.SectionId)
                    {
                        case VortexBundleFormat.SECTION_ASSETS:
                            data.Assets = ReadAssets(br);

                            break;
                        case VortexBundleFormat.SECTION_ALIASES:
                            data.Aliases = ReadAliases(br);

                            break;
                        case VortexBundleFormat.SECTION_VISUALIZATION:
                            data.Visualizations = ReadVisualizations(br);

                            break;
                        case VortexBundleFormat.SECTION_LOGIC:
                            data.Logic = ReadLogic(br);

                            break;
                        case VortexBundleFormat.SECTION_ANIMATION:
                            data.Animations = ReadAnimations(br);

                            break;
                        case VortexBundleFormat.SECTION_PALETTES:
                            data.Palettes = ReadPalettes(br);

                            break;
                        case VortexBundleFormat.SECTION_SPRITESHEET_META:
                            data.SpritesheetMeta = ReadSpritesheetMeta(br);

                            break;
                        case VortexBundleFormat.SECTION_SPRITESHEET_IMAGE:
                            data.SpritesheetImage = ReadSpritesheetImage(br);

                            break;
                        case VortexBundleFormat.SECTION_RAW_DATA:
                            data.RawData = ReadRawData(br, data.StringTable);

                            break;
                    }

                    break;
            }

        }

        return data;
    }

    private static StringTable ReadStringTable(BinaryReader br)
    {
        uint count = br.ReadUInt32();
        string[] entries = new string[count];

        for (uint i = 0; i < count; i++)
        {
            ushort byteLen = br.ReadUInt16();
            byte[] bytes = br.ReadBytes(byteLen);
            entries[i] = Encoding.UTF8.GetString(bytes);
        }

        return new StringTable(entries);
    }

    private static AssetEntry[] ReadAssets(BinaryReader br)
    {
        uint count = br.ReadUInt32();
        AssetEntry[] assets = new AssetEntry[count];

        for (uint i = 0; i < count; i++)
        {
            assets[i] = new AssetEntry
            {
                NameIndex = br.ReadUInt32(),
                OffsetX = br.ReadInt16(),
                OffsetY = br.ReadInt16(),
                SourceIndex = br.ReadUInt32(),
                Flags = br.ReadByte(),
            };
        }

        return assets;
    }

    private static AliasEntry[] ReadAliases(BinaryReader br)
    {
        uint count = br.ReadUInt32();
        AliasEntry[] aliases = new AliasEntry[count];

        for (uint i = 0; i < count; i++)
        {
            aliases[i] = new AliasEntry
            {
                NameIndex = br.ReadUInt32(), LinkIndex = br.ReadUInt32(), Flags = br.ReadByte(),
            };
        }

        return aliases;
    }

    private static VisualizationData[] ReadVisualizations(BinaryReader br)
    {
        ushort vizCount = br.ReadUInt16();
        VisualizationData[] vizs = new VisualizationData[vizCount];

        for (int v = 0; v < vizCount; v++)
        {
            VisualizationData viz = new()
            {
                Size = br.ReadUInt16(), LayerCount = br.ReadUInt16(), Angle = br.ReadUInt16(),
            };

            // Layers
            ushort layerCount = br.ReadUInt16();
            viz.Layers = new VisualizationLayer[layerCount];
            for (int i = 0; i < layerCount; i++)

            {
                viz.Layers[i] = new VisualizationLayer
                {
                    Id = br.ReadUInt16(),
                    X = br.ReadInt16(),
                    Y = br.ReadInt16(),
                    Z = br.ReadInt16(),
                    Alpha = br.ReadUInt16(),
                    InkIndex = br.ReadUInt32(),
                    TagIndex = br.ReadUInt32(),
                    Flags = br.ReadByte(),
                };
            }

            // Colors
            ushort colorCount = br.ReadUInt16();
            viz.Colors = new VisualizationColor[colorCount];

            for (int i = 0; i < colorCount; i++)
            {
                VisualizationColor color = new()
                {
                    ColorId = br.ReadUInt32(),
                };
                ushort clCount = br.ReadUInt16();
                color.Layers = new VisualizationColorLayer[clCount];

                for (int j = 0; j < clCount; j++)
                {
                    color.Layers[j] = new VisualizationColorLayer
                    {
                        LayerId = br.ReadUInt16(), Color = br.ReadUInt32(),
                    };
                }

                viz.Colors[i] = color;
            }

            // Directions
            ushort dirCount = br.ReadUInt16();
            viz.Directions = new VisualizationDirection[dirCount];

            for (int i = 0; i < dirCount; i++)
            {
                VisualizationDirection dir = new()
                {
                    DirectionId = br.ReadUInt16(),
                };
                ushort overCount = br.ReadUInt16();
                dir.LayerOverrides = new VisualizationDirectionLayer[overCount];

                for (int j = 0; j < overCount; j++)
                {
                    dir.LayerOverrides[j] = new VisualizationDirectionLayer
                    {
                        LayerId = br.ReadUInt16(), Z = br.ReadInt16(),
                    };
                }

                viz.Directions[i] = dir;
            }

            // Animations
            ushort animCount = br.ReadUInt16();
            viz.Animations = new VisualizationAnimation[animCount];

            for (int i = 0; i < animCount; i++)
            {
                VisualizationAnimation anim = new()
                {
                    AnimationId = br.ReadUInt32(),
                };
                ushort seqCount = br.ReadUInt16();
                anim.Sequences = new VisualizationAnimSequence[seqCount];

                for (int j = 0; j < seqCount; j++)
                {
                    VisualizationAnimSequence seq = new()
                    {
                        LayerId = br.ReadUInt16(), LoopCount = br.ReadUInt16(), Random = br.ReadByte() != 0,
                    };
                    ushort frameCount = br.ReadUInt16();
                    seq.Frames = new VisualizationAnimFrame[frameCount];

                    for (int k = 0; k < frameCount; k++)
                    {
                        seq.Frames[k] = new VisualizationAnimFrame
                        {
                            Id = br.ReadUInt16(), OffsetX = br.ReadInt16(), OffsetY = br.ReadInt16(),
                        };
                    }

                    anim.Sequences[j] = seq;
                }

                viz.Animations[i] = anim;
            }

            // Postures
            ushort postureCount = br.ReadUInt16();
            viz.Postures = new VisualizationPosture[postureCount];

            for (int i = 0; i < postureCount; i++)
            {
                viz.Postures[i] = new VisualizationPosture
                {
                    NameIndex = br.ReadUInt32(), AnimationId = br.ReadUInt32(),
                };
            }

            // Gestures
            ushort gestureCount = br.ReadUInt16();
            viz.Gestures = new VisualizationGesture[gestureCount];

            for (int i = 0; i < gestureCount; i++)
            {
                viz.Gestures[i] = new VisualizationGesture
                {
                    NameIndex = br.ReadUInt32(), AnimationId = br.ReadUInt32(),
                };
            }

            vizs[v] = viz;
        }

        return vizs;
    }

    private static LogicData ReadLogic(BinaryReader br)
    {
        LogicData logic = new()
        {
            HasModel = br.ReadByte() != 0, HasAction = br.ReadByte() != 0, HasSound = br.ReadByte() != 0, HasParticles = br.ReadByte() != 0,
        };

        if (logic.HasModel)
        {
            logic.DimensionX = br.ReadInt16();
            logic.DimensionY = br.ReadInt16();
            logic.DimensionZ = br.ReadSingle();
            logic.CenterZ = br.ReadSingle();
            ushort dirCount = br.ReadUInt16();
            logic.Directions = new ushort[dirCount];

            for (int i = 0; i < dirCount; i++)
            {
                logic.Directions[i] = br.ReadUInt16();
            }
        }

        if (logic.HasAction)
        {
            logic.ActionLinkIndex = br.ReadUInt32();
            logic.ActionStartState = br.ReadInt32();
        }

        if (logic.HasSound)
        {
            logic.SoundSampleId = br.ReadUInt32();
            logic.SoundNameIndex = br.ReadUInt32();
        }

        if (logic.HasParticles)
        {
            ushort count = br.ReadUInt16();
            logic.ParticleBlobs = new byte[count][];

            for (int i = 0; i < count; i++)
            {
                uint blobLen = br.ReadUInt32();
                logic.ParticleBlobs[i] = br.ReadBytes((int)blobLen);
            }
        }

        return logic;
    }

    private static AnimationData[] ReadAnimations(BinaryReader br)
    {
        ushort animCount = br.ReadUInt16();
        AnimationData[] anims = new AnimationData[animCount];

        for (int a = 0; a < animCount; a++)
        {
            AnimationData anim = new()
            {
                NameIndex = br.ReadUInt32(), DescriptionIndex = br.ReadUInt32(), ResetOnToggle = br.ReadByte() != 0,
            };

            // Sprites
            ushort spriteCount = br.ReadUInt16();
            anim.Sprites = new AnimSprite[spriteCount];

            for (int i = 0; i < spriteCount; i++)
            {
                anim.Sprites[i] = new AnimSprite
                {
                    IdIndex = br.ReadUInt32(),
                    MemberIndex = br.ReadUInt32(),
                    Directions = br.ReadByte(),
                    StaticY = br.ReadInt16(),
                    InkIndex = br.ReadUInt32(),
                };
            }

            // Frames
            ushort frameCount = br.ReadUInt16();
            anim.Frames = new AnimFrame[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                anim.Frames[i].Number = br.ReadUInt16();
                ushort partCount = br.ReadUInt16();
                anim.Frames[i].Parts = new AnimFramePart[partCount];

                for (int j = 0; j < partCount; j++)
                {
                    anim.Frames[i].Parts[j] = new AnimFramePart
                    {
                        SetTypeIndex = br.ReadUInt32(),
                        SetIdIndex = br.ReadUInt32(),
                        ActionIndex = br.ReadUInt32(),
                        Dx = br.ReadInt16(),
                        Dy = br.ReadInt16(),
                        Dd = br.ReadInt16(),
                    };
                }
            }

            // Overrides
            ushort overCount = br.ReadUInt16();
            anim.Overrides = new AnimOverride[overCount];

            for (int i = 0; i < overCount; i++)
            {
                anim.Overrides[i].NameIndex = br.ReadUInt32();
                anim.Overrides[i].OverrideIndex = br.ReadUInt32();
                ushort oFrameCount = br.ReadUInt16();
                anim.Overrides[i].Frames = new AnimFrame[oFrameCount];

                for (int j = 0; j < oFrameCount; j++)
                {
                    anim.Overrides[i].Frames[j].Number = br.ReadUInt16();
                    ushort partCount = br.ReadUInt16();
                    anim.Overrides[i].Frames[j].Parts = new AnimFramePart[partCount];
 for (int k = 0; k < partCount; k++)
                    {
                        anim.Overrides[i].Frames[j].Parts[k] = new AnimFramePart
                        {
                            SetTypeIndex = br.ReadUInt32(),
                            SetIdIndex = br.ReadUInt32(),
                            ActionIndex = br.ReadUInt32(),
                            Dx = br.ReadInt16(),
                            Dy = br.ReadInt16(),
                            Dd = br.ReadInt16(),
                        };
                    }
                }
            }

            // Adds
            ushort addCount = br.ReadUInt16();
            anim.Adds = new AnimAdd[addCount];

            for (int i = 0; i < addCount; i++)
            {
                anim.Adds[i] = new AnimAdd
                {
                    IdIndex = br.ReadUInt32(), AlignIndex = br.ReadUInt32(), BaseIndex = br.ReadUInt32(),
                };
            }

            // Removes
            ushort removeCount = br.ReadUInt16();
            anim.Removes = new AnimRemove[removeCount];

            for (int i = 0; i < removeCount; i++)
            {
                anim.Removes[i] = new AnimRemove
                {
                    IdIndex = br.ReadUInt32(),
                };
            }

            // Shadows
            ushort shadowCount = br.ReadUInt16();
            anim.Shadows = new AnimShadow[shadowCount];

            for (int i = 0; i < shadowCount; i++)
            {
                anim.Shadows[i] = new AnimShadow
                {
                    IdIndex = br.ReadUInt32(),
                };
            }

            // Avatars
            ushort avatarCount = br.ReadUInt16();
            anim.Avatars = new AnimAvatar[avatarCount];

            for (int i = 0; i < avatarCount; i++)
            {
                anim.Avatars[i] = new AnimAvatar
                {
                    InkIndex = br.ReadUInt32(), ForegroundIndex = br.ReadUInt32(), BackgroundIndex = br.ReadUInt32(),
                };
            }

            anims[a] = anim;
        }

        return anims;
    }

    private static PaletteData[] ReadPalettes(BinaryReader br)
    {
        ushort count = br.ReadUInt16();
        PaletteData[] palettes = new PaletteData[count];

        for (int i = 0; i < count; i++)
        {
            PaletteData pal = new()
            {
                Id = br.ReadUInt32(),
                SourceIndex = br.ReadUInt32(),
                Master = br.ReadByte() != 0,
                Breed = br.ReadUInt16(),
                ColorTag = br.ReadUInt16(),
                Color1Index = br.ReadUInt32(),
                Color2Index = br.ReadUInt32(),
            };

            ushort tagCount = br.ReadUInt16();
            pal.TagIndices = new uint[tagCount];

            for (int j = 0; j < tagCount; j++)
            {
                pal.TagIndices[j] = br.ReadUInt32();
            }

            ushort rgbCount = br.ReadUInt16();
            pal.Colors = new PaletteColor[rgbCount];

            for (int j = 0; j < rgbCount; j++)
            {
                pal.Colors[j] = new PaletteColor
                {
                    R = br.ReadByte(), G = br.ReadByte(), B = br.ReadByte(),
                };
            }

            palettes[i] = pal;
        }

        return palettes;
    }

    private static SpritesheetMeta ReadSpritesheetMeta(BinaryReader br)
    {
        SpritesheetMeta meta = new()
        {
            Width = br.ReadUInt16(), Height = br.ReadUInt16(),
        };

        uint frameCount = br.ReadUInt32();
        meta.Frames = new FrameData[frameCount];

        for (uint i = 0; i < frameCount; i++)
        {
            meta.Frames[i] = new FrameData
            {
                NameIndex = br.ReadUInt32(),
                X = br.ReadUInt16(),
                Y = br.ReadUInt16(),
                Width = br.ReadUInt16(),
                Height = br.ReadUInt16(),
                SourceWidth = br.ReadUInt16(),
                SourceHeight = br.ReadUInt16(),
                TrimX = br.ReadInt16(),
                TrimY = br.ReadInt16(),
            };
        }

        return meta;
    }

    private static byte[] ReadSpritesheetImage(BinaryReader br)
    {
        uint byteLen = br.ReadUInt32();

        return br.ReadBytes((int)byteLen);
    }

    private static Dictionary<string, byte[]> ReadRawData(BinaryReader br, StringTable? stringTable)
    {
        uint count = br.ReadUInt32();
        Dictionary<string, byte[]> rawData = new((int)count);

        for (uint i = 0; i < count; i++)
        {
            uint nameIndex = br.ReadUInt32();
            uint dataLength = br.ReadUInt32();
            byte[] data = br.ReadBytes((int)dataLength);

            string? name = stringTable?.Resolve(nameIndex);

            if (name != null)
            {
                rawData[name] = data;
            }
        }

        return rawData;
    }
}
