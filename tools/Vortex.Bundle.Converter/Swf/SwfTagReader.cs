namespace Vortex.Bundle.Converter.Swf;

/// <summary>
/// Reads SWF tags sequentially from a decompressed SWF buffer.
/// @see nitro-converter-main/src/swf/ReadSWFTags.ts
/// </summary>
public static class SwfTagReader
{
    // SWF tag type constants
    public const ushort TAG_END = 0;
    public const ushort TAG_DEFINE_BITS_LOSSLESS = 20;
    public const ushort TAG_DEFINE_BITS_JPEG2 = 21;
    public const ushort TAG_DEFINE_BITS_JPEG3 = 35;
    public const ushort TAG_DEFINE_BITS_LOSSLESS2 = 36;
    public const ushort TAG_SYMBOL_CLASS = 76;
    public const ushort TAG_DEFINE_BINARY_DATA = 87;
    public const ushort TAG_DEFINE_BITS_JPEG4 = 90;

    /// <summary>
    /// Reads all tags from the SWF buffer.
    /// </summary>
    public static List<SwfTag> ReadTags(SwfBinaryReader reader)
    {
        List<SwfTag> tags = new();

        while (reader.HasMore)
        {
            ushort tagCodeAndLength = reader.ReadUInt16();
            ushort tagCode = (ushort)(tagCodeAndLength >> 6);
            int tagLength = tagCodeAndLength & 0x3F;

            // Extended length
            if (tagLength == 0x3F)
            {
                tagLength = reader.ReadInt32();
            }

            if (tagCode == TAG_END)
            {
                break;
            }

            int tagStart = reader.Position;

            SwfTag tag = tagCode switch
            {
                TAG_SYMBOL_CLASS => ReadSymbolClassTag(reader),
                TAG_DEFINE_BINARY_DATA => ReadDefineBinaryDataTag(reader, tagLength),
                TAG_DEFINE_BITS_LOSSLESS => ReadDefineBitsLosslessTag(reader, tagLength, hasAlpha: false),
                TAG_DEFINE_BITS_LOSSLESS2 => ReadDefineBitsLosslessTag(reader, tagLength, hasAlpha: true),
                TAG_DEFINE_BITS_JPEG2 => ReadDefineBitsJpegTag(reader, tagLength, hasAlpha: false),
                TAG_DEFINE_BITS_JPEG3 => ReadDefineBitsJpegTag(reader, tagLength, hasAlpha: true),
                TAG_DEFINE_BITS_JPEG4 => ReadDefineBitsJpegTag(reader, tagLength, hasAlpha: true),
                _ => ReadGenericTag(reader, tagLength),
            };

            tag.Code = tagCode;

            // Ensure we consumed exactly tagLength bytes
            int consumed = reader.Position - tagStart;
            if (consumed < tagLength)
            {
                reader.Skip(tagLength - consumed);
            }

            tags.Add(tag);
        }

        return tags;
    }

    /// <summary>
    /// Cross-references SymbolClassTags with image/binary tags to assign class names.
    /// @see nitro-converter-main/src/swf/HabboAssetSWF.ts assignClassesToSymbols
    /// </summary>
    public static void AssignSymbolClasses(List<SwfTag> tags)
    {
        // Build symbol map: charId → className
        Dictionary<ushort, string> symbolMap = new();
        foreach (SwfTag tag in tags)
        {
            if (tag is SymbolClassTag sct)
            {
                foreach ((ushort charId, string className) in sct.Symbols)
                {
                    symbolMap[charId] = className;
                }
            }
        }

        // Assign class names to character-defining tags
        foreach (SwfTag tag in tags)
        {
            switch (tag)
            {
                case DefineBinaryDataTag bdt when symbolMap.TryGetValue(bdt.CharacterId, out string? name):
                    bdt.ClassName = name;
                    break;
                case DefineBitsLosslessTag blt when symbolMap.TryGetValue(blt.CharacterId, out string? name):
                    blt.ClassName = name;
                    break;
                case DefineBitsJpegTag bjt when symbolMap.TryGetValue(bjt.CharacterId, out string? name):
                    bjt.ClassName = name;
                    break;
            }
        }
    }

    private static SymbolClassTag ReadSymbolClassTag(SwfBinaryReader reader)
    {
        SymbolClassTag tag = new();
        ushort numSymbols = reader.ReadUInt16();
        for (int i = 0; i < numSymbols; i++)
        {
            ushort charId = reader.ReadUInt16();
            string name = reader.ReadString();
            tag.Symbols.Add((charId, name));
        }
        return tag;
    }

    private static DefineBinaryDataTag ReadDefineBinaryDataTag(SwfBinaryReader reader, int tagLength)
    {
        DefineBinaryDataTag tag = new()
        {
            CharacterId = reader.ReadUInt16(),
        };
        reader.Skip(4); // Reserved uint32
        int dataLen = tagLength - 6;
        tag.BinaryData = reader.ReadBytes(dataLen);
        return tag;
    }

    private static DefineBitsLosslessTag ReadDefineBitsLosslessTag(SwfBinaryReader reader, int tagLength, bool hasAlpha)
    {
        int startPos = reader.Position;
        DefineBitsLosslessTag tag = new()
        {
            CharacterId = reader.ReadUInt16(),
            BitmapFormat = reader.ReadByte(),
            BitmapWidth = reader.ReadUInt16(),
            BitmapHeight = reader.ReadUInt16(),
            HasAlpha = hasAlpha,
        };

        // Format 3 has a color table size byte
        if (tag.BitmapFormat == 3)
        {
            tag.BitmapColorTableSize = reader.ReadByte();
        }

        int headerSize = reader.Position - startPos;
        int zlibLen = tagLength - headerSize;
        tag.ZlibBitmapData = reader.ReadBytes(zlibLen);

        return tag;
    }

    private static DefineBitsJpegTag ReadDefineBitsJpegTag(SwfBinaryReader reader, int tagLength, bool hasAlpha)
    {
        DefineBitsJpegTag tag = new()
        {
            CharacterId = reader.ReadUInt16(), HasAlpha = hasAlpha,
        };

        if (hasAlpha)
        {
            // uint32 alphaDataOffset: offset from start of image data to alpha data
            uint alphaOffset = reader.ReadUInt32();
            tag.ImageData = reader.ReadBytes((int)alphaOffset);
            int alphaLen = tagLength - 6 - (int)alphaOffset;
            if (alphaLen > 0)
            {
                tag.AlphaData = reader.ReadBytes(alphaLen);
            }
        }
        else
        {
            tag.ImageData = reader.ReadBytes(tagLength - 2);
        }

        return tag;
    }

    private static SwfTag ReadGenericTag(SwfBinaryReader reader, int tagLength)
    {
        SwfTag tag = new()
        {
            Data = reader.ReadBytes(tagLength),
        };
        return tag;
    }
}
