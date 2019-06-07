using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros
{
    public class ErosMessageExchangeParameters : IMessageExchangeParameters
    {
        public TxPower? TransmissionLevelOverride { get; set; }
        public bool AllowAutoLevelAdjustment { get; set; }
        public uint? AckAddressOverride { get; set;  }
        public uint? AddressOverride { get; set;  }
        public int? MessageSequenceOverride { get; set; }
        public bool RepeatFirstPacket { get; set;  }
        public bool CriticalWithFollowupRequired { get; set; }
        public Nonce Nonce { get; set; }
    }
}
