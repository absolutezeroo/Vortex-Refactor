using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object;

/// <summary>
/// Converts heightmap tile data into 3D room geometry (floor planes, wall planes, landscape planes).
/// Handles tile expansion (4x), wall boundary tracing, floor holes, and highlight areas.
/// </summary>
/// @see com.sulake.habbo.room.object.RoomPlaneParser
public class RoomPlaneParser
{
    private const double FLOOR_THICKNESS = 0.25;
    private const double WALL_THICKNESS = 0.25;
    private const double MAX_WALL_ADDITIONAL_HEIGHT = 20;

    public const int TILE_BLOCKED = -110;
    public const int TILE_HOLE = -100;

    private List<double[]> _tileMatrix = new();
    private List<double[]> _tileMatrixOriginal = new();
    private List<RoomPlaneData> _planes = new();
    private readonly List<RoomPlaneData> _highlightPlanes = new();
    private double _wallHeight = 3.6;
    private double _wallThicknessMultiplier = 1;
    private double _floorThicknessMultiplier = 1;
    private int _fixedWallsHeight = -1;
    private Dictionary<int, RoomFloorHole> _floorHoles = new();
    private Dictionary<int, RoomFloorHole> _floorHolesInverted = new();
    private List<bool[]> _holeFlags = new();
    private int[][]? _expandedTiles;

    private double _floorHeight;

    public double FloorHeight => _fixedWallsHeight != -1 ? _fixedWallsHeight : _floorHeight;

    public int MinX { get; private set; }

    public int MaxX { get; private set; }

    public int MinY { get; private set; }

    public int MaxY { get; private set; }

    public int TileMapWidth { get; private set; }

    public int TileMapHeight { get; private set; }

    public int PlaneCount => _planes.Count;

    public double WallHeight
    {
        get => _fixedWallsHeight != -1 ? _fixedWallsHeight + 3.6 : _wallHeight;
        set
        {
            if (value < 0)
            {
                value = 0;
            }

            _wallHeight = value;
        }
    }

    public double WallThicknessMultiplier
    {
        get => _wallThicknessMultiplier;
        set
        {
            if (value < 0)
            {
                value = 0;
            }

            _wallThicknessMultiplier = value;
        }
    }

    public double FloorThicknessMultiplier
    {
        get => _floorThicknessMultiplier;
        set
        {
            if (value < 0)
            {
                value = 0;
            }

            _floorThicknessMultiplier = value;
        }
    }

    public void Dispose()
    {
        _planes = null!;
        _tileMatrix = null!;
        _tileMatrixOriginal = null!;
        _holeFlags = null!;
        _floorHoles = null!;
        _floorHolesInverted = null!;
    }

    public void Reset()
    {
        _planes = new List<RoomPlaneData>();
        _tileMatrix = new List<double[]>();
        _tileMatrixOriginal = new List<double[]>();
        TileMapWidth = 0;
        TileMapHeight = 0;
        MinX = 0;
        MaxX = 0;
        MinY = 0;
        MaxY = 0;
        _floorHeight = 0;
        _holeFlags = new List<bool[]>();
    }

    public bool InitializeTileMap(int width, int height)
    {
        if (width < 0)
        {
            width = 0;
        }

        if (height < 0)
        {
            height = 0;
        }

        _tileMatrix = new List<double[]>();
        _tileMatrixOriginal = new List<double[]>();
        _holeFlags = new List<bool[]>();

        for (int y = 0;
             y < height;
             y++)
        {
            double[] row = new double[width];
            double[] origRow = new double[width];
            bool[] holes = new bool[width];

            for (int x = 0;
                 x < width;
                 x++)
            {
                row[x] = TILE_BLOCKED;
                origRow[x] = TILE_BLOCKED;
                holes[x] = false;
            }

            _tileMatrix.Add(row);
            _tileMatrixOriginal.Add(origRow);
            _holeFlags.Add(holes);
        }

        TileMapWidth = width;
        TileMapHeight = height;
        MinX = TileMapWidth;
        MaxX = -1;
        MinY = TileMapHeight;
        MaxY = -1;

        return true;
    }

    public bool SetTileHeight(int x, int y, double height)
    {
        if (x < 0 || x >= TileMapWidth || y < 0 || y >= TileMapHeight)
        {
            return false;
        }

        _tileMatrix[y][x] = height;

        if (height >= 0)
        {
            if (x < MinX)
            {
                MinX = x;
            }
            if (x > MaxX)
            {
                MaxX = x;
            }
            if (y < MinY)
            {
                MinY = y;
            }
            if (y > MaxY)
            {
                MaxY = y;
            }
        }
        else
        {
            if (x == MinX || x == MaxX)
            {
                bool found = false;

                for (int iy = MinY;
                     iy < MaxY;
                     iy++)
                {
                    if (!(GetTileHeightInternal(x, iy) >= 0))
                    {
                        continue;
                    }

                    found = true;

                    break;
                }

                if (!found)
                {
                    if (x == MinX)
                    {
                        MinX++;
                    }

                    if (x == MaxX)
                    {
                        MaxX--;
                    }
                }
            }
            if (y == MinY || y == MaxY)
            {
                bool found = false;

                for (int ix = MinX;
                     ix < MaxX;
                     ix++)
                {
                    if (!(GetTileHeight(ix, y) >= 0))
                    {
                        continue;
                    }

                    found = true;

                    break;
                }

                if (found)
                {
                    return true;
                }

                if (y == MinY)
                {
                    MinY++;
                }

                if (y == MaxY)
                {
                    MaxY--;
                }
            }
        }
        return true;
    }

    public double GetTileHeight(int x, int y)
    {
        if (x < 0 || x >= TileMapWidth || y < 0 || y >= TileMapHeight)
        {
            return TILE_BLOCKED;
        }

        return Math.Abs(_tileMatrix[y][x]);
    }

