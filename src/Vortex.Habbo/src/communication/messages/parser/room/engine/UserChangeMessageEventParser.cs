using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.UserChangeMessageEventParser
public class UserChangeMessageEventParser : IMessageParser
{
    public int Id { get; private set; }
    public string Figure { get; private set; } = "";
    public string Sex { get; private set; } = "";
    public string CustomInfo { get; private set; } = "";
    public int AchievementScore { get; private set; }

    public bool Flush()
    {
        Id = 0;
        Figure = "";
        Sex = "";
        CustomInfo = "";
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        Id = param1.ReadInteger();
        Figure = param1.ReadString();
        Sex = param1.ReadString();
        CustomInfo = param1.ReadString();
        AchievementScore = param1.ReadInteger();
        if (!string.IsNullOrEmpty(Sex))
        {
            Sex = Sex.ToUpperInvariant();
        }
        return true;
    }
}
