namespace OmniCore.Shared.Dto.Api;

public record ClientJoinResponse : ApiResponse
{
    public string Username { get; init; }
    public string Dsn { get; init; }
    public string Queue { get; init; }
    public string Exchange { get; init; }
}