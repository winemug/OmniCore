using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OmniCore.Model.Protocol.Base
{
    public class BitBuffer
    {
        private long bufferPosition = 0;
        private MemoryStream buffer;

        public long ByteLength { get => this.buffer.Length; }

        public BitBuffer()
        {
            buffer = new MemoryStream(1024);
        }

        public BitBuffer(byte[] data):this()
        {
            if (data.Length > 1024)
            {
                buffer.Write(data, 0, data.Length);
            }
            else
            {
                buffer = new MemoryStream(1024);
            }
        }

        public byte[] ToByteArray()
        {
            return buffer.ToArray();
        }

        public void Skip(int bitCount)
        {
            throw new NotImplementedException();
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
