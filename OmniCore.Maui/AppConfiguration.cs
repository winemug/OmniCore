using System.Text.Json;
using OmniCore.Services.Interfaces.Core;
using Microsoft.Maui.Storage;
using OmniCore.Services.Interfaces.Platform;
using OmniCore.Shared.Extensions;

namespace OmniCore.Maui;

public class AppConfiguration : IAppConfiguration
{
    private IPlatformInfo _platformInfo;
    public AppConfiguration(IPlatformInfo platformInfo)
    {
        _platformInfo = platformInfo;
    }
    
#if DEBUG
    public Uri ApiAddress => new Uri("http://192.168.1.50:8000");
#else
    public Uri ApiAddress => new Uri("https://api.balya.net:8080");
#endif
    public string ClientName
    {
        get
        {
            var name = Preferences.Get(nameof(ClientName), null);
            if (name == null)
                name = _platformInfo.GetUserName();
            return name;
        }
        set => Preferences.Set(nameof(ClientName), value);
    }

    private ApiAuthorization? _authorization;
    public ApiAuthorization? Authorization
    {
        get
        {
            if (_authorization == null)
            {
                var val = Preferences.Get(nameof(Authorization), null);
                _authorization = JsonSerializerWrapper.TryDeserialize<ApiAuthorization>(val);
            }
            return _authorization;
        }
        set
        {
            _authorization = value;
            var strVal = JsonSerializerWrapper.TrySerialize(value);
            Preferences.Set(nameof(Authorization), strVal);
        }
    }

    private EndpointDefinition? _endpoint;
    public EndpointDefinition? Endpoint
    {
        get
        {
            if (_endpoint == null)
            {
                var val = Preferences.Get(nameof(Endpoint), null);
                _endpoint = JsonSerializerWrapper.TryDeserialize<EndpointDefinition>(val);
            }

            return _endpoint;
        }
        set
        {
            _endpoint = value;
            var strVal = JsonSerializerWrapper.TrySerialize(value);
            Preferences.Set(nameof(Endpoint), strVal);
        }
    }
}