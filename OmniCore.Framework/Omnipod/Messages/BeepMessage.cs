using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OmniCore.Common.Pod;
using OmniCore.Framework.Omnipod.Parts;
using OmniCore.Services;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OmniCore.Framework.Omnipod.Messages;

public class BeepMessage : Message<BeepMessageData>
{
    public override IMessageParts DataToParts(BeepMessageData md)
    {
        var d0 = 0;
        d0 |= md.OnBasalStart ? 0x00800000 : 0x00000000;
        d0 |= md.OnBasalEnd ? 0x00400000 : 0x00000000;
        d0 |= (md.BasalBeepInterval & 0b00111111) << 16;

        d0 |= md.OnTempBasalStart ? 0x00008000 : 0x00000000;
        d0 |= md.OnTempBasalEnd ? 0x00004000 : 0x00000000;
        d0 |= (md.TempBasalBeepInterval & 0b00111111) << 8;

        d0 |= md.OnExtendedBolusStart ? 0x00000080 : 0x00000000;
        d0 |= md.OnExtendedBolusEnd ? 0x00000040 : 0x00000000;
        d0 |= md.ExtendedBolusBeepInterval & 0b00111111;

        d0 |= ((int)md.BeepNow & 0x0F) << 24;
        var data = new Bytes((uint)d0);

        return new MessageParts(
            new MessagePart
            {
                Type = PodMessagePartType.RequestBeepConfig,
                Data = data,
                RequiresNonce = false
            });
    }

    public override BeepMessageData PartsToData(IMessageParts parts)
    {
        var data = parts.MainPart.Data;
        var d0 = data.DWord(0);
        var md = new BeepMessageData();
        md.OnBasalStart = (d0 & 0x00800000) > 0;
        md.OnBasalEnd = (d0 & 0x00400000) > 0;
        md.OnTempBasalStart = (d0 & 0x00008000) > 0;
        md.OnTempBasalEnd = (d0 & 0x00004000) > 0;
        md.OnExtendedBolusStart = (d0 & 0x00000080) > 0;
        md.OnExtendedBolusEnd = (d0 & 0x00000040) > 0;
        md.BasalBeepInterval = (int)(d0 >> 16 & 0b00111111);
        md.TempBasalBeepInterval = (int)(d0 >> 8 & 0b00111111);
        md.ExtendedBolusBeepInterval = (int)(d0 & 0b00111111);
        md.BeepNow = (BeepType)(d0 >> 24 & 0X0F);
        return md;
    }
}

public class BeepMessageData : IMessageData
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
    public PodMessagePartType MainPartType => PodMessagePartType.RequestBeepConfig;
    public PodMessagePartType? SubPartType => null;
}