using OmniCore.Common.Pod;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;

namespace OmniCore.Services;

public class PodMessageBuilder
{
    public PodMessageBuilder WithCriticalFollowup()
    {
        _criticalFollowUp = true;
        return this;
    }

    public PodMessage AssignAddress()
    {
        return BuildMessage(
            new MessagePart
            {
                Type = PodMessagePartType.RequestAssignAddress,
                Data = new Bytes(_address),
                RequiresNonce = false
            });
    }

    public PodMessage ConfigureBeep(BeepType beepNow,
        bool onBasalStart, bool onBasalEnd, int basalBeepInterval,
        bool onTempBasalStart, bool onTempBasalEnd, int tempBasalBeepInterval,
        bool onExtendedBolusStart, bool onExtendedBolusEnd, int extendedBolusBeepInterval)
    {
        var d0 = 0;
        d0 |= onBasalStart ? 0x00800000 : 0x00000000;
        d0 |= onBasalEnd ? 0x00400000 : 0x00000000;
        d0 |= (basalBeepInterval & 0b00111111) << 16;

        d0 |= onTempBasalStart ? 0x00008000 : 0x00000000;
        d0 |= onTempBasalEnd ? 0x00004000 : 0x00000000;
        d0 |= (tempBasalBeepInterval & 0b00111111) << 8;

        d0 |= onExtendedBolusStart ? 0x00000080 : 0x00000000;
        d0 |= onExtendedBolusEnd ? 0x00000040 : 0x00000000;
        d0 |= extendedBolusBeepInterval & 0b00111111;

        d0 |= ((int)beepNow & 0x0F) << 24;
        var data = new Bytes((uint)d0);
        
        return BuildMessage(
            new MessagePart
            {
                Type = PodMessagePartType.RequestBeepConfig,
                Data = data,
                RequiresNonce = false
            });
    }
    
    public PodMessage AcknowledgeAlerts()
    {
        return BuildMessage(
            new MessagePart
            {
                Type = PodMessagePartType.RequestAcknowledgeAlerts,
                Data = new Bytes((byte)0xff),
                RequiresNonce = true
            });
    }

    public PodMessage Cancel(
        BeepType beep,
        bool cancelExtendedBolus,
        bool cancelBolus,
        bool cancelTempBasal,
        bool cancelBasal)
    {
        var b0 = (int)beep << 4;
        b0 |= cancelExtendedBolus ? 0x08 : 0x00;
        b0 |= cancelBolus ? 0x04 : 0x00;
        b0 |= cancelTempBasal ? 0x02 : 0x00;
        b0 |= cancelBasal ? 0x01 : 0x00;

        var data = new Bytes((byte)b0);
        return BuildMessage(
            new MessagePart
            {
                Type = PodMessagePartType.RequestCancelDelivery,
                Data = data,
                RequiresNonce = true
            });
    }

    public PodMessage ConfigureAlerts(AlertConfiguration[] alertConfigurations)
    {
        var data = new Bytes();
        foreach (var alertConfiguration in alertConfigurations)
        {
            var b0 = (alertConfiguration.AlertIndex & 0x07) << 4;
            b0 |= alertConfiguration.SetActive ? 0x08 : 0x00;
            b0 |= alertConfiguration.ReservoirBased ? 0x04 : 0x00;
            b0 |= alertConfiguration.SetAutoOff ? 0x02 : 0x00;

            var d = alertConfiguration.AlertDurationMinutes & 0x1FF;
            b0 |= d >> 8;
            var b1 = d & 0xFF;
            data.Append((byte)b0).Append((byte)b1);

            var u0 = alertConfiguration.AlertAfter & 0x3FFF;
            var b2 = (byte)alertConfiguration.BeepPattern;
            var b3 = (byte)alertConfiguration.BeepType;
            data.Append((ushort)u0).Append(b2).Append(b3);
        }
        return BuildMessage(
            new MessagePart
            {
                Type = PodMessagePartType.RequestConfigureAlerts,
                Data = data,
                RequiresNonce = true
            });
    }

    public PodMessage Deactivate()
    {
        return BuildMessage(
            new MessagePart
            {
                Type = PodMessagePartType.RequestDeactivatePod,
                Data = new Bytes(),
                RequiresNonce = true
            });
    }

