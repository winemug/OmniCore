using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Requests;

namespace OmniCore.Eros
{
    public class ErosPodScheduledDeliveryRequest : ErosPodRequest, IPodScheduledDeliveryRequest
    {
        public IPodScheduledDeliveryRequest WithDeliverySchedule(IDeliverySchedule schedule)
        {
            throw new NotImplementedException();
        }

        public IPodScheduledDeliveryRequest WithTimeOffset(DateTimeOffset timeOffset)
        {
            throw new NotImplementedException();
        }

        public IPodScheduledDeliveryRequest WithTemporaryRate(decimal hourlyRateUnits, TimeSpan duration)
        {
            throw new NotImplementedException();
        }

        public IPodScheduledDeliveryRequest WithScheduledRate()
        {
            throw new NotImplementedException();
        }

        protected override async Task ExecuteRequestInternal(IErosRadio radio, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ErosPodScheduledDeliveryRequest(IRepositoryService repositoryService) : base(repositoryService)
        {
        }
    }
}