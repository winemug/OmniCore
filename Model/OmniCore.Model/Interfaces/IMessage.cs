using System;
using System.Collections.Generic;
using OmniCore.Model.Enums;
using OmniCore.Model.Utilities;

namespace OmniCore.Model.Interfaces
{
    public interface IMessage
    {
        RequestType RequestType { get; set; }
        string Parameters { get; set; }
        IList<IMessagePart> GetParts();
    }
}