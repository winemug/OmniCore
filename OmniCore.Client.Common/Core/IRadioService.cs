using OmniCore.Common.Radio;

namespace OmniCore.Common.Core;

public interface IRadioService : ICoreService
{
    Task<IRadioConnection> GetIdealConnectionAsync(
        CancellationToken cancellationToken = default);

    Task<IRadioConnection> GetConnectionByNameAsync(string name,
        CancellationToken cancellationToken = default);

    Task<IRadioConnection> GetConnectionByIdAsync(Guid id,
        CancellationToken cancellationToken = default);
}