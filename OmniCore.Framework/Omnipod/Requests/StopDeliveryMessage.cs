using OmniCore.Common.Pod;
using OmniCore.Services;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OmniCore.Framework.Omnipod.Requests
{
    public class StopDeliveryMessage : IMessageData
    {
        public BeepType Beep { get; set; }
        public bool StopExtendedBolus { get; set; }
        public bool StopBolus { get; set; }
        public bool StopTempBasal { get; set; }
        public bool StopBasal { get; set; }

        public static Predicate<IMessageParts> CanParse => (parts) => parts.MainPart.Type == PodMessagePartType.RequestCancelDelivery;

        public IMessageParts ToParts()
        {
            var b0 = (int)Beep << 4;
            b0 |= StopExtendedBolus ? 0x08 : 0x00;
            b0 |= StopBolus ? 0x04 : 0x00;
            b0 |= StopTempBasal ? 0x02 : 0x00;
            b0 |= StopBasal ? 0x01 : 0x00;

            var data = new Bytes((byte)b0);
            return new MessageParts(new MessagePart
            {
                Data = data,
                Type = PodMessagePartType.RequestCancelDelivery,
                RequiresNonce = true
            });
        }

        public IMessageData FromParts(IMessageParts parts)
        {
            var data = parts.MainPart.Data;
            var b0 = data.Byte(0);
            StopExtendedBolus = (b0 & 0x08) > 0;
            StopBolus = (b0 & 0x04) > 0;
            StopTempBasal = (b0 & 0x02) > 0;
            StopBasal = (b0 & 0x01) > 0;
            return this;
        }
    }
}
