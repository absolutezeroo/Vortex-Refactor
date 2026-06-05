using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Avatar;

/// @see com.sulake.habbo.communication.messages.parser.avatar.FigureUpdateParser
public class FigureUpdateMessageEventParser : IMessageParser
{
    public string figure { get; private set; } = "";
    public string gender { get; private set; } = "";

    public bool Flush()
    {
        figure = "";
        gender = "";
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        figure = param1.ReadString();
        gender = param1.ReadString().ToUpperInvariant();
        return true;
    }
}
