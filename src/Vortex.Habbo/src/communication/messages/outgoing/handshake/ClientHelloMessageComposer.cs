using Godot;

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Handshake;

public class ClientHelloMessageComposer : IMessageComposer, IPreEncryptionMessage
{
    private readonly string var_1585 = "FLASH20";

    public void Dispose() { }

    public List<object> GetMessageArray()
    {
        string version = "WIN63-202407091256-704579380";
        int platform = 0;
        string? os = OS.GetName();
        if (os.Contains("Windows"))
        {
            platform = 6;
        }
        else if (os.Contains("macOS") || os.Contains("Mac"))
        {
            platform = 5;
        }
        else if (os.Contains("Linux"))
        {
            platform = 7;
        }

        return
        [
            version,
            var_1585,
            platform,
            4,
        ];
    }
}
