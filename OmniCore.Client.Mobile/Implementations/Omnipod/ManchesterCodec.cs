using OmniCore.Common.Entities;

namespace OmniCore.Framework.Omnipod;

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

    public static Bytes Decode(Bytes data)
    {
        var len = data.Length;
        if (len % 2 != 0)
            len = len - 1;
        var decoded = new Bytes();

        for (var i = 0; i < len; i += 2)
        {
            var e = data.Word(i);
            if (!DecodeDictionary.ContainsKey(e))
                return decoded;
            decoded.Append(DecodeDictionary[e]);
        }

        return decoded;
    }

    public static Bytes Encode(Bytes data, int padLength = 0)
    {
        var encoded = new Bytes();
        var len = data.Length;
        for (var i = 0; i < len; i++) encoded.Append(EncodeDictionary[data[i]]);

        for (var i = len; i < padLength; i++) encoded.Append((ushort)0x0000);

        return encoded;
    }
}