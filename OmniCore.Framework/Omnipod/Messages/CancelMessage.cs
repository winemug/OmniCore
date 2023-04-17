using OmniCore.Common.Pod;
using OmniCore.Framework.Omnipod.Parts;
using OmniCore.Services;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OmniCore.Framework.Omnipod.Messages
{
    public class CancelMessage : IMessage<CancelMessageData>
    {
        public override IMessageParts DataToParts(CancelMessageData md)
        {
            var b0 = (int)md.Beep << 4;
            b0 |= md.CancelExtendedBolus ? 0x08 : 0x00;
            b0 |= md.CancelBolus ? 0x04 : 0x00;
            b0 |= md.CancelTempBasal ? 0x02 : 0x00;
            b0 |= md.CancelBasal ? 0x01 : 0x00;

            var data = new Bytes((byte)b0);
            return new MessageParts(new MessagePart
            {
                Data = data,
                Type = PodMessagePartType.RequestCancelDelivery,
                RequiresNonce = true
            });
        }

        public override CancelMessageData PartsToData(IMessageParts parts)
        {
            var data = parts.MainPart.Data;
            var b0 = data.Byte(0);
            var md = new CancelMessageData();
            md.CancelExtendedBolus = (b0 & 0x08) > 0;
            md.CancelBolus = (b0 & 0x04) > 0;
            md.CancelTempBasal= (b0 & 0x02) > 0;
            md.CancelBasal = (b0 & 0x01) > 0;
            return md;
        }
    }

    public class CancelMessageData : MessageData
    {
        public BeepType Beep { get; set; }
        public bool CancelExtendedBolus { get; set; }
        public bool CancelBolus { get; set; }
        public bool CancelTempBasal { get; set; }
        public bool CancelBasal { get; set; }
    }
}
