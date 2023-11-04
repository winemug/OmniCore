namespace OmniCore.Shared.Dto.Api;

public record ClientJoinRequest
{
    public Guid Id { get; init; }
    public string Version { get; init; }
    public byte[] Token { get; init; }
}