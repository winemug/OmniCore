namespace OmniCore.Shared.Api;

public record ClientRegistrationRequest
{
    public string Email { get; init; }
    public string Password { get; init; }
    public string ClientName { get; init; }
}