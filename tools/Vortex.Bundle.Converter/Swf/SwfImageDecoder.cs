using System.IO.Compression;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Vortex.Bundle.Converter.Swf;

/// <summary>
/// Decodes SWF image tags into RGBA pixel data.
/// Handles DefineBitsLossless (paletted/ARGB) and DefineBitsJPEG (JPEG+alpha).
/// @see nitro-converter-main/src/swf/ReadImagesDefineBitsLossless.ts
/// @see nitro-converter-main/src/swf/ReadImagesJPEG3or4.ts
/// </summary>
public static class SwfImageDecoder
{
    /// <summary>
    /// Decodes a DefineBitsLossless/2 tag to an RGBA Image.
    /// </summary>
    public static Image<Rgba32> DecodeLossless(DefineBitsLosslessTag tag)
    {
        // Decompress zlib data
        byte[] decompressed = DecompressZlib(tag.ZlibBitmapData);

        int width = tag.BitmapWidth;
        int height = tag.BitmapHeight;
        Image<Rgba32> image = new(width, height);

        if (tag.BitmapFormat == 5)
        {
            // 32-bit ARGB, pre-multiplied alpha
            DecodeFormat5(decompressed, image, width, height);
        }
        else if (tag.BitmapFormat == 3)
        {
            // 8-bit palette-indexed
            DecodeFormat3(decompressed, image, width, height, tag.BitmapColorTableSize, tag.HasAlpha);
        }
        else
        {
            throw new NotSupportedException($"Unsupported bitmap format: {tag.BitmapFormat}");
        }

        return image;
    }

    /// <summary>
    /// Decodes a DefineBitsJPEG3/4 tag to an RGBA Image.
    /// </summary>
    public static Image<Rgba32> DecodeJpeg(DefineBitsJpegTag tag)
    {
        // Load JPEG image data
        Image<Rgba32> jpegImage = Image.Load<Rgba32>(tag.ImageData);

        if (tag is
            {
                HasAlpha: true,
                AlphaData.Length: > 0,
            })
        {
            // Decompress alpha channel (zlib compressed)
            byte[] alphaData = DecompressZlib(tag.AlphaData);

            // Merge alpha channel into JPEG pixels
            jpegImage.ProcessPixelRows(accessor =>
            {
                int alphaIdx = 0;
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgba32> row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        if (alphaIdx < alphaData.Length)
                        {
                            row[x].A = alphaData[alphaIdx++];
                        }
                    }
                }
            });
        }

        return jpegImage;
    }

    /// <summary>
    /// Format 5: 32-bit ARGB with pre-multiplied alpha.
    /// Un-premultiplies alpha to produce straight RGBA.
    /// </summary>
    private static void DecodeFormat5(byte[] data, Image<Rgba32> image, int width, int height)
    {
        image.ProcessPixelRows(accessor =>
        {
            int ptr = 0;
            for (int y = 0; y < height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < width; x++)
                {
                    byte a = data[ptr];
                    byte r = data[ptr + 1];
                    byte g = data[ptr + 2];
                    byte b = data[ptr + 3];
                    ptr += 4;

                    // Un-premultiply
                    if (a is > 0 and < 255)
                    {
                        r = (byte)Math.Min(255, r * 255 / a);
                        g = (byte)Math.Min(255, g * 255 / a);
                        b = (byte)Math.Min(255, b * 255 / a);
                    }

                    row[x] = new Rgba32(r, g, b, a);
                }
            }
        });
    }

    /// <summary>
    /// Format 3: 8-bit palette-indexed with optional alpha in palette.
    /// </summary>
    private static void DecodeFormat3(byte[] data, Image<Rgba32> image, int width, int height,
        byte colorTableSize, bool hasAlpha)
    {
        int paletteCount = colorTableSize + 1;
        int bytesPerColor = hasAlpha ? 4 : 3;
        Rgba32[] palette = new Rgba32[paletteCount];

        int ptr = 0;
        for (int i = 0; i < paletteCount; i++)
        {
            byte r = data[ptr++];
            byte g = data[ptr++];
            byte b = data[ptr++];
            byte a = hasAlpha ? data[ptr++] : (byte)255;
            palette[i] = new Rgba32(r, g, b, a);
        }

        // Row padding: each row is padded to 4-byte boundary
        int rowStride = (width + 3) & ~3;

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < width; x++)
                {
                    byte index = data[ptr + x];
                    row[x] = index < paletteCount ? palette[index] : default;
                }
                ptr += rowStride;
            }
        });
    }

    private static byte[] DecompressZlib(byte[] data)
    {
        // SWF uses raw DEFLATE wrapped with zlib header (2 bytes)
        using MemoryStream ms = new(data);

        // Skip zlib header (2 bytes) if present
        if (data.Length >= 2 && (data[0] & 0x0F) == 0x08)
        {
            ms.Position = 2;
        }

        using DeflateStream deflate = new(ms, CompressionMode.Decompress);
        using MemoryStream output = new();
        deflate.CopyTo(output);
        return output.ToArray();
    }
}
