using OmniCore.Model.Enums;
using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros
{
    public class ErosPodFault : IPodFault
    {
        public uint? Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime Created { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Guid PodId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int? FaultCode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int? FaultRelativeTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool? FaultedWhileImmediateBolus { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public uint? FaultInformation2LastWord { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int? InsulinStateTableCorruption { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int? InternalFaultVariables { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public PodProgress? ProgressBeforeFault { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public PodProgress? ProgressBeforeFault2 { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int? TableAccessFault { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
