using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using OmniCore.Services.Interfaces.Amqp;
using OmniCore.Services.Interfaces.Core;
using OmniCore.Shared.Api;

namespace OmniCore.Common.Api;

public class ApiClient : IApiClient
{
    private IAppConfiguration _appConfiguration;

    private HttpClient? _httpClient;
    public ApiClient(IAppConfiguration appConfiguration)
    {
        _appConfiguration = appConfiguration;
        _httpClient = new HttpClient
        {
            BaseAddress = appConfiguration.ApiAddress
        };
    }
    
    public async Task<ClientRegistrationResponse?> RegisterClientAsync(
        ClientRegistrationRequest clientRegistrationRequest,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _httpClient.PostAsJsonAsync(new Uri("/client/register", UriKind.Relative),
                clientRegistrationRequest, cancellationToken);
            return await result.Content.ReadFromJsonAsync<ClientRegistrationResponse>(JsonSerializerOptions.Default,
                cancellationToken);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    public void Dispose()
    {
        _httpClient?.Dispose();
        _httpClient = null;
    }
}