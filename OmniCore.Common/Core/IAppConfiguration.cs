namespace OmniCore.Services.Interfaces.Core;

public interface IAppConfiguration
{
    string ClientName { get; set; }
    Uri ApiAddress { get; }
    ApiAuthorization? Authorization { get; set; }
    EndpointDefinition? Endpoint { get; set; }
}

public record ApiAuthorization
{
    public Guid ClientId { get; init; }
    public string ApiToken { get; init; }
}

public record EndpointDefinition
{
    public string Dsn { get; init; }
    public string Queue { get; init; }
    public string Exchange { get; init; }
}