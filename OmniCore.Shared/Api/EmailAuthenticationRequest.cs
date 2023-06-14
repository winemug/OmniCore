namespace OmniCore.Shared.Api;

public record EmailAuthenticationRequest
{
    public string Email { get; init; }
}