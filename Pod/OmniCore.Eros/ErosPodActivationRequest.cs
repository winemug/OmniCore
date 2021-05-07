using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Requests;

namespace OmniCore.Eros
{
    public class ErosPodActivationRequest : IPodActivationRequest
    {
        public IPodActivationRequest WithRadio(IErosRadio radio)
        {
            throw new NotImplementedException();
        }

        public IPodActivationRequest ForPod(IErosPod pod)
        {
            return this;
        }
        public IPodActivationRequest WithNewRadioAddress(uint radioAddress)
        {
            throw new NotImplementedException();
        }

        public IPodActivationRequest WithPairAndPrime()
        {
            throw new NotImplementedException();
        }

        public IPodActivationRequest WithInjectAndStart(IDeliverySchedule deliverySchedule)
        {
            throw new NotImplementedException();
        }

        public IPodActivationRequest WithDeactivate()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<IPodRequest> Submit(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

}