using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object;

/// <summary>
/// Represents the traced boundary wall perimeter of a room floor.
/// Each segment has a corner position, direction, length, and flags.
/// </summary>
/// @see com.sulake.habbo.room.object.RoomWallData
public class RoomWallData
{
    public static readonly IVector3d[] WALL_DIRECTION_VECTORS =
        [new Vector3d(1, 0, 0), new Vector3d(0, 1, 0), new Vector3d(-1, 0, 0), new Vector3d(0, -1, 0)];

    public static readonly IVector3d[] WALL_NORMAL_VECTORS =
        [new Vector3d(0, 1, 0), new Vector3d(-1, 0, 0), new Vector3d(0, -1, 0), new Vector3d(1, 0, 0)];

    private readonly List<(int X, int Y)> _corners = [];
    private readonly List<int> _directions = [];
    private readonly List<int> _lengths = [];
    private readonly List<bool> _borders = [];
    private readonly List<bool> _leftTurns = [];
    private readonly List<bool> _hideWalls = [];
    private readonly List<bool> _manuallyLeftCut = [];
    private readonly List<bool> _manuallyRightCut = [];

    public int Count { get; private set; }

    public void AddWall((int X, int Y) corner, int direction, int length, bool isBorder, bool isLeftTurn)
    {
        if (!CheckIsNotDuplicate(corner, direction, length, isBorder, isLeftTurn))
        {
            return;
        }
        _corners.Add(corner);
        _directions.Add(direction);
        _lengths.Add(length);
        _borders.Add(isBorder);
        _leftTurns.Add(isLeftTurn);
        _hideWalls.Add(false);
        _manuallyLeftCut.Add(false);
        _manuallyRightCut.Add(false);
        _cachedEndPoints = null;
        Count++;
    }

    public (int X, int Y) GetCorner(int index)
    {
        return _corners[index];
    }
    public int GetDirection(int index)
    {
        return _directions[index];
    }
    public int GetLength(int index)
    {
        return _lengths[index];
    }
    public bool GetBorder(int index)
    {
        return _borders[index];
    }
    public bool GetLeftTurn(int index)
    {
        return _leftTurns[index];
    }
    public bool GetHideWall(int index)
    {
        return _hideWalls[index];
    }
    public bool GetManuallyLeftCut(int index)
    {
        return _manuallyLeftCut[index];
    }
    public bool GetManuallyRightCut(int index)
    {
        return _manuallyRightCut[index];
    }

    public void SetHideWall(int index, bool value)
    {
        _hideWalls[index] = value;
    }

    public void SetLength(int index, int value)
    {
        if (value >= _lengths[index])
        {
            return;
        }

        _lengths[index] = value;
        _manuallyRightCut[index] = true;
        _cachedEndPoints = null;
    }

    public void MoveCorner(int index, int offset)
    {
        if (offset <= 0 || offset >= _lengths[index])
        {
            return;
        }

        IVector3d dirVec = WALL_DIRECTION_VECTORS[GetDirection(index)];
        (int X, int Y) c = _corners[index];
        _corners[index] = (c.X + (int)(offset * dirVec.X), c.Y + (int)(offset * dirVec.Y));
        _lengths[index] -= offset;
        _manuallyLeftCut[index] = true;
        _cachedEndPoints = null;
    }

    public (int X, int Y) GetEndPoint(int index)
    {
        CalculateEndPoints();

        return _cachedEndPoints![index];
    }

    private List<(int X, int Y)>? _cachedEndPoints;

    private void CalculateEndPoints()
    {
        if (_cachedEndPoints != null && _cachedEndPoints.Count == Count)
        {
            return;
        }

        _cachedEndPoints = new List<(int X, int Y)>();

        for (int i = 0;
             i < Count;
             i++)
        {
            (int X, int Y) corner = _corners[i];
            IVector3d dirVec = WALL_DIRECTION_VECTORS[_directions[i]];
            int length = _lengths[i];

            _cachedEndPoints.Add((corner.X + (int)(dirVec.X * length), corner.Y + (int)(dirVec.Y * length)));
        }
    }

    private bool CheckIsNotDuplicate((int X, int Y) corner, int direction, int length, bool isBorder, bool isLeftTurn)
    {
        for (int i = 0;
             i < Count;
             i++)
        {
            if (_corners[i].X == corner.X && _corners[i].Y == corner.Y &&
                _directions[i] == direction && _lengths[i] == length &&
                _borders[i] == isBorder && _leftTurns[i] == isLeftTurn)
            {
                return false;
            }
        }

        return true;
    }
}
