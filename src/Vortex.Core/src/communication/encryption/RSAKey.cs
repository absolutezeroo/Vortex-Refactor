using System;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Vortex.Core.Communication.Encryption;

/// @see com.hurlant.crypto.rsa.RSAKey (used by AS3 Habbo client for DH handshake)
public class RSAKey : IDisposable
{
    private BigInteger _modulus;
    private BigInteger _exponent;
    private readonly int _blockSize;

    private RSAKey(BigInteger modulus, BigInteger exponent)
    {
        _modulus = modulus;
        _exponent = exponent;
        _blockSize = modulus.GetByteCount(isUnsigned: true);
    }

    /// @see com.hurlant.crypto.rsa.RSAKey::parsePublicKey
    public static RSAKey ParsePublicKey(string n, string e)
    {
        BigInteger modulus = BigInteger.Parse("0" + n, NumberStyles.HexNumber);
        BigInteger exponent = BigInteger.Parse("0" + e, NumberStyles.HexNumber);

        return new RSAKey(modulus, exponent);
    }

    /// RSA verify (decrypt with public key): signature^e mod n
    /// @see com.hurlant.crypto.rsa.RSAKey::verify
    public byte[] Verify(byte[] src)
    {
        BigInteger input = BytesToBigInt(src);
        BigInteger result = BigInteger.ModPow(input, _exponent, _modulus);

        return BigIntToBytes(result);
    }

    /// RSA encrypt with PKCS#1 v1.5 type 0x02 padding, then plaintext^e mod n
    /// @see com.hurlant.crypto.rsa.RSAKey::encrypt
    public byte[] Encrypt(byte[] src)
    {
        byte[] padded = AddPKCS1Padding(src);
        BigInteger input = BytesToBigInt(padded);
        BigInteger result = BigInteger.ModPow(input, _exponent, _modulus);

        return BigIntToBytes(result, _blockSize);
    }

    /// Hex → bytes → Verify → remove PKCS#1 → string
    /// @see IncomingMessages.as lines 115-123 (decrypt prime/generator from server)
    public string DecryptString(string hex)
    {
        byte[] bytes = HexToBytes(hex);
        byte[] decrypted = Verify(bytes);

        return RemovePKCS1Padding(decrypted);
    }

    /// String → bytes → add PKCS#1 → Encrypt → hex
    /// @see IncomingMessages.as lines 154-158 (encrypt client public key for server)
    public string EncryptString(string value)
    {
        byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(value);
        byte[] encrypted = Encrypt(bytes);

        return BytesToHex(encrypted);
    }

    /// @see com.hurlant.crypto.rsa.RSAKey::dispose
    public void Dispose()
    {
        _modulus = BigInteger.Zero;
        _exponent = BigInteger.Zero;

        GC.SuppressFinalize(this);
    }

    /// @see com.hurlant.crypto.rsa.RSAKey::_decrypt (PKCS#1 v1.5 unpadding)
    private static string RemovePKCS1Padding(byte[] data)
    {
        int i = 0;

        // Skip leading zeros
        while (i < data.Length && data[i] == 0)
        {
            i++;
        }

        // Check for type byte (0x01 for signing, 0x02 for encryption)
        if (i >= data.Length || (data[i] != 0x01 && data[i] != 0x02))
        {
            return Encoding.GetEncoding("iso-8859-1").GetString(data, i, data.Length - i);
        }

        i++;

        // Skip padding bytes until 0x00 separator
        while (i < data.Length && data[i] != 0)
        {
            i++;
        }

        // Skip the 0x00 separator
        if (i < data.Length)
        {
            i++;
        }

        return Encoding.GetEncoding("iso-8859-1").GetString(data, i, data.Length - i);
    }

    /// @see com.hurlant.crypto.rsa.RSAKey::_encrypt (PKCS#1 v1.5 type 0x02 padding)
    private byte[] AddPKCS1Padding(byte[] data)
    {
        int paddingLength = _blockSize - data.Length - 3;

        if (paddingLength < 8)
        {
            throw new InvalidOperationException("Data too long for RSA encryption");
        }

        byte[] padded = new byte[_blockSize];
        padded[0] = 0x00;
        padded[1] = 0x02;

        // Fill with random non-zero bytes
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        byte[] singleByte = new byte[1];

        for (int j = 0;
             j < paddingLength;
             j++)
        {
            do
            {
                rng.GetBytes(singleByte);
            }
            while (singleByte[0] == 0);

            padded[2 + j] = singleByte[0];
        }

        padded[2 + paddingLength] = 0x00;
        Array.Copy(data, 0, padded, 3 + paddingLength, data.Length);

        return padded;
    }

    private static BigInteger BytesToBigInt(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return BigInteger.Zero;
        }

        return new BigInteger(bytes, true, true);
    }

    private static byte[] BigIntToBytes(BigInteger value, int minLength = 0)
    {
        byte[] bytes = value.ToByteArray(true, true);

        if (minLength <= 0 || bytes.Length >= minLength)
        {
            return bytes;
        }

        byte[] padded = new byte[minLength];

        Array.Copy(bytes, 0, padded, minLength - bytes.Length, bytes.Length);

        return padded;
    }

    private static byte[] HexToBytes(string hex)
    {
        if (hex.Length % 2 != 0)
        {
            hex = "0" + hex;
        }

        byte[] bytes = new byte[hex.Length / 2];

        for (int i = 0;
             i < bytes.Length;
             i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }

        return bytes;
    }

    private static string BytesToHex(byte[] bytes)
    {
        StringBuilder sb = new(bytes.Length * 2);

        foreach (byte b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }
}
