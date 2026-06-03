using System.Text;

namespace Vortex.Bundle.Converter.Swf;

/// <summary>
/// Bit-aligned binary reader for SWF data.
/// Handles variable-width bit fields, rects, and SWF-specific encodings.
/// @see nitro-converter-main/src/swf/SWFBuffer.ts
/// </summary>
public sealed class SwfBinaryReader(byte[] data, int offset = 0)
{
    private int _bitPos = 0;

    public int Position { get; private set; } = offset;

    public int Length => data.Length;
    public int Remaining => data.Length - Position;
    public bool HasMore => Position < data.Length;

    public byte ReadByte()
    {
        AlignToByte();
        return data[Position++];
    }

    public byte[] ReadBytes(int count)
    {
        AlignToByte();
        byte[] result = new byte[count];
        Buffer.BlockCopy(data, Position, result, 0, count);
        Position += count;
        return result;
    }

    public ushort ReadUInt16()
    {
        AlignToByte();
        ushort val = (ushort)(data[Position] | (data[Position + 1] << 8));
        Position += 2;
        return val;
    }

    public short ReadInt16()
    {
        return (short)ReadUInt16();
    }

    public uint ReadUInt32()
    {
        AlignToByte();
        uint val = (uint)(data[Position]
                          | (data[Position + 1] << 8)
                          | (data[Position + 2] << 16)
                          | (data[Position + 3] << 24));
        Position += 4;
        return val;
    }

    public int ReadInt32()
    {
        return (int)ReadUInt32();
    }

    /// <summary>
    /// Read a variable-width bit field (SWF format).
    /// </summary>
    public int ReadBits(int numBits, bool signed = false)
    {
        int result = 0;
        for (int i = 0; i < numBits; i++)
        {
            int byteIndex = Position + (_bitPos >> 3);
            int bitIndex = 7 - (_bitPos & 7);
            int bit = (data[byteIndex] >> bitIndex) & 1;
            result = (result << 1) | bit;
            _bitPos++;
        }

        // Advance byte position
        Position += _bitPos >> 3;
        _bitPos &= 7;

        // Sign extend
        if (signed && numBits > 0 && (result & (1 << (numBits - 1))) != 0)
        {
            result |= -(1 << numBits);
        }

        return result;
    }

    /// <summary>
    /// Read SWF RECT structure: NBits followed by 4 signed fields.
    /// Values are in twips (1/20 pixel).
    /// </summary>
    public SwfRect ReadRect()
    {
        int nBits = ReadBits(5);
        SwfRect rect = new()
        {
            XMin = ReadBits(nBits, true), XMax = ReadBits(nBits, true), YMin = ReadBits(nBits, true), YMax = ReadBits(nBits, true),
        };
        AlignToByte();
        return rect;
    }

    /// <summary>
    /// Read null-terminated UTF-8 string.
    /// </summary>
    public string ReadString()
    {
        AlignToByte();
        int start = Position;
        while (Position < data.Length && data[Position] != 0)
        {
            Position++;
        }
        string str = Encoding.UTF8.GetString(data, start, Position - start);
        if (Position < data.Length)
        {
            Position++; // Skip null terminator
        }
        return str;
    }

    /// <summary>
    /// SWF encoded uint32: 7-bit chunks with MSB continuation flag.
    /// </summary>
    public uint ReadEncodedU32()
    {
        AlignToByte();
        uint result = data[Position++];
        if ((result & 0x80) == 0)
        {
            return result;
        }

        result = (result & 0x7F) | ((uint)data[Position++] << 7);
        if ((result & 0x4000) == 0)
        {
            return result;
        }

        result = (result & 0x3FFF) | ((uint)data[Position++] << 14);
        if ((result & 0x200000) == 0)
        {
            return result;
        }

        result = (result & 0x1FFFFF) | ((uint)data[Position++] << 21);
        if ((result & 0x10000000) == 0)
        {
            return result;
        }

        result = (result & 0xFFFFFFF) | ((uint)data[Position++] << 28);
        return result;
    }

    public void Skip(int count)
    {
        AlignToByte();
        Position += count;
    }

    public void Seek(int position)
    {
        Position = position;
        _bitPos = 0;
    }

    private void AlignToByte()
    {
        if (_bitPos != 0)
        {
            Position += (_bitPos + 7) >> 3;
            _bitPos = 0;
        }
    }
}
