using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros
{
    public class ErosPodRadioIndicators : IPodRadioIndicators
    {
        [PrimaryKey, AutoIncrement]
        public long? Id { get; set; }
        public long ResultId { get; set; }

        public int? RadioLowGain { get; set; }
        public int? RadioRssi { get; set; }

        public ErosPodRadioIndicators()
        {
        }
    }
}
