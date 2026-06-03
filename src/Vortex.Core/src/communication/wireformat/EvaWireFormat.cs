using System;
using System.IO;
using System.Text;

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Encryption;
using Vortex.Core.Communication.Messages;
using Vortex.Core.Communication.Util;

using Byte = Vortex.Core.Communication.Util.Byte;

namespace Vortex.Core.Communication.Wireformat;

public sealed class EvaWireFormat : IWireFormat, IDisposable
{
    private const uint MAX_DATA = 262144;
    private byte[] _remainder = [];

    public void Dispose() { }

    public byte[] Encode(int param1, List<object> param2)
    {
        using MemoryStream body = new();
        WriteInt16Be(body, (short)param1);

        foreach (object value in param2)
        {
            switch (value)
            {
                case string s:
                    byte[] text = Encoding.UTF8.GetBytes(s);
                    WriteInt16Be(body, (short)text.Length);
                    body.Write(text, 0, text.Length);
                    break;
                case int i:
                    WriteInt32Be(body, i);
                    break;
                case bool b:
                    body.WriteByte((byte)(b ? 1 : 0));
                    break;
                case Short s:
                    WriteInt16Be(body, (short)s.value);
                    break;
                case Byte b:
                    body.WriteByte((byte)b.value);
                    break;
                case Long l:
                    WriteInt64Be(body, l.value);
                    break;
                case byte[] bytes:
                    WriteInt32Be(body, bytes.Length);
                    body.Write(bytes, 0, bytes.Length);
                    break;
            }
        }

        using MemoryStream packet = new();
        WriteInt32Be(packet, (int)body.Length);
        byte[] bodyBytes = body.ToArray();
        packet.Write(bodyBytes, 0, bodyBytes.Length);
        return packet.ToArray();
    }

    public List<IMessageDataWrapper> SplitMessages(byte[] param1, IConnection param2)
    {
        List<IMessageDataWrapper> result = new();
        int position = 0;

        while (param1.Length - position >= 6)
        {
            int start = position;
            IEncryption? encryption = param2.GetServerToClientEncryption();

            encryption?.Mark();

            byte[] lengthBytes = new byte[4];
            Array.Copy(param1, position, lengthBytes, 0, 4);
            position += 4;

            encryption?.Decipher(lengthBytes);

            int messageLength = ReadInt32Be(lengthBytes, 0);
            if (messageLength < 2 || messageLength > MAX_DATA)
            {
                throw new Exception("Invalid message length " + messageLength);
            }

            if (param1.Length - position < messageLength)
            {
                encryption?.Reset();
                position = start;
                break;
            }

            byte[] payloadWithId = new byte[messageLength];
            Array.Copy(param1, position, payloadWithId, 0, messageLength);
            position += messageLength;

            encryption?.Decipher(payloadWithId);

            int messageId = ReadInt16Be(payloadWithId, 0);
            byte[] payload = new byte[messageLength - 2];
            Array.Copy(payloadWithId, 2, payload, 0, payload.Length);
            result.Add(new EvaMessageDataWrapper(messageId, payload));
        }

        if (position < param1.Length)
        {
            _remainder = new byte[param1.Length - position];
            Array.Copy(param1, position, _remainder, 0, _remainder.Length);
        }
        else
        {
            _remainder = [];
        }

        return result;
    }

    public byte[] GetRemainder()
    {
        return _remainder;
    }

    private static void WriteInt16Be(Stream stream, short value)
    {
        stream.WriteByte((byte)((value >> 8) & 0xFF));
        stream.WriteByte((byte)(value & 0xFF));
    }

    private static void WriteInt32Be(Stream stream, int value)
    {
        stream.WriteByte((byte)((value >> 24) & 0xFF));
        stream.WriteByte((byte)((value >> 16) & 0xFF));
        stream.WriteByte((byte)((value >> 8) & 0xFF));
        stream.WriteByte((byte)(value & 0xFF));
    }

    private static void WriteInt64Be(Stream stream, double value)
    {
        long longValue = (long)value;
        byte[] bytes = BitConverter.GetBytes(longValue);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        stream.Write(bytes, 0, bytes.Length);
    }

    private static int ReadInt16Be(byte[] data, int offset)
    {
        return (short)((data[offset] << 8) | data[offset + 1]);
    }

    private static int ReadInt32Be(byte[] data, int offset)
    {
        return (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];
    }
}
