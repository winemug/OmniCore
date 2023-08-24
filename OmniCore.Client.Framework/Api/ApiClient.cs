using System.Net.Http.Json;
using System.Text.Json;
using OmniCore.Common.Api;
using OmniCore.Common.Core;
using OmniCore.Shared.Api;

namespace OmniCore.Framework.Api;

public class ApiClient : IApiClient
{
    private IAppConfiguration _appConfiguration;

    private bool _disposed;

    private readonly HttpClient _httpClient;

    public ApiClient(IAppConfiguration appConfiguration)
    {
        _appConfiguration = appConfiguration;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(appConfiguration.ApiAddress, UriKind.Absolute)
        };
    }

    public async Task<TResponse?> PostRequestAsync<TRequest, TResponse>(string route, TRequest request,
        CancellationToken cancellationToken = default) where TResponse : ApiResponse
    {
        try
        {
            var result = await _httpClient.PostAsJsonAsync(new Uri(route, UriKind.Relative),
                request, cancellationToken);
            return await result.Content.ReadFromJsonAsync<TResponse>((JsonSerializerOptions?)null, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
    }
}