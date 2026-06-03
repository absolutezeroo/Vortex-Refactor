using System;
using System.IO;
using System.Text;

using Vortex.Core.Communication.Messages;

namespace Vortex.Core.Communication.Wireformat;

public sealed class EvaMessageDataWrapper(int param1, byte[] param2) : IMessageDataWrapper
{
    private int _position;

    public int GetId()
    {
        return param1;
    }

    public string ReadString()
    {
        if (bytesAvailable < 2)
        {
            return string.Empty;
        }

        short length = ReadInt16Be(param2, _position);
        _position += 2;

        if (length < 0 || bytesAvailable < length)
        {
            return string.Empty;
        }

        string s = Encoding.UTF8.GetString(param2, _position, length);

        _position += length;

        return s;
    }

    public int ReadInteger()
    {
        if (bytesAvailable < 4)
        {
            return 0;
        }

        int value = ReadInt32Be(param2, _position);
        _position += 4;

        return value;
    }

    public double ReadLong()
    {
        if (bytesAvailable < 8)
        {
            return 0;
        }

        uint high = ReadUInt32Be(param2, _position);
        _position += 4;

        uint low = ReadUInt32Be(param2, _position);
        _position += 4;

        bool negative = (high & 0x80000000) != 0;

        if (negative)
        {
            high = ~high & 0x7FFFFFFF;
            low = ~low + 1;

            if (low == 0)
            {
                high += 1;
            }
        }

        double value = (high * 4294967296d) + low;

        return negative ? -value : value;
    }

    public bool ReadBoolean()
    {
        if (bytesAvailable < 1)
        {
            return false;
        }

        return param2[_position++] != 0;
    }

    public int ReadShort()
    {
        if (bytesAvailable < 2)
        {
            return 0;
        }

        short value = ReadInt16Be(param2, _position);
        _position += 2;

        return value;
    }

    public int ReadByte()
    {
        if (bytesAvailable < 1)
        {
            return 0;
        }

        return (sbyte)param2[_position++];
    }

    public float ReadFloat()
    {
        if (bytesAvailable < 4)
        {
            throw new InvalidOperationException("Buffer underflow: need 4 bytes for float");
        }

        byte[] bytes = new byte[4];
        Array.Copy(param2, _position, bytes, 0, 4);
        _position += 4;

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        return BitConverter.ToSingle(bytes, 0);
    }

    public double ReadDouble()
    {
        if (bytesAvailable < 8)
        {
            throw new InvalidOperationException("Buffer underflow: need 8 bytes for double");
        }

        byte[] bytes = new byte[8];
        Array.Copy(param2, _position, bytes, 0, 8);
        _position += 8;

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        return BitConverter.ToDouble(bytes, 0);
    }

    public uint bytesAvailable => (uint)(param2.Length - _position);

    private static short ReadInt16Be(byte[] buffer, int offset)
    {
        return (short)((buffer[offset] << 8) | buffer[offset + 1]);
    }

    private static int ReadInt32Be(byte[] buffer, int offset)
    {
        return (buffer[offset] << 24) | (buffer[offset + 1] << 16) | (buffer[offset + 2] << 8) | buffer[offset + 3];
    }

    private static uint ReadUInt32Be(byte[] buffer, int offset)
    {
        return (uint)ReadInt32Be(buffer, offset);
    }

    public override string ToString()
    {
        using MemoryStream ms = new(param2);

        return $"id={param1}, pos={_position}, data={Convert.ToHexString(ms.ToArray())}";
    }
}
