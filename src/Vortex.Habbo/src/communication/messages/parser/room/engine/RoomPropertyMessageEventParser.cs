using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.RoomPropertyMessageEventParser
public class RoomPropertyMessageEventParser : IMessageParser
{
    public string? FloorType { get; private set; }
    public string? WallType { get; private set; }
    public string? LandscapeType { get; private set; }
    public string? AnimatedLandscapeType { get; private set; }

    public bool Flush()
    {
        FloorType = null;
        WallType = null;
        LandscapeType = null;
        AnimatedLandscapeType = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        string key = param1.ReadString();
        string value = param1.ReadString();
        switch (key)
        {
            case "floor":
                FloorType = value;
                break;
            case "wallpaper":
                WallType = value;
                break;
            case "landscape":
                LandscapeType = value;
                break;
            case "landscapeanim":
                AnimatedLandscapeType = value;
                break;
        }
        return true;
    }
}
