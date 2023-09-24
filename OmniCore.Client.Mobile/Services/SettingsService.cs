using System.ComponentModel;
using System.Runtime.CompilerServices;
using OmniCore.Client.Interfaces.Services;

namespace OmniCore.Client.Mobile.Services;

public class SettingsService : ISettingsService
{
    private ClientAuthorization? _clientAuthorization;

    private AmqpEndpointDefinition? _endpoint;

    public SettingsService()
    {
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
            OnPropertyChanged();
        }
    }

    public string? AccountEmail
    {
        get => Preferences.Get(nameof(AccountEmail), null);
        set
        {
            Preferences.Set(nameof(AccountEmail), value);
            OnPropertyChanged();
        }
    }

    public bool AccountVerified
    {
        get => Preferences.Get(nameof(AccountVerified), false);
        set
        {
            Preferences.Set(nameof(AccountVerified), value);
            OnPropertyChanged();
        }
    }

    public string ClientName
    {
        get
        {
            var name = Preferences.Get(nameof(ClientName), null);
            if (name == null)
                name = "what a user";
            return name;
        }
        set
        {
            Preferences.Set(nameof(ClientName), value);
            OnPropertyChanged();
        }
    }

    public ClientAuthorization? ClientAuthorization
    {
        get
        {
            if (_clientAuthorization == null)
            {
                var val = Preferences.Get(nameof(ClientAuthorization), null);
                _clientAuthorization = val.TryDeserialize<ClientAuthorization>();
            }

            return _clientAuthorization;
        }
        set
        {
            _clientAuthorization = value;
            var strVal = value.TrySerialize();
            Preferences.Set(nameof(ClientAuthorization), strVal);
            OnPropertyChanged();
        }
    }

    public AmqpEndpointDefinition? Endpoint
    {
        get
        {
            if (_endpoint == null)
            {
                var val = Preferences.Get(nameof(Endpoint), null);
                _endpoint = val.TryDeserialize<AmqpEndpointDefinition>();
            }

            return _endpoint;
        }
        set
        {
            _endpoint = value;
            var strVal = value.TrySerialize();
            Preferences.Set(nameof(Endpoint), strVal);
            OnPropertyChanged();
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}