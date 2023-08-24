using OmniCore.Shared.Api;

namespace OmniCore.Common.Api;

public interface IApiClient : IDisposable
{
    Task<TResponse?> PostRequestAsync<TRequest, TResponse>(string route, TRequest request,
        CancellationToken cancellationToken = default) where TResponse : ApiResponse;
}