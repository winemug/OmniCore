using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Data.Entities;

namespace OmniCore.Model.Interfaces.Platform
{
    public interface IPodRequest
    {
        IPodRequestEntity Entity { get; }
        Task WaitForResult(CancellationToken cancellationToken);
        Task Cancel();
    }
}