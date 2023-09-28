using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml;
using Nito.AsyncEx;
using OmniCore.Client.Interfaces.Services;

namespace OmniCore.Client.Mobile.Services;

public class SettingsService : ISettingsService
{
    private ClientAuthorization? _clientAuthorization;

    private AmqpEndpointDefinition? _endpoint;

    private ConcurrentDictionary<string, AsyncManualResetEvent> _keyValueChangedEvents;

    public SettingsService()
    {
        _keyValueChangedEvents = new ConcurrentDictionary<string, AsyncManualResetEvent>();
    }

    private void EnsureKeyEvent(string key)
    {

    }

    public async Task<string?> WaitForValueChangedAsync(string key, CancellationToken cancellationToken)
    {
        var resetEvent = _keyValueChangedEvents.GetOrAdd(key, s => new AsyncManualResetEvent());
        await resetEvent.WaitAsync(cancellationToken);
        return Get(key, (string?) null);
    }

    public string? Get(string key, string? defaultValue = null) => Preferences.Get(key, defaultValue);
    public void Set(string key, string? value) => Preferences.Set(key, value);

    public int Get(string key, int defaultValue = default) => Preferences.Get(key, defaultValue);
    public void Set(string key, int value) => Preferences.Set(key, value);

    public long Get(string key, long defaultValue = default) => Preferences.Get(key, defaultValue);
    public void Set(string key, long value) => Preferences.Set(key, value);

    public bool Get(string key, bool defaultValue = default) => Preferences.Get(key, defaultValue);
    public void Set(string key, bool value) => Preferences.Set(key, value);

    public T? Get<T>(string key, T? defaultValue = null) where T : class, new()
    {
        var strVal = Get(key, defaultValue.TrySerialize());
        return strVal.TryDeserialize<T>();
    }

    public void Set<T>(string key, T? value) where T : class, new()
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