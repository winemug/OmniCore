namespace OmniCore.Shared.Api;

public record AccountRegistrationRequest
{
    public string Email { get; init; }
}