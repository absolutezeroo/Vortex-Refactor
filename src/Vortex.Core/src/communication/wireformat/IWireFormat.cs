using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;

namespace Vortex.Core.Communication.Wireformat;

public interface IWireFormat
{
    void Dispose();

    byte[] Encode(int param1, List<object> param2);

    List<IMessageDataWrapper> SplitMessages(byte[] param1, IConnection param2);

    byte[] GetRemainder();
}
