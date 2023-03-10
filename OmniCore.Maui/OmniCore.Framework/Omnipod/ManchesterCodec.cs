using System;
using System.Collections.Generic;
using System.Linq;

namespace OmniCore.Services;

public static class ManchesterCodec
{
    private static readonly Dictionary<byte, ushort> EncodeDictionary = new();
    private static readonly Dictionary<ushort, byte> DecodeDictionary = new();

    static ManchesterCodec()
    {
        for (var i = 0; i < 256; i++)
        {
            var encoded = EncodeByte((byte)i);
            EncodeDictionary.Add((byte)i, encoded);
            DecodeDictionary.Add(encoded, (byte)i);
        }
    }

    private static ushort EncodeByte(byte d)
    {
        ushort e = 0;
        for (var i = 0; i < 8; i++)
        {
            e = (ushort)(e >> 2);
            if ((d & 0x01) == 0)
                e |= 0x8000;
            else
                e |= 0x4000;

            d = (byte)(d >> 1);
        }

        return e;
    }

    public static byte[] Decode(byte[] data)
    {
        var len = data.Length;
        if (len % 2 != 0)
            len = len - 1;
        var decoded = new byte[len / 2];

        for (var i = 0; i < len; i += 2)
        {
            var e = (data[i] << 8) | data[i + 1];
            if (!DecodeDictionary.ContainsKey((ushort)e))
                return new ArraySegment<byte>(decoded, 0, i / 2).ToArray();
            decoded[i / 2] = DecodeDictionary[(ushort)e];
        }

        return decoded;
    }

    public static byte[] Encode(byte[] data)
    {
        var len = data.Length;
        var encoded = new byte[len * 2];

        for (var i = 0; i < len; i++)
        {
            var e = EncodeDictionary[data[i]];
            encoded[2 * i] = (byte)((e >> 8) & 0xFF);
            encoded[2 * i + 1] = (byte)(e & 0xFF);
        }

        return encoded;
    }
}