using OmniCore.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OmniCore.Model.Eros
{
    public class ErosPodUserSettings : IPodUserSettings
    {
        public uint? Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime Created { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Guid PodId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public decimal? ReservoirAlertLevel { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int? ExpiryAlertMinutes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
