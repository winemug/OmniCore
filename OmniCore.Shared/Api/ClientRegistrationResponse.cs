namespace OmniCore.Shared.Api;

public record ClientRegistrationResponse : ApiResponse
{
    public Guid ClientId { get; init; }
    
    public byte[] Token { get; init; }
}
