using System;
using System.Collections.Generic;
using System.Linq;

namespace OmniCore.Services
{
    public static class ManchesterCodec
    {
        private static Dictionary<byte, UInt16> EncodeDictionary = new();
        private static Dictionary<UInt16, byte> DecodeDictionary = new();

        static UInt16 EncodeByte(byte d)
        {
            UInt16 e = 0;
            for (int i = 0; i < 8; i++)
            {
                e = (UInt16)(e >> 2);
                if ((d & 0x01) == 0)
                {
                    e |= 0x8000;
                }
                else
                {
                    e |= 0x4000;
                }

                d = (byte)(d >> 1);
            }

            return e;
        }
        
        static ManchesterCodec()
        {
            for (int i = 0; i < 256; i++)
            {
                var encoded = EncodeByte((byte)i);
                EncodeDictionary.Add((byte)i, encoded);
                DecodeDictionary.Add(encoded, (byte)i);
            }
        }

        public static byte[] Decode(byte[] data)
        {
            var len = data.Length;
            if (len % 2 != 0)
                len = len - 1;
            var decoded = new byte[len / 2];

            for (int i = 0; i < len; i += 2)
            {
                var e = data[i] << 8 | data[i + 1];
                if (!DecodeDictionary.ContainsKey((UInt16)e))
                {
                    return new ArraySegment<byte>(decoded, 0, i / 2).ToArray();
                }
                else
                {
                    decoded[i / 2] = DecodeDictionary[(UInt16)e];
                }
            }
            return decoded;
        }

        public static byte[] Encode(byte[] data)
        {
            var len = data.Length;
            var encoded = new byte[len * 2];

            for (int i = 0; i < len; i++)
            {
                var e = EncodeDictionary[data[i]];
                encoded[2 * i] = (byte)((e >> 8) & 0xFF);
                encoded[2 * i + 1] = (byte)((e) & 0xFF);
            }

            return encoded;
        }
    }
}