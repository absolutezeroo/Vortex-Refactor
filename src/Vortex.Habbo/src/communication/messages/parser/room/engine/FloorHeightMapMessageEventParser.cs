using System;
using System.Globalization;

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.FloorHeightMapMessageEventParser
public class FloorHeightMapMessageEventParser : IMessageParser
{
    private int[][] _tileMap = [];

    public string Text { get; private set; } = "";
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int FixedWallsHeight { get; private set; } = -1;
    public double Scale { get; private set; }
    public List<AreaHideMessageData>? AreaHideData { get; private set; }

    public int GetTileHeight(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return -110;
        }
        return _tileMap[y][x];
    }

    public bool Flush()
    {
        _tileMap = [];
        Width = 0;
        Height = 0;
        Text = "";
        FixedWallsHeight = -1;
        AreaHideData = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }

        bool isSmallScale = param1.ReadBoolean();
        FixedWallsHeight = param1.ReadInteger();
        Text = param1.ReadString();

        string[] rows = Text.Split('\r');
        int rowCount = rows.Length;
        if (rowCount > 0 && rows[rowCount - 1] == "")
        {
            rowCount--;
        }

        int maxWidth = 0;
        for (int i = 0; i < rowCount; i++)
        {
            if (rows[i].Length > maxWidth)
            {
                maxWidth = rows[i].Length;
            }
        }

        _tileMap = new int[rowCount][];
        for (int y = 0; y < rowCount; y++)
        {
            _tileMap[y] = new int[maxWidth];
            Array.Fill(_tileMap[y], -110);

            string row = rows[y];
            for (int x = 0; x < row.Length; x++)
            {
                char c = row[x];
                _tileMap[y][x] = c is 'x' or 'X'
                    ? -110
                    : int.Parse(c.ToString(), NumberStyles.HexNumber);
            }
        }

        Width = maxWidth;
        Height = rowCount;
        Scale = isSmallScale ? 32 : 64;

        AreaHideData = [];
        int areaCount = param1.ReadInteger();
        for (int i = 0; i < areaCount; i++)
        {
            AreaHideData.Add(new AreaHideMessageData(param1));
        }
        return true;
    }
}

/// @see com.sulake.habbo.communication.messages.parser.room.engine.AreaHideMessageData
public class AreaHideMessageData(IMessageDataWrapper param1)
{
    public int Id { get; } = param1.ReadInteger();
    public bool IsHidden { get; } = param1.ReadBoolean();
    public int RootX { get; } = param1.ReadInteger();
    public int RootY { get; } = param1.ReadInteger();
    public int Width { get; } = param1.ReadInteger();
    public int Height { get; } = param1.ReadInteger();
    public bool InvertArea { get; } = param1.ReadBoolean();
    public int WallsMode { get; } = param1.ReadInteger();
}
