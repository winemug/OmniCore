using OmniCore.Shared.Api;

namespace OmniCore.Client.Interfaces.Services;

public interface IApiClient : IDisposable
{
    Task<TResponse?> PostRequestAsync<TRequest, TResponse>(string route, TRequest request,
        CancellationToken cancellationToken = default) where TResponse : ApiResponse;
}