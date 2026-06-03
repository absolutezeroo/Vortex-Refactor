using System.Globalization;
using System.Numerics;

using Vortex.Core.Communication.Handshake;

namespace Vortex.Habbo.Communication.Encryption;

/// @see WIN63-202407091256-704579380-Source-main/habbo/communication/encryption/DiffieHellman.as
public class DiffieHellman : IKeyExchange
{
    private readonly BigInteger _generator;
    private readonly BigInteger _minimumPublicKey = 2;
    private readonly BigInteger _minimumSharedKey = 2;
    private readonly BigInteger _prime;
    private BigInteger _privateKey;
    private BigInteger _publicKey;
    private BigInteger _serverPublicKey;
    private BigInteger _sharedKey;

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/encryption/DiffieHellman.as::DiffieHellman
    public DiffieHellman(object? param1, object? param2)
    {
        _prime = ParseBigInteger(param1);
        _generator = ParseBigInteger(param2);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/encryption/DiffieHellman.as::init
    public bool Init(string param1, uint param2 = 16)
    {
        _privateKey = ParseHex(param1, param2);
        _publicKey = BigInteger.ModPow(_generator, _privateKey, _prime);

        return true;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/encryption/DiffieHellman.as::generateSharedKey
    public string GenerateSharedKey(string param1, uint param2 = 16)
    {
        _serverPublicKey = ParseHex(param1, param2);
        _sharedKey = BigInteger.ModPow(_serverPublicKey, _privateKey, _prime);

        return ToHex(_sharedKey, param2);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/encryption/DiffieHellman.as::getSharedKey
    public string GetSharedKey(uint param1 = 16)
    {
        return ToHex(_sharedKey, param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/encryption/DiffieHellman.as::getPublicKey
    public string GetPublicKey(uint param1 = 16)
    {
        return ToHex(_publicKey, param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/encryption/DiffieHellman.as::isValidServerPublicKey
    public bool IsValidServerPublicKey()
    {
        return _serverPublicKey >= _minimumPublicKey;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/encryption/DiffieHellman.as::isValidSharedKey
    public bool IsValidSharedKey()
    {
        return _sharedKey >= _minimumSharedKey;
    }

    private static BigInteger ParseBigInteger(object? value)
    {
        if (value is BigInteger bi)
        {
            return bi;
        }

        if (value is string s && !string.IsNullOrEmpty(s))
        {
            return ParseHex(s, 16);
        }

        return BigInteger.Zero;
    }

    private static BigInteger ParseHex(string value, uint radix)
    {
        if (string.IsNullOrEmpty(value))
        {
            return BigInteger.Zero;
        }

        if (radix == 10)
        {
            return BigInteger.Parse(value);
        }

        // Prepend "0" to ensure positive interpretation
        return BigInteger.Parse("0" + value, NumberStyles.HexNumber);
    }

    private static string ToHex(BigInteger value, uint radix)
    {
        if (radix == 10)
        {
            return value.ToString();
        }

        string hex = value.ToString("x");

        // Strip leading zero used for a positive sign
        if (hex.Length <= 1 || hex[0] != '0')
        {
            return hex;
        }

        hex = hex.TrimStart('0');

        if (hex.Length == 0)
        {
            hex = "0";
        }

        return hex;
    }
}
