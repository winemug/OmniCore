using System;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Services.Interfaces.Radio;

namespace OmniCore.Services.Interfaces.Core;

public interface IRadioService : ICoreService
{
    Task<IRadioConnection> GetIdealConnectionAsync(
        CancellationToken cancellationToken = default);

    Task<IRadioConnection> GetConnectionByNameAsync(string name,
        CancellationToken cancellationToken = default);

    Task<IRadioConnection> GetConnectionByIdAsync(Guid id,
        CancellationToken cancellationToken = default);
}