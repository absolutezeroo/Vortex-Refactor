namespace Vortex.Core.Communication.Handshake;

public interface IKeyExchange
{
    bool Init(string param1, uint param2 = 16);

    string GenerateSharedKey(string param1, uint param2 = 16);

    string GetSharedKey(uint param1 = 16);

    string GetPublicKey(uint param1 = 16);

    bool IsValidServerPublicKey();

    bool IsValidSharedKey();
}
