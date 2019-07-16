using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros.Data
{
    public class ErosMessageExchangeParameters : IMessageExchangeParameters
    {
        [PrimaryKey, AutoIncrement]
        public long? Id { get; set; }
        public Guid PodId { get; set; }
        public DateTimeOffset Created { get; set; }

        public TxPower? TransmissionLevelOverride { get; set; }
        public bool AllowAutoLevelAdjustment { get; set; }
        public uint? AckAddressOverride { get; set;  }
        public uint? AddressOverride { get; set;  }
        public int? MessageSequenceOverride { get; set; }
        public bool RepeatFirstPacket { get; set;  }
        public bool CriticalWithFollowupRequired { get; set; }

        public ushort? FirstPacketPreambleLength { get; set; }
        public ushort? SubsequentPacketPreambleLength { get; set; }
        public uint? FirstPacketTimeout { get; set; }
        public uint? SubsequentPacketTimeout { get; set; }

        public int? FirstExchangeTimeout { get; set; }
        public int? SubsequentExchangeTimeout { get; set; }

        [Ignore]
        public Nonce Nonce { get; set; }
    }
}
