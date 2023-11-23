using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OmniCore.Framework.Omnipod;
using OmniCore.Framework.Omnipod.Parts;
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
        var body = span[6..(BodyLength + 6 - 2)];
        var crc = span[(6+BodyLength)..].Read16();
       
        var idx = 6;
        while (idx < 6 + BodyLength - 2)
        {
            int t = span[idx];
            var type = (PodMessagePartType)t;
            int start, nStart, end;

            if (type == PodMessagePartType.ResponseStatus)
            {
                start = idx + 1;
                nStart = start;
                end = idx + 11;
            }
            else
            {
                var plen = span[idx + 1];
                start = idx + 2;
                nStart = idx + 6;
                end = idx + 2 + plen;
            }
            idx = end;

            IMessagePart part;

            switch (type)
            {
                case PodMessagePartType.ResponseStatus:
                    part = ResponseStatus.ToInstance(span[start..end]);
                    break;
                case PodMessagePartType.RequestSetupPod:
                    part = RequestSetupPod.ToInstance(span[start..end]);
                    break;
                case PodMessagePartType.RequestAssignAddress:
                    part = RequestAssignAddress.ToInstance(span[start..end]);
                    break;
                case PodMessagePartType.RequestStatus:
                    part = RequestStatus.ToInstance(span[start..end]);
                    break;
                case PodMessagePartType.RequestBasal:
                    part = RequestBasalIntervalSchedule.ToInstance(span[start..end]);
                    break;
                case PodMessagePartType.RequestTempBasal:
                    part = RequestTempBasalIntervalSchedule.ToInstance(span[start..end]);
                    break;
                case PodMessagePartType.RequestBolus:
                    part = RequestBolusIntervalSchedule.ToInstance(span[start..end]);
                    break;
                case PodMessagePartType.RequestBeepConfig:
                    part = RequestBeepConfig.ToInstance(span[start..end]);
                    break;
                case PodMessagePartType.ResponseVersionInfo:
                    if (end - start > 21)
                        part = ResponseVersionInfoExtended.ToInstance(span[start..end]);
                    else
                        part = ResponseVersionInfo.ToInstance(span[start..end]);
                    break;
                case PodMessagePartType.ResponseInfo:
                    if (span[start] == (byte)PodStatusType.Extended)
                        part = ResponseInfoExtendedStatus.ToInstance(span[start..end]);
                    else
                        part = ResponseInfo.ToInstance(span[start..end]);
                    break;
                case PodMessagePartType.ResponseError:
                    part = ResponseError.ToInstance(span[start..end]);
                    break;
                case PodMessagePartType.RequestSetDeliveryFlags:
                    part = RequestSetDeliveryFlags.ToInstance(span[nStart..end]);
                    break;
                case PodMessagePartType.RequestAcknowledgeAlerts:
                    part = RequestAcknowledgeAlerts.ToInstance(span[nStart..end]);
                    break;
                case PodMessagePartType.RequestConfigureAlerts:
                    part = RequestConfigureAlerts.ToInstance(span[nStart..end]);
                    break;
                case PodMessagePartType.RequestInsulinSchedule:
                    part = RequestBlockSchedule.ToInstance(span[nStart..end]);
                    break;
                case PodMessagePartType.RequestDeactivatePod:
                    part = RequestDeactivatePod.ToInstance(span[nStart..end]);
                    break;
                case PodMessagePartType.RequestCancelDelivery:
                    part = RequestCancelDelivery.ToInstance(span[nStart..end]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            partList.Add(part);
        }
        
        return new PodMessage
        {
            Address = this.Address.Value,
            Critical = this.Critical,
            Sequence = this.Sequence.Value,
            Parts = partList.ToArray()
        };

    }

    public bool CanBuild()
    {
        if (!Address.HasValue)
            return false;
        return true;
    }
}