using System;

using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Utils;

/// @see com.sulake.habbo.room.utils.LegacyWallGeometry
public class LegacyWallGeometry
{
    private const string DIRECTION_LEFT = "l";
    private const string DIRECTION_RIGHT = "r";

    private double[][]? _heights;
    private int _width;
    private int _height;
    private int _floorHeight;

    public bool Disposed { get; private set; }

    public int Scale { get; set; } = 64;

    public void Dispose()
    {
        Reset();

        Disposed = true;
    }

    /// @see com.sulake.habbo.room.utils.LegacyWallGeometry::initialize
    public void Initialize(int width, int height, int floorHeight)
    {
        if (width <= _width && height <= _height)
        {
            _width = width;
            _height = height;
            _floorHeight = floorHeight;

            return;
        }

        Reset();

        _heights = new double[height][];

        for (int y = 0; y < height; y++)
        {
            _heights[y] = new double[width];
        }

        _width = width;
        _height = height;
        _floorHeight = floorHeight;
    }

    private void Reset()
    {
        _heights = null;
    }

    public bool SetTileHeight(int x, int y, double height)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
        {
            return false;
        }

        if (_heights?[y] == null)
        {
            return false;
        }

        _heights[y][x] = height;
 return true;

    }

    public double GetTileHeight(int x, int y)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
        {
            return 0;
        }

        if (_heights?[y] != null)
        {
            return _heights[y][x];
        }

        return 0;
    }

    /// @see com.sulake.habbo.room.utils.LegacyWallGeometry::getLocation
    public IVector3d GetLocation(int x, int y, int pixelX, int pixelY, string direction)
    {
        if (x == 0 && y == 0)
        {
            x = _width;
            y = _height;

            int roundedScale = (int)Math.Round(Scale / 10.0);

            if (direction == DIRECTION_RIGHT)
            {
                for (int col = _width - 1; col >= 0; col--)
                {
                    for (int row = 1; row < _height; row++)
                    {
                        if (!(GetTileHeight(col, row) <= _floorHeight))
                        {
                            continue;
                        }

                        if (row - 1 < y)
                        {
                            x = col;
                            y = row - 1;
                        }

                        break;
                    }
                }

                pixelY = (int)(pixelY + (Scale / 4.0 - roundedScale / 2.0));
                pixelX = (int)(pixelX + Scale / 2.0);
            }
            else
            {
                for (int row = _height - 1; row >= 0; row--)
                {
                    for (int col = 1; col < _width; col++)
                    {
                        if (!(GetTileHeight(col, row) <= _floorHeight))
                        {
                            continue;
                        }

                        if (col - 1 < x)
                        {
                            x = col - 1;
                            y = row;
                        }

                        break;
                    }
                }

                pixelY = (int)(pixelY + (Scale / 4.0 - roundedScale / 2.0));
                pixelX -= roundedScale;
            }
        }

        double locX = x;
        double locY = y;
        double locZ = GetTileHeight(x, y);
        double halfScale = Scale / 2.0;

        if (direction == DIRECTION_RIGHT)
        {
            locX += pixelX / halfScale - 0.5;
            locY += 0.5;
            locZ -= (pixelY - pixelX / 2.0) / halfScale;
        }
        else
        {
            locY += (halfScale - pixelX) / halfScale - 0.5;
            locX += 0.5;
            locZ -= (pixelY - (halfScale - pixelX) / 2.0) / halfScale;
        }

        return new Vector3d(locX, locY, locZ);
    }

    /// @see com.sulake.habbo.room.utils.LegacyWallGeometry::getLocationOldFormat
    public IVector3d GetLocationOldFormat(double x, double y, string direction)
    {
        int tileY = (int)Math.Ceiling(x);
        int offset = tileY - (int)x;
        int tileX = 0;

        for (int col = 0; col < _width; col++)
        {
            if (tileY >= 0 && tileY < _height)
            {
                if (GetTileHeight(col, tileY) <= _floorHeight)
                {
                    tileX = col - 1;
                    direction = DIRECTION_LEFT;

                    break;
                }

                if (GetTileHeight(col, tileY + 1) <= _floorHeight)
                {
                    tileX = col;
                    offset = tileY - (int)x;
                    direction = DIRECTION_RIGHT;

                    break;
                }
            }

            tileY++;
        }

        int pixelX = (int)(Scale / 2.0 * offset);
        double heightOffset = -y * 18.0 / 32.0 * Scale / 2.0;
        double tileHeight = GetTileHeight(tileX, tileY);
        int pixelY = (int)(tileHeight * Scale / 2.0 + heightOffset);

        if (direction == DIRECTION_RIGHT)
        {
            pixelY = (int)(pixelY + offset * Scale / 4.0);
        }
        else
        {
            pixelY = (int)(pixelY + (1 - offset) * Scale / 4.0);
        }

        return GetLocation(tileX, tileY, pixelX, pixelY, direction);
    }

    /// @see com.sulake.habbo.room.utils.LegacyWallGeometry::getOldLocation
    public double[]? GetOldLocation(IVector3d? location, double angle)
    {
        if (location == null)
        {
            return null;
        }

        int tileX;
        int tileY;
        double pixelX;
        double pixelY;
        string dir;
        double halfScale = Scale / 2.0;

        switch (angle)
        {
            case 90:
                {
                    tileX = (int)Math.Floor(location.X - 0.5);
                    tileY = (int)Math.Floor(location.Y + 0.5);

                    double tileHeight = GetTileHeight(tileX, tileY);
                    pixelX = halfScale - (location.Y - tileY + 0.5) * halfScale;
                    pixelY = (tileHeight - location.Z) * halfScale + (halfScale - pixelX) / 2.0;
                    dir = DIRECTION_LEFT;

                    break;
                }
            case 180:
                {
                    tileX = (int)Math.Floor(location.X + 0.5);
                    tileY = (int)Math.Floor(location.Y - 0.5);

                    double tileHeight = GetTileHeight(tileX, tileY);
                    pixelX = (location.X + 0.5 - tileX) * halfScale;
                    pixelY = (tileHeight - location.Z) * halfScale + pixelX / 2.0;
                    dir = DIRECTION_RIGHT;

                    break;
                }
            default:
                return null;
        }

        return [tileX, tileY, pixelX, pixelY, dir == DIRECTION_LEFT ? 0 : 1];
    }

    /// @see com.sulake.habbo.room.utils.LegacyWallGeometry::getOldLocationString
    public string? GetOldLocationString(IVector3d? location, double angle)
    {
        double[]? result = GetOldLocation(location, angle);

        if (result == null)
        {
            return null;
        }

        int x = (int)result[0];
        int y = (int)result[1];
        int px = (int)result[2];
        int py = (int)result[3];
        string dir = result[4] == 0 ? DIRECTION_LEFT : DIRECTION_RIGHT;

        return $":w={x},{y} l={px},{py} {dir}";
    }

    /// @see com.sulake.habbo.room.utils.LegacyWallGeometry::getDirection
    public double GetDirection(string direction)
    {
        if (direction == DIRECTION_RIGHT)
        {
            return 180;
        }

        return 90;
    }

    /// @see com.sulake.habbo.room.utils.LegacyWallGeometry::getFloorAltitude
    public double GetFloorAltitude(int x, int y)
    {
        double height = GetTileHeight(x, y);
        double adjacent = height + 1;

        bool hasAdjacentHigher =
            GetTileHeight(x - 1, y - 1) == adjacent ||
            GetTileHeight(x, y - 1) == adjacent ||
            GetTileHeight(x + 1, y - 1) == adjacent ||
            GetTileHeight(x - 1, y) == adjacent ||
            GetTileHeight(x + 1, y) == adjacent ||
            GetTileHeight(x - 1, y + 1) == adjacent ||
            GetTileHeight(x, y + 1) == adjacent ||
            GetTileHeight(x + 1, y + 1) == adjacent;

        return height + (hasAdjacentHigher ? 0.5 : 0);
    }

    /// @see com.sulake.habbo.room.utils.LegacyWallGeometry::isRoomTile
    public bool IsRoomTile(int x, int y)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height && _heights != null && _heights[y][x] >= 0;
    }
}
