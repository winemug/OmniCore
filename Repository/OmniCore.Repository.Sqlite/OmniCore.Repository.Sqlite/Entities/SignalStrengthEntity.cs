using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Entities;
using SQLite;

namespace OmniCore.Repository.Sqlite.Entities
{
    public class SignalStrengthEntity : BasicEntity, ISignalStrengthEntity
    {
        public int Rssi { get; set; }

        public long? PodId => Pod?.Id;
        public long? RadioId => Radio?.Id;
        public long? RequestId => Request?.Id;

        [Ignore]
        public IPodEntity Pod { get; set; }

        [Ignore]
        public IRadioEntity Radio { get; set; }

        [Ignore]
        public IPodRequestEntity Request { get; set; }
    }
}
