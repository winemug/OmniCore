using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Py
{
    public static class extensions
    {
        public static uint GetUInt32(this byte[] array, int index)
        {
            var b0 = (uint)array[index];
            var b1 = (uint)array[index + 1];
            var b2 = (uint)array[index + 2];
            var b3 = (uint)array[index + 3];

            return (b0 << 24) + (b1 << 16) + (b2 << 8) + b3;
        }

        public static ushort GetUInt16(this byte[] array, int index)
        {
            var b0 = (ushort)array[index];
            var b1 = (ushort)array[index + 1];

            return (ushort)((b0 << 8) + b1);
        }

        public static void SetUint32(this byte[] array, uint value, int index)
        {
            array[index++] = (byte)((value & 0xFF000000) >> 24);
            array[index++] = (byte)((value & 0x00FF0000) >> 16);
            array[index++] = (byte)((value & 0x0000FF00) >> 8);
            array[index] = (byte)(value & 0x000000FF);
        }

        public static void SetUint16(this byte[] array, ushort value, int index)
        {
            array[index++] = (byte)((value & 0xFF00) >> 8);
            array[index] = (byte)(value & 0x00FF);
        }

        public static byte[] Sub(this byte[] array, int startIndex)
        {
            byte[] newArray = new byte[array.Length - startIndex];
            Buffer.BlockCopy(array, startIndex, newArray, 0, newArray.Length);
            return newArray;
        }

        public static byte[] Sub(this byte[] array, int startIndex, int endIndex)
        {
            byte[] newArray = new byte[endIndex - startIndex];
            Buffer.BlockCopy(array, startIndex, newArray, 0, newArray.Length);
            return newArray;
        }

        public static byte[] Append(this byte[] array, byte[] array2)
        {
            byte[] newArray = new byte[array.Length + array2.Length];
            Buffer.BlockCopy(array, 0, newArray, 0, array.Length);
            Buffer.BlockCopy(array2, 0, newArray, array.Length, array2.Length);
            return newArray;
        }

        public static byte[] Append(this byte[] array, byte addByte)
        {
            byte[] newArray = new byte[array.Length + 1];
            Buffer.BlockCopy(array, 0, newArray, 0, array.Length);
            newArray[array.Length] = addByte;
            return newArray;
        }

        public static string Hex(this byte[] array)
        {
            var sb = new StringBuilder();
            foreach(byte b in array)
            {
                sb.Append($"{b:02X}");
            }
            return sb.ToString();
        }

        public static byte[] ToBytes(this ushort value)
        {
            var result = new byte[2];
            result[0] = (byte)((value & 0xFF00) >> 8);
            result[1] = (byte)(value & 0x00FF);
            return result;
        }

        public static byte[] ToBytes(this uint value)
        {
            var result = new byte[4];
            result[0] = (byte)((value & 0xFF000000) >> 24);
            result[1] = (byte)((value & 0x00FF0000) >> 16);
            result[2] = (byte)((value & 0x0000FF00) >> 8);
            result[3] = (byte)(value & 0x000000FF);
            return result;
        }
    }
}
