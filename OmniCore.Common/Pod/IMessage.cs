using OmniCore.Services.Interfaces.Entities;
using OmniCore.Services.Interfaces.Pod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Common.Pod;

public interface IMessage<T> where T : IMessageData
{
    IMessage<T> WithAddress(uint address);
    IMessage<T> WithSequence(int sequence);
    IMessage<T> AsCritical();
    IMessage<T> WithNonceProvider(INonceProvider nonceProvider);
    IMessage<T> WithMessageData(T messageData);
}
