using OmniCore.Common.Pod;

namespace OmniCore.Services;

public class NonceProvider : INonceProvider
{
    private int _nonceIndex;
    private uint[] _nonceTable;

    private uint? _lastNonce;
    private uint _lot;
    private uint _serial;
    
    public NonceProvider(uint lot, uint serial, uint? lastNonce = default)
    {
        _lot = lot;
        _serial = serial;
        _lastNonce = lastNonce;
        InitializeNonceTable(0);
    }
    
    public uint NextNonce()
    {
        if (!_lastNonce.HasValue)
        {
            var b = new byte[4];
            new Random().NextBytes(b);
            _lastNonce = (uint)((b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3]);
        }
        else
        {
            _lastNonce = _nonceTable[_nonceIndex];
            _nonceTable[_nonceIndex] = GenerateNonce();
            _nonceIndex = (int)((_lastNonce.Value & 0x0F) + 2);
        }

        return _lastNonce.Value;
    }

    public void SyncNonce(ushort syncWord, int syncMessageSequence)
    {
        uint w = (ushort)(_lastNonce.Value & 0xFFFF) + (uint)(CrcUtil.Crc16Table[syncMessageSequence] & 0xFFFF)
                                                     + (uint)(_lot & 0xFFFF) + (uint)(_serial & 0xFFFF);
        var seed = (ushort)(((w & 0xFFFF) ^ syncWord) & 0xff);
        InitializeNonceTable(seed);
    }
    private uint GenerateNonce()
    {
        _nonceTable[0] = ((_nonceTable[0] >> 16) + (_nonceTable[0] & 0xFFFF) * 0x5D7F) & 0xFFFFFFFF;
        _nonceTable[1] = ((_nonceTable[1] >> 16) + (_nonceTable[1] & 0xFFFF) * 0x8CA0) & 0xFFFFFFFF;
        return (_nonceTable[1] + (_nonceTable[0] << 16)) & 0xFFFFFFFF;
    }

    private void InitializeNonceTable(ushort seed)
    {
        _nonceTable = new uint[18];
        _nonceTable[0] = (uint)(((_lot & 0xFFFF) + 0x55543DC3 + (_lot >> 16) + (seed & 0xFF)) & 0xFFFFFFFF);
        _nonceTable[1] = (uint)(((_serial & 0xFFFF) + 0xAAAAE44E + (_serial >> 16) + (seed >> 8)) & 0xFFFFFFFF);
        for (var i = 2; i < 18; i++) _nonceTable[i] = GenerateNonce();

        _nonceIndex = (int)(((_nonceTable[0] + _nonceTable[1]) & 0xF) + 2);
    }
}