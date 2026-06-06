// Golden tests for EvaWireFormat encode/decode.
// These byte sequences are the protocol ground-truth: the rewrite (Span<byte> + BinaryPrimitives
// + ArrayPool) must produce bit-for-bit identical output. Run before AND after the rewrite.

using System;
using System.Collections.Generic;
using System.Text;

using Godot.Collections;

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Encryption;
using Vortex.Core.Communication.Messages;
using Vortex.Core.Communication.Util;
using Vortex.Core.Communication.Wireformat;

using Xunit;

using Byte = Vortex.Core.Communication.Util.Byte;

namespace Vortex.Core.Tests.Communication;

public sealed class EvaWireFormatGoldenTests
{
    // ── Encode golden tests ────────────────────────────────────────────────

    [Fact]
    public void Encode_EmptyPayload_FourByteLength_TwoByteId()
    {
        // [4 bytes: body length = 2][2 bytes: messageId = 0x0001]
        byte[] expected = [0x00, 0x00, 0x00, 0x02, 0x00, 0x01];

        byte[] actual = new EvaWireFormat().Encode(1, []);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Encode_Int32Payload_BigEndian()
    {
        // body = [0x00, 0x01] (id) + [0x00, 0x00, 0x00, 0x2A] (42)
        // length prefix = 6
        byte[] expected = [0x00, 0x00, 0x00, 0x06, 0x00, 0x01, 0x00, 0x00, 0x00, 0x2A];

        byte[] actual = new EvaWireFormat().Encode(1, [42]);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Encode_NegativeInt_TwosComplement()
    {
        // -1 as int32 big-endian = 0xFF FF FF FF
        byte[] expected = [0x00, 0x00, 0x00, 0x06, 0x00, 0x64, 0xFF, 0xFF, 0xFF, 0xFF];

        byte[] actual = new EvaWireFormat().Encode(100, [(int)-1]);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Encode_BoolTrue_SingleByte()
    {
        // body = [id 2 bytes] + [0x01]  → length 3
        byte[] expected = [0x00, 0x00, 0x00, 0x03, 0x00, 0x07, 0x01];

        byte[] actual = new EvaWireFormat().Encode(7, [true]);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Encode_BoolFalse_SingleByte()
    {
        byte[] expected = [0x00, 0x00, 0x00, 0x03, 0x00, 0x07, 0x00];

        byte[] actual = new EvaWireFormat().Encode(7, [false]);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Encode_ShortPayload_BigEndian()
    {
        // Short(300) = 0x012C; body = [id][0x01, 0x2C] → length 4
        byte[] expected = [0x00, 0x00, 0x00, 0x04, 0x00, 0x05, 0x01, 0x2C];

        byte[] actual = new EvaWireFormat().Encode(5, [new Short(300)]);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Encode_BytePayload_SingleByte()
    {
        // Byte(0xAB); body = [id][0xAB] → length 3
        byte[] expected = [0x00, 0x00, 0x00, 0x03, 0x00, 0x02, 0xAB];

        byte[] actual = new EvaWireFormat().Encode(2, [new Byte(0xAB)]);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Encode_StringPayload_Utf8WithShortLengthPrefix()
    {
        // "Hi" in UTF-8 = 0x48 0x69; prefixed with uint16(2) = 0x00 0x02
        // body = [id] + [0x00, 0x02, 0x48, 0x69] → length 6
        byte[] expected = [0x00, 0x00, 0x00, 0x06, 0x00, 0x03, 0x00, 0x02, 0x48, 0x69];

        byte[] actual = new EvaWireFormat().Encode(3, ["Hi"]);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Encode_ByteArrayPayload_Int32LengthThenBytes()
    {
        // byte[] {0xCA, 0xFE}; body = [id] + [0x00,0x00,0x00,0x02, 0xCA, 0xFE] → length 8
        byte[] expected =
        [
            0x00, 0x00, 0x00, 0x08,  // length = 8
            0x00, 0x0A,              // messageId = 10
            0x00, 0x00, 0x00, 0x02,  // byte[] length = 2
            0xCA, 0xFE,              // bytes
        ];

        byte[] actual = new EvaWireFormat().Encode(10, [new byte[] { 0xCA, 0xFE }]);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Encode_MixedPayload_OrderPreserved()
    {
        // int(1), bool(true), string("ok")
        // "ok" UTF8 = 0x6F 0x6B; body = [id=0x00,0x0F][4 int][1 bool][2 len + 2 bytes] = 11
        byte[] expected =
        [
            0x00, 0x00, 0x00, 0x0B,  // length = 11
            0x00, 0x0F,              // id = 15
            0x00, 0x00, 0x00, 0x01,  // int(1)
            0x01,                    // bool(true)
            0x00, 0x02, 0x6F, 0x6B, // string "ok"
        ];

        byte[] actual = new EvaWireFormat().Encode(15, [1, true, "ok"]);

        Assert.Equal(expected, actual);
    }

    // ── SplitMessages golden tests ─────────────────────────────────────────

    [Fact]
    public void SplitMessages_SinglePacket_DecodesIdAndPayload()
    {
        // Encode then split: round-trip must recover id and int payload.
        EvaWireFormat fmt = new();
        byte[] packet = fmt.Encode(42, [99]);

        List<IMessageDataWrapper> messages = fmt.SplitMessages(packet, NoEncryption.Instance);

        Assert.Single(messages);
        Assert.Equal(42, messages[0].GetId());
        Assert.Equal(99, messages[0].ReadInteger());
    }

    [Fact]
    public void SplitMessages_TwoPacketsConcatenated_BothDecoded()
    {
        EvaWireFormat fmt = new();
        byte[] p1 = fmt.Encode(1, [111]);
        byte[] p2 = fmt.Encode(2, ["abc"]);
        byte[] combined = [.. p1, .. p2];

        List<IMessageDataWrapper> messages = fmt.SplitMessages(combined, NoEncryption.Instance);

        Assert.Equal(2, messages.Count);
        Assert.Equal(1, messages[0].GetId());
        Assert.Equal(111, messages[0].ReadInteger());
        Assert.Equal(2, messages[1].GetId());
        Assert.Equal("abc", messages[1].ReadString());
    }

    [Fact]
    public void SplitMessages_PartialPacket_RemainderStored()
    {
        EvaWireFormat fmt = new();
        byte[] full = fmt.Encode(7, [1]);
        // Feed only the first 4 bytes (just the length prefix — body incomplete)
        byte[] partial = full[..4];

        List<IMessageDataWrapper> messages = fmt.SplitMessages(partial, NoEncryption.Instance);

        Assert.Empty(messages);
        Assert.Equal(partial, fmt.GetRemainder());
    }

    [Fact]
    public void SplitMessages_InvalidLength_ThrowsException()
    {
        // Length of 1 is invalid (< 2)
        byte[] bad = [0x00, 0x00, 0x00, 0x01, 0x00, 0x00];

        Assert.Throws<Exception>(() =>
            new EvaWireFormat().SplitMessages(bad, NoEncryption.Instance));
    }

    // ── No-op IConnection stub (no encryption) ─────────────────────────────

    private sealed class NoEncryption : IConnection
    {
        public static readonly NoEncryption Instance = new();

        public bool disposed => false;
        public bool connected => true;
        public int timeout { set { } }

        public IEncryption? GetServerToClientEncryption() => null;

        public void Dispose() { }
        public bool Init(string p1, uint p2 = 0, bool p3 = true) => true;
        public bool Send(IMessageComposer p1) => false;
        public bool SendUnencrypted(IMessageComposer p1) => false;
        public void SetEncryption(IEncryption p1, IEncryption? p2) { }
        public void RegisterMessageClasses(IMessageConfiguration p1) { }
        public void AddMessageEvent(IMessageEvent p1) { }
        public void RemoveMessageEvent(IMessageEvent p1) { }
        public void ProcessReceivedData() { }
        public void Close() { }
        public void IsAuthenticated() { }
        public void IsConfigured() { }
        public void CreateSocket() { }
        public void AddListener(string p1, Action<Dictionary> p2) { }
    }
}
