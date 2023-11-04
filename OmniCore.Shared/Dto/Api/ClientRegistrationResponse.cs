namespace OmniCore.Shared.Dto.Api;

public record ClientRegistrationResponse : ApiResponse
{
    public Guid ClientId { get; init; }

    public byte[] Token { get; init; }
}