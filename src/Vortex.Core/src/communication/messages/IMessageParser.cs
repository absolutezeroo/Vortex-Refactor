namespace Vortex.Core.Communication.Messages;

public interface IMessageParser
{
    bool Flush();

    bool Parse(IMessageDataWrapper param1);
}