    public bool InitializeFromTileData(int fixedWallsHeight = -1)
    {
        _fixedWallsHeight = fixedWallsHeight;

        for (int y = 0;
             y < TileMapHeight;
             y++)
        {
            for (int x = 0;
                 x < TileMapWidth;
                 x++)
            {
                _tileMatrixOriginal[y][x] = _tileMatrix[y][x];
            }
        }

        (int X, int Y)? entrance = FindEntranceTile(_tileMatrix);

        for (int y = 0;
             y < TileMapHeight;
             y++)
        {
            for (int x = 0;
                 x < TileMapWidth;
                 x++)
            {
                if (_holeFlags[y][x])
                {
                    SetTileHeight(x, y, TILE_HOLE);
                }
            }
        }

        return Initialize(entrance);
    }

    public void InitializeHighlightArea(int x, int y, int width, int height)
    {
        ClearHighlightArea();

        if (_expandedTiles != null)
        {
            ExtractPlanes(_expandedTiles, x * 4, y * 4, width * 4, height * 4, true);
        }
    }

    public int ClearHighlightArea()
    {
        int count = _highlightPlanes.Count;

        if (count > 0)
        {
            _planes.RemoveRange(_planes.Count - count, count);
        }

        _highlightPlanes.Clear();

        return count;
    }

    public bool InitializeFromXML(XElement? xml)
    {
        if (xml == null)
        {
            return false;
        }

        Reset();
        ResetFloorHoles();

        XElement? tileMapElement = xml.Element("tileMap");

        if (tileMapElement == null)
        {
            return false;
        }

        if (!XMLValidator.CheckRequiredAttributes(tileMapElement, ["width", "height", "wallHeight"]))
        {
            return false;
        }

        int w = int.Parse(tileMapElement.Attribute("width")!.Value, CultureInfo.InvariantCulture);
        int h = int.Parse(tileMapElement.Attribute("height")!.Value, CultureInfo.InvariantCulture);
        double wh = double.Parse(tileMapElement.Attribute("wallHeight")!.Value, CultureInfo.InvariantCulture);
        int fwh = -1;
        XAttribute? fwhAttr = tileMapElement.Attribute("fixedWallsHeight");

        if (fwhAttr != null)
        {
            fwh = int.Parse(fwhAttr.Value, CultureInfo.InvariantCulture);
        }

        InitializeTileMap(w, h);

        int rowIndex = 0;

        foreach (XElement rowElement in tileMapElement.Elements("tileRow"))
        {
            int colIndex = 0;

            foreach (XElement tileElement in rowElement.Elements("tile"))
            {
                double tileHeight = double.Parse(tileElement.Attribute("height")!.Value, CultureInfo.InvariantCulture);

                SetTileHeight(colIndex, rowIndex, tileHeight);

                colIndex++;
            }

            rowIndex++;
        }

        XElement? holeMapElement = xml.Element("holeMap");

        if (holeMapElement != null)
        {
            foreach (XElement holeElement in holeMapElement.Elements("hole"))
            {
                if (!XMLValidator.CheckRequiredAttributes(holeElement, ["id", "x", "y", "width", "height", "invert"]))
                {
                    continue;
                }

                int holeId = int.Parse(holeElement.Attribute("id")!.Value, CultureInfo.InvariantCulture);
                int hx = int.Parse(holeElement.Attribute("x")!.Value, CultureInfo.InvariantCulture);
                int hy = int.Parse(holeElement.Attribute("y")!.Value, CultureInfo.InvariantCulture);
                int hw = int.Parse(holeElement.Attribute("width")!.Value, CultureInfo.InvariantCulture);
                int hh = int.Parse(holeElement.Attribute("height")!.Value, CultureInfo.InvariantCulture);
                bool invert = holeElement.Attribute("invert")!.Value == "true";

                AddFloorHole(holeId, hx, hy, hw, hh, invert);
            }

            InitializeHoleMap();
        }

        WallHeight = wh;

        InitializeFromTileData(fwh);

        return true;
    }

    public bool IsPlaneTemporaryHighlighter(int planeIndex)
    {
        if (planeIndex < 0 || planeIndex >= PlaneCount)
        {
            return false;
        }

        RoomPlaneData plane = _planes[planeIndex];

        return _highlightPlanes.Contains(plane);
    }

    public XElement GetXML()
    {
        XElement tileMap = new(
            "tileMap",
            new XAttribute("width", TileMapWidth),
            new XAttribute("height", TileMapHeight),
            new XAttribute("wallHeight", _wallHeight.ToString(CultureInfo.InvariantCulture)),
            new XAttribute("fixedWallsHeight", _fixedWallsHeight)
        );

        for (int y = 0;
             y < TileMapHeight;
             y++)
        {
            XElement row = new("tileRow");
            double[] origRow = _tileMatrixOriginal[y];

            for (int x = 0;
                 x < TileMapWidth;
                 x++)
            {
                row.Add(
                    new XElement(
                        "tile",
                        new XAttribute("height", origRow[x].ToString(CultureInfo.InvariantCulture))
                    )
                );
            }

            tileMap.Add(row);
        }

        XElement holeMap = new("holeMap");
        foreach ((int key, RoomFloorHole hole) in _floorHoles)
        {
            holeMap.Add(
                new XElement(
                    "hole",
                    new XAttribute("id", key),
                    new XAttribute("x", hole.X), new XAttribute("y", hole.Y),
                    new XAttribute("width", hole.Width), new XAttribute("height", hole.Height),
                    new XAttribute("invert", "false")
                )
            );
        }
        foreach ((int key, RoomFloorHole hole) in _floorHolesInverted)
        {
            holeMap.Add(
                new XElement(
                    "hole",
                    new XAttribute("id", key),
                    new XAttribute("x", hole.X), new XAttribute("y", hole.Y),
                    new XAttribute("width", hole.Width), new XAttribute("height", hole.Height),
                    new XAttribute("invert", "true")
                )
            );
        }

        XElement roomData = new("roomData");

        roomData.Add(tileMap);
        roomData.Add(holeMap);
        roomData.Add(
            new XElement(
                "dimensions",
                new XAttribute("minX", MinX), new XAttribute("maxX", MaxX),
                new XAttribute("minY", MinY), new XAttribute("maxY", MaxY)
            )
        );

        return roomData;
    }

