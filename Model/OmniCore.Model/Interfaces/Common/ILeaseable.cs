using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces.Common
{
    public interface ILeaseable<T>
    {
        Task<ILease<T>> Lease(CancellationToken cancellationToken);
        bool OnLease { get; set; }
        void ThrowIfNotOnLease();
    }
}
