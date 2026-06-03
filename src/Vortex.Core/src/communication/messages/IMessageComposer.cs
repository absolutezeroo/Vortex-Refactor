namespace Vortex.Core.Communication.Messages;

public interface IMessageComposer
{
    List<object> GetMessageArray();

    void Dispose();
}
