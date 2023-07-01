namespace OmniCore.Common.Core;

public interface IAppConfiguration
{
    string? AccountEmail { get; set; }
    bool AccountVerified { get; set; }
    string ClientName { get; set; }
    string ApiAddress { get; set; }
    ClientAuthorization? ClientAuthorization { get; set; }
    event EventHandler ConfigurationChanged;
}

public record ClientAuthorization
{
    public Guid ClientId { get; init; }
    public byte[] Token { get; init; }
}
