using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Shared.Extensions;

public static class ByteSpanExtensions
{
    private static readonly byte[] _mask8 = new byte[8];
    private static readonly uint[] _mask32 = new uint[32];

    static ByteSpanExtensions()
    {
        for(int i=0; i<8; i++)
        {
            _mask8[i] = (byte)(0x80 >> i);
        }

        for(int i=0; i<32; i++)
        {
            _mask32[i] = ((uint)(0x01)) << i;
        }
    }
    public static void WriteBit(this Span<byte> span, bool val, int bitOffset)
    {
        WriteBits(span, (uint)(val ? 1 : 0), bitOffset, 1);
    }
    public static void WriteBits(this Span<byte> span, ushort val, int bitOffset, int bitLength)
    {
        WriteBits(span, (uint)val, bitOffset, bitLength);
    }
    public static void WriteBits(this Span<byte> span, int val, int bitOffset, int bitLength)
    {
        WriteBits(span, (uint)val, bitOffset, bitLength);
    }

    public static void WriteBits(this Span<byte> span, uint val, int bitOffset, int bitLength)
    {
        int maskIdx = bitOffset % 8;
        for (int bitIdx = 0; bitIdx < bitLength; bitIdx++)
        {
            if ((val & _mask32[bitLength - bitIdx - 1]) != 0)
                span[(bitIdx + bitOffset) >> 3] |= _mask8[maskIdx++ & 0x7];
            else
                span[(bitIdx + bitOffset) >> 3] &= (byte)(~_mask8[maskIdx++ & 0x7]);
        }
    }

    public static bool ReadBit(this Span<byte> span, int bitOffset)
    {
        return ReadBits(span, bitOffset, 1) != 0;
    }

    public static uint ReadBits(this Span<byte> span, int bitOffset, int bitLength)
    {
        uint val = 0;
        int maskIdx = bitOffset % 8;
        for(int bitIdx = 0; bitIdx < bitLength; bitIdx++)
        {
            if ((span[(bitIdx + bitOffset) >> 3] & _mask8[maskIdx++ & 0x7]) != 0)
                val |= _mask32[bitLength - bitIdx - 1];
        }
        return val;
    }

    public static void Write16(this Span<byte> buffer, int val)
    {
        Write16(buffer, (ushort)val);
    }
    public static void Write16(this Span<byte> buffer, ushort val)
    {
        buffer[0] = (byte)(val >> 8);
        buffer[1] = (byte)(val & 0xFF);
    }

    public static void Write32(this Span<byte> buffer, int val)
    {
        Write32(buffer, (uint)val);
    }
    public static void Write32(this Span<byte> buffer, uint val)
    {
        buffer[0] = (byte)(val >> 24);
        buffer[1] = (byte)(val >> 16);
        buffer[2] = (byte)(val >> 8);
        buffer[3] = (byte)(val & 0xFF);
    }

    public static ushort Read16(this Span<byte> buffer)
    {
        ushort val0 = buffer[0];
        ushort val1 = buffer[1];   
        return (ushort)(val0 << 8 | val1);
    }

    public static uint Read32(this Span<byte> buffer)
    {
        uint val0 = buffer[0];
        uint val1 = buffer[1];
        uint val2 = buffer[2];
        uint val3 = buffer[3];
        return (uint)(
            val0 << 24 |
            val1 << 16 |
            val2 << 8 |
            val3);
    }

    public static string ToFormattedString(this Span<byte> buffer)
    {
        var sb = new StringBuilder();
        foreach (var b in buffer)
            sb.Append(b.ToString("X2")).Append(" ");
        return sb.ToString();
    }
}
