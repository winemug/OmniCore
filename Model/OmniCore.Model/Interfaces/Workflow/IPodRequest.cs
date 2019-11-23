using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Entities;

namespace OmniCore.Model.Interfaces.Workflow
{
    public interface IPodRequest
    {
        IPodRequestEntity Entity { get; }
        Task<bool> WaitForResult(CancellationToken cancellationToken);
        Task<bool> Cancel();
    }
}