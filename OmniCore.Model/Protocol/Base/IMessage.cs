using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Protocol.Base
{
    public interface IMessage
    {
        bool WillFollowUpWithCriticalRequest { get; }
        IEnumerable<IMessagePart> Parts { get; }
        byte[] GetMessageData();
    }
}
