using System;
using System.Text;

namespace Vortex.Core.Communication.Encryption;

/// @see WIN63-202407091256-704579380-Source-main/core/communication/encryption/CryptoTools.as
public static class CryptoTools
{
    /// @see WIN63-202407091256-704579380-Source-main/core/communication/encryption/CryptoTools.as::hexStringToByteArray
    public static byte[] HexStringToByteArray(string hex)
    {
        if (hex.Length % 2 != 0)
        {
            hex = "0" + hex;
        }

        byte[] result = new byte[hex.Length / 2];

        for (int i = 0;
             i < result.Length;
             i++)
        {
            result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }

        return result;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/encryption/CryptoTools.as::byteArrayToHexString
    public static string ByteArrayToHexString(byte[] data, bool uppercase = false)
    {
        StringBuilder sb = new(data.Length * 2);

        foreach (byte b in data)
        {
            sb.Append(b.ToString(uppercase ? "X2" : "x2"));
        }

        return sb.ToString();
    }
}
