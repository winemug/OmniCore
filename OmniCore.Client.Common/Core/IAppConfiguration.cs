namespace OmniCore.Common.Core;

public interface IAppConfiguration
{
    string? AccountEmail { get; set; }
    bool AccountVerified { get; set; }
    string ClientName { get; set; }
    Uri ApiAddress { get; }
    ClientAuthorization? ClientAuthorization { get; set; } 
}

public record ClientAuthorization
{
    public Guid ClientId { get; init; }
    public byte[] Token { get; init; }
}
