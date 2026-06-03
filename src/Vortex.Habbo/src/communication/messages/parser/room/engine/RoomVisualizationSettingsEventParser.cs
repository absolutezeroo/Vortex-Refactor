using System;

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.RoomVisualizationSettingsEventParser
public class RoomVisualizationSettingsEventParser : IMessageParser
{
    public bool WallsHidden { get; private set; }
    public double WallThicknessMultiplier { get; private set; } = 1;
    public double FloorThicknessMultiplier { get; private set; } = 1;

    public bool Flush()
    {
        WallsHidden = false;
        WallThicknessMultiplier = 1;
        FloorThicknessMultiplier = 1;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        WallsHidden = param1.ReadBoolean();
        int wallThickness = param1.ReadInteger();
        int floorThickness = param1.ReadInteger();
        wallThickness = Math.Clamp(wallThickness, -2, 1);
        floorThickness = Math.Clamp(floorThickness, -2, 1);
        WallThicknessMultiplier = Math.Pow(2, wallThickness);
        FloorThicknessMultiplier = Math.Pow(2, floorThickness);
        return true;
    }
}
