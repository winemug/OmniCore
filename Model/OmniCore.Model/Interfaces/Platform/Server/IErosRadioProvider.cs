using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Model.Interfaces.Platform.Server
{
    public interface IErosRadioProvider : IServerResolvable
    {
        Guid ServiceUuid { get; }
        Task<IErosRadio> GetRadio(IBlePeripheral peripheral, CancellationToken cancellationToken);
    }
}