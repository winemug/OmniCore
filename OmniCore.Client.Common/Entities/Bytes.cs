using System.Globalization;
using System.Text;

namespace OmniCore.Common.Entities;

public class Bytes : IComparable<Bytes>
{
    private const int PAGE_SIZE = 256;
    public byte[] ByteBuffer = new byte[PAGE_SIZE];

    public Bytes()
    {
    }

    public Bytes(byte val) : this()
    {
        Append(val);
    }

    public Bytes(uint val) : this()
    {
        Append(val);
    }

    public Bytes(ushort val) : this()
    {
        Append(val);
    }

    public Bytes(byte[]? val) : this()
    {
        if (val != null)
            Append(val);
    }

    public Bytes(Bytes b) : this()
    {
        Append(b);
    }

    public Bytes(Bytes b1, Bytes b2) : this()
    {
        Append(b1).Append(b2);
    }

    public Bytes(string hexString) : this()
    {
        if (hexString.Length % 2 != 0)
            throw new ArgumentException();

        hexString = hexString.ToUpperInvariant();

        var bytes = new byte[hexString.Length / 2];
        for (var i = 0; i < hexString.Length / 2; i += 1)
            bytes[i] = byte.Parse(hexString.Substring(i * 2, 2), NumberStyles.HexNumber);

        Append(bytes);
    }

    public int Length { get; private set; }

    public byte this[int index] => ByteBuffer[index];

    public int CompareTo(Bytes other)
    {
        if (Length > other.Length)
            return 1;

        if (Length < other.Length)
            return -1;

        for (var i = 0; i < Length; i++)
            if (this[i] != other[i])
                return this[i].CompareTo(other[i]);
        return 0;
    }

    private void EnsureBufferSpace(int size)
    {
        if (Length + size > ByteBuffer.Length)
        {
            var newBuffer = new byte[((Length + size) / PAGE_SIZE + 1) * PAGE_SIZE];
            Buffer.BlockCopy(ByteBuffer, 0, newBuffer, 0, Length);
            ByteBuffer = newBuffer;
        }
    }

    public byte[] GetBuffer()
    {
        return ByteBuffer;
    }

    public Bytes Append(uint val)
    {
        EnsureBufferSpace(4);
        ByteBuffer[Length++] = (byte)((val & 0xFF000000) >> 24);
        ByteBuffer[Length++] = (byte)((val & 0x00FF0000) >> 16);
        ByteBuffer[Length++] = (byte)((val & 0x0000FF00) >> 8);
        ByteBuffer[Length++] = (byte)(val & 0x000000FF);
        return this;
    }

    public Bytes Append(ushort val)
    {
        EnsureBufferSpace(2);
        ByteBuffer[Length++] = (byte)((val & 0xFF00) >> 8);
        ByteBuffer[Length++] = (byte)(val & 0x00FF);
        return this;
    }

    public Bytes Append(byte val)
    {
        EnsureBufferSpace(1);
        ByteBuffer[Length++] = val;
        return this;
    }

    public Bytes Append(byte[] buffer)
    {
        if (buffer != null)
        {
            EnsureBufferSpace(buffer.Length);
            Buffer.BlockCopy(buffer, 0, ByteBuffer, Length, buffer.Length);
            Length += buffer.Length;
        }

        return this;
    }

    public Bytes Append(Bytes otherBytes)
    {
        if (otherBytes != null)
        {
            EnsureBufferSpace(otherBytes.Length);
            Buffer.BlockCopy(otherBytes.ByteBuffer, 0, ByteBuffer, Length, otherBytes.Length);
            Length += otherBytes.Length;
        }

        return this;
    }

    public byte[] ToArray()
    {
        return ToArray(0, Length);
    }

    public byte[] ToArray(int start)
    {
        return ToArray(start, Length);
    }

    public byte[] ToArray(int start, int end)
    {
        if (end <= start || start >= Length)
            return new byte[0];

        var arr = new byte[end - start];
        Buffer.BlockCopy(ByteBuffer, start, arr, 0, end - start);
        return arr;
    }

    public Bytes Sub(int start)
    {
        return Sub(start, Length);
    }

    public Bytes Sub(int start, int end)
    {
        return new Bytes(ToArray(start, end));
    }

    public byte Byte(int index)
    {
        return ByteBuffer[index];
    }

    public ushort Word(int index)
    {
        var ret = (ushort)(ByteBuffer[index++] << 8);
        ret |= ByteBuffer[index];
        return ret;
    }

    public uint DWord(int index)
    {
        var ret = (uint)(ByteBuffer[index++] << 24);
        ret |= (uint)(ByteBuffer[index++] << 16);
        ret |= (uint)(ByteBuffer[index++] << 8);
        ret |= ByteBuffer[index++];
        return ret;
    }

    public string ToHex(int start, int end)
    {
        var sb = new StringBuilder();
        for (var i = start; i < end; i++)
            sb.Append($"{ByteBuffer[i]:X2}");
        return sb.ToString();
    }

    public string ToHex(int start)
    {
        return ToHex(start, Length);
    }

    public string ToHex()
    {
        return ToHex(0, Length);
    }

    public override string ToString()
    {
        return ToHex();
    }
}