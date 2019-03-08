using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Utilities
{
    public static class ManchesterCodec
    {
        private static byte[] EncodedHi;
        private static byte[] EncodedLo;

        private static byte?[] DecodedHi;
        private static byte?[] DecodedLo;

        private static ushort[] Encoding0;
        private static ushort[] Encoding1;

        private static byte[] Mask;


        static ManchesterCodec()
        {
            EncodedHi = new byte[256];
            EncodedLo = new byte[256];
            DecodedHi = new byte?[256];
            DecodedLo = new byte?[256];

            Encoding0 = new ushort[8];
            Encoding1 = new ushort[8];
            Mask = new byte[8];

            for (int b = 0; b<8; b++)
            {
                Mask[b] = (byte)(1 << b);
                Encoding0[b] = (ushort)(2 << (b * 2));
                Encoding1[b] = (ushort)(1 << (b * 2));
            }

            for (byte dec=0; dec<255; dec++)
            {
                ushort enc = 0;
                for(int b=0; b<8; b++)
                {
                    if ((dec & Mask[b]) == 0)
                        enc |= Encoding0[b];
                    else
                        enc |= Encoding1[b];
                }
                var ebHi = (byte)((enc & 0xFF00) >> 8);
                var ebLo = (byte)(enc & 0x00FF);
                EncodedHi[dec] = ebHi;
                EncodedLo[dec] = ebLo;
                DecodedHi[ebHi] = (byte)(dec & 0xf0);
                DecodedLo[ebLo] = (byte)(dec & 0x0f);
            }
        }

        public static byte[] Encode(byte[] decodedBytes)
        {
            var encodedBytes = new byte[decodedBytes.Length * 2];
            var di = 0;
            for(int i=0; i<encodedBytes.Length; i += 2)
            {
                var d = decodedBytes[di++];
                encodedBytes[i] = EncodedHi[d];
                encodedBytes[i + 1] = EncodedLo[d];
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