    public PodMessage SetDeliveryFlags(byte b16, byte b17)
    {
        return BuildMessage(
            new MessagePart
            {
                Type = PodMessagePartType.RequestSetDeliveryFlags,
                Data = new Bytes(b16).Append(b17),
                RequiresNonce = true
            });
    }

    public PodMessage Setup(uint radioAddress, uint lot, uint serial,
        int packetTimeout,
        int year, int month, int day, int hour, int minute)
    {
        var data = new Bytes(radioAddress).Append(0);
        if (packetTimeout > 50)
            data.Append(50);
        else
            data.Append((byte)packetTimeout);
        data.Append((byte)month).Append((byte)day).Append((byte)(year - 2000))
            .Append((byte)hour).Append((byte)minute);
        data.Append(lot).Append(serial);
        return BuildMessage(
            new MessagePart
            {
                Type = PodMessagePartType.RequestSetupPod,
                Data = data,
                RequiresNonce = false
            });
    }
    
    public PodMessage Status(RequestStatusType type)
    {
        return BuildMessage(
            new MessagePart
            {
                Type = PodMessagePartType.RequestStatus,
                Data = new Bytes((byte)type),
                RequiresNonce = false
            });
    }

    public PodMessage Bolus(BolusEntry bolusEntry)
    {
        ushort totalPulses10 = (ushort)(bolusEntry.ImmediatePulseCount * 10);
        uint pulseInterval = (uint)(bolusEntry.ImmediatePulseInterval125ms * 100000 / 8);
        var mainData = new Bytes(0)
            .Append(totalPulses10)
            .Append(pulseInterval)
            .Append((ushort)0)
            .Append((uint)0);
        
        var schedules = new[]
        {
            new InsulinSchedule
            {
                BlockCount = 1,
                AddAlternatingExtraPulse = false,
                PulsesPerBlock = bolusEntry.ImmediatePulseCount
            }
        };
        
        var subData = GetData(ScheduleType.Bolus,
            1,
            (ushort)(bolusEntry.ImmediatePulseCount * bolusEntry.ImmediatePulseInterval125ms),
            (ushort)(bolusEntry.ImmediatePulseCount),
            schedules);
        
        return BuildMessage(
            new MessagePart
            {
                Type = PodMessagePartType.RequestBolus,
                Data = mainData,
                RequiresNonce = false
            },
            new MessagePart
            {
                Type = PodMessagePartType.RequestInsulinSchedule,
                Data = subData,
                RequiresNonce = true
            });
    }

    public PodMessage TempBasal(BasalRateEntry tempBasalEntry)
    {
        var mainData = new Bytes();
        mainData.Append(0).Append(0);

        var totalPulses10 = tempBasalEntry.PulsesPerHour * tempBasalEntry.HalfHourCount * 10 / 2;
        var hhPulses10 = tempBasalEntry.PulsesPerHour * 10 / 2;

        var avgPulseIntervalMs = 1800000000;
        if (tempBasalEntry.PulsesPerHour > 0)
            avgPulseIntervalMs = 360000000 / tempBasalEntry.PulsesPerHour;

        var pulseRecord = new Bytes();
        if (totalPulses10 == 0)
        {
            for (int i = 0; i < tempBasalEntry.HalfHourCount; i++)
                pulseRecord.Append((ushort)0).Append((uint)avgPulseIntervalMs);
        }
        else
        {
            var pulses10remaining = totalPulses10;
            while (pulses10remaining > 0)
            {
                var pulses10record = pulses10remaining;
                if (pulses10remaining > 0xFFFF)
                {
                    if (hhPulses10 > 0xFFFF)
                        pulses10record = 0XFFFF;
                    else
                    {
                        var hhCountFitting = 0xFFFF / hhPulses10;
                        if (hhCountFitting % 2 == 0)
                            hhCountFitting--;
                        pulses10record = hhCountFitting * hhPulses10;
                    }
                }
                pulseRecord.Append((ushort)pulses10record).Append((uint)avgPulseIntervalMs);
                pulses10remaining -= pulses10record;
            }
        }
        mainData.Append(pulseRecord.Sub(0, 6)).Append(pulseRecord);

        var schedules = new[]
        {
            new InsulinSchedule
            {
                BlockCount = tempBasalEntry.HalfHourCount,
                AddAlternatingExtraPulse = tempBasalEntry.PulsesPerHour % 2 == 1,
                PulsesPerBlock = tempBasalEntry.PulsesPerHour / 2
            }
        };
        
        var subData = GetData(ScheduleType.TempBasal,
            (byte)tempBasalEntry.HalfHourCount,
            30 * 60 * 8,
            (ushort)(tempBasalEntry.PulsesPerHour / 2),
            schedules);
        
        return BuildMessage(
            new MessagePart
            {
                Type = PodMessagePartType.RequestTempBasal,
                Data = mainData,
                RequiresNonce = false
            },
            new MessagePart
            {
                Type = PodMessagePartType.RequestInsulinSchedule,
                Data = subData,
                RequiresNonce = true
            });
    }

