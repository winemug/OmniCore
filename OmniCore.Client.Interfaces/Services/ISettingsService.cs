using System.ComponentModel;

namespace OmniCore.Client.Interfaces.Services;

public interface ISettingsService : INotifyPropertyChanged
{
    string? Get(string key, string? defaultValue = null);
    void Set(string key, string? value);
    int Get(string key, int defaultValue = default);
    void Set(string key, int value);
    long Get(string key, long defaultValue = default);
    void Set(string key, long value);
    bool Get(string key, bool defaultValue = default);
    void Set(string key, bool value);
    T? Get<T>(string key, T? defaultValue = null) where T : class, new();
    void Set<T>(string key, T? value) where T : class, new();
}

public record ClientAuthorization
{
    public Guid ClientId { get; init; }
    public byte[] Token { get; init; }
}

public class AmqpEndpointDefinition
{
    public string UserId { get; set; }
    public string Dsn { get; set; }
    public string Queue { get; set; }
    public string Exchange { get; set; }
}