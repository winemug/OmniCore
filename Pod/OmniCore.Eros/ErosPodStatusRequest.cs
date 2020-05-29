using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Interfaces.Services.Requests;

namespace OmniCore.Eros
{
    public class ErosPodStatusRequest : ErosPodRequest, IPodStatusRequest
    {
        public StatusRequestType RequestType { get; set; }
        
        private readonly IContainer Container;
        public ErosPodStatusRequest(IContainer container) : base(null)
        {
            Container = container;
        }
        
        public IPodStatusRequest WithUpdateStatus()
        {
            RequestType = StatusRequestType.Standard;
            return this;
        }

        protected override async Task ExecuteRequestInternal(IErosRadio radio, CancellationToken cancellationToken)
        {
            var requestMessage = await Container.Get<ErosPodRequestMessage>();
            requestMessage
                .WithPod(Pod)
                .WithStatusRequest(RequestType);

            await Pod.ConversationHandler.Execute(this, requestMessage, radio, new ErosPodConversationOptions
            {
                DynamicTxAttenuation = true,
                TransmissionPowerOverride = TransmissionPower.BelowNormal
            });
        }
    }
}