﻿using System;
using System.Collections.Generic;
using System.Text;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces.Platform.Common.Data.Entities;
using SQLite;

namespace OmniCore.Repository.Sqlite.Entities
{
    public class RadioEventEntity : Entity, IRadioEventEntity
    {
        public RadioEvent EventType { get; set; }
        public byte[] Data { get; set; }
        public string Text { get; set; }

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