    public IVector3d? GetPlaneLocation(int index)
    {
        if (index < 0 || index >= PlaneCount)
        {
            return null;
        }

        return _planes[index].Location;
    }

    public IVector3d? GetPlaneNormal(int index)
    {
        if (index < 0 || index >= PlaneCount)
        {
            return null;
        }

        return _planes[index].Normal;
    }

    public IVector3d? GetPlaneLeftSide(int index)
    {
        if (index < 0 || index >= PlaneCount)
        {
            return null;
        }

        return _planes[index].LeftSide;
    }

    public IVector3d? GetPlaneRightSide(int index)
    {
        if (index < 0 || index >= PlaneCount)
        {
            return null;
        }

        return _planes[index].RightSide;
    }

    public IVector3d? GetPlaneNormalDirection(int index)
    {
        if (index < 0 || index >= PlaneCount)
        {
            return null;
        }

        return _planes[index].NormalDirection;
    }

    public List<IVector3d>? GetPlaneSecondaryNormals(int index)
    {
        if (index < 0 || index >= PlaneCount)
        {
            return null;
        }

        RoomPlaneData plane = _planes[index];
        List<IVector3d> result = new();

        for (int i = 0;
             i < plane.SecondaryNormalCount;
             i++)
        {
            result.Add(plane.GetSecondaryNormal(i));
        }

        return result;
    }

    public int GetPlaneType(int index)
    {
        if (index < 0 || index >= PlaneCount)
        {
            return 0;
        }

        return _planes[index].Type;
    }

    public int GetPlaneMaskCount(int index)
    {
        if (index < 0 || index >= PlaneCount)
        {
            return 0;
        }

        return _planes[index].MaskCount;
    }

    public double GetPlaneMaskLeftSideLoc(int planeIndex, int maskIndex)
    {
        if (planeIndex < 0 || planeIndex >= PlaneCount)
        {
            return -1;
        }

        return _planes[planeIndex].GetMaskLeftSideLoc(maskIndex);
    }

    public double GetPlaneMaskRightSideLoc(int planeIndex, int maskIndex)
    {
        if (planeIndex < 0 || planeIndex >= PlaneCount)
        {
            return -1;
        }

        return _planes[planeIndex].GetMaskRightSideLoc(maskIndex);
    }

    public double GetPlaneMaskLeftSideLength(int planeIndex, int maskIndex)
    {
        if (planeIndex < 0 || planeIndex >= PlaneCount)
        {
            return -1;
        }

        return _planes[planeIndex].GetMaskLeftSideLength(maskIndex);
    }

    public double GetPlaneMaskRightSideLength(int planeIndex, int maskIndex)
    {
        if (planeIndex < 0 || planeIndex >= PlaneCount)
        {
            return -1;
        }

        return _planes[planeIndex].GetMaskRightSideLength(maskIndex);
    }

    public void AddFloorHole(int id, int x, int y, int width, int height, bool invert = false)
    {
        RemoveFloorHole(id);

        RoomFloorHole hole = new(x, y, width, height);

        if (invert)
        {
            _floorHolesInverted[id] = hole;
        }
        else
        {
            _floorHoles[id] = hole;
        }
    }

    public void RemoveFloorHole(int id)
    {
        _floorHoles.Remove(id);
        _floorHolesInverted.Remove(id);
    }

    public void ResetFloorHoles()
    {
        _floorHoles.Clear();
        _floorHolesInverted.Clear();
    }

    // --- Private methods ---

    private double GetTileHeightOriginal(int x, int y)
    {
        if (x < 0 || x >= TileMapWidth || y < 0 || y >= TileMapHeight)
        {
            return TILE_BLOCKED;
        }

        if (_holeFlags[y][x])
        {
            return TILE_HOLE;
        }

        return _tileMatrixOriginal[y][x];
    }

    private double GetTileHeightInternal(int x, int y)
    {
        if (x < 0 || x >= TileMapWidth || y < 0 || y >= TileMapHeight)
        {
            return TILE_BLOCKED;
        }

        return _tileMatrix[y][x];
    }

    private bool Initialize((int X, int Y)? entrance)
    {
        double entranceHeight = 0.0;

        if (entrance != null)
        {
            entranceHeight = GetTileHeight(entrance.Value.X, entrance.Value.Y);
            SetTileHeight(entrance.Value.X, entrance.Value.Y, TILE_BLOCKED);
        }

        _floorHeight = GetFloorHeight(_tileMatrix);

        CreateWallPlanes();

        // Build int tile matrix from double tile matrix
        List<int[]> intMatrix = new();

        foreach (double[] row in _tileMatrix)
        {
            int[] intRow = new int[row.Length];

            for (int i = 0;
                 i < row.Length;
                 i++)
            {
                intRow[i] = (int)row[i];
            }

            intMatrix.Add(intRow);
        }

        PadHeightMap(intMatrix);
        AddTileTypes(intMatrix);
        UnpadHeightMap(intMatrix);

        _expandedTiles = ExpandFloorTiles(intMatrix);
        ExtractPlanes(_expandedTiles);

        if (entrance == null)
        {
            return true;
        }

        SetTileHeight(entrance.Value.X, entrance.Value.Y, entranceHeight);
        AddFloor(
            new Vector3d(entrance.Value.X + 0.5, entrance.Value.Y + 0.5, entranceHeight),
            new Vector3d(-1, 0, 0), new Vector3d(0, -1, 0),
            false, false, false, false
        );

        return true;
    }

    private static double GetFloorHeight(List<double[]> tiles)
    {
        if (tiles.Count == 0)
        {
            return 0;
        }

        return tiles.SelectMany(row => row).Prepend(0.0).Max();
    }

