using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Entities;
using SQLite;

namespace OmniCore.Repository.Sqlite.Entities
{
    public class RadioEventEntity : Entity, IRadioEventEntity
    {
        public RadioEvent EventType { get; set; }
        public bool Success { get; set; }
        public byte[] Data { get; set; }

        public long? RadioId => Radio?.Id;
        [Ignore]
        public IRadioEntity Radio { get; set; }
        public long? PodId => Pod?.Id;
        [Ignore]
        public IPodEntity Pod { get; set; }
        [Ignore]
        public long? RequestId => Request?.Id;
        public IPodRequestEntity Request { get; set; }
    }
}