    protected PodMessage BuildMessage(
        uint address, int sequence, INonceProvider? nonceProvider,
        IMessagePart part)
    {
        return BuildMessage(new MessageParts(part));
    }

    protected PodMessage BuildMessage(
        uint address, int sequence, INonceProvider? nonceProvider,
        IMessagePart mainPart, IMessagePart subPart)
    {
        return BuildMessage(new MessageParts(mainPart, subPart));
    }

    private PodMessage BuildMessage(
        uint address, int sequence, INonceProvider? nonceProvider,
        IMessageParts parts)
    {
        return new PodMessage
        {
            Address = _address,
            Sequence = _sequence,
            WithCriticalFollowup = _criticalFollowUp,
            Body = BuildMessageBody(parts)
        };
    }
    private Bytes BuildMessageBody(IMessageParts parts)
    {
        var msgParts = new List<IMessagePart>();
        var partsList = parts.AsList();
        var bodyLength = 0;
        foreach (var part in partsList)
        {
            if (part.RequiresNonce)
            {
                if (_nonceProvider == null)
                    throw new ApplicationException("This message requires a nonce provider");
                part.Nonce = _nonceProvider.NextNonce();
                bodyLength += 4;
            }
            bodyLength += part.Data.Length + 2;
            msgParts.Add(part);
        }

        var messageBody = new Bytes(_address);
        byte b0 = 0x00;
        if (_criticalFollowUp)
            b0 = 0x80;
        b0 |= (byte)(_sequence << 2);
        b0 |= (byte)((bodyLength >> 8) & 0x03);
        var b1 = (byte)(bodyLength & 0xff);
        messageBody.Append(new[] { b0, b1 });
        foreach (var part in partsList)
        {
            messageBody.Append((byte)part.Type);
            if (part.RequiresNonce)
            {
                messageBody.Append((byte)(part.Data.Length + 4));
                messageBody.Append(part.Nonce);
            }
            else
            {
                messageBody.Append((byte)part.Data.Length);
            }

            messageBody.Append(part.Data);
        }

        var messageCrc = CrcUtil.Crc16(messageBody.ToArray());
        messageBody.Append(messageCrc);
        return messageBody;
    }
    
    private Bytes GetData(ScheduleType type,
        byte halfHourCount,
        ushort initialDuration125ms,
        ushort initialPulseCount,
        InsulinSchedule[] schedules
    )
    {
        var elements = new Bytes();
        foreach (var schedule in schedules)
        {
            var scheduleBlocksAdded = 0;
            while (scheduleBlocksAdded < schedule.BlockCount)
            {
                var blockCount = schedule.BlockCount - scheduleBlocksAdded;
                if (blockCount > 16)
                    blockCount = 16;
                var b0 = ((blockCount - 1) & 0x0f) << 4;
                if (schedule.AddAlternatingExtraPulse) b0 |= 0x08;

                b0 |= schedule.PulsesPerBlock >> 8;
                var b1 = schedule.PulsesPerBlock & 0xFF;
                elements.Append((byte)b0).Append((byte)b1);
                scheduleBlocksAdded += blockCount;
            }
        }

        var header = new Bytes(halfHourCount).Append(initialDuration125ms).Append(initialPulseCount);
        var checksum = header[0] + header[1] + header[2] + header[3] + header[4];

        // 'generated' table
        var hh_idx = 0;
        foreach (var schedule in schedules)
            for (var i = 0; i < schedule.BlockCount; i++)
            {
                var pw = schedule.PulsesPerBlock;
                if (schedule.AddAlternatingExtraPulse && hh_idx % 2 == 1)
                    pw += 1;
                checksum += (pw >> 8) & 0xFF;
                checksum += pw & 0xFF;
                hh_idx++;
            }

        return new Bytes((byte)type).Append((ushort)checksum).Append(header).Append(elements);
    }
}
