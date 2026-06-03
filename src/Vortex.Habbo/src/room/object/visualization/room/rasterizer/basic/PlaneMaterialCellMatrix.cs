using System;
using System.Linq;

using Godot;

using Vortex.Habbo.Room.Object.Visualization.Room.Utils;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.basic.PlaneMaterialCellMatrix
public class PlaneMaterialCellMatrix
{
    public const int REPEAT_MODE_ALL = 1;
    public const int REPEAT_MODE_BORDERS = 2;
    public const int REPEAT_MODE_CENTER = 3;
    public const int REPEAT_MODE_FIRST = 4;
    public const int REPEAT_MODE_LAST = 5;
    public const int REPEAT_MODE_RANDOM = 6;
    public const int REPEAT_MODE_DEFAULT = 1;

    public const int ALIGN_TOP = 1;
    public const int ALIGN_BOTTOM = 2;
    public const int ALIGN_DEFAULT = 1;

    private readonly List<PlaneMaterialCellColumn?> _columns;
    private readonly int _repeatMode;
    private readonly int _align;
    private Image? _cachedBitmap;
    private Vector3d? _cachedBitmapNormal;
    private int _cachedHeight;
    private bool _isCached;

    public PlaneMaterialCellMatrix(
        int columnCount, int repeatMode = REPEAT_MODE_ALL, int align = ALIGN_DEFAULT,
        double normalMinX = -1, double normalMaxX = 1,
        double normalMinY = -1, double normalMaxY = 1)
    {
        if (columnCount < 1)
        {
            columnCount = 1;
        }
        _columns = new List<PlaneMaterialCellColumn?>(columnCount);
        for (int i = 0; i < columnCount; i++)
        {
            _columns.Add(null);
        }

        _repeatMode = repeatMode;
        _align = align;
        NormalMinX = normalMinX;
        NormalMaxX = normalMaxX;
        NormalMinY = normalMinY;
        NormalMaxY = normalMaxY;

        if (_repeatMode == REPEAT_MODE_RANDOM)
        {
            IsStatic = false;
        }
    }

    public double NormalMinX { get; }

    public double NormalMaxX { get; }

    public double NormalMinY { get; }

    public double NormalMaxY { get; }

    public bool IsStatic { get; private set; } = true;

    public bool IsBottomAligned => _align == ALIGN_BOTTOM;

    public void Dispose()
    {
        _cachedBitmap = null;
        _cachedBitmapNormal = null;
    }

    public void ClearCache()
    {
        if (!_isCached)
        {
            return;
        }

        _cachedBitmap = null;

        if (_cachedBitmapNormal != null)
        {
            _cachedBitmapNormal.X = 0;
            _cachedBitmapNormal.Y = 0;
            _cachedBitmapNormal.Z = 0;
        }

        _cachedHeight = 0;

        foreach (PlaneMaterialCellColumn? col in _columns)
        {
            col?.ClearCache();
        }

        _isCached = false;
    }

    public bool CreateColumn(int index, int width, List<PlaneMaterialCell>? cells, int repeatMode = PlaneMaterialCellColumn.REPEAT_MODE_ALL)
    {
        if (index < 0 || index >= _columns.Count)
        {
            return false;
        }

        PlaneMaterialCellColumn newColumn = new(width, cells, repeatMode);
        _columns[index]?.Dispose();
        _columns[index] = newColumn;

        if (!newColumn.IsStatic)
        {
            IsStatic = false;
        }

        return true;
    }

