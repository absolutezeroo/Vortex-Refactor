using System.Text;

using Vortex.Room.Object;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Utils;

/// @see com.sulake.habbo.room.utils.TileObjectMap
public class TileObjectMap
{
    private IRoomObject?[][]? _grid;
    private int _width;
    private int _height;

    public TileObjectMap(int width, int height)
    {
        _width = width;
        _height = height;
        _grid = new IRoomObject?[height][];

        for (int y = 0; y < height; y++)
        {
            _grid[y] = new IRoomObject?[width];
        }
    }

    public void Clear()
    {
        if (_grid == null)
        {
            return;
        }

        foreach (IRoomObject?[] row in _grid)
        {
            for (int x = 0; x < _width; x++)
            {
                row[x] = null;
            }
        }
    }

    public void Populate(IRoomObject[] objects)
    {
        Clear();

        foreach (IRoomObject obj in objects)
        {
            AddRoomObject(obj);
        }
    }

    public void Dispose()
    {
        _grid = null;
        _width = 0;
        _height = 0;
    }

    public IRoomObject? GetObjectIntTile(int x, int y)
    {
        if (x >= 0 && x < _width && y >= 0 && y < _height)
        {
            return _grid![y][x];
        }

        return null;
    }

    public void SetObjectInTile(int x, int y, IRoomObject obj)
    {
        if (!obj.IsInitialized)
        {
            return;
        }

        if (x >= 0 && x < _width && y >= 0 && y < _height)
        {
            _grid![y][x] = obj;
        }
    }

    /// @see com.sulake.habbo.room.utils.TileObjectMap::addRoomObject
    public void AddRoomObject(IRoomObject obj)
    {
        if (obj == null || !obj.IsInitialized)
        {
            return;
        }

        IVector3d loc = obj.Location;
        IVector3d dir = obj.Direction;

        int sizeX = (int)obj.Model.GetNumber("furniture_size_x");
        int sizeY = (int)obj.Model.GetNumber("furniture_size_y");

        if (sizeX < 1)
        {
            sizeX = 1;
        }

        if (sizeY < 1)
        {
            sizeY = 1;
        }

        // Rotate if direction is 90 or 270 degrees
        int dirQuadrant = (int)(((int)dir.X + 45) % 360 / 90);

        if (dirQuadrant == 1 || dirQuadrant == 3)
        {
            (sizeX, sizeY) = (sizeY, sizeX);
        }

        for (int tileY = (int)loc.Y; tileY < (int)loc.Y + sizeY; tileY++)
        {
            for (int tileX = (int)loc.X; tileX < (int)loc.X + sizeX; tileX++)
            {
                IRoomObject? existing = GetObjectIntTile(tileX, tileY);

                if (existing == null || (existing != obj && existing.Location.Z <= loc.Z))
                {
                    SetObjectInTile(tileX, tileY, obj);
                }
            }
        }
    }

    public override string ToString()
    {
        if (_grid == null)
        {
            return "";
        }

        StringBuilder sb = new();

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                IRoomObject? obj = _grid[y][x];
                sb.Append(obj != null ? obj.Id.ToString() : "x");
                sb.Append('\t');
            }

            sb.Append('\n');
        }

        return sb.ToString();
    }
}
