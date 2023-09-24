using System.ComponentModel;

namespace OmniCore.Client.Interfaces.Services;

public interface ISettingsService : INotifyPropertyChanged
{
    string? AccountEmail { get; set; }
    bool AccountVerified { get; set; }
    string ClientName { get; set; }
    string ApiAddress { get; set; }
    ClientAuthorization? ClientAuthorization { get; set; }
    AmqpEndpointDefinition? Endpoint { get; set; }
}

public record ClientAuthorization
{
    public Guid ClientId { get; init; }
    public byte[] Token { get; init; }
}

public record AmqpEndpointDefinition
{
    public string UserId { get; init; }
    public string Dsn { get; init; }
    public string Queue { get; init; }
    public string Exchange { get; init; }
}