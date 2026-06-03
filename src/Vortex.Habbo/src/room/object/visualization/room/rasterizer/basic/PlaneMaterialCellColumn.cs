using System.Linq;

using Godot;

using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.basic.PlaneMaterialCellColumn
public class PlaneMaterialCellColumn
{
    public const int REPEAT_MODE_NONE = 0;
    public const int REPEAT_MODE_ALL = 1;
    public const int REPEAT_MODE_BORDERS = 2;
    public const int REPEAT_MODE_CENTER = 3;
    public const int REPEAT_MODE_FIRST = 4;
    public const int REPEAT_MODE_LAST = 5;

    private List<PlaneMaterialCell>? _cells = [];
    private readonly int _repeatMode;
    private Image? _cachedBitmap;
    private Vector3d? _cachedBitmapNormal;
    private int _cachedOffsetX;
    private int _cachedOffsetY;
    private bool _isCached;

    public PlaneMaterialCellColumn(int width, List<PlaneMaterialCell>? cells, int repeatMode = REPEAT_MODE_ALL)
    {
        if (width < 1)
        {
            width = 1;
        }

        Width = width;

        if (cells != null)
        {
            foreach (PlaneMaterialCell cell in cells)
            {
                _cells!.Add(cell);

                if (!cell.IsStatic)
                {
                    IsStatic = false;
                }
            }
        }

        _repeatMode = repeatMode;
    }

    public int Width { get; }

    public bool IsStatic { get; } = true;

    public bool IsRepeated => _repeatMode != REPEAT_MODE_NONE;

    public void Dispose()
    {
        if (_cells != null)
        {
            foreach (PlaneMaterialCell cell in _cells)
            {
                cell.Dispose();
            }

            _cells = null;
        }

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

        if (_cells != null)
        {
            foreach (PlaneMaterialCell cell in _cells)
            {
                cell.ClearCache();
            }
        }

        _isCached = false;
    }

    public Image? Render(int height, IVector3d normal, int offsetX, int offsetY)
    {
        int totalHeight = height;

        if (_repeatMode == REPEAT_MODE_NONE)
        {
            totalHeight = GetCellsHeight(_cells, normal);
            height = totalHeight;
        }

        _cachedBitmapNormal ??= new Vector3d();

        if (IsStatic)
        {
            if (_cachedBitmap != null)
            {
                if (_cachedBitmap.GetHeight() == height &&
                    Vector3d.IsEqual(_cachedBitmapNormal, normal) &&
                    _cachedOffsetX == offsetX && _cachedOffsetY == offsetY)
                {
                    return _cachedBitmap;
                }

                _cachedBitmap = null;
            }
        }
        else if (_cachedBitmap != null)
        {
            if (_cachedBitmap.GetHeight() == height)
            {
                _cachedBitmap.Fill(new Color(1f, 1f, 1f, 0f));
            }
            else
            {
                _cachedBitmap = null;
            }
        }

        _isCached = true;

        _cachedBitmap ??= Image.CreateEmpty(Width, height, false, Image.Format.Rgba8);

        _cachedBitmapNormal.Assign(normal);
        _cachedOffsetX = offsetX;
        _cachedOffsetY = offsetY;

        if (_cells == null || _cells.Count == 0)
        {
            return _cachedBitmap;
        }

        switch (_repeatMode)
        {
            case REPEAT_MODE_NONE:
                RenderRepeatNone(normal);

                break;
            case REPEAT_MODE_BORDERS:
                RenderRepeatBorders(normal);

                break;
            case REPEAT_MODE_CENTER:
                RenderRepeatCenter(normal);

                break;
            case REPEAT_MODE_FIRST:
                RenderRepeatFirst(normal);

                break;
            case REPEAT_MODE_LAST:
                RenderRepeatLast(normal);

                break;
            default:
                RenderRepeatAll(normal, offsetX, offsetY);

                break;
        }

        return _cachedBitmap;
    }

    public List<PlaneMaterialCell>? GetCells()
    {
        return _cells;
    }

    private static int GetCellsHeight(List<PlaneMaterialCell>? cells, IVector3d normal)
    {
        if (cells == null || cells.Count == 0)
        {
            return 0;
        }

        return cells.Sum(cell => cell.GetHeight(normal));
    }

    private int RenderCells(List<PlaneMaterialCell> cells, int yPos, bool forward, IVector3d normal, int offsetX = 0, int offsetY = 0)
    {
        if (cells.Count == 0 || _cachedBitmap == null)
        {
            return yPos;
        }

        int bmpH = _cachedBitmap.GetHeight();

        for (int i = 0; i < cells.Count; i++)
        {
            PlaneMaterialCell cell = forward ? cells[i] : cells[cells.Count - 1 - i];
            Image? rendered = cell.Render(normal, offsetX, offsetY);

            if (rendered == null)
            {
                continue;
            }

            int rh = rendered.GetHeight();
            int rw = rendered.GetWidth();

            if (!forward)
            {
                yPos -= rh;
            }

            // copyPixels with mergeAlpha
            BlitWithAlpha(_cachedBitmap, rendered,
                new Rect2I(0, 0, rw, rh),
                new Vector2I(0, yPos));

            if (forward)
            {
                yPos += rh;
            }

            if ((forward && yPos >= bmpH) || (!forward && yPos <= 0))
            {
                return yPos;
            }
        }

        return yPos;
    }

