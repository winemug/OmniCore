using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OmniCore.Common.Pod;
using OmniCore.Services;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OmniCore.Framework.Omnipod.Messages;

public class BeepMessage : IMessageData
{
    public BeepType BeepNow { get; set; }
    public bool OnBasalStart { get; set; }
    public bool OnBasalEnd { get; set; }
    public int BasalBeepInterval { get; set; }
    public bool OnTempBasalStart { get; set; }
    public bool OnTempBasalEnd { get; set; }
    public int TempBasalBeepInterval { get; set; }
    public bool OnExtendedBolusStart { get; set; }
    public bool OnExtendedBolusEnd { get; set; }
    public int ExtendedBolusBeepInterval { get; set; }

    public static Predicate<IMessageParts> CanParse => (parts) => parts.MainPart.Type == PodMessagePartType.RequestBeepConfig;

    public IMessageParts ToParts()
    {
        var d0 = 0;
        d0 |= OnBasalStart ? 0x00800000 : 0x00000000;
        d0 |= OnBasalEnd ? 0x00400000 : 0x00000000;
        d0 |= (BasalBeepInterval & 0b00111111) << 16;

        d0 |= OnTempBasalStart ? 0x00008000 : 0x00000000;
        d0 |= OnTempBasalEnd ? 0x00004000 : 0x00000000;
        d0 |= (TempBasalBeepInterval & 0b00111111) << 8;

        d0 |= OnExtendedBolusStart ? 0x00000080 : 0x00000000;
        d0 |= OnExtendedBolusEnd ? 0x00000040 : 0x00000000;
        d0 |= ExtendedBolusBeepInterval & 0b00111111;

        d0 |= ((int)BeepNow & 0x0F) << 24;
        var data = new Bytes((uint)d0);

        return new MessageParts(
            new MessagePart
            {
                Type = PodMessagePartType.RequestBeepConfig,
                Data = data,
                RequiresNonce = false
            });
    }

    public IMessageData FromParts(IMessageParts parts)
    {
        var data = parts.MainPart.Data;
        var d0 = data.DWord(0);
        OnBasalStart = (d0 & 0x00800000) > 0;
        OnBasalEnd = (d0 & 0x00400000) > 0;
        OnTempBasalStart = (d0 & 0x00008000) > 0;
        OnTempBasalEnd = (d0 & 0x00004000) > 0;
        OnExtendedBolusStart = (d0 & 0x00000080) > 0;
        OnExtendedBolusEnd = (d0 & 0x00000040) > 0;
        BasalBeepInterval = (int)(d0 >> 16 & 0b00111111);
        TempBasalBeepInterval = (int)(d0 >> 8 & 0b00111111);
        ExtendedBolusBeepInterval = (int)(d0 & 0b00111111);
        BeepNow = (BeepType)(d0 >> 24 & 0X0F);
        return this;
    }

}
