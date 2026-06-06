// @see com.sulake.core.communication.wireformat.EvaWireFormat (WIN63-202111081545-75921380)

using System;
using System.Buffers;
using System.Buffers.Binary;
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

    // @see EvaWireFormat.as::encode
    public byte[] Encode(int param1, List<object> param2)
    {
        // Build the body (messageId + payload) into a MemoryStream, then prepend the
        // 4-byte big-endian length. BinaryPrimitives makes the big-endian intent explicit;
        // stack-allocated Span<byte> avoids per-field heap allocations for the header writes.
        using MemoryStream body = new();

        Span<byte> buf2 = stackalloc byte[2];
        Span<byte> buf4 = stackalloc byte[4];
        Span<byte> buf8 = stackalloc byte[8];

        BinaryPrimitives.WriteInt16BigEndian(buf2, (short)param1);
        body.Write(buf2);

        foreach (object value in param2)
        {
            switch (value)
            {
                case string s:
                    byte[] text = Encoding.UTF8.GetBytes(s);
                    BinaryPrimitives.WriteInt16BigEndian(buf2, (short)text.Length);
                    body.Write(buf2);
                    body.Write(text);
                    break;
                case int i:
                    BinaryPrimitives.WriteInt32BigEndian(buf4, i);
                    body.Write(buf4);
                    break;
                case bool b:
                    body.WriteByte((byte)(b ? 1 : 0));
                    break;
                case Short s:
                    BinaryPrimitives.WriteInt16BigEndian(buf2, (short)s.value);
                    body.Write(buf2);
                    break;
                case Byte b:
                    body.WriteByte((byte)b.value);
                    break;
                case Long l:
                    BinaryPrimitives.WriteInt64BigEndian(buf8, (long)l.value);
                    body.Write(buf8);
                    break;
                case byte[] bytes:
                    BinaryPrimitives.WriteInt32BigEndian(buf4, bytes.Length);
                    body.Write(buf4);
                    body.Write(bytes);
                    break;
            }
        }

        int bodyLength = (int)body.Length;

        // Rent a packet buffer, write length prefix, copy body — return an exact-length copy.
        byte[] rented = ArrayPool<byte>.Shared.Rent(4 + bodyLength);
        try
        {
            BinaryPrimitives.WriteInt32BigEndian(rented, bodyLength);
            body.GetBuffer().AsSpan(0, bodyLength).CopyTo(rented.AsSpan(4));
            return rented.AsSpan(0, 4 + bodyLength).ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    // @see EvaWireFormat.as::splitMessages
    public List<IMessageDataWrapper> SplitMessages(byte[] param1, IConnection param2)
    {
        List<IMessageDataWrapper> result = new();
        int position = 0;

        while (param1.Length - position >= 6)
        {
            int start = position;
            IEncryption? encryption = param2.GetServerToClientEncryption();

            encryption?.Mark();

            int messageLength;

            if (encryption != null)
            {
                byte[] lengthBytes = new byte[4];
                param1.AsSpan(position, 4).CopyTo(lengthBytes);
                position += 4;
                encryption.Decipher(lengthBytes);
                messageLength = BinaryPrimitives.ReadInt32BigEndian(lengthBytes);
            }
            else
            {
                messageLength = BinaryPrimitives.ReadInt32BigEndian(param1.AsSpan(position, 4));
                position += 4;
            }

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
            param1.AsSpan(position, messageLength).CopyTo(payloadWithId);
            position += messageLength;

            encryption?.Decipher(payloadWithId);

            int messageId = BinaryPrimitives.ReadInt16BigEndian(payloadWithId);
            byte[] payload = new byte[messageLength - 2];
            payloadWithId.AsSpan(2, messageLength - 2).CopyTo(payload);
            result.Add(new EvaMessageDataWrapper(messageId, payload));
        }

        _remainder = position < param1.Length
            ? param1.AsSpan(position).ToArray()
            : [];

        return result;
    }

    public byte[] GetRemainder()
    {
        return _remainder;
    }
}
