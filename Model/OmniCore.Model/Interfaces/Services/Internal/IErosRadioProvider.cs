using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Base;

namespace OmniCore.Model.Interfaces.Services.Internal
{
    public interface IErosRadioProvider : IServerResolvable
    {
        Guid ServiceUuid { get; }
        Task<IErosRadio> GetRadio(IBlePeripheral peripheral, CancellationToken cancellationToken);
    }
}