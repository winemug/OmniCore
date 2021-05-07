using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Requests;

namespace OmniCore.Eros
{
    public class ErosPodBolusRequest : ErosPodRequest, IPodBolusRequest
    {
        public decimal ImmediateBolusUnits { get; }
        public bool ExtendedBolus { get; }
        public bool ExtendedBolusTotalUnits { get; }
        public TimeSpan ExtendedBolusTotalDuration { get; }
        public IPodBolusRequest WithImmediateBolus(decimal immediateBolusUnits)
        {
            throw new NotImplementedException();
        }

        public IPodBolusRequest WithExtendedBolus(decimal extendedBolusTotalUnits, TimeSpan extendedBolusDuration)
        {
            throw new NotImplementedException();
        }

        protected override async Task ExecuteRequestInternal(IErosRadio radio, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ErosPodBolusRequest(IRepositoryService repositoryService) : base(repositoryService)
        {
        }
    }

}