using System.Text.Json;
using OmniCore.Common.Amqp;
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

    public string ApiAddress
    {
        get
        {
#if DEBUG
    var defaultAddress = "http://192.168.1.50:5097";
#else
    var defaultAddress = "https://api.balya.net:8080";
#endif
            return Preferences.Get(nameof(ApiAddress), defaultAddress);
        }
        set
        {
            if (value != null)
                Preferences.Set(nameof(ApiAddress), value);
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public string? AccountEmail
    {
        get => Preferences.Get(nameof(AccountEmail), null);
        set
        {
            Preferences.Set(nameof(AccountEmail), value);
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool AccountVerified
    {
        get => Preferences.Get(nameof(AccountVerified), false);
        set
        {
            Preferences.Set(nameof(AccountVerified), value);
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }
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
        set
        {
            Preferences.Set(nameof(ClientName), value);
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }
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
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private AmqpEndpointDefinition? _endpoint;
    public AmqpEndpointDefinition? Endpoint
    {
        get
        {
            if (_endpoint == null)
            {
                var val = Preferences.Get(nameof(Endpoint), null);
                _endpoint = JsonSerializerWrapper.TryDeserialize<AmqpEndpointDefinition>(val);
            }

            return _endpoint;
        }
        set
        {
            _endpoint = value;
            var strVal = JsonSerializerWrapper.TrySerialize(value);
            Preferences.Set(nameof(Endpoint), strVal);
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public event EventHandler? ConfigurationChanged;
}