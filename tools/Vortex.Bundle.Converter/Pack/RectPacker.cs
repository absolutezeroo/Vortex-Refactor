namespace Vortex.Bundle.Converter.Pack;

/// <summary>
/// MaxRects bin-packing algorithm (Best Short Side Fit variant).
/// Packs rectangles into a power-of-two atlas with minimal wasted space.
/// </summary>
public sealed class RectPacker
{
    private readonly List<PackRect> _freeRects = [];

    public int Width { get; }

    public int Height { get; }

    public RectPacker(int initialWidth, int initialHeight)
    {
        Width = initialWidth;
        Height = initialHeight;
        _freeRects.Add(new PackRect(0, 0, Width, Height));
    }

    /// <summary>
    /// Attempts to insert a rectangle. Returns the placed position, or null if it doesn't fit.
    /// Uses Best Short Side Fit heuristic.
    /// </summary>
    public PackRect? Insert(int width, int height)
    {
        int bestShortSide = int.MaxValue;
        int bestLongSide = int.MaxValue;
        int bestIndex = -1;
        PackRect bestRect = new();

        for (int i = 0; i < _freeRects.Count; i++)
        {
            PackRect free = _freeRects[i];

            if (free.Width >= width && free.Height >= height)
            {
                int leftoverH = Math.Abs(free.Width - width);
                int leftoverV = Math.Abs(free.Height - height);
                int shortSide = Math.Min(leftoverH, leftoverV);
                int longSide = Math.Max(leftoverH, leftoverV);

                if (shortSide < bestShortSide ||
                    (shortSide == bestShortSide && longSide < bestLongSide))
                {
                    bestShortSide = shortSide;
                    bestLongSide = longSide;
                    bestIndex = i;
                    bestRect = new PackRect(free.X, free.Y, width, height);
                }
            }
        }

        if (bestIndex < 0)
        {
            return null;
        }

        PlaceRect(bestRect);
        return bestRect;
    }

    private void PlaceRect(PackRect placed)
    {
        int count = _freeRects.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            if (SplitFreeNode(_freeRects[i], placed, out List<PackRect> splits))
            {
                _freeRects.RemoveAt(i);
                _freeRects.AddRange(splits);
            }
        }

        PruneFreeList();
    }

    private static bool SplitFreeNode(PackRect free, PackRect placed, out List<PackRect> splits)
    {
        splits = [];

        // No overlap?
        if (placed.X >= free.X + free.Width || placed.X + placed.Width <= free.X ||
            placed.Y >= free.Y + free.Height || placed.Y + placed.Height <= free.Y)
        {
            return false;
        }

        // Split horizontally
        if (placed.X < free.X + free.Width && placed.X + placed.Width > free.X)
        {
            // Top
            if (placed.Y > free.Y)
            {
                splits.Add(new PackRect(free.X, free.Y, free.Width, placed.Y - free.Y));
            }

            // Bottom
            if (placed.Y + placed.Height < free.Y + free.Height)
            {
                splits.Add(new PackRect(free.X, placed.Y + placed.Height, free.Width,
                    free.Y + free.Height - (placed.Y + placed.Height)));
            }
        }

        // Split vertically
        if (placed.Y < free.Y + free.Height && placed.Y + placed.Height > free.Y)
        {
            // Left
            if (placed.X > free.X)
            {
                splits.Add(new PackRect(free.X, free.Y, placed.X - free.X, free.Height));
            }

            // Right
            if (placed.X + placed.Width < free.X + free.Width)
            {
                splits.Add(new PackRect(placed.X + placed.Width, free.Y,
                    free.X + free.Width - (placed.X + placed.Width), free.Height));
            }
        }

        return true;
    }

    private void PruneFreeList()
    {
        for (int i = 0; i < _freeRects.Count; i++)
        {
            for (int j = i + 1; j < _freeRects.Count; j++)
            {
                if (_freeRects[j].Contains(_freeRects[i]))
                {
                    _freeRects.RemoveAt(i);
                    i--;
                    break;
                }

                if (_freeRects[i].Contains(_freeRects[j]))
                {
                    _freeRects.RemoveAt(j);
                    j--;
                }
            }
        }
    }
}

public struct PackRect(int x, int y, int width, int height)
{
    public int X = x;
    public int Y = y;
    public int Width = width;
    public int Height = height;

    public bool Contains(PackRect other)
    {
        return other.X >= X && other.Y >= Y &&
               other.X + other.Width <= X + Width &&
               other.Y + other.Height <= Y + Height;
    }
}
