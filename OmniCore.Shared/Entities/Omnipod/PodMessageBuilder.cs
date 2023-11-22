using OmniCore.Framework.Omnipod;
using OmniCore.Shared.Entities.Omnipod.Parts;
using OmniCore.Shared.Enums;
using OmniCore.Shared.Extensions;

namespace OmniCore.Shared.Entities.Omnipod;

public class PodMessageBuilder
{
    public uint? Address { get; private set; }
    public int? Sequence { get; private set; }
    public bool Critical { get; private set; }
    
    public int BodyLength { get; private set; }

    // public List<IMessagePart> Parts = new List<IMessagePart>();
    
    private byte[] _buffer = new byte[512];
    private int _bufferLen = 0;
    
    public PodMessageBuilder WithAddress(uint address)
    {
        Address = address;
        return this;
    }
    
    public PodMessageBuilder WithSequence(int sequence)
    {
        Sequence = sequence;
        return this;
    }

    public PodMessageBuilder AsCritical()
    {
        Critical = true;
        return this;
    }

    public PodMessageBuilder WithPart(IMessagePart part)
    {
        return this;
    }
    
    public PodMessageBuilder WithData(byte[] data)
    {
        data.CopyTo(_buffer, _bufferLen);
        _bufferLen += data.Length;
        return this;
    }

    public PodMessage Build()
    {
        var partList = new List<IMessagePart>();
        Span<byte> span = _buffer;
        span = span[0.._bufferLen];

        Address = span[0..].Read32();
        Critical = span[4..].ReadBit(0);
        Sequence = (int)span[4..].ReadBits(2, 4);
        BodyLength = (int)span[4..].ReadBits(6, 10);
        var body = span[6..(BodyLength + 6)];
        var crc = span[(6+BodyLength)..].Read16();
        
        var parts = partList.ToArray();
        var idx = 6;
        while (idx < 6 + BodyLength)
        {
            var type = (PodMessagePartType)span[idx];
            uint? nonce = null;
            Span<byte> pSpan;
            
            switch (type)
            {
                case PodMessagePartType.ResponseStatus:
                    pSpan = span[(idx + 1)..(idx + 9)];
                    idx += 10;
                    break;
                case PodMessagePartType.RequestSetupPod:
                case PodMessagePartType.RequestAssignAddress:
                case PodMessagePartType.RequestStatus:
                case PodMessagePartType.RequestBasal:
                case PodMessagePartType.RequestTempBasal:
                case PodMessagePartType.RequestBolus:
                case PodMessagePartType.RequestBeepConfig:
                case PodMessagePartType.ResponseVersionInfo:
                case PodMessagePartType.ResponseInfo:
                case PodMessagePartType.ResponseError:
                    pSpan = span[(idx + 2)..(span[idx + 1] + 2 + idx)];
                    idx += span[idx + 1] + 2;
                    break;
                case PodMessagePartType.RequestSetDeliveryFlags:
                case PodMessagePartType.RequestAcknowledgeAlerts:
                case PodMessagePartType.RequestConfigureAlerts:
                case PodMessagePartType.RequestInsulinSchedule:
                case PodMessagePartType.RequestDeactivatePod:
                case PodMessagePartType.RequestCancelDelivery:
                    nonce = span[(idx + 2)..].Read32();
                    pSpan = span[(idx + 6)..(span[idx + 1] + 6 + idx)];
                    idx += span[idx + 1] + 6;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        return new PodMessage
        {
            Address = this.Address.Value,
            Critical = this.Critical,
            Sequence = this.Sequence.Value,
            Parts = parts
        };
    }

    public bool CanBuild()
    {
        if (!Address.HasValue)
            return false;
        return true;
    }
}