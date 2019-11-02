using OmniCore.Repository.Enums;
using OmniCore.Model.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IMessagePart
    {
        PartType PartType { get; }
        Bytes PartData { get; }
    }
}
