using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.HeightMapMessageEventParser
public class HeightMapMessageEventParser : IMessageParser
{
    private int _stackingBlockedMaskBit = 16384;
    private int _tileHeightMask = 16383;
    private int[]? _data;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public int StackingBlockedMaskBit
    {
        set
        {
            _stackingBlockedMaskBit = 1 << value;
            _tileHeightMask = _stackingBlockedMaskBit - 1;
        }
    }

    public static double DecodeTileHeight(int value, int mask)
    {
        return value == -1 ? -1 : (value & mask) / 256.0;
    }

    public static bool DecodeIsStackingBlocked(int value, int mask)
    {
        return (value & mask) != 0;
    }

    public static bool DecodeIsRoomTile(int value)
    {
        return value != -1;
    }

    public double GetTileHeight(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return -1;
        }
        return DecodeTileHeight(_data![(y * Width) + x], _tileHeightMask);
    }

    public bool GetStackingBlocked(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return true;
        }
        return DecodeIsStackingBlocked(_data![(y * Width) + x], _stackingBlockedMaskBit);
    }

    public bool IsRoomTile(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return false;
        }
        return DecodeIsRoomTile(_data![(y * Width) + x]);
    }

    public bool Flush()
    {
        _data = null;
        Width = 0;
        Height = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }

        Width = param1.ReadInteger();
        int totalTiles = param1.ReadInteger();
        Height = totalTiles / Width;
        _data = new int[totalTiles];

        for (int i = 0; i < totalTiles; i++)
        {
            _data[i] = param1.ReadShort();
        }

        return true;
    }
}
