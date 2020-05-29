using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Services;
using OmniCore.Model.Interfaces.Services.Requests;

namespace OmniCore.Eros
{
    public class ErosPodAlarmRequest : ErosPodRequest, IPodAlarmRequest
    {
        protected override async Task ExecuteRequestInternal(IErosRadio radio, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ErosPodAlarmRequest(IRepositoryService repositoryService) : base(repositoryService)
        {
        }
    }
}