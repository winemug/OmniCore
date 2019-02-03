using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Protocol.Base
{
    public static class ByteExtensions
    {
        public static uint GetUInt32BigEndian(this byte[] array, int index)
        {
            var b0 = (uint)array[index];
            var b1 = (uint)array[index + 1];
            var b2 = (uint)array[index + 2];
            var b3 = (uint)array[index + 3];

            return (b0 << 24) + (b1 << 16) + (b2 << 8) + b3;
        }

        public static ushort GetUInt16BigEndian(this byte[] array, int index)
        {
            var b0 = (ushort)array[index];
            var b1 = (ushort)array[index + 1];

            return (ushort)((b0 << 8) + b1);
        }

        public static void PutUint32BigEndian(this byte[] array, uint value, int index)
        {
            array[index++] = (byte)((value & 0xFF000000) >> 24);
            array[index++] = (byte)((value & 0x00FF0000) >> 16);
            array[index++] = (byte)((value & 0x0000FF00) >> 8);
            array[index] = (byte)(value & 0x000000FF);
        }

        public static void PutUint16BigEndian(this byte[] array, ushort value, int index)
        {
            array[index++] = (byte)((value & 0xFF00) >> 8);
            array[index] = (byte)(value & 0x00FF);
        }
    }
}
