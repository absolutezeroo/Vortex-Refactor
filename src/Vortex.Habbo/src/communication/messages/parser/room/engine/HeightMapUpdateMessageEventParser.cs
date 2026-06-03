using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.HeightMapUpdateMessageEventParser
public class HeightMapUpdateMessageEventParser : IMessageParser
{
    private IMessageDataWrapper? _wrapper;
    private int _remaining;
    private int _value;
    private int _stackingBlockedMaskBit = 16384;
    private int _tileHeightMask = 16383;

    public int X { get; private set; }
    public int Y { get; private set; }

    public int StackingBlockedMaskBit
    {
        set
        {
            _stackingBlockedMaskBit = 1 << value;
            _tileHeightMask = _stackingBlockedMaskBit - 1;
        }
    }

    public double TileHeight => HeightMapMessageEventParser.DecodeTileHeight(_value, _tileHeightMask);
    public bool IsStackingBlocked => HeightMapMessageEventParser.DecodeIsStackingBlocked(_value, _stackingBlockedMaskBit);
    public bool IsRoomTile => HeightMapMessageEventParser.DecodeIsRoomTile(_value);

    public bool Next()
    {
        if (_remaining == 0)
        {
            return false;
        }
        _remaining--;
        X = _wrapper!.ReadByte();
        Y = _wrapper.ReadByte();
        _value = _wrapper.ReadShort();
        return true;
    }

    public bool Flush()
    {
        _remaining = 0;
        _wrapper = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        _wrapper = param1;
        _remaining = param1.ReadByte();
        return true;
    }
}
