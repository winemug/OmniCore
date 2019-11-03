using OmniCore.Repository.Enums;
using SQLite;
using System;

namespace OmniCore.Repository.Entities
{
    public class FaultResponse : Entity
    {
        [Indexed]
        public long RequestId { get; set; }
        public int FaultCode { get; set; }
        public int FaultRelativeTime { get; set; }
        public bool FaultedWhileImmediateBolus { get; set; }
        public uint FaultInformation2LastWord { get; set; }
        public int StateTableCorruption { get; set; }
        public int InternalFaultVariables { get; set; }
        public PodProgress ProgressBeforeFault { get; set; }
        public PodProgress ProgressBeforeFault2 { get; set; }
        public int TableAccessFault { get; set; }

    }
}
