using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Requests;

namespace OmniCore.Eros
{
    public class ErosPodDeliveryCancellationRequest : ErosPodRequest, IPodDeliveryCancellationRequest
    {
        public bool StopBolusDelivery { get; }
        public bool StopExtendedBolusDelivery { get; }
        public bool StopBasalDelivery { get; }
        public IPodDeliveryCancellationRequest WithCancelImmediateBolus()
        {
            throw new NotImplementedException();
        }

        public IPodDeliveryCancellationRequest WithCancelExtendedBolus()
        {
            throw new NotImplementedException();
        }

        public IPodDeliveryCancellationRequest WithStopBasalDelivery()
        {
            throw new NotImplementedException();
        }

        public IPodDeliveryCancellationRequest CancelAll()
        {
            throw new NotImplementedException();
        }

        protected override async Task ExecuteRequestInternal(IErosRadio radio, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ErosPodDeliveryCancellationRequest(IRepositoryService repositoryService) : base(repositoryService)
        {
        }
    }
}