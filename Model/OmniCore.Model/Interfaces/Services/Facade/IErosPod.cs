using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Services.Facade
{
    public interface IErosPod : IPod
    {
        Task<IPodRequest> Activate(IRadio radio, CancellationToken cancellationToken);
        Task<IPodRequest> Acquire(IRadio radio, CancellationToken cancellationToken);
        Task<IPodRequest> VerifyIdentity(uint lotNumber, uint serialNumber, CancellationToken cancellationToken);
    }
}