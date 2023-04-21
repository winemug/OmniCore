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

namespace OmniCore.Framework.Omnipod.Messages;

public class AssignAddressMessage : IMessageData
{
    public static Predicate<IMessageParts> CanParse => (parts) => parts.MainPart.Type == PodMessagePartType.RequestAssignAddress;

    public uint Address { get; set; }

    public IMessageData FromParts(IMessageParts parts)
    {
        var data = parts.MainPart.Data;
        Address = data.DWord(0);
        return this;
    }

    public IMessageParts ToParts()
    {
        return new MessageParts(
            new MessagePart
            {
                Type = PodMessagePartType.RequestAssignAddress,
                Data = new Bytes(Address),
                RequiresNonce = false
            });
    }
}
