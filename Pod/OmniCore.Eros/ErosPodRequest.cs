using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Entities;
using OmniCore.Model.Interfaces.Workflow;

namespace OmniCore.Eros
{
    public class ErosPodRequest : IPodRequest
    {
        public IPodRequestEntity Entity { get; }
        
        public Task WaitForResult(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task Cancel()
        {
            throw new System.NotImplementedException();
        }
    }
}