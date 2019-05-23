using OmniCore.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Interfaces
{
    public interface IMessageExchangeParameters
    {
        TxPower? TransmissionLevelOverride { get; }
    }
}
