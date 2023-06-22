using OmniCore.Services.Interfaces.Amqp;
using OmniCore.Shared.Api;

namespace OmniCore.Common.Api;

public interface IApiClient : IDisposable
{
    Task<ClientRegistrationResponse?> RegisterClientAsync(ClientRegistrationRequest clientRegistrationRequest,
        CancellationToken cancellationToken = default);
}