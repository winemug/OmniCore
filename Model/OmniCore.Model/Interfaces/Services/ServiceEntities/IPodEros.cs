using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Interfaces.Platform.Server
{
    public interface IPodEros : IPod
    {
        Task<IPodRequest> Activate(IRadio radio, CancellationToken cancellationToken);
        Task<IPodRequest> Acquire(IRadio radio, CancellationToken cancellationToken);
        Task<IPodRequest> VerifyIdentity(uint lotNumber, uint serialNumber, CancellationToken cancellationToken);
    }
}