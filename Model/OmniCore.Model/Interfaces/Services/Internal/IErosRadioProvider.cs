using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Common;
using OmniCore.Model.Interfaces.Services.Facade;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IErosRadioProvider : IServiceInstance, IDisposable
    {
        Guid ServiceUuid { get; }
        Task<IErosRadio> GetRadio(IBlePeripheral peripheral, CancellationToken cancellationToken);
    }
}