    public Image? Render(
        Image? canvas, int width, int height,
        IVector3d normal, bool useTexture,
        int offsetX, int offsetY, bool topAligned)
    {
        if (width < 1)
        {
            width = 1;
        }

        if (height < 1)
        {
            height = 1;
        }

        if (canvas != null && (canvas.GetWidth() != width || canvas.GetHeight() != height))
        {
            canvas = null;
        }

        _cachedBitmapNormal ??= new Vector3d();

        if (IsStatic)
        {
            if (_cachedBitmap != null)
            {
                if (_cachedBitmap.GetWidth() == width && _cachedBitmap.GetHeight() == height &&
                    Vector3d.IsEqual(_cachedBitmapNormal, normal))
                {
                    if (canvas == null)
                    {
                        return _cachedBitmap;
                    }

                    CopyCachedBitmapOnCanvas(canvas, _cachedHeight, offsetY, topAligned);

                    return canvas;
                }
                _cachedBitmap = null;
            }
        }
        else if (_cachedBitmap != null)
        {
            if (_cachedBitmap.GetWidth() == width && _cachedBitmap.GetHeight() == height)
            {
                _cachedBitmap.Fill(new Color(1f, 1f, 1f, 0f));
            }
            else
            {
                _cachedBitmap = null;
            }
        }

        _isCached = true;
        _cachedBitmapNormal.Assign(normal);

        if (!useTexture)
        {
            _cachedHeight = height;
            _cachedBitmap ??= Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
            _cachedBitmap.Fill(Colors.White);

            if (canvas == null)
            {
                return _cachedBitmap;
            }

            CopyCachedBitmapOnCanvas(canvas, height, offsetY, topAligned);

            return canvas;
        }

        if (_cachedBitmap == null)
        {
            _cachedHeight = height;
            _cachedBitmap = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        }

        // Render columns
        List<Image> renderedColumns = _columns.OfType<PlaneMaterialCellColumn>()
                                              .Select(column => column.Render(height, normal, offsetX, offsetY)).OfType<Image>().ToList();

        if (renderedColumns.Count == 0)
        {
            return canvas ?? _cachedBitmap;
        }

        int compositeHeight = _repeatMode switch
        {
            REPEAT_MODE_BORDERS => RenderRepeatBorders(_cachedBitmap, renderedColumns),
            REPEAT_MODE_CENTER => RenderRepeatCenter(_cachedBitmap, renderedColumns),
            REPEAT_MODE_FIRST => RenderRepeatFirst(_cachedBitmap, renderedColumns),
            REPEAT_MODE_LAST => RenderRepeatLast(_cachedBitmap, renderedColumns),
            REPEAT_MODE_RANDOM => RenderRepeatRandom(_cachedBitmap, renderedColumns),
            _ => RenderRepeatAll(_cachedBitmap, renderedColumns),
        };

        _cachedHeight = compositeHeight;

        if (canvas == null)
        {
            return _cachedBitmap;
        }

        CopyCachedBitmapOnCanvas(canvas, compositeHeight, offsetY, topAligned);

        return canvas;
    }

    public List<PlaneMaterialCellColumn?> GetColumns(int width)
    {
        if (_repeatMode != REPEAT_MODE_RANDOM)
        {
            return _columns;
        }

        List<PlaneMaterialCellColumn?> result = new();
        int pos = 0;

        while (pos < width)
        {
            PlaneMaterialCellColumn? col = _columns[NextRandomColumnIndex(_columns.Count)];
            if (col == null)
            {
                break;
            }

            result.Add(col);

            if (col.Width <= 1)
            {
                break;
            }

            pos += col.Width;
        }
        return result;
    }

    private void CopyCachedBitmapOnCanvas(Image canvas, int height, int offsetY, bool topAligned)
    {
        if (_cachedBitmap == null || canvas == _cachedBitmap)
        {
            return;
        }

        if (!topAligned)
        {
            offsetY = canvas.GetHeight() - height - offsetY;
        }

        Rect2I srcRect;

        if (_align == ALIGN_TOP)
        {
            srcRect = new Rect2I(0, 0, _cachedBitmap.GetWidth(), _cachedHeight);
        }
        else
        {
            srcRect = new Rect2I(0, _cachedBitmap.GetHeight() - _cachedHeight, _cachedBitmap.GetWidth(), _cachedHeight);
        }

        BlitWithAlpha(canvas, _cachedBitmap, srcRect, new Vector2I(0, offsetY));
    }

    private static int GetColumnsWidth(List<Image> columns)
    {
        return columns.Sum(col => col.GetWidth());
    }

