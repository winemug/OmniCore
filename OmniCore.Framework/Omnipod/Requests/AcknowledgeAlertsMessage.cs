using OmniCore.Common.Pod;
using OmniCore.Services;
using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Framework.Omnipod.Requests;

public class AcknowledgeAlertsMessage : IMessageData
{
    public static Predicate<IMessageParts> CanParse => (parts) => parts.MainPart.Type == PodMessagePartType.RequestAcknowledgeAlerts;

    public bool[] AlertIndices { get; set; }

    public AcknowledgeAlertsMessage()
    {
        AlertIndices = new bool[8];
    }

    public IMessageData FromParts(IMessageParts parts)
    {
        var data = parts.MainPart.Data.Sub(4);
        var d0 = data.Byte(0);
        for (int i=0; i<8; i++)
        {
            var im = 1 << i;
            AlertIndices[i] = (d0 & im) > 0;
        }
        return this;
    }

    public IMessageParts ToParts()
    {
        byte d0 = 0;
        for(int i=0; i<8; i++)
        {
            if (AlertIndices[i])
                d0 |= (byte) (1 << i);
        }
        var data = new Bytes(d0);
        return new MessageParts(
        new MessagePart
        {
            Type = PodMessagePartType.RequestAcknowledgeAlerts,
            Data = data,
            RequiresNonce = true
        });
    }
}