    private static (int X, int Y)? FindEntranceTile(List<double[]>? tiles)
    {
        if (tiles == null || tiles.Count == 0)
        {
            return null;
        }

        int rowCount = tiles.Count;
        List<int> firstValidCols = new();

        for (int y = 0;
             y < rowCount;
             y++)
        {
            double[]? row = tiles[y];

            if (row == null || row.Length == 0)
            {
                return null;
            }

            int foundCol = row.Length + 1;

            for (int x = 0;
                 x < row.Length;
                 x++)
            {
                if (!(row[x] >= 0))
                {
                    continue;
                }

                foundCol = x;

                break;
            }
            firstValidCols.Add(foundCol);
        }

        for (int y = 1;
             y < firstValidCols.Count - 1;
             y++)
        {
            if (firstValidCols[y] <= firstValidCols[y - 1] - 1 &&
                firstValidCols[y] <= firstValidCols[y + 1] - 1)
            {
                return (firstValidCols[y], y);
            }
        }

        return null;
    }

    private static int[][] ExpandFloorTiles(List<int[]> tiles)
    {
        int rows = tiles.Count;
        int cols = tiles[0].Length;
        int[][] result = new int[rows * 4][];

        for (int r = 0;
             r < rows * 4;
             r++)
        {
            result[r] = new int[cols * 4];
        }

        for (int r = 0;
             r < rows;
             r++)
        {
            int outRow = r * 4;

            for (int c = 0;
                 c < cols;
                 c++)
            {
                int outCol = c * 4;
                int val = tiles[r][c];

                if (val is < 0 or <= 255)
                {
                    for (int dy = 0;
                         dy < 4;
                         dy++)
                    {
                        for (int dx = 0;
                             dx < 4;
                             dx++)
                        {
                            result[outRow + dy][outCol + dx] = val < 0 ? val : val * 4;
                        }
                    }
                }
                else
                {
                    int baseVal = (val & 255) * 4;
                    int corner0 = baseVal + (((val >> 11) & 1) * 3);
                    int corner1 = baseVal + (((val >> 10) & 1) * 3);
                    int corner2 = baseVal + (((val >> 9) & 1) * 3);
                    int corner3 = baseVal + (((val >> 8) & 1) * 3);

                    for (int i = 0;
                         i < 3;
                         i++)
                    {
                        int j = i + 1;
                        result[outRow][outCol + i] = ((corner0 * (3 - i)) + (corner1 * i)) / 3;
                        result[outRow + 3][outCol + j] = ((corner2 * (3 - j)) + (corner3 * j)) / 3;
                        result[outRow + j][outCol] = ((corner0 * (3 - j)) + (corner2 * j)) / 3;
                        result[outRow + i][outCol + 3] = ((corner1 * (3 - i)) + (corner3 * i)) / 3;
                    }

                    result[outRow + 1][outCol + 1] = corner0 > baseVal ? baseVal + 2 : baseVal + 1;
                    result[outRow + 1][outCol + 2] = corner1 > baseVal ? baseVal + 2 : baseVal + 1;
                    result[outRow + 2][outCol + 1] = corner2 > baseVal ? baseVal + 2 : baseVal + 1;
                    result[outRow + 2][outCol + 2] = corner3 > baseVal ? baseVal + 2 : baseVal + 1;
                }
            }
        }

        return result;
    }

    private static void AddTileTypes(List<int[]> tiles)
    {
        int rows = tiles.Count - 1;
        int cols = tiles[0].Length - 1;

        for (int r = 1;
             r < rows;
             r++)
        {
            for (int c = 1;
                 c < cols;
                 c++)
            {
                int val = tiles[r][c];
                if (val < 0)
                {
                    continue;
                }

                int nw = tiles[r - 1][c - 1] & 255;
                int n = tiles[r - 1][c] & 255;
                int ne = tiles[r - 1][c + 1] & 255;
                int w = tiles[r][c - 1] & 255;
                int e = tiles[r][c + 1] & 255;
                int sw = tiles[r + 1][c - 1] & 255;
                int s = tiles[r + 1][c] & 255;
                int se = tiles[r + 1][c + 1] & 255;

                int hi = val + 1;

                _ = val - 1;

                int flags = (nw == hi || n == hi || w == hi ? 8 : 0)
                            | (ne == hi || n == hi || e == hi ? 4 : 0)
                            | (sw == hi || s == hi || w == hi ? 2 : 0)
                            | (se == hi || s == hi || e == hi ? 1 : 0);

                if (flags == 15)
                {
                    flags = 0;
                }

                tiles[r][c] = val | (flags << 8);
            }
        }
    }

    private static void UnpadHeightMap(List<int[]> tiles)
    {
        tiles.RemoveAt(0);
        tiles.RemoveAt(tiles.Count - 1);

        for (int i = 0; i < tiles.Count; i++)
        {
            int[] row = tiles[i];
            int[] trimmed = new int[row.Length - 2];

            Array.Copy(row, 1, trimmed, 0, trimmed.Length);

            tiles[i] = trimmed;
        }
    }

    private static void PadHeightMap(List<int[]> tiles)
    {
        int width = tiles[0].Length + 2;

        // Extend each row
        for (int i = 0;
             i < tiles.Count;
             i++)
        {
            int[] oldRow = tiles[i];
            int[] newRow = new int[width];
            newRow[0] = TILE_BLOCKED;
            Array.Copy(oldRow, 0, newRow, 1, oldRow.Length);
            newRow[width - 1] = TILE_BLOCKED;
            tiles[i] = newRow;
        }

        // Add top and bottom rows
        int[] topRow = new int[width];
        int[] bottomRow = new int[width];
        Array.Fill(topRow, TILE_BLOCKED);
        Array.Fill(bottomRow, TILE_BLOCKED);
        tiles.Insert(0, topRow);
        tiles.Add(bottomRow);
    }

