using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageBuilder
    {
        IMessageBuilder WithPart(IMessagePart request);
        IMessage Build();
    }
}
