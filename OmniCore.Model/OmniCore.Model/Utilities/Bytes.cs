using System;
using System.Text;

namespace OmniCore.Model.Utilities
{
    public class Bytes
    {
        private const int PAGE_SIZE = 256;
        public byte[] ByteBuffer = new byte[PAGE_SIZE];
        public int Length { get; private set; }

        private void EnsureBufferSpace(int size)
        {
            if (this.Length + size > this.ByteBuffer.Length)
            {
                var newBuffer = new byte[(((this.Length + size) / PAGE_SIZE) + 1) * PAGE_SIZE];
                Buffer.BlockCopy(ByteBuffer, 0, newBuffer, 0, this.Length);
                this.ByteBuffer = newBuffer;
            }
        }

        public Bytes()
        {
        }

        public Bytes(byte val) : this()
        {
            this.Append(val);
        }

        public Bytes(uint val) : this()
        {
            this.Append(val);
        }

        public Bytes(ushort val) : this()
        {
            this.Append(val);
        }

        public Bytes(byte[] val) : this()
        {
            this.Append(val);
        }

        public Bytes(Bytes b) : this()
        {
            this.Append(b);
        }

        public Bytes(Bytes b1, Bytes b2) : this()
        {
            this.Append(b1).Append(b2);
        }

        public byte this[int index]
        {
            get
            {
                return this.ByteBuffer[index];
            }
        }

        public byte[] GetBuffer()
        {
            return this.ByteBuffer;
        }

        public Bytes Append(uint val)
        {
            EnsureBufferSpace(4);
            ByteBuffer[this.Length++] = (byte)((val & 0xFF000000) >> 24);
            ByteBuffer[this.Length++] = (byte)((val & 0x00FF0000) >> 16);
            ByteBuffer[this.Length++] = (byte)((val & 0x0000FF00) >> 8);
            ByteBuffer[this.Length++] = (byte)(val & 0x000000FF);
            return this;
        }

        public Bytes Append(ushort val)
        {
            EnsureBufferSpace(2);
            ByteBuffer[this.Length++] = (byte)((val & 0xFF00) >> 8);
            ByteBuffer[this.Length++] = (byte)(val & 0x00FF);
            return this;
        }

        public Bytes Append(byte val)
        {
            EnsureBufferSpace(1);
            ByteBuffer[this.Length++] = val;
            return this;
        }

        public Bytes Append(byte[] buffer)
        {
            EnsureBufferSpace(buffer.Length);
            Buffer.BlockCopy(buffer, 0, this.ByteBuffer, this.Length, buffer.Length);
            this.Length += buffer.Length;
            return this;
        }

        public Bytes Append(Bytes otherBytes)
        {
            EnsureBufferSpace(otherBytes.Length);
            Buffer.BlockCopy(otherBytes.ByteBuffer, 0, this.ByteBuffer, this.Length, otherBytes.Length);
            this.Length += otherBytes.Length;
            return this;
        }

        public byte[] ToArray()
        {
            return ToArray(0, this.Length);
        }

        public byte[] ToArray(int start)
        {
            return ToArray(start, this.Length);
        }

        public byte[] ToArray(int start, int end)
        {
            var arr = new byte[end - start];
            Buffer.BlockCopy(this.ByteBuffer, start, arr, 0, end - start);
            return arr;
        }

        public Bytes Sub(int start)
        {
            return Sub(start, this.Length);
        }

        public Bytes Sub(int start, int end)
        {
            return new Bytes(ToArray(start, end));
        }

        public byte Byte(int index)
        {
            return this.ByteBuffer[index];
        }

        public ushort Word(int index)
        {
            ushort ret = (ushort)(this.ByteBuffer[index++] << 8);
            ret |= this.ByteBuffer[index];
            return ret;
        }

        public uint DWord(int index)
        {
            uint ret = (uint)(ByteBuffer[index++] << 24);
            ret |= (uint)(ByteBuffer[index++] << 16);
            ret |= (uint)(ByteBuffer[index++] << 8);
            ret |= ByteBuffer[index++];
            return ret;
        }

        public string ToHex(int start, int end)
        {
            var sb = new StringBuilder();
            for (int i = start; i < end; i++)
                sb.Append($"{this.ByteBuffer[i]:X2}");
            return sb.ToString();
        }

        public string ToHex(int start)
        {
            return ToHex(start, this.Length);
        }

        public string ToHex()
        {
            return ToHex(0, this.Length);
        }

        public override string ToString()
        {
            return this.ToHex();
        }
    }
}
