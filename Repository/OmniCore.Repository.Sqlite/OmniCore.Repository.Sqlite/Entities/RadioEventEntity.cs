using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Data.Entities;
using SQLite;

namespace OmniCore.Repository.Sqlite.Entities
{
    public class RadioEventEntity : Entity, IRadioEventEntity
    {
        public RadioEvent EventType { get; set; }
        public bool Success { get; set; }
        public byte[] Data { get; set; }

        [Ignore]
        public IRadioEntity Radio { get; set; }
        public long? RadioId { get; set; }

        [Ignore]
        public IPodEntity Pod { get; set; }
        public long? PodId { get; set; }

        [Ignore]
        public IPodRequestEntity Request { get; set; }
        public long? RequestId { get; set; }
    }
}
