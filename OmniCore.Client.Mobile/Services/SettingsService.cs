using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml;
using Nito.AsyncEx;
using OmniCore.Client.Interfaces.Services;

namespace OmniCore.Client.Mobile.Services;

public class SettingsService : ISettingsService
{
    public ClientAuthorization? Authorization
    {
        get => Get<ClientAuthorization>(nameof(ClientAuthorization));
        set
        {
            Set(nameof(ClientAuthorization), value);
            OnPropertyChanged();
        }
    }

    public AmqpEndpointDefinition? EndpointDefinition
    {
        get => Get<AmqpEndpointDefinition>(nameof(AmqpEndpointDefinition));
        set
        {
            Set(nameof(AmqpEndpointDefinition), value);
            OnPropertyChanged();
        }
    }

    private string? Get(string key, string? defaultValue = null) => Preferences.Get(key, defaultValue);
    private void Set(string key, string? value) => Preferences.Set(key, value);

    private int Get(string key, int defaultValue = default) => Preferences.Get(key, defaultValue);
    private void Set(string key, int value) => Preferences.Set(key, value);

    private long Get(string key, long defaultValue = default) => Preferences.Get(key, defaultValue);
    private void Set(string key, long value) => Preferences.Set(key, value);

    private bool Get(string key, bool defaultValue = default) => Preferences.Get(key, defaultValue);
    private void Set(string key, bool value) => Preferences.Set(key, value);

    private T? Get<T>(string key, T? defaultValue = null) where T : class, new()
    {
        var strVal = Get(key, defaultValue.TrySerialize());
        return strVal.TryDeserialize<T>();
    }

    private void Set<T>(string key, T? value) where T : class, new()
    {
        Set(key, value.TrySerialize());
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