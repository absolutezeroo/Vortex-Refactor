namespace Vortex.Core.Communication.Encryption;

public interface IEncryption
{
    void Init(byte[] param1);

    void Encipher(byte[] param1);

    void Decipher(byte[] param1);

    void Mark();

    void Reset();
}
