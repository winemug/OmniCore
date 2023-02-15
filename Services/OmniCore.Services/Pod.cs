using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Xamarin.Forms;

namespace OmniCore.Services;

public class Pod
{
    public Guid Id { get; set; }
    public uint RadioAddress { get; set; }
    
    public int UnitsPerMilliliter { get; set; }
    public int Lot { get; set; }
    public int Serial { get; set; }
    public PodProgress Progress { get; set; }
    public bool Faulted { get; set; }
    public bool ExtendedBolusActive { get; set; }
    public bool ImmediateBolusActive { get; set;}
    public bool TempBasalActive { get; set;}
    public bool BasalActive { get; set;}
    
    public int PulsesDelivered { get; set;}
    
    public int PulsesPending { get; set;}
    
    public int? PulsesRemaining { get; set;}
    
    public int ActiveMinutes { get; set;}
    
    public int UnackedAlertsMask { get; set;}
    
    public int LastProgrammingCommandSequence { get; set;}
    public uint? LastNonce { get; private set; }
    public int NextMessageSequence { get; set; }
    public int NextPacketSequence { get; set; }
    public DateTimeOffset? LastRadioPacketReceived { get; set; }
    
    private AsyncLock _allocationLock = new ();

    public Pod()
    {
        InitializeNonceTable(0);
    }
    public async Task<IDisposable> LockAsync(CancellationToken cancellationToken)
    {
        return await _allocationLock.LockAsync(cancellationToken);
    }

    public void ProcessResponse(RadioMessage message)
    {
        foreach (var part in message.Parts)
        {
            switch (part.Type)
            {
                case RadioMessageType.ResponseVersionInfo:
                    break;
                case RadioMessageType.ResponseDetailInfo:
                    break;
                case RadioMessageType.ResponseStatus:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void ProcessVersionInfo(ResponseVersionInfoPart part)
    {
        
    }

    private void ProcessDetailInfo(ResponseDetailInfoPart part)
    {
        
    }

    private void ProcessStatus(ResponseStatusPart part)
    {
        
    }

    private void ProcessError(ResponseErrorPart part)
    {
        
    }
    
    public uint NextNonce()
    {
        if (!LastNonce.HasValue)
        {
            var b = new byte[4];
            new Random().NextBytes(b);
            LastNonce = (uint)(b[0] << 24 | b[1] << 16 | b[2] << 8 | b[3]);
        }
        else
        {
            LastNonce = NonceTable[NonceIndex];
            NonceTable[NonceIndex] = GenerateNonce();
            NonceIndex = (int)((LastNonce.Value & 0x0F) + 2);
        }

        return LastNonce.Value;
    }

    public void SyncNonce(ushort syncWord, int syncMessageSequence)
    {
        var w = (LastNonce.Value & 0xFFFF) + (CrcUtil.Crc16Table[syncMessageSequence] & 0xFFFF) + (Lot & 0xFFFF) +
                (Serial & 0xFFFF);
        var seed = (ushort)(((w & 0xFFFF) ^ syncWord) & 0xff);
        InitializeNonceTable(seed);
    }

    private uint[] NonceTable;
    private int NonceIndex = 0;

    private uint GenerateNonce()
    {
        NonceTable[0] = ((NonceTable[0] >> 16) + (NonceTable[0] & 0xFFFF) * 0x5D7F) & 0xFFFFFFFF;
        NonceTable[1] = ((NonceTable[1] >> 16) + (NonceTable[1] & 0xFFFF) * 0x8CA0) & 0xFFFFFFFF;
        return (NonceTable[1] + (NonceTable[0] << 16)) & 0xFFFFFFFF;
    }
    
    private void InitializeNonceTable(ushort seed)
    {
        NonceTable = new uint[18];
        NonceTable[0] = (uint)(((Lot & 0xFFFF) + 0x55543DC3 + (Lot >> 16) + (seed & 0xFF)) & 0xFFFFFFFF);
        NonceTable[1] = (uint)(((Serial & 0xFFFF) + 0xAAAAE44E + (Serial >> 16) + (seed >> 8)) & 0xFFFFFFFF);
        for (int i = 2; i < 18; i++)
        {
            NonceTable[i] = GenerateNonce();
        }

        NonceIndex = (int)(((NonceTable[0] + NonceTable[1]) & 0xF) + 2);
    }
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}