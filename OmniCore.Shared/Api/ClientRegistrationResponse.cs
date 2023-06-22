namespace OmniCore.Shared.Api;

public record ClientRegistrationResponse : ApiResponse
{
    public Guid ClientId { get; init; }
    public string Username { get; init; }
    public string Dsn { get; init; }
    public string Queue { get; init; }
    public string Exchange { get; init; }
}