    private RoomWallData? GenerateWallData((int X, int Y) startPoint, bool treatHolesAsWalls)
    {
        RoomWallData wallData = new();
        Func<(int X, int Y), bool, (int X, int Y)?>[] extractors = [ExtractTopWall, ExtractRightWall, ExtractBottomWall, ExtractLeftWall];

        int direction = 0;
        (int X, int Y) current = startPoint;
        int iteration = 0;

        while (iteration++ < 1000)
        {
            bool isBorder = current.X < MinX || current.X > MaxX || current.Y < MinY || current.Y > MaxY;

            int prevDirection = direction;
            (int X, int Y)? next = extractors[direction](current, treatHolesAsWalls);
            if (next == null)
            {
                return null;
            }

            int length = Math.Abs(next.Value.X - current.X) + Math.Abs(next.Value.Y - current.Y);
            bool isLeftTurn = false;

            if (current.X == next.Value.X || current.Y == next.Value.Y)
            {
                direction = (direction - 1 + extractors.Length) % extractors.Length;
                length += 1;
                isLeftTurn = true;
            }
            else
            {
                direction = (direction + 1) % extractors.Length;
                length -= 1;
            }

            wallData.AddWall(current, prevDirection, length, isBorder, isLeftTurn);

            if (next.Value.X == startPoint.X && next.Value.Y == startPoint.Y &&
                (next.Value.X != current.X || next.Value.Y != current.Y))
            {
                break;
            }

            current = next.Value;
        }

        return wallData.Count == 0 ? null : wallData;
    }

    private static void HidePeninsulaWallChains(RoomWallData wallData)
    {
        int count = wallData.Count;
        int i = 0;
        while (i < count)
        {
            int start = i;
            int end = i;
            int leftTurnCount = 0;
            bool hasDeepPeninsula = false;

            while (!wallData.GetBorder(i) && i < count)
            {
                if (wallData.GetLeftTurn(i))
                {
                    leftTurnCount++;
                }
                else if (leftTurnCount > 0)
                {
                    leftTurnCount--;
                }

                if (leftTurnCount > 1)
                {
                    hasDeepPeninsula = true;
                }

                end = i;
                i++;
            }

            if (hasDeepPeninsula)
            {
                for (int j = start;
                     j <= end;
                     j++)
                {
                    wallData.SetHideWall(j, true);
                }
            }
            i++;
        }
    }

    private void UpdateWallsNextToHoles(RoomWallData wallData)
    {
        int count = wallData.Count;
        for (int i = 0;
             i < count;
             i++)
        {
            if (wallData.GetHideWall(i))
            {
                continue;
            }

            (int X, int Y) corner = wallData.GetCorner(i);
            int dir = wallData.GetDirection(i);
            int length = wallData.GetLength(i);
            IVector3d dirVec = RoomWallData.WALL_DIRECTION_VECTORS[dir];
            IVector3d normVec = RoomWallData.WALL_NORMAL_VECTORS[dir];

            int holeCount = 0;
            for (int step = 0;
                 step < length;
                 step++)
            {
                int tx = (int)(corner.X + (step * dirVec.X) - normVec.X);
                int ty = (int)(corner.Y + (step * dirVec.Y) - normVec.Y);

                if (GetTileHeightInternal(tx, ty) == TILE_HOLE)
                {
                    if (step > 0 && holeCount == 0)
                    {
                        wallData.SetLength(i, step);
                        break;
                    }

                    holeCount++;
                }
                else if (holeCount > 0)
                {
                    wallData.MoveCorner(i, holeCount);
                    break;
                }
            }

            if (holeCount == length)
            {
                wallData.SetHideWall(i, true);
            }
        }
    }