    private (int x, int maxHeight) RenderColumns(Image dest, List<Image> columns, int xPos, bool forward)
    {
        if (columns.Count == 0)
        {
            return (xPos, 0);
        }

        int maxH = 0;

        for (int i = 0; i < columns.Count; i++)
        {
            Image col = forward ? columns[i] : columns[columns.Count - 1 - i];
            int cw = col.GetWidth();
            int ch = col.GetHeight();

            if (!forward)
            {
                xPos -= cw;
            }

            int yPos = _align == ALIGN_BOTTOM ? dest.GetHeight() - ch : 0;

            BlitWithAlpha(dest, col, new Rect2I(0, 0, cw, ch), new Vector2I(xPos, yPos));

            if (ch > maxH)
            {
                maxH = ch;
            }

            if (forward)
            {
                xPos += cw;
            }

            if ((forward && xPos >= dest.GetWidth()) || (!forward && xPos <= 0))
            {
                return (xPos, maxH);
            }
        }
        return (xPos, maxH);
    }

    private int RenderRepeatAll(Image dest, List<Image> columns)
    {
        if (columns.Count == 0)
        {
            return 0;
        }

        int maxH = 0;
        int pos = 0;

        while (pos < dest.GetWidth())
        {
            (int x, int h) = RenderColumns(dest, columns, pos, true);
            pos = x;

            if (h > maxH)
            {
                maxH = h;
            }

            if (x == 0)
            {
                return maxH;
            }
        }
        return maxH;
    }

    private int RenderRepeatBorders(Image dest, List<Image> columns)
    {
        if (columns.Count == 0)
        {
            return 0;
        }

        int maxH = 0;

        List<Image> centerColumns = new();
        int centerWidth = 0;

        for (int i = 1; i < columns.Count - 1; i++)
        {
            centerWidth += columns[i].GetWidth();
            centerColumns.Add(columns[i]);
        }

        if (_columns.Count == 1 && columns.Count > 0)
        {
            centerWidth = columns[0].GetWidth();
            centerColumns = [columns[0]];
        }

        int startX = (dest.GetWidth() - centerWidth) >> 1;
        (int endX, int h) = RenderColumns(dest, centerColumns, startX, true);

        if (h > maxH)
        {
            maxH = h;
        }

        // Left border
        List<Image> leftColumns = new()
        {
            columns[0],
        };

        while (startX >= 0)
        {
            (startX, h) = RenderColumns(dest, leftColumns, startX, false);

            if (h > maxH)
            {
                maxH = h;
            }
        }

        // Right border
        List<Image> rightColumns = new()
        {
            columns[^1],
        };

        while (endX < dest.GetHeight())
        {
            (endX, h) = RenderColumns(dest, rightColumns, endX, true);

            if (h > maxH)
            {
                maxH = h;
            }
        }

        return maxH;
    }

    private int RenderRepeatCenter(Image dest, List<Image> columns)
    {
        if (columns.Count == 0)
        {
            return 0;
        }
        int maxH = 0;

        List<Image> leftColumns = new();
        List<Image> rightColumns = new();
        int leftWidth = 0;
        int rightWidth = 0;
        int half = columns.Count >> 1;

        for (int i = 0; i < half; i++)
        {
            leftWidth += columns[i].GetWidth();
            leftColumns.Add(columns[i]);
        }

        for (int i = half + 1; i < columns.Count; i++)
        {
            rightWidth += columns[i].GetWidth();
            rightColumns.Add(columns[i]);
        }

        int startX = 0;
        int endX = dest.GetWidth();
        int overflow = 0;

        if (leftWidth + rightWidth > dest.GetWidth())
        {
            overflow = leftWidth + rightWidth - dest.GetWidth();
            startX -= overflow >> 1;
            endX += overflow - (overflow >> 1);
        }

        if (overflow == 0 && half < columns.Count)
        {
            Image centerBmp = columns[half];
            int cw = centerBmp.GetWidth();

            if (cw > 0)
            {
                int available = dest.GetWidth() - (leftWidth + rightWidth);
                int filled = (int)Math.Ceiling((double)available / cw) * cw;
                int pos = leftWidth - ((filled - available) >> 1);
                int end = pos + filled;
                List<Image> centerList = new()
                {
                    centerBmp,
                };

                while (pos < end)
                {
                    (pos, int h2) = RenderColumns(dest, centerList, pos, true);

                    if (h2 > maxH)
                    {
                        maxH = h2;
                    }
                }
            }
        }

        startX = 0;
        (_, int h3) = RenderColumns(dest, leftColumns, startX, true);

        if (h3 > maxH)
        {
            maxH = h3;
        }

        (_, h3) = RenderColumns(dest, rightColumns, endX, false);

        if (h3 > maxH)
        {
            maxH = h3;
        }

        return maxH;
    }

