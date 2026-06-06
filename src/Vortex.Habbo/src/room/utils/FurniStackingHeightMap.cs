using System;

namespace Vortex.Habbo.Room.Utils;

/// @see com.sulake.habbo.room.utils.FurniStackingHeightMap
public class FurniStackingHeightMap
{
    private double[]? _heights;
    private bool[]? _stackingBlocked;
    private bool[]? _roomTiles;

    public FurniStackingHeightMap(int width, int height)
    {
        Width = width;
        Height = height;

        int size = width * height;
        _heights = new double[size];
        _stackingBlocked = new bool[size];
        _roomTiles = new bool[size];
    }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public void Dispose()
    {
        _heights = null;
        _stackingBlocked = null;
        _roomTiles = null;
        Width = 0;
        Height = 0;
    }

    private bool ValidPosition(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public double GetTileHeight(int x, int y)
    {
        return ValidPosition(x, y) ? _heights![(y * Width) + x] : 0;
    }

    public void SetTileHeight(int x, int y, double height)
    {
        if (ValidPosition(x, y))
        {
            _heights![(y * Width) + x] = height;
        }
    }

    public void SetStackingBlocked(int x, int y, bool blocked)
    {
        if (ValidPosition(x, y))
        {
            _stackingBlocked![(y * Width) + x] = blocked;
        }
    }

    public void SetIsRoomTile(int x, int y, bool isRoom)
    {
        if (ValidPosition(x, y))
        {
            _roomTiles![(y * Width) + x] = isRoom;
        }
    }

    /// @see com.sulake.habbo.room.utils.FurniStackingHeightMap::validateLocation
    public bool ValidateLocation(
        int x, int y, int width, int height,
        int excludeX, int excludeY, int excludeWidth, int excludeHeight,
        bool onlyRoomTiles, double referenceHeight = -1)
    {
        if (!ValidPosition(x, y) || !ValidPosition(x + width - 1, y + height - 1))
        {
            return false;
        }

        if (excludeX < 0 || excludeX >= Width)
        {
            excludeX = 0;
        }

        if (excludeY < 0 || excludeY >= Height)
        {
            excludeY = 0;
        }

        excludeWidth = Math.Min(excludeWidth, Width - excludeX);
        excludeHeight = Math.Min(excludeHeight, Height - excludeY);

        if (referenceHeight < 0)
        {
            referenceHeight = GetTileHeight(x, y);
        }

        for (int tileY = y; tileY < y + height; tileY++)
        {
            for (int tileX = x; tileX < x + width; tileX++)
            {
                // Skip tiles in the exclusion zone
                if (tileX >= excludeX && tileX < excludeX + excludeWidth &&
                    tileY >= excludeY && tileY < excludeY + excludeHeight)
                {
                    continue;
                }

                int index = (tileY * Width) + tileX;

                if (onlyRoomTiles)
                {
                    if (!_roomTiles![index])
                    {
                        return false;
                    }
                }
                else
                {
                    if (_stackingBlocked![index] || !_roomTiles![index] ||
                        Math.Abs(_heights![index] - referenceHeight) > 0.01)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
}
