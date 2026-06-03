using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Error;

public class ErrorReportEventParser : IMessageParser
{
    public int errorCode { get; private set; }

    public int messageId { get; private set; }

    public string timestamp { get; private set; } = string.Empty;

    public bool Parse(IMessageDataWrapper param1)
    {
        messageId = param1.ReadInteger();
        errorCode = param1.ReadInteger();
        timestamp = param1.ReadString();
        return true;
    }

    public bool Flush()
    {
        errorCode = 0;
        messageId = 0;
        timestamp = string.Empty;
        return true;
    }
}