    private int RenderRepeatFirst(Image dest, List<Image> columns)
    {
        if (columns.Count == 0)
        {
            return 0;
        }

        int maxH = 0;

        int pos = dest.GetWidth();
        (int x, int h) = RenderColumns(dest, columns, pos, false);
        pos = x;

        if (h > maxH)
        {
            maxH = h;
        }

        List<Image> firstColumns = new()
        {
            columns[0],
        };

        while (pos >= 0)
        {
            (pos, h) = RenderColumns(dest, firstColumns, pos, false);

            if (h > maxH)
            {
                maxH = h;
            }
        }

        return maxH;
    }

    private int RenderRepeatLast(Image dest, List<Image> columns)
    {
        if (columns.Count == 0)
        {
            return 0;
        }

        int maxH = 0;

        int pos = 0;
        (int x, int h) = RenderColumns(dest, columns, pos, true);
        pos = x;

        if (h > maxH)
        {
            maxH = h;
        }

        List<Image> lastColumns = new()
        {
            columns[^1],
        };

        while (pos < dest.GetWidth())
        {
            (pos, h) = RenderColumns(dest, lastColumns, pos, true);

            if (h > maxH)
            {
                maxH = h;
            }
        }

        return maxH;
    }

    private int RenderRepeatRandom(Image dest, List<Image> columns)
    {
        if (columns.Count == 0)
        {
            return 0;
        }

        int maxH = 0;
        int pos = 0;

        while (pos < dest.GetWidth())
        {
            Image col = columns[NextRandomColumnIndex(columns.Count)];
            List<Image> singleList = new()
            {
                col,
            };
            (int x, int h) = RenderColumns(dest, singleList, pos, true);
            pos = x;

            if (h > maxH)
            {
                maxH = h;
            }
        }

        return maxH;
    }

    private static int NextRandomColumnIndex(int count)
    {
        int[] values = Randomizer.GetValues(1, 0, count * 17631);

        return values[0] % count;
    }

    private static void BlitWithAlpha(Image dest, Image src, Rect2I srcRect, Vector2I destPos)
    {
        int sw = src.GetWidth();
        int sh = src.GetHeight();
        int dw = dest.GetWidth();
        int dh = dest.GetHeight();

        for (int y = 0; y < srcRect.Size.Y; y++)
        {
            int sy = srcRect.Position.Y + y;
            int dy = destPos.Y + y;

            if (sy < 0 || sy >= sh || dy < 0 || dy >= dh)
            {
                continue;
            }

            for (int x = 0; x < srcRect.Size.X; x++)
            {
                int sx = srcRect.Position.X + x;
                int dx = destPos.X + x;

                if (sx < 0 || sx >= sw || dx < 0 || dx >= dw)
                {
                    continue;
                }

                Color srcColor = src.GetPixel(sx, sy);

                switch (srcColor.A)
                {
                    case <= 0f:
                        continue;
                    case >= 1f:
                        dest.SetPixel(dx, dy, srcColor);

                        break;
                    default:
                        {
                            Color dstColor = dest.GetPixel(dx, dy);
                            float outA = srcColor.A + dstColor.A * (1f - srcColor.A);

                            if (outA > 0f)
                            {
                                float r = (srcColor.R * srcColor.A + dstColor.R * dstColor.A * (1f - srcColor.A)) / outA;
                                float g = (srcColor.G * srcColor.A + dstColor.G * dstColor.A * (1f - srcColor.A)) / outA;
                                float b = (srcColor.B * srcColor.A + dstColor.B * dstColor.A * (1f - srcColor.A)) / outA;

                                dest.SetPixel(dx, dy, new Color(r, g, b, outA));
                            }

                            break;
                        }
                }

            }
        }
    }
}