    private static int ResolveOriginalWallIndex((int X, int Y) p1, (int X, int Y) p2, RoomWallData wallData)
    {
        int minY = Math.Min(p1.Y, p2.Y);
        int maxY = Math.Max(p1.Y, p2.Y);
        int minX = Math.Min(p1.X, p2.X);
        int maxX = Math.Max(p1.X, p2.X);

        for (int i = 0;
             i < wallData.Count;
             i++)
        {
            (int X, int Y) corner = wallData.GetCorner(i);
            (int X, int Y) end = wallData.GetEndPoint(i);

            if (p1.X == p2.X)
            {
                if ((int)corner.X != p1.X || (int)end.X != p1.X)
                {
                    continue;
                }

                int cMinY = Math.Min((int)corner.Y, (int)end.Y);
                int cMaxY = Math.Max((int)corner.Y, (int)end.Y);

                if (cMinY <= minY && maxY <= cMaxY)
                {
                    return i;
                }
            }
            else if (p1.Y == p2.Y)
            {
                if ((int)corner.Y != p1.Y || (int)end.Y != p1.Y)
                {
                    continue;
                }

                int cMinX = Math.Min((int)corner.X, (int)end.X);
                int cMaxX = Math.Max((int)corner.X, (int)end.X);

                if (cMinX <= minX && maxX <= cMaxX)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private void HideOriginallyHiddenWalls(RoomWallData current, RoomWallData original)
    {
        for (int i = 0;
             i < current.Count;
             i++)
        {
            if (current.GetHideWall(i))
            {
                continue;
            }

            (int X, int Y) corner = current.GetCorner(i);
            IVector3d dirVec = RoomWallData.WALL_DIRECTION_VECTORS[current.GetDirection(i)];
            int length = current.GetLength(i);

            int endX = (int)(corner.X + (dirVec.X * length));
            int endY = (int)(corner.Y + (dirVec.Y * length));

            int origIndex = ResolveOriginalWallIndex(
                ((int)corner.X, (int)corner.Y), (endX, endY), original
            );

            if (origIndex >= 0)
            {
                if (original.GetHideWall(origIndex))
                {
                    current.SetHideWall(i, true);
                }
            }
            else
            {
                current.SetHideWall(i, true);
            }
        }
    }

    private void CheckWallHiding(RoomWallData current, RoomWallData original)
    {
        HidePeninsulaWallChains(original);
        UpdateWallsNextToHoles(current);
        HideOriginallyHiddenWalls(current, original);
    }

    private void AddWalls(RoomWallData wallData, RoomWallData originalWallData)
    {
        int count = wallData.Count;
        int origCount = originalWallData.Count;

        for (int i = 0;
             i < count;
             i++)
        {
            if (wallData.GetHideWall(i))
            {
                continue;
            }

            (int X, int Y) corner = wallData.GetCorner(i);
            int direction = wallData.GetDirection(i);
            int length = wallData.GetLength(i);
            IVector3d dirVec = RoomWallData.WALL_DIRECTION_VECTORS[direction];
            IVector3d normVec = RoomWallData.WALL_NORMAL_VECTORS[direction];

            double minTileHeight = -1.0;

            for (int step = 0;
                 step < length;
                 step++)
            {
                double h = GetTileHeightInternal(
                    (int)(corner.X + (step * dirVec.X) + normVec.X),
                    (int)(corner.Y + (step * dirVec.Y) + normVec.Y)
                );

                if (h >= 0 && (h < minTileHeight || minTileHeight < 0))
                {
                    minTileHeight = h;
                }
            }

            double tileZ = minTileHeight;
            Vector3d loc = new(corner.X, corner.Y, tileZ);
            loc = Vector3d.Sum(loc, Vector3d.Product(normVec, 0.5))!;
            loc = Vector3d.Sum(loc, Vector3d.Product(dirVec, -0.5))!;
            double wallH = WallHeight + Math.Min(MAX_WALL_ADDITIONAL_HEIGHT, FloorHeight) - minTileHeight;
            Vector3d leftSide = Vector3d.Product(dirVec, -length)!;
            Vector3d rightSide = new(0, 0, wallH);
            loc = Vector3d.Dif(loc, leftSide)!;

            (int X, int Y) endPt = wallData.GetEndPoint(i);
            int origIdx = ResolveOriginalWallIndex(
                ((int)corner.X, (int)corner.Y), ((int)endPt.X, (int)endPt.Y), originalWallData
            );

            int nextDir,
                prevDir;

            if (origIdx >= 0)
            {
                nextDir = originalWallData.GetDirection((origIdx + 1) % origCount);
                prevDir = originalWallData.GetDirection((origIdx - 1 + origCount) % origCount);
            }
            else
            {
                nextDir = wallData.GetDirection((i + 1) % count);
                prevDir = wallData.GetDirection((i - 1 + count) % count);
            }

            IVector3d? secondaryNormal = null;

            if ((nextDir - direction + 4) % 4 == 3)
            {
                secondaryNormal = RoomWallData.WALL_NORMAL_VECTORS[nextDir];
            }
            else if ((direction - prevDir + 4) % 4 == 3)
            {
                secondaryNormal = RoomWallData.WALL_NORMAL_VECTORS[prevDir];
            }

            bool isLeftTurn = wallData.GetLeftTurn(i);
            bool isPrevLeftTurn = wallData.GetLeftTurn((i - 1 + count) % count);
            bool isNextHidden = wallData.GetHideWall((i + 1) % count);
            bool isManuallyLeftCut = wallData.GetManuallyLeftCut(i);
            bool isManuallyRightCut = wallData.GetManuallyRightCut(i);

            AddWall(
                loc, leftSide, rightSide, secondaryNormal,
                !isPrevLeftTurn || isManuallyLeftCut,
                !isLeftTurn || isManuallyRightCut,
                !isNextHidden
            );
        }
    }

    private bool CreateWallPlanes()
    {
        if (_tileMatrix.Count == 0)
        {
            return false;
        }

        int minRowLen = 0;

        foreach (double[] row in _tileMatrix)
        {
            if (row.Length == 0)
            {
                return false;
            }

            minRowLen = minRowLen > 0 ? Math.Min(minRowLen, row.Length) : row.Length;
        }

        double floorH = Math.Min(
            MAX_WALL_ADDITIONAL_HEIGHT,
            _fixedWallsHeight != -1 ? _fixedWallsHeight : GetFloorHeight(_tileMatrix)
        );

        int startX = MinX;
        int startY = MinY;

        // Find the first row that has a tile > TILE_HOLE at startX
        for (;
             startY <= MaxY;
             startY++)
        {
            if (!(GetTileHeightInternal(startX, startY) > TILE_HOLE))
            {
                continue;
            }

            startY--;

            break;
        }

        if (startY > MaxY)
        {
            return false;
        }

        (int X, int Y) startPoint = (X: startX, Y: startY);
        RoomWallData? wallData = GenerateWallData(startPoint, true);
        RoomWallData? originalWallData = GenerateWallData(startPoint, false);

        if (wallData != null)
        {
            CheckWallHiding(wallData, originalWallData!);
            AddWalls(wallData, originalWallData!);
        }

        // Fill unused tiles with negative height
        for (int y = 0;
             y < TileMapHeight;
             y++)
        {
            for (int x = 0;
                 x < TileMapWidth;
                 x++)
            {
                if (GetTileHeightInternal(x, y) < 0)
                {
                    SetTileHeight(x, y, -(floorH + WallHeight));
                }
            }
        }

        return true;
    }

    private (int X, int Y)? ExtractTopWall((int X, int Y) pos, bool treatHolesAsWalls)
    {
        int threshold = treatHolesAsWalls ? TILE_HOLE : TILE_BLOCKED;

        for (int step = 1;
             step < 1000;
             step++)
        {
            if (GetTileHeightInternal(pos.X + step, pos.Y) > threshold)
            {
                return (pos.X + step - 1, pos.Y);
            }

            if (GetTileHeightInternal(pos.X + step, pos.Y + 1) <= threshold)
            {
                return (pos.X + step, pos.Y + 1);
            }
        }

        return null;
    }

    private (int X, int Y)? ExtractRightWall((int X, int Y) pos, bool treatHolesAsWalls)
    {
        int threshold = treatHolesAsWalls ? TILE_HOLE : TILE_BLOCKED;

        for (int step = 1;
             step < 1000;
             step++)
        {
            if (GetTileHeightInternal(pos.X, pos.Y + step) > threshold)
            {
                return (pos.X, pos.Y + step - 1);
            }

            if (GetTileHeightInternal(pos.X - 1, pos.Y + step) <= threshold)
            {
                return (pos.X - 1, pos.Y + step);
            }
        }

        return null;
    }

    private (int X, int Y)? ExtractBottomWall((int X, int Y) pos, bool treatHolesAsWalls)
    {
        int threshold = treatHolesAsWalls ? TILE_HOLE : TILE_BLOCKED;

        for (int step = 1;
             step < 1000;
             step++)
        {
            if (GetTileHeightInternal(pos.X - step, pos.Y) > threshold)
            {
                return (pos.X - step + 1, pos.Y);
            }

            if (GetTileHeightInternal(pos.X - step, pos.Y - 1) <= threshold)
            {
                return (pos.X - step, pos.Y - 1);
            }
        }

        return null;
    }

    private (int X, int Y)? ExtractLeftWall((int X, int Y) pos, bool treatHolesAsWalls)
    {
        int threshold = treatHolesAsWalls ? TILE_HOLE : TILE_BLOCKED;

        for (int step = 1;
             step < 1000;
             step++)
        {
            if (GetTileHeightInternal(pos.X, pos.Y - step) > threshold)
            {
                return (pos.X, pos.Y - step + 1);
            }

            if (GetTileHeightInternal(pos.X + 1, pos.Y - step) <= threshold)
            {
                return (pos.X + 1, pos.Y - step);
            }
        }

        return null;
    }

    private void AddWall
    (
        IVector3d location,
        IVector3d leftSide,
        IVector3d rightSide,
        IVector3d? secondaryNormal,
        bool cutLeft,
        bool cutRight,
        bool showNextCorner
    )
    {
        List<IVector3d>? snList = secondaryNormal != null
            ? new List<IVector3d>
            {
                secondaryNormal,
            }
            : null;
        AddPlane(RoomPlaneData.TYPE_WALL, location, leftSide, rightSide, snList);
        AddPlane(RoomPlaneData.TYPE_LANDSCAPE, location, leftSide, rightSide, snList);

        double wallThick = WALL_THICKNESS * _wallThicknessMultiplier;
        double floorThick = FLOOR_THICKNESS * _floorThicknessMultiplier;

        Vector3d? cross = Vector3d.CrossProduct(leftSide, rightSide);

        if (cross == null)
        {
            return;
        }

        Vector3d normalizedThickness = Vector3d.Product(cross, 1.0 / cross.Length * -wallThick)!;

        // Top wall
        List<IVector3d>? topNormals = BuildSecondaryNormals(cross, secondaryNormal);

        AddPlane(
            RoomPlaneData.TYPE_WALL,
            Vector3d.Sum(location, rightSide)!,
            leftSide, normalizedThickness, topNormals
        );

        // Left edge
        if (cutLeft)
        {
            Vector3d extendedRight = Vector3d.Product(rightSide, -(rightSide.Length + floorThick) / rightSide.Length)!;

            AddPlane(
                RoomPlaneData.TYPE_WALL,
                Vector3d.Sum(Vector3d.Sum(location, leftSide)!, rightSide)!,
                extendedRight, normalizedThickness,
                BuildSecondaryNormals(cross, secondaryNormal)
            );
        }

        // Right edge
        if (cutRight)
        {
            Vector3d extendedRight = Vector3d.Product(rightSide, (rightSide.Length + floorThick) / rightSide.Length)!;
            Vector3d offsetLoc = Vector3d.Sum(location, Vector3d.Product(rightSide, -floorThick / rightSide.Length)!)!;
            AddPlane(
                RoomPlaneData.TYPE_WALL, offsetLoc, extendedRight, normalizedThickness,
                BuildSecondaryNormals(cross, secondaryNormal)
            );

            if (!showNextCorner)
            {
                return;
            }

            Vector3d cornerSide = Vector3d.Product(leftSide, wallThick / leftSide.Length)!;

            AddPlane(
                RoomPlaneData.TYPE_WALL,
                Vector3d.Sum(Vector3d.Sum(location, rightSide)!, Vector3d.Product(cornerSide, -1)!)!,
                cornerSide, normalizedThickness,
                BuildSecondaryNormals(cross, leftSide, secondaryNormal)
            );
        }
    }

    private static List<IVector3d>? BuildSecondaryNormals(params IVector3d?[] normals)
    {
        List<IVector3d>? result = null;

        foreach (IVector3d? n in normals)
        {
            if (n == null)
            {
                continue;
            }

            result ??= new List<IVector3d>();
            result.Add(n);
        }

        return result;
    }

    private void AddFloor
    (
        IVector3d location,
        IVector3d leftSide,
        IVector3d rightSide,
        bool showRight,
        bool showLeft,
        bool showBottom,
        bool showTop,
        bool isHighlight = false
    )
    {
        RoomPlaneData? plane = AddPlane(RoomPlaneData.TYPE_FLOOR, location, leftSide, rightSide, null, isHighlight);

        if (plane == null)
        {
            return;
        }

        double floorThick = FLOOR_THICKNESS * _floorThicknessMultiplier;
        Vector3d thicknessVec = new(0, 0, floorThick);
        Vector3d loweredLoc = Vector3d.Dif(location, thicknessVec)!;

        if (showBottom)
        {
            AddPlane(RoomPlaneData.TYPE_FLOOR, loweredLoc, leftSide, thicknessVec, null, isHighlight);
        }

        if (showTop)
        {
            AddPlane(
                RoomPlaneData.TYPE_FLOOR,
                Vector3d.Sum(loweredLoc, Vector3d.Sum(leftSide, rightSide)!)!,
                Vector3d.Product(leftSide, -1)!, thicknessVec, null, isHighlight
            );
        }

        if (showRight)
        {
            AddPlane(
                RoomPlaneData.TYPE_FLOOR,
                Vector3d.Sum(loweredLoc, rightSide)!,
                Vector3d.Product(rightSide, -1)!, thicknessVec, null, isHighlight
            );
        }

        if (showLeft)
        {
            AddPlane(
                RoomPlaneData.TYPE_FLOOR,
                Vector3d.Sum(loweredLoc, leftSide)!,
                rightSide, thicknessVec, null, isHighlight
            );
        }
    }

    private RoomPlaneData? AddPlane
    (
        int type,
        IVector3d location,
        IVector3d leftSide,
        IVector3d rightSide,
        List<IVector3d>? secondaryNormals = null,
        bool isHighlight = false
    )
    {
        if (leftSide.Length == 0 || rightSide.Length == 0)
        {
            return null;
        }

        RoomPlaneData plane = new(type, location, leftSide, rightSide, secondaryNormals);
        _planes.Add(plane);

        if (isHighlight)
        {
            _highlightPlanes.Add(plane);
        }

        return plane;
    }

    private void InitializeHoleMap()
    {
        bool hasInverted = _floorHolesInverted.Count > 0;

        for (int y = 0;
             y < TileMapHeight;
             y++)
        {
            bool[] row = _holeFlags[y];

            for (int x = 0;
                 x < TileMapWidth;
                 x++)
            {
                row[x] = hasInverted;
            }
        }

        foreach (RoomFloorHole hole in _floorHolesInverted.Values)
        {
            InitializeHole(hole, true);
        }

        foreach (RoomFloorHole hole in _floorHoles.Values)
        {
            InitializeHole(hole);
        }
    }

    private void InitializeHole(RoomFloorHole hole, bool invert = false)
    {
        int startX = Math.Max(0, hole.X);
        int endX = Math.Min(TileMapWidth - 1, hole.X + hole.Width - 1);
        int startY = Math.Max(0, hole.Y);
        int endY = Math.Min(TileMapHeight - 1, hole.Y + hole.Height - 1);

        for (int y = startY;
             y <= endY;
             y++)
        {
            bool[] row = _holeFlags[y];

            for (int x = startX;
                 x <= endX;
                 x++)
            {
                row[x] = !invert;
            }
        }
    }

    private void ExtractPlanes
    (
        int[][] tiles,
        int startX = 0,
        int startY = 0,
        int regionWidth = -1,
        int regionHeight = -1,
        bool isHighlight = false
    )
    {
        int totalRows = tiles.Length;
        int totalCols = tiles[0].Length;
        int endRow = regionHeight == -1 ? totalRows : Math.Min(totalRows, startY + regionHeight);
        int endCol = regionWidth == -1 ? totalCols : Math.Min(totalCols, startX + regionWidth);

        bool[][] visited = new bool[endRow][];

        for (int r = 0;
             r < endRow;
             r++)
        {
            visited[r] = new bool[endCol];
        }

        for (int r = startY;
             r < endRow;
             r++)
        {
            for (int c = startX;
                 c < endCol;
                 c++)
            {
                int val = tiles[r][c];

                if (val < 0 || visited[r][c])
                {
                    continue;
                }

                // Determine edge flags for this starting cell
                bool isLeftEdge = c == 0 || tiles[r][c - 1] != val;
                bool isTopEdge = r == 0 || tiles[r - 1][c] != val;

                // Expand right
                int rightEnd = c + 1;

                while (rightEnd < endCol)
                {
                    if (tiles[r][rightEnd] != val || visited[r][rightEnd] ||
                        (r > 0 && (tiles[r - 1][rightEnd] == val) == isTopEdge))
                    {
                        break;
                    }

                    rightEnd++;
                }

                bool isRightEdge = rightEnd == totalCols || tiles[r][rightEnd] != val;

                // Expand down
                bool done = false;
                int bottomEnd = r + 1;
                bool isBottomEdge = false;

                while (bottomEnd <= endRow && !done)
                {
                    isBottomEdge = bottomEnd == totalRows || tiles[bottomEnd][c] != val;
                    done = bottomEnd == endRow || isBottomEdge ||
                           (c > 0 && (tiles[bottomEnd][c - 1] == val) == isLeftEdge) ||
                           (rightEnd < totalCols && (tiles[bottomEnd][rightEnd] == val) == isRightEdge);

                    if (bottomEnd == totalRows)
                    {
                        break;
                    }

                    // Check each column in the row
                    for (int cc = c;
                         cc < rightEnd;
                         cc++)
                    {
                        if ((tiles[bottomEnd][cc] == val) != isBottomEdge)
                        {
                            continue;
                        }

                        done = true;
                        rightEnd = cc;

                        break;
                    }

                    if (done)
                    {
                        break;
                    }

                    bottomEnd++;
                }

                if (!isBottomEdge)
                {
                    isBottomEdge = bottomEnd == totalRows;
                }

                isRightEdge = rightEnd == totalCols || tiles[r][rightEnd] != val;

                // Mark visited
                for (int vr = r;
                     vr < bottomEnd;
                     vr++)
                {
                    for (int vc = c;
                         vc < rightEnd;
                         vc++)
                    {
                        visited[vr][vc] = true;
                    }
                }

                // Create floor plane
                double px = (c / 4.0) - 0.5;
                double py = (r / 4.0) - 0.5;
                double pw = (rightEnd - c) / 4.0;
                double ph = (bottomEnd - r) / 4.0;

                AddFloor(
                    new Vector3d(px + pw, py + ph, val / 4.0),
                    new Vector3d(-pw, 0, 0),
                    new Vector3d(0, -ph, 0),
                    isRightEdge, isLeftEdge, isBottomEdge, isTopEdge,
                    isHighlight
                );
            }
        }
    }
}