    private void RenderRepeatNone(IVector3d normal)
    {
        if (_cells == null || _cells.Count == 0 || _cachedBitmap == null)
        {
            return;
        }

        RenderCells(_cells, 0, true, normal);
    }

    private void RenderRepeatAll(IVector3d normal, int offsetX, int offsetY)
    {
        if (_cells == null || _cells.Count == 0 || _cachedBitmap == null)
        {
            return;
        }

        int pos = 0;

        while (pos < _cachedBitmap.GetHeight())
        {
            pos = RenderCells(_cells, pos, true, normal, offsetX, offsetY);

            if (pos == 0)
            {
                return;
            }
        }
    }

    private void RenderRepeatBorders(IVector3d normal)
    {
        if (_cells == null || _cells.Count == 0 || _cachedBitmap == null)
        {
            return;
        }

        List<PlaneMaterialCell> centerCells = new();
        int centerHeight = 0;

        for (int i = 1; i < _cells.Count - 1; i++)
        {
            int h = _cells[i].GetHeight(normal);

            if (h <= 0)
            {
                continue;
            }

            centerHeight += h;

            centerCells.Add(_cells[i]);
        }

        if (_cells.Count == 1)
        {
            int h = _cells[0].GetHeight(normal);

            if (h > 0)
            {
                centerHeight += h;

                centerCells.Add(_cells[0]);
            }
        }

        int startY = (_cachedBitmap.GetHeight() - centerHeight) >> 1;
        int endY = RenderCells(centerCells, startY, true, normal);

        // Top border
        List<PlaneMaterialCell> topCells = new()
        {
            _cells[0],
        };

        while (startY >= 0)
        {
            startY = RenderCells(topCells, startY, false, normal);
        }

        // Bottom border
        List<PlaneMaterialCell> bottomCells = new()
        {
            _cells[^1],
        };

        while (endY < _cachedBitmap.GetHeight())
        {
            endY = RenderCells(bottomCells, endY, true, normal);
        }
    }

    private void RenderRepeatCenter(IVector3d normal)
    {
        if (_cells == null || _cells.Count == 0 || _cachedBitmap == null)
        {
            return;
        }

        List<PlaneMaterialCell> topCells = new();
        List<PlaneMaterialCell> bottomCells = new();
        int topHeight = 0;
        int bottomHeight = 0;

        int half = _cells.Count >> 1;

        for (int i = 0; i < half; i++)
        {
            int h = _cells[i].GetHeight(normal);

            if (h <= 0)
            {
                continue;
            }

            topHeight += h;

            topCells.Add(_cells[i]);
        }

        for (int i = half + 1; i < _cells.Count; i++)
        {
            int h = _cells[i].GetHeight(normal);

            if (h <= 0)
            {
                continue;
            }

            bottomHeight += h;

            bottomCells.Add(_cells[i]);
        }

        int bmpH = _cachedBitmap.GetHeight();
        int overflow = 0;
        int startY = 0;
        int endY = bmpH;

        if (topHeight + bottomHeight > bmpH)
        {
            overflow = topHeight + bottomHeight - bmpH;
            startY -= overflow >> 1;
            endY += overflow - (overflow >> 1);
        }

        if (overflow == 0)
        {
            PlaneMaterialCell centerCell = _cells[half];
            int cellH = centerCell.GetHeight(normal);

            if (cellH > 0)
            {
                int available = bmpH - (topHeight + bottomHeight);
                int filled = (int)System.Math.Ceiling((double)available / cellH) * cellH;
                int pos = topHeight - ((filled - available) >> 1);
                int end = pos + filled;
                List<PlaneMaterialCell> centerList = new()
                {
                    centerCell,
                };

                while (pos < end)
                {
                    pos = RenderCells(centerList, pos, true, normal);
                }
            }
        }

        startY = 0;

        RenderCells(topCells, startY, true, normal);
        RenderCells(bottomCells, endY, false, normal);
    }

    private void RenderRepeatFirst(IVector3d normal)
    {
        if (_cells == null || _cells.Count == 0 || _cachedBitmap == null)
        {
            return;
        }

        int pos = _cachedBitmap.GetHeight();
        pos = RenderCells(_cells, pos, false, normal);

        List<PlaneMaterialCell> firstCells = new()
        {
            _cells[0],
        };

        while (pos >= 0)
        {
            pos = RenderCells(firstCells, pos, false, normal);
        }
    }

    private void RenderRepeatLast(IVector3d normal)
    {
        if (_cells == null || _cells.Count == 0 || _cachedBitmap == null)
        {
            return;
        }

        int pos = 0;
        pos = RenderCells(_cells, pos, true, normal);

        List<PlaneMaterialCell> lastCells = new()
        {
            _cells[^1],
        };

        while (pos < _cachedBitmap.GetHeight())
        {
            pos = RenderCells(lastCells, pos, true, normal);
        }
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
