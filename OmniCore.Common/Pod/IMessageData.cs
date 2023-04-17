using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Common.Pod
{
    public interface IMessageData
    {
        PodMessagePartType MainPartType { get; }
        PodMessagePartType? SubPartType { get; }
    }
}
