using OmniCore.Common.Pod;
using OmniCore.Services.Interfaces.Pod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OmniCore.Framework.Omnipod.Responses;

public class StatusMessage : IMessageData
{
    public static Predicate<IMessageParts> CanParse => (parts) => parts.MainPart.Type == PodMessagePartType.ResponseStatus;

    public PodStatusModel StatusModel { get; set; }
    public PodProgressModel ProgressModel { get; set; }

    public StatusMessage()
    {
        //StatusModel = new PodStatusModel();
        //ProgressModel = new PodProgressModel();
    }

    public IMessageData FromParts(IMessageParts parts)
    {
        var data = parts.MainPart.Data;
        var b0 = data[0];
        var d0 = data.DWord(1);
        var d1 = data.DWord(5);

        ProgressModel = new PodProgressModel
        {
            Progress = (PodProgress)(b0 & 0x0F),
            Faulted = (d1 & 0x80000000) != 0,
        };
        StatusModel = new PodStatusModel
        {
            ExtendedBolusActive = (b0 & 0b10000000) > 0,
            ImmediateBolusActive = (b0 & 0b01000000) > 0,
            TempBasalActive = (b0 & 0b00100000) > 0,
            BasalActive = (b0 & 0b00010000) > 0,

            PulsesDelivered = (int)((d0 >> 15) & 0b000001111111111111),
            LastProgrammingCommandSequence = (int)((d0 >> 11) & 0b00001111),
            PulsesPending = (int)(d0 & 0b0000011111111111),

            UnackedAlertsMask = (int)((d1 >> 23) & 0x0F),
            ActiveMinutes = (int)((d1 >> 10) & 0b0001111111111111),
            PulsesRemaining = (int)(d1 & 0b0000001111111111),
        };
        return this;
    }

    public IMessageParts ToParts()
    {
        throw new NotImplementedException();
    }
}
