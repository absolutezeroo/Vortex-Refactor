using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using Vortex.Bundle.Data;
using Vortex.Bundle.Converter.Convert;

namespace Vortex.Bundle.Converter.Pack;

/// <summary>
/// Packs individual sprite images into a single spritesheet atlas.
/// Trims transparent borders, packs via MaxRects, and encodes to WebP.
/// @see nitro-converter-main/src/swf/GenerateSpritesheet.ts
/// </summary>
public sealed class SpritesheetPacker
{
    /// <summary>
    /// Packs all provided images into a spritesheet.
    /// Returns the atlas metadata, frame data, and encoded image bytes.
    /// </summary>
    public static (SpritesheetMeta Meta, byte[] ImageBytes) Pack(
        Dictionary<string, Image<Rgba32>> images,
        StringTableBuilder strings,
        bool useWebP = true)
    {
        if (images.Count == 0)
        {
            return (new SpritesheetMeta(), []);
        }

        // 1. Trim transparent borders from each image
        List<TrimmedSprite> trimmed = new();
        foreach ((string name, Image<Rgba32> image) in images)
        {
            (Image<Rgba32> Image, short TrimX, short TrimY) trim = TrimTransparent(image);
            trimmed.Add(new TrimmedSprite
            {
                Name = name,
                Image = trim.Image,
                TrimX = trim.TrimX,
                TrimY = trim.TrimY,
                SourceWidth = (ushort)image.Width,
                SourceHeight = (ushort)image.Height,
            });
        }

        // 2. Sort by area (largest first) for better packing
        trimmed.Sort((a, b) => (b.Image.Width * b.Image.Height).CompareTo(a.Image.Width * a.Image.Height));

        // 3. Find minimum atlas size using MaxRects
        int atlasWidth, atlasHeight;
        PackRect[] placements;
        FindOptimalSize(trimmed, out atlasWidth, out atlasHeight, out placements);

        // 4. Blit trimmed sprites onto atlas
        using Image<Rgba32> atlas = new(atlasWidth, atlasHeight);
        FrameData[] frames = new FrameData[trimmed.Count];

        for (int i = 0; i < trimmed.Count; i++)
        {
            TrimmedSprite sprite = trimmed[i];
            PackRect placement = placements[i];

            atlas.Mutate(ctx => ctx.DrawImage(sprite.Image, new Point(placement.X, placement.Y), 1f));

            frames[i] = new FrameData
            {
                NameIndex = strings.Add(sprite.Name),
                X = (ushort)placement.X,
                Y = (ushort)placement.Y,
                Width = (ushort)placement.Width,
                Height = (ushort)placement.Height,
                SourceWidth = sprite.SourceWidth,
                SourceHeight = sprite.SourceHeight,
                TrimX = sprite.TrimX,
                TrimY = sprite.TrimY,
            };
        }

        // 5. Encode atlas
        byte[] imageBytes;
        using (MemoryStream ms = new())
        {
            if (useWebP)
            {
                WebpEncoder encoder = new()
                {
                    FileFormat = WebpFileFormatType.Lossless,
                };
                atlas.Save(ms, encoder);
            }
            else
            {
                atlas.SaveAsPng(ms);
            }
            imageBytes = ms.ToArray();
        }

        // Dispose trimmed images
        foreach (TrimmedSprite sprite in trimmed)
        {
            sprite.Image.Dispose();
        }

        SpritesheetMeta meta = new()
        {
            Width = (ushort)atlasWidth, Height = (ushort)atlasHeight, Frames = frames,
        };

        return (meta, imageBytes);
    }

    private static void FindOptimalSize(
        List<TrimmedSprite> sprites,
        out int atlasWidth,
        out int atlasHeight,
        out PackRect[] placements)
    {
        // Start with smallest power-of-two that could fit everything
        int totalArea = sprites.Sum(s => s.Image.Width * s.Image.Height);
        int maxDim = sprites.Max(s => Math.Max(s.Image.Width, s.Image.Height));
        int startSize = Math.Max(NextPowerOfTwo((int)Math.Sqrt(totalArea)), NextPowerOfTwo(maxDim));

        // Try progressively larger sizes
        for (int size = startSize; size <= 8192; size *= 2)
        {
            // Try square first, then wider, then taller
            int[][] candidates = [[size, size], [size * 2, size], [size, size * 2]];

            foreach (int[] candidate in candidates)
            {
                RectPacker packer = new(candidate[0], candidate[1]);
                PackRect[] results = new PackRect[sprites.Count];
                bool success = true;

                for (int i = 0; i < sprites.Count; i++)
                {
                    PackRect? placed = packer.Insert(sprites[i].Image.Width, sprites[i].Image.Height);
                    if (placed == null)
                    {
                        success = false;
                        break;
                    }
                    results[i] = placed.Value;
                }

                if (success)
                {
                    atlasWidth = candidate[0];
                    atlasHeight = candidate[1];
                    placements = results;
                    return;
                }
            }
        }

        // Fallback: huge atlas
        atlasWidth = 8192;
        atlasHeight = 8192;
        RectPacker fallbackPacker = new(atlasWidth, atlasHeight);
        placements = new PackRect[sprites.Count];
        for (int i = 0; i < sprites.Count; i++)
        {
            placements[i] = fallbackPacker.Insert(sprites[i].Image.Width, sprites[i].Image.Height)
                            ?? new PackRect(0, 0, sprites[i].Image.Width, sprites[i].Image.Height);
        }
    }

    private static (Image<Rgba32> Image, short TrimX, short TrimY) TrimTransparent(Image<Rgba32> source)
    {
        int minX = source.Width, minY = source.Height, maxX = 0, maxY = 0;

        source.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    if (row[x].A > 0)
                    {
                        if (x < minX)
                        {
                            minX = x;
                        }
                        if (x > maxX)
                        {
                            maxX = x;
                        }
                        if (y < minY)
                        {
                            minY = y;
                        }
                        if (y > maxY)
                        {
                            maxY = y;
                        }
                    }
                }
            }
        });

        // Fully transparent image
        if (minX > maxX || minY > maxY)
        {
            Image<Rgba32> pixel = new(1, 1);
            return (pixel, 0, 0);
        }

        int trimW = maxX - minX + 1;
        int trimH = maxY - minY + 1;

        Image<Rgba32> trimmed = source.Clone(ctx => ctx.Crop(new Rectangle(minX, minY, trimW, trimH)));
        return (trimmed, (short)minX, (short)minY);
    }

    private static int NextPowerOfTwo(int v)
    {
        v--;
        v |= v >> 1;
        v |= v >> 2;
        v |= v >> 4;
        v |= v >> 8;
        v |= v >> 16;
        return v + 1;
    }

    private sealed class TrimmedSprite
    {
        public required string Name;
        public required Image<Rgba32> Image;
        public short TrimX;
        public short TrimY;
        public ushort SourceWidth;
        public ushort SourceHeight;
    }
}
