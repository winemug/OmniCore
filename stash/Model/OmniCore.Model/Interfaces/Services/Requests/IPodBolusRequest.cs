using System;

namespace OmniCore.Model.Interfaces.Services.Requests
{
    public interface IPodBolusRequest : IPodRequest
    {
        public decimal ImmediateBolusUnits { get; }
        public bool ExtendedBolus { get; }
        public bool ExtendedBolusTotalUnits { get; }
        public TimeSpan ExtendedBolusTotalDuration { get; }
        public IPodBolusRequest WithImmediateBolus(decimal immediateBolusUnits);
        public IPodBolusRequest WithExtendedBolus(decimal extendedBolusTotalUnits, TimeSpan extendedBolusDuration);
    }
}