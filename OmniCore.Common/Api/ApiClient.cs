using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Android.Util;
using OmniCore.Services.Interfaces.Configuration;
using OmniCore.Services.Interfaces.Core;

namespace OmniCore.Common.Api;

public class AuthResponse
{
    public string? token { get; set; }
    public string? token_type { get; set; }
}

public class ClientRegisterRequest
{
    public string name { get; set; }
    public string platform { get; set; }
    public string hw_version { get; set; }
    public string sw_version { get; set; }
}

public class ClientRegisterResponse
{
    public string account_id { get; set; }
    public string client_id { get; set; }
    public string token { get; set; }
}

public class EndpointRequest
{
    public string sw_version { get; set; }
}

public class EndpointResponse
{
    public string user_id { get; set; }
    public string dsn { get; set; }
    public string queue { get; set; }
    public string exchange { get; set; }
}

public class ApiClient
{
    private bool _authorizedWithAccount;
    private bool _authorizedWithClientToken;
    private IConfigurationStore _configurationStore;

    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("http://192.168.1.50:8000/")
    };

    public ApiClient(IConfigurationStore configurationStore)
    {
        _configurationStore = configurationStore;
    }
    
    public async Task AuthorizeAccountAsync(string email, string password)
    {
        UnauthorizeAsync();

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("username", email), 
            new KeyValuePair<string, string>("password", password) 
        });
        var result = await _httpClient.PostAsync(new Uri("/auth/token"), content);
        var resultContent = await result.Content.ReadAsStringAsync();
        var ar = JsonSerializer.Deserialize<AuthResponse>(resultContent);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ar.token);
        _authorizedWithAccount = true;
        _authorizedWithClientToken = false;
    }

    public async Task RegisterClientAsync()
    {
        UnauthorizeAsync();
        var cc = await _configurationStore.GetConfigurationAsync();

        var rr = new ClientRegisterRequest()
        {
            name = "luks",
            platform = "droid",
            hw_version = "1.0",
            sw_version = "0.0"
        };

        var js = JsonSerializer.Serialize(rr, JsonSerializerOptions.Default);
        
        var content = new StringContent(js, Encoding.Default, "application/json");
        var result = await _httpClient.PostAsync(new Uri("/client/register"), content);
        var resultContent = await result.Content.ReadAsStringAsync();

        var crr = JsonSerializer.Deserialize<ClientRegisterResponse>(resultContent); 
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", crr.token);
        _authorizedWithClientToken = true;
        _authorizedWithAccount = false;
        cc.AccountId = Guid.Parse(crr.account_id);
        cc.ClientId = Guid.Parse(crr.client_id);
        cc.ClientAuthorizationToken = crr.token;
        await _configurationStore.SetConfigurationAsync(cc);
    }

    public async Task<EndpointResponse> GetClientEndpointAsync()
    {
        if (!_authorizedWithClientToken)
            throw new ApplicationException("Not authorized with client token");

        var cc = await _configurationStore.GetConfigurationAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cc.ClientAuthorizationToken);
        _authorizedWithClientToken = true;
        _authorizedWithAccount = false;
        
        var er = new EndpointRequest()
        {
            sw_version = "0.0"
        };

        var js = JsonSerializer.Serialize(er, JsonSerializerOptions.Default);
        var content = new StringContent(js, Encoding.Default,"application/json");

        var result = await _httpClient.PostAsync(new Uri("/client/endpoint"), content);
        var resultContent = await result.Content.ReadAsStringAsync();
        var erp = JsonSerializer.Deserialize<EndpointResponse>(resultContent);
        return erp;
    }

    public void UnauthorizeAsync()
    {
        _authorizedWithAccount = false;
        _authorizedWithClientToken = false;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}