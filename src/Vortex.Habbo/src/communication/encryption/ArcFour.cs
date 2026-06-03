using Vortex.Core.Communication.Encryption;

namespace Vortex.Habbo.Communication.Encryption;

/// @see WIN63-202407091256-704579380-Source-main/habbo/communication/encryption/ArcFour.as
public class ArcFour : IEncryption
{
    private int _i;
    private int _j;
    private int _markedI;
    private int _markedJ;
    private byte[] _markedSbox = [];
    private byte[] _sbox = new byte[256];

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/encryption/ArcFour.as::init
    public void Init(byte[] param1)
    {
        _i = 0;
        _j = 0;

        for (int i = 0;
             i < 256;
             i++)
        {
            _sbox[i] = (byte)i;
        }

        int j = 0;

        for (int i = 0;
             i < 256;
             i++)
        {
            j = (j + _sbox[i] + param1[i % param1.Length]) & 255;

            (_sbox[i], _sbox[j]) = (_sbox[j], _sbox[i]);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/encryption/ArcFour.as::encipher
    public void Encipher(byte[] param1)
    {
        for (int k = 0;
             k < param1.Length;
             k++)
        {
            param1[k] = (byte)(param1[k] ^ Next());
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/encryption/ArcFour.as::decipher
    public void Decipher(byte[] param1)
    {
        Encipher(param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/encryption/ArcFour.as::mark
    public void Mark()
    {
        _markedI = _i;
        _markedJ = _j;
        _markedSbox = (byte[])_sbox.Clone();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/encryption/ArcFour.as::reset
    public void Reset()
    {
        if (_markedSbox.Length <= 0)
        {
            return;
        }

        _i = _markedI;
        _j = _markedJ;
        _sbox = (byte[])_markedSbox.Clone();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/encryption/ArcFour.as::next
    private byte Next()
    {
        _i = (_i + 1) & 255;
        _j = (_j + _sbox[_i]) & 255;

        (_sbox[_i], _sbox[_j]) = (_sbox[_j], _sbox[_i]);

        return _sbox[(_sbox[_i] + _sbox[_j]) & 255];
    }
}
