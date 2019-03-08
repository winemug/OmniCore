using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OmniCore.Model.Utilities
{
    public class BitBuffer
    {
        private long bitPosition = 0;
        private MemoryStream buffer;

        public long ByteLength { get => this.buffer.Length; }

        public BitBuffer()
        {
            buffer = new MemoryStream(1024);
        }

        public BitBuffer(byte[] data, int? offset = null, int? length = null):this()
        {
            if (offset.HasValue && length.HasValue)
            {
                buffer.Write(data, offset.Value, length.Value);
                buffer.Position = 0;
            }
            else
            {
                buffer.Write(data, 0, data.Length);
                buffer.Position = 0;
            }
        }

        public byte[] ToByteArray()
        {
            return buffer.ToArray();
        }

        public void Skip(int bitCount)
        {
            bitPosition += bitCount;
        }

        public void GoToEnd()
        {

        }

        public void ByteAlign()
        {
            throw new NotImplementedException();
        }

        public byte[] ReadBytes(int bitCount)
        {
            throw new NotImplementedException();
        }

        public ulong Read(int bitCount)
        {
            if (bitCount > 64)
                throw new ArgumentException();

            throw new NotImplementedException();
        }

        public ushort Read16()
        {
            throw new NotImplementedException();
        }

        public uint Read32()
        {
            throw new NotImplementedException();
        }

        public ulong Read64()
        {
            throw new NotImplementedException();
        }
    }
}
