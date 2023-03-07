using System;
using System.Threading;
using System.Threading.Tasks;

namespace OmniCore.Services.Interfaces;

public interface IRadioService
{
    void Start();
    void Stop();

    Task<IRadioConnection> GetIdealConnectionAsync(
        CancellationToken cancellationToken = default);

    Task<IRadioConnection> GetConnectionByNameAsync(string name,
        CancellationToken cancellationToken = default);

    Task<IRadioConnection> GetConnectionByIdAsync(Guid id,
        CancellationToken cancellationToken = default);
}