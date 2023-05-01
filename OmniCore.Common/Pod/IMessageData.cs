using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OmniCore.Common.Pod
{
    public interface IMessageData
    {
        IMessageParts ToParts();
        IMessageData FromParts(IMessageParts parts);

        [JsonIgnore]
        abstract static Predicate<IMessageParts> CanParse { get; }

    }
}
