namespace OmniCore.Common.Core;

public record ClientAuthorization
{
    public Guid ClientId { get; init; }
    public byte[] Token { get; init; }
}