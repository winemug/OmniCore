using System.Text.Json;
using OmniCore.Common.Core;
using OmniCore.Common.Platform;
using OmniCore.Shared.Extensions;

namespace OmniCore.Maui.Services;

public class AppConfiguration : IAppConfiguration
{
    private IPlatformInfo _platformInfo;
    public AppConfiguration(IPlatformInfo platformInfo)
    {
        _platformInfo = platformInfo;
    }
    
#if DEBUG
    public Uri ApiAddress => new Uri("http://192.168.1.50:5097");
#else
    public Uri ApiAddress => new Uri("https://api.balya.net:8080");
#endif
    public string? AccountEmail
    {
        get => Preferences.Get(nameof(AccountEmail), null);
        set => Preferences.Set(nameof(AccountEmail), value);
    }

    public bool AccountVerified
    {
        get => Preferences.Get(nameof(AccountVerified), false);
        set => Preferences.Set(nameof(AccountVerified), value);
    }

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
    
    private ClientAuthorization? _clientAuthorization;
    public ClientAuthorization? ClientAuthorization
    {
        get
        {
            if (_clientAuthorization == null)
            {
                var val = Preferences.Get(nameof(ClientAuthorization), null);
                _clientAuthorization = JsonSerializerWrapper.TryDeserialize<ClientAuthorization>(val);
            }

            return _clientAuthorization;
        }
        set
        {
            _clientAuthorization = value;
            var strVal = JsonSerializerWrapper.TrySerialize(value);
            Preferences.Set(nameof(ClientAuthorization), strVal);
        }
    }
}