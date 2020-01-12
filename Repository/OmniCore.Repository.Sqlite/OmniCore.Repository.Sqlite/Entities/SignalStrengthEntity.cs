using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using SQLite;

namespace OmniCore.Repository.Sqlite.Entities
{
    public class SignalStrengthEntity : Entity, ISignalStrengthEntity
    {
        public int Rssi { get; set; }


        [Ignore]
        public IPodEntity Pod { get; set; }
        public long? PodId => Pod?.Id;

        [Ignore]
        public IRadioEntity Radio { get; set; }
        public long? RadioId => Radio?.Id;

        [Ignore]
        public IPodRequestEntity Request { get; set; }
        public long? RequestId => Request?.Id;
    }
}
