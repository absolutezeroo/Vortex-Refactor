using System.IO.Compression;

using SharpCompress.Compressors.LZMA;

namespace Vortex.Bundle.Converter.Swf;

/// <summary>
/// Reads and decompresses a SWF file.
/// Supports FWS (raw), CWS (zlib), ZWS (LZMA) compression.
/// @see nitro-converter-main/src/swf/UncompressSWF.ts
/// @see nitro-converter-main/src/swf/ReadSWFBuffer.ts
/// </summary>
public static class SwfReader
{
    /// <summary>
    /// Reads a SWF from a file path, decompresses if needed, and returns a parsed SwfFile.
    /// </summary>
    public static SwfFile Read(string filePath)
    {
        byte[] raw = File.ReadAllBytes(filePath);
        return Read(raw);
    }

    /// <summary>
    /// Reads a SWF from raw bytes.
    /// </summary>
    public static SwfFile Read(byte[] data)
    {
        if (data.Length < 8)
        {
            throw new InvalidDataException("SWF file too small.");
        }

        byte signature = data[0];
        // bytes 1-2 should be 'W' 'S'
        if (data[1] != (byte)'W' || data[2] != (byte)'S')
        {
            throw new InvalidDataException("Invalid SWF signature.");
        }

        byte version = data[3];
        uint fileLength = BitConverter.ToUInt32(data, 4);

        byte[] decompressed = signature switch
        {
            0x46 => data,                             // 'F' — uncompressed
            0x43 => DecompressZlib(data, fileLength), // 'C' — zlib
            0x5A => DecompressLzma(data, fileLength), // 'Z' — LZMA
            _ => throw new InvalidDataException($"Unknown SWF compression: 0x{signature:X2}"),
        };

        return ParseSwf(decompressed, version, fileLength);
    }

    private static byte[] DecompressZlib(byte[] data, uint fileLength)
    {
        // First 8 bytes are header (uncompressed), rest is zlib
        byte[] output = new byte[fileLength];
        Buffer.BlockCopy(data, 0, output, 0, 8);
        output[0] = 0x46; // Mark as uncompressed

        using MemoryStream compressed = new(data, 8, data.Length - 8);
        using ZLibStream deflate = new(compressed, CompressionMode.Decompress);

        int offset = 8;
        int remaining = (int)fileLength - 8;
        while (remaining > 0)
        {
            int read = deflate.Read(output, offset, remaining);
            if (read == 0)
            {
                break;
            }
            offset += read;
            remaining -= read;
        }

        return output;
    }

    private static byte[] DecompressLzma(byte[] data, uint fileLength)
    {
        // ZWS format: 8 bytes header, 4 bytes compressed size, 5 bytes LZMA props, then data
        byte[] output = new byte[fileLength];
        Buffer.BlockCopy(data, 0, output, 0, 8);
        output[0] = 0x46;

        // Skip: 8 header + 4 compressed_size = 12, then LZMA properties start
        using MemoryStream compressed = new(data, 12, data.Length - 12);
        using LzmaStream lzma = new(data.AsSpan(12, 5).ToArray(), compressed);

        int offset = 8;
        int remaining = (int)fileLength - 8;
        while (remaining > 0)
        {
            int read = lzma.Read(output, offset, remaining);
            if (read == 0)
            {
                break;
            }
            offset += read;
            remaining -= read;
        }

        return output;
    }

    private static SwfFile ParseSwf(byte[] data, byte version, uint fileLength)
    {
        SwfBinaryReader reader = new(data, 8); // Skip 8-byte header

        SwfFile swf = new()
        {
            Version = version,
            FileLength = fileLength,
            FrameSize = reader.ReadRect(),
            FrameRate = reader.ReadUInt16() / 256.0f,
            FrameCount = reader.ReadUInt16(),
        };

        swf.Tags = SwfTagReader.ReadTags(reader);
        SwfTagReader.AssignSymbolClasses(swf.Tags);

        return swf;
    }
}
