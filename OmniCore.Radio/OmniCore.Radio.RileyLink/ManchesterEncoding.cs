using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OmniCore.Radio.RileyLink
{
    public static class ManchesterEncoding
    {
        private static ushort[] Encoded;
        private static Dictionary<ushort, byte> Decoded;

        private static byte[] Noise;
        private static Random Rnd;

        static ManchesterEncoding()
        {
            Encoded = new ushort[256];
            Decoded = new Dictionary<ushort, byte>();

            var encoding0 = new ushort[8];
            var encoding1 = new ushort[8];
            var mask = new byte[8];

            for (int b = 0; b < 8; b++)
            {
                mask[b] = (byte)(1 << b);
                encoding0[b] = (ushort)(2 << (b * 2));
                encoding1[b] = (ushort)(1 << (b * 2));
            }

            for (int dec = 0; dec < 256; dec++)
            {
                ushort enc = 0;
                for (int b = 0; b < 8; b++)
                {
                    if ((dec & mask[b]) == 0)
                        enc |= encoding0[b];
                    else
                        enc |= encoding1[b];
                }
                var ebHi = (byte)((enc & 0xFF00) >> 8);
                var ebLo = (byte)(enc & 0x00FF);
                Encoded[dec] = enc;
                Decoded.Add(enc, (byte)dec);
            }

            Rnd = new Random();
            Noise = new byte[256+160];
            for(int i=0; i< Noise.Length; i++)
            {
                byte noise = 0;
                for(int j=0; j<4; j++)
                {
                    noise = (byte)(noise << 2);
                    if (Rnd.Next() % 2 == 0)
                        noise |= 0x00;
                    else
                        noise |= 0x03;
                }
                Noise[i] = noise;
            }
        }

        public static Bytes Encode(Bytes toEncode)
        {
            var encoded = new Bytes();
            int noiseIndex = Rnd.Next(0, 256);
            for (int i = 0; i < 42; i++)
            {
                if (i < toEncode.Length)
                {
                    var byteToEncode = toEncode[i];
                    encoded.Append(Encoded[byteToEncode]);
                }
                else
                {
                    encoded.Append(Noise[noiseIndex + i * 2]);
                    encoded.Append(Noise[noiseIndex + (i * 2 + 1)]);
                }
            }
            return encoded;
        }

        public static Bytes Decode(Bytes toDecode)
        {
            var decoded = new Bytes();
            for (int i = 0; i < toDecode.Length; i += 2)
            {
                var wordToDecode = toDecode.Word(i);
                if (Decoded.ContainsKey(wordToDecode))
                {
                    decoded.Append(Decoded[wordToDecode]);
                }
                else
                {
                    break;
                }
            }
            return decoded;
        }

    }
}
