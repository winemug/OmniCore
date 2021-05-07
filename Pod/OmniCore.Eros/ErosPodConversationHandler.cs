using System.Threading.Tasks;
using OmniCore.Model.Interfaces;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Internal;

namespace OmniCore.Eros
{
    public class ErosPodConversationHandler
    {
        private ErosPod Pod;
        
        private readonly IContainer Container;
        private readonly IPodService PodService;
        private readonly ILogger Logger;
        private readonly IRepositoryService RepositoryService;
        public ErosPodConversationHandler(IContainer container,
            IRepositoryService repositoryService,
            IPodService podService,
            ILogger logger)
        {
            RepositoryService = repositoryService;
            PodService = podService;
            Container = container;
            Logger = logger;
        }

        public async Task Execute(ErosPodRequest podRequest,
            ErosPodRequestMessage requestMessage,
            IErosRadio radio,
            ErosPodConversationOptions conversationOptions)
        {
            var requestData = requestMessage
                .WithMessageSequence(Pod.Entity.NextMessageSequence)
                .GetRequestData();

            if (radio.HasProperOmnipodInterface)
            {
                
            }
            else
            {
            }
        }
    }
}