using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Model.Interfaces
{
    public interface IForegroundTask : IDisposable
    {
        Task Execute(CancellationToken cancellationToken);
    }
}