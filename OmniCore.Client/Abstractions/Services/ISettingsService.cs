using System.ComponentModel;

namespace OmniCore.Client.Abstractions.Services;

public interface ISettingsService : INotifyPropertyChanged
{
    ClientAuthorization? Authorization { get; set; }
    AmqpEndpointDefinition? EndpointDefinition { get; set; }
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