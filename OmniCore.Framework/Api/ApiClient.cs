using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using OmniCore.Services.Interfaces.Amqp;
using OmniCore.Services.Interfaces.Core;

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
    
    public async Task AuthorizeAccountAsync(string email, string password)
    {
        
        //TODO: re
        
        // var content = new FormUrlEncodedContent(new[]
        // {
        //     new KeyValuePair<string, string>("username", email), 
        //     new KeyValuePair<string, string>("password", password) 
        // });
        // var result = await _httpClient.PostAsync(new Uri("/auth/token", UriKind.Relative), content);
        // var resultContent = await result.Content.ReadAsStringAsync();
        // var ar = JsonSerializer.Deserialize<AuthResponse>(resultContent);
        // _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ar.access_token);
    }

    public async Task RegisterClientAsync()
    {
        //TODO: new
        
        // var cc = await _configurationStore.GetConfigurationAsync();
        //
        // var rr = new ClientRegisterRequest()
        // {
        //     name = "luks",
        //     platform = "droid",
        //     hw_version = "1.0",
        //     sw_version = "0.0"
        // };
        //
        // var js = JsonSerializer.Serialize(rr, JsonSerializerOptions.Default);
        //
        // var content = new StringContent(js, Encoding.Default, "application/json");
        // var result = await _httpClient.PostAsync(new Uri("/client/register", UriKind.Relative), content);
        // var resultContent = await result.Content.ReadAsStringAsync();
        //
        // var crr = JsonSerializer.Deserialize<ClientRegisterResponse>(resultContent); 
        // cc.AccountId = Guid.Parse(crr.account_id);
        // cc.ClientId = Guid.Parse(crr.client_id);
        // cc.ClientAuthorizationToken = crr.token;
        // await _configurationStore.SetConfigurationAsync(cc);
    }

    public async Task<AmqpEndpoint> GetClientEndpointAsync()
    {
        // var cc = await _configurationStore.GetConfigurationAsync();
        // _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cc.ClientAuthorizationToken);
        //
        // var er = new EndpointRequest()
        // {
        //     sw_version = "0.0"
        // };
        //
        // var js = JsonSerializer.Serialize(er, JsonSerializerOptions.Default);
        // var content = new StringContent(js, Encoding.Default,"application/json");
        //
        // var result = await _httpClient.PostAsync(new Uri("/client/endpoint", UriKind.Relative), content);
        // var resultContent = await result.Content.ReadAsStringAsync();
        // var erp = JsonSerializer.Deserialize<EndpointResponse>(resultContent);
        // return new AmqpEndpoint
        // {
        //     UserId = erp.user_id,
        //     Dsn = erp.dsn,
        //     Queue = erp.queue,
        //     Exchange = erp.exchange
        // };
        
        //TODO: new
        return new AmqpEndpoint();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _httpClient = null;
    }
}