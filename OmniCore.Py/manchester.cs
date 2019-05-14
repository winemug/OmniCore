using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Py
{
    public static class manchester
    {
        private static byte[] EncodedHi;
        private static byte[] EncodedLo;

        private static byte?[] DecodedHi;
        private static byte?[] DecodedLo;

        private static byte[] Noise;
        private static Random Rnd;

        static manchester()
        {
            EncodedHi = new byte[256];
            EncodedLo = new byte[256];
            DecodedHi = new byte?[256];
            DecodedLo = new byte?[256];

            var encoding0 = new ushort[8];
            var encoding1 = new ushort[8];
            var mask = new byte[8];

            for (int b = 0; b < 8; b++)
            {
                mask[b] = (byte)(1 << b);
                encoding0[b] = (ushort)(2 << (b * 2));
                encoding1[b] = (ushort)(1 << (b * 2));
            }

            for (byte dec = 0; dec < 255; dec++)
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
                EncodedHi[dec] = ebHi;
                EncodedLo[dec] = ebLo;
                DecodedHi[ebHi] = (byte)(dec & 0xf0);
                DecodedLo[ebLo] = (byte)(dec & 0x0f);
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

        public static byte[] Encode(byte[] decodedBytes)
        {
            var encodedBytes = new byte[80];
            var di = 0;
            var de = decodedBytes.Length * 2;
            for (int i = 0; i < de; i += 2)
            {
                var d = decodedBytes[di++];
                encodedBytes[i] = EncodedHi[d];
                encodedBytes[i + 1] = EncodedLo[d];
            }

            if (de < encodedBytes.Length)
            {
                int noiseStartIndex = Rnd.Next(0, 256);
                Buffer.BlockCopy(Noise, noiseStartIndex, encodedBytes, de, encodedBytes.Length - de);
            }
            return encodedBytes;
        }

        public static byte[] Decode(byte[] encodedBytes)
        {
            var decodedBytes = new byte[encodedBytes.Length / 2];
            var di = 0;
            for (int i = 0; i < encodedBytes.Length; i += 2)
            {
                var eHi = DecodedHi[encodedBytes[i]];
                var eLo = DecodedLo[encodedBytes[i + 1]];

                if (eHi.HasValue && eLo.HasValue)
                {
                    decodedBytes[di++] = (byte)(eHi | eLo);
                }
                else
                {
                    var decodedCut = new byte[di];
                    Buffer.BlockCopy(decodedBytes, 0, decodedCut, 0, di);
                    return decodedCut;
                }
            }
            return decodedBytes;
        }

    }
}
