using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Entities;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services.Internal;
using OmniCore.Model.Interfaces.Services.Requests;

namespace OmniCore.Eros
{
    public class ErosPodStatusRequest : ErosPodRequest, IPodStatusRequest
    {
        private readonly IContainer Container;
        public ErosPodStatusRequest(IContainer container)
        {
            Container = container;
        }
        
        public IPodStatusRequest WithUpdateStatus()
        {
            return this;
        }

        protected override async Task ExecuteRequestInternal(CancellationToken cancellationToken)
        {
            var requestMessage = await Container.Get<ErosPodRequestMessage>();
            requestMessage.WithStatusRequest(StatusRequestType.Standard);
            var conversation = await Container.Get<IErosPodConversation>();
            var radio = await this.Pod.RadioSelector.Select(cancellationToken);

            // await conversation.Run(radio, requestMessage, cancellationToken);
        }
    }
}