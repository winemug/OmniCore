namespace OmniCore.Services.Interfaces.Core;

public interface IAppConfiguration
{
    string ClientName { get; set; }
    Uri ApiAddress { get; }
    EndpointDefinition? Endpoint { get; set; }
}

public record EndpointDefinition
{
    public Guid ClientId { get; init; }
    public string Dsn { get; init; }
    public string Queue { get; init; }
    public string Exchange { get; init; }
}