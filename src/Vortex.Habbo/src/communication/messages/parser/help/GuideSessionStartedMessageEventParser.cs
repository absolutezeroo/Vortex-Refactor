using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Help;

/// @see com.sulake.habbo.communication.messages.parser.help.GuideSessionStartedMessageParser
public class GuideSessionStartedMessageEventParser : IMessageParser
{
    public int requesterUserId { get; private set; }
    public string requesterName { get; private set; } = "";
    public string requesterFigure { get; private set; } = "";
    public int guideUserId { get; private set; }
    public string guideName { get; private set; } = "";
    public string guideFigure { get; private set; } = "";

    public bool Flush()
    {
        requesterUserId = 0;
        requesterName = "";
        requesterFigure = "";
        guideUserId = 0;
        guideName = "";
        guideFigure = "";
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        requesterUserId = param1.ReadInteger();
        requesterName = param1.ReadString();
        requesterFigure = param1.ReadString();
        guideUserId = param1.ReadInteger();
        guideName = param1.ReadString();
        guideFigure = param1.ReadString();
        return true;
    }
}
