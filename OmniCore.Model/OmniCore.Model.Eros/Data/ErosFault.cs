using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros.Data
{
    public class ErosFault : IFault
    {
        [PrimaryKey, AutoIncrement]
        public long? Id { get; set; }
        public Guid PodId { get; set; }
        public DateTimeOffset Created { get; set; }

        public int? FaultCode { get; set; }
        public int? FaultRelativeTime { get; set; }
        public bool? FaultedWhileImmediateBolus { get; set; }
        public uint? FaultInformation2LastWord { get; set; }
        public int? InsulinStateTableCorruption { get; set; }
        public int? InternalFaultVariables { get; set; }
        public PodProgress? ProgressBeforeFault { get; set; }
        public PodProgress? ProgressBeforeFault2 { get; set; }
        public int? TableAccessFault { get; set; }

    }
}